namespace FastReport.Code.CodeDom.Compiler;

public class CompilerError
{
    public int Line { get; set; }
    public int Column { get; set; }
    public string ErrorText { get; set; }
    public string ErrorNumber { get; set; }
}