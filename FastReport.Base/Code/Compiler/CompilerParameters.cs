using System.Collections.Specialized;

namespace FastReport.Code.CodeDom.Compiler;

public class CompilerParameters
{
    public bool GenerateInMemory { get; set; }
    public StringCollection ReferencedAssemblies { get; } = new StringCollection();
    public TempFileCollection TempFiles { get; set; } = new TempFileCollection("", false);
}