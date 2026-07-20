using System.IO;

using FastReport.Export;

namespace FastReport.Web.Application.ReportExporter.Strategies;

internal sealed class DefaultExportStrategy : IExportStrategy
{
    public void Export(Stream stream, Report report, ExportBase export)
    {
        report.Export(export, stream);
    }
}