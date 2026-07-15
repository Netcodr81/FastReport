#if !WASM
using Microsoft.AspNetCore.Html;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FastReport.Web.Application.Components
{
    /// <summary>
    /// Framework-agnostic web report component that can be used from Razor Pages,
    /// MVC, or Minimal APIs in ASP.NET Core applications.
    /// </summary>
    public sealed class WebReportComponent
    {
        private readonly WebReport _webReport;

        public WebReportComponent(WebReport webReport)
        {
            _webReport = webReport ?? throw new ArgumentNullException(nameof(webReport));
        }

        /// <summary>
        /// Wrapped FastReport web report instance.
        /// </summary>
        public WebReport Report => _webReport;

        /// <summary>
        /// Render report as framework-neutral HTML text.
        /// </summary>
        public Task<string> RenderHtmlAsync(CancellationToken cancellationToken = default)
            => _webReport.RenderHtmlAsync(cancellationToken);

        /// <summary>
        /// Render report as ASP.NET Core HTML content.
        /// </summary>
        public Task<HtmlString> RenderAsync()
            => _webReport.Render();
    }

    /// <summary>
    /// Factory abstraction for creating reusable web report components through DI.
    /// </summary>
    public interface IWebReportComponentFactory
    {
        WebReportComponent Create(Action<WebReport> configure = null);

        WebReportComponent Create(WebReport webReport);
    }

    internal sealed class WebReportComponentFactory : IWebReportComponentFactory
    {
        public WebReportComponent Create(Action<WebReport> configure = null)
        {
            var webReport = new WebReport();
            configure?.Invoke(webReport);
            return new WebReportComponent(webReport);
        }

        public WebReportComponent Create(WebReport webReport)
            => new WebReportComponent(webReport);
    }
}
#endif
