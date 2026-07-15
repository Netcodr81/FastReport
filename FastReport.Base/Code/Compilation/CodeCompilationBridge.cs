#if NETSTANDARD || NETCOREAPP
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FastReport.Code.CodeDom.Compiler;

namespace FastReport.Code.Compilation
{
    internal static class CodeCompilationBridge
    {
        internal static string TryFixAssemblyReference(Assembly assembly)
        {
            return CodeDomProvider.TryFixAssemblyReference(assembly);
        }

        internal static ValueTask<string> TryFixAssemblyReferenceAsync(Assembly assembly, CancellationToken cancellationToken)
        {
            return CodeDomProvider.TryFixAssemblyReferenceAsync(assembly, cancellationToken);
        }

        internal static CSharp.CSharpCodeProvider CreateCSharpProvider()
        {
            return new CSharp.CSharpCodeProvider();
        }


        internal static CompilerParameters CreateCompilerParameters()
        {
            return new CompilerParameters();
        }
    }
}
#endif
