using System.Collections.Generic;
using System.Reflection;

namespace FastReport.Code.CodeDom.Compiler;

public class CompilerResults
{
    public CompilerResults()
    {
    }

    public CompilerResults(System.Reflection.Assembly compiledAssembly)
    {
        CompiledAssembly = compiledAssembly;
    }

    public List<CompilerError> Errors { get; } = new List<CompilerError>();
    public System.Reflection.Assembly CompiledAssembly { get; }
}