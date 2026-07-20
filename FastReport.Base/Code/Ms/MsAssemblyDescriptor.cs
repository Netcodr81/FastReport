using FastReport.Code.CodeDom.Compiler;
using FastReport.Code.CSharp;
using FastReport.Utils;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace FastReport.Code.Ms
{
    partial class MsAssemblyDescriptor : AssemblyDescriptor
    {
        private static readonly ConcurrentDictionary<string, Assembly> _assemblyCache;
        private const string shaKey = "FastReportCode";
        private readonly static object _compileLocker;
        private readonly string _currentFolder;

        public Assembly Assembly { get; private set; }

        protected override ExpressionDescriptor CreateExpressionDescriptor(string methodName)
        {
            return new MsExpressionDescriptor(this, methodName);
        }

        protected override void InitField(string name, object c)
        {
            FieldInfo info = Instance.GetType().GetField(name,
              BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            info.SetValue(Instance, c);
        }

        private static bool ContainsAssembly(StringCollection assemblies, string assembly)
        {
            string asmName = Path.GetFileName(assembly);
            foreach (string a in assemblies)
            {
                string asmName1 = Path.GetFileName(a);
                if (String.Compare(asmName, asmName1, true) == 0)
                    return true;
            }
            return false;
        }

        private static void AddFastReportAssemblies(StringCollection assemblies)
        {
            foreach (Assembly assembly in RegisteredObjects.Assemblies)
            {
                string aLocation = assembly.Location;
                if (string.IsNullOrEmpty(aLocation))
                {
                    string fixedReference = CodeDomProvider.TryFixAssemblyReference(assembly);
                    if (!string.IsNullOrEmpty(fixedReference))
                        aLocation = fixedReference;
                }
                if (!ContainsAssembly(assemblies, aLocation))
                    assemblies.Add(aLocation);
            }
        }

        private void AddReferencedAssemblies(StringCollection assemblies, string defaultPath)
        {
            for (int i = 0; i < Report.ReferencedAssemblies.Length; i++)
            {
                string s = Report.ReferencedAssemblies[i];

if (s == "SkiaSharp.dll")
{
    var assemblyWithSkiaSharp = typeof(SKBitmap).Assembly.GetName()?.Name ?? "SkiaSharp";
    s = assemblyWithSkiaSharp;
}
                // fix for old reports with DataVisualization in referenced assemblies 
                if (s.Contains("DataVisualization"))
                    s = "FastReport.DataVisualization";

                AddReferencedAssembly(assemblies, defaultPath, s);
            }


            // these two required for "dynamic" type support
            AddReferencedAssembly(assemblies, defaultPath, "System.Core");
            AddReferencedAssembly(assemblies, defaultPath, "Microsoft.CSharp");
        }

        private void AddReferencedAssembly(StringCollection assemblies, string defaultPath, string assemblyName)
        {
            string location = GetFullAssemblyReference(assemblyName, defaultPath);
            if (location != "" && !ContainsAssembly(assemblies, location))
                assemblies.Add(location);
        }

        private string GetFullAssemblyReference(string relativeReference, string defaultPath)
        {
            return relativeReference;
        }

        public override void Compile()
        {
            if (NeedCompile)
            {
                lock (_compileLocker)
                {
                    if (NeedCompile)
                        InternalCompile();
                }
            }
        }

        protected override void CheckScriptSecurity()
        {
            base.CheckScriptSecurity();

            if (Config.WebMode &&
                Config.EnableScriptSecurity &&
                Config.ScriptSecurityProps.AddStubClasses)
                AddStubClasses();
        }

        private void InternalCompile()
        {
            CheckScriptSecurity();

            CompilerParameters cp = GetCompilerParameters();
            bool exception = !TryInternalCompile(cp, out var cr);

            for (int i = 0; exception && i < Config.CompilerSettings.RecompileCount; i++)
            {
                exception = !TryRecompile(cp, ref cr);
            }

            if (cr != null)
            {
                var ex = HandleCompileErrors(cr);

                if (exception && ex != null)
                    throw ex;
            }
        }

        private CompilerParameters GetCompilerParameters()
        {
            // configure compiler options
            CompilerParameters cp = new CompilerParameters();
            AddFastReportAssemblies(cp.ReferencedAssemblies);   // 2
            AddReferencedAssemblies(cp.ReferencedAssemblies, _currentFolder);    // 9
            ReviewReferencedAssemblies(cp.ReferencedAssemblies);
            cp.GenerateInMemory = true;
            // sometimes the system temp folder is not accessible...
            if (Config.TempFolder != null)
                cp.TempFiles = new TempFileCollection(Config.TempFolder, false);
            return cp;
        }

        private string GetAssemblyHash(CompilerParameters cp)
        {
            var assemblyHashSB = new StringBuilder();

            foreach (string a in cp.ReferencedAssemblies)
            {
                assemblyHashSB.Append(a);
            }

            assemblyHashSB.Append(ScriptText);
            byte[] hash;

            using (HMACSHA1 hMACSHA1 = new HMACSHA1(Encoding.ASCII.GetBytes(shaKey)))
            {
                hash = hMACSHA1.ComputeHash(Encoding.Unicode.GetBytes(assemblyHashSB.ToString()));
            }

            return Convert.ToBase64String(hash);
        }

        private CodeDomProvider GetCodeProvider()
        {
            return new CSharpCodeProvider();

        }

        /// <summary>
        /// Returns true, if compilation is successful
        /// </summary>
        private bool TryInternalCompile(CompilerParameters cp, out CompilerResults cr)
        {
            // find assembly in cache
            string assemblyHash = GetAssemblyHash(cp);
            Assembly cachedAssembly;
            if (_assemblyCache.TryGetValue(assemblyHash, out cachedAssembly))
            {
                Assembly = cachedAssembly;
                var reportScript = Assembly.CreateInstance("FastReport.ReportScript");
                InitInstance(reportScript);
                cr = null;
                return true;
            }

            // compile report scripts
            using (var provider = GetCodeProvider())
            {
                string script = ScriptText.ToString();
                ScriptSecurityEventArgs ssea = new ScriptSecurityEventArgs(Report, script, Report.ReferencedAssemblies);
                Config.OnScriptCompile(ssea);

provider.BeforeEmitCompilation += Config.OnBeforeScriptCompilation;
cr = provider.CompileAssemblyFromSource(cp, script, Config.CompilerSettings.CultureInfo);
                Assembly = null;
                Instance = null;

                if (cr.Errors.Count != 0)   // Compile errors
                    return false;

                _assemblyCache.TryAdd(assemblyHash, cr.CompiledAssembly);

                Assembly = cr.CompiledAssembly;
                var reportScript = Assembly.CreateInstance("FastReport.ReportScript");
                InitInstance(reportScript);
                return true;
            }
        }

        private CompilerException HandleCompileErrors(CompilerResults cr)
        {
            Regex regex;

            if (Config.WebMode && Config.EnableScriptSecurity)
            {
                for (int i = 0; i < cr.Errors.Count;)
                {
                    CompilerError ce = cr.Errors[i];
                    if (ce.ErrorNumber == "CS1685") // duplicate class
                    {
                        cr.Errors.Remove(ce);
                        continue;
                    }
                    else if (ce.ErrorNumber == "CS0436") // user using a forbidden type 
                    {
                        const string pattern = "[\"'](\\S+)[\"']";
                        regex = new Regex(pattern, RegexOptions.Compiled);
                        string typeName = regex.Match(ce.ErrorText).Value;

                        const string res = "Web,ScriptSecurity,ForbiddenType";
                        string message = Res.TryGet(res);
                        if (string.Equals(res, message))
                            message = "Please don't use the type " + typeName;
                        else
                            message = message.Replace("{typeName}", typeName); //$"Please don't use the type {typeName}";

                        ce.ErrorText = message;

                    }
                    else if (ce.ErrorNumber == "CS0117") // user using a forbidden method
                    {
                        const string pattern = "[\"'](\\S+)[\"']";
                        regex = new Regex(pattern, RegexOptions.Compiled);
                        MatchCollection mathes = regex.Matches(ce.ErrorText);
                        if (mathes.Count > 1)
                        {
                            string methodName = mathes[1].Value;

                            const string res = "Web,ScriptSecurity,ForbiddenMethod";
                            string message = Res.TryGet(res);
                            if (string.Equals(res, message))
                                message = "Please don't use the method " + methodName;
                            else
                                message = message.Replace("{methodName}", methodName); //$"Please don't use the method {methodName}";

                            ce.ErrorText = message;
                        }
                    }

                    i++;
                }
            }

            var errors = new List<CompilerException.Info>();
            var errorMsg = "";

            foreach (CompilerError ce in cr.Errors)
            {
                int line = GetScriptLine(ce.Line);
                // error is inside own items
                if (line == -1)
                {
                    string errObjName = GetErrorObjectName(ce.Line);

                    if (Config.CompilerSettings.ExceptionBehaviour != CompilerExceptionBehaviour.Default)
                    {
                        // handle errors when name does not exist in the current context
                        if (ce.ErrorNumber == "CS0103")
                        {
                            TextObjectBase text = Report.FindObject(errObjName) as TextObjectBase;
                            text.Text = ReplaceExpression(ce.ErrorText, text);
                            if (Config.CompilerSettings.ExceptionBehaviour == CompilerExceptionBehaviour.ShowExceptionMessage)
                                System.Diagnostics.Debug.WriteLine(ce.ErrorText);
                            continue;
                        }
                    }

                    // handle division by zero errors
                    if (ce.ErrorNumber == "CS0020")
                    {
                        TextObjectBase text = Report.FindObject(errObjName) as TextObjectBase;
                        text.CanGrow = true;
                        text.FillColor = SKColors.Red;
                        text.Text = "DIVISION BY ZERO!";
                        continue;
                    }
                    else
                    {
                        var msg = $"({errObjName}): {Res.Get("Messages,Error")} {ce.ErrorNumber}: {ce.ErrorText}";
                        errors.Add(new CompilerException.Info(0, 0, errObjName, msg));
                        errorMsg += msg + "\r\n";
                    }
                }
                else
                {
                    var msg = $"{Res.Get("Messages,Error")} {ce.ErrorNumber}: {ce.ErrorText}";
                    errors.Add(new CompilerException.Info(line, ce.Column, null, msg));
                    errorMsg += $"({line},{ce.Column}): {msg}\r\n";
                }
            }

            if (errors.Count > 0)
            {
                return new CompilerException(errorMsg, errors.ToArray());
            }

            return null;
        }

        /// <summary>
        /// Returns true if recompilation is successful
        /// </summary>
        private bool TryRecompile(CompilerParameters cp, ref CompilerResults cr)
        {
            List<string> additionalAssemblies = new List<string>(4);

            foreach (CompilerError ce in cr.Errors)
            {
                if (ce.ErrorNumber == "CS0012") // missing reference on assembly
                {
                    // try to add reference
                    try
                    {
                        // in .Net Core compiler will return other quotes
const string quotes = "\'";  // .NET Core compiler uses single quotes
                        const string pattern = quotes + @"(\S{1,}),";
                        Regex regex = new Regex(pattern, RegexOptions.Compiled);
                        string assemblyName = regex.Match(ce.ErrorText).Groups[1].Value;   // Groups[1] include string without quotes and , symbols
                        if (!additionalAssemblies.Contains(assemblyName))
                            additionalAssemblies.Add(assemblyName);
                        continue;
                    }
                    catch { }
                }
            }

            if (additionalAssemblies.Count > 0)  // need recompile
            {
                // try to load missing assemblies
                foreach (string assemblyName in additionalAssemblies)
                {
                    AddReferencedAssembly(cp.ReferencedAssemblies, _currentFolder, assemblyName);
                }

                return TryInternalCompile(cp, out cr);
            }

            return false;
        }

        public MsAssemblyDescriptor(Report report, string scriptText) : base(report, scriptText)
        {
            // set the current folder
            _currentFolder = Config.ApplicationFolder;
            if (Config.WebMode)
            {
                try
                {
                    string bin_directory = Path.Combine(_currentFolder, "Bin");
                    if (Directory.Exists(bin_directory))
                        _currentFolder = bin_directory;
                }
                catch
                {
                }
            }
        }

        static MsAssemblyDescriptor()
        {
            _assemblyCache = new ConcurrentDictionary<string, Assembly>();
            _compileLocker = new object();
        }
    }
}
