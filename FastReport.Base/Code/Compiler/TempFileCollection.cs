namespace FastReport.Code.CodeDom.Compiler;

public class TempFileCollection
{
    public string tempFolder;
    public bool v;

    public TempFileCollection(string tempFolder, bool v)
    {
        this.tempFolder = tempFolder;
        this.v = v;
    }
}