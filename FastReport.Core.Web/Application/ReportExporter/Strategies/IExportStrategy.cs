using System.IO;

using FastReport.Export;

namespace FastReport.Web.Application.ReportExporter.Strategies;

internal interface IExportStrategy
{
    void Export(Stream stream, Report report, ExportBase export);
}