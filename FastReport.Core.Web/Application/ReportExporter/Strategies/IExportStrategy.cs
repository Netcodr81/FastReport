using FastReport.Export;
using System.IO;

namespace FastReport.Web.Application.ReportExporter.Strategies
{
    internal interface IExportStrategy
    {
        void Export(Stream stream, Report report, ExportBase export);
    }
}