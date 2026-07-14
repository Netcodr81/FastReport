namespace FastReport.Web
{
    partial class WebReport
    {
        string template_style() => $" :root{{ {GetStyleVars()} }}";
    }
}