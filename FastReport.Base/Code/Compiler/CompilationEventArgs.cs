using Microsoft.CodeAnalysis;

namespace FastReport.Code.CodeDom.Compiler;

public class CompilationEventArgs : System.EventArgs
{
    public Compilation Compilation { get; }

    public CompilationEventArgs(Compilation compilation)
    {
        Compilation = compilation;
    }
}