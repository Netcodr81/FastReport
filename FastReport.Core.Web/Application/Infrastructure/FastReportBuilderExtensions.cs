#if !WASM
using FastReport.Web.Application.Cache;
using FastReport.Web.Services.Abstract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace FastReport.Web.Application.Infrastructure
{
    public static class FastReportBuilderExtensions
    {
        public static IApplicationBuilder UseFastReport(this IApplicationBuilder app, Action<FastReportOptions> setupAction = null)
        {
            var options = SetupFastReport(setupAction, app.ApplicationServices);
            FastReportGlobal.FastReportOptions = options;

            ControllerBuilder.InitializeControllers();

            return app.UseMiddleware<FastReportMiddleware>();
        }

        /// <summary>
        /// Maps FastReport endpoints into endpoint routing so FastReport can be used in
        /// Razor Pages, MVC, or Minimal API hosts without relying on middleware ordering.
        /// </summary>
        public static IEndpointRouteBuilder MapFastReport(this IEndpointRouteBuilder endpoints, Action<FastReportOptions> setupAction = null)
        {
            var options = SetupFastReport(setupAction, endpoints.ServiceProvider);
            FastReportGlobal.FastReportOptions = options;

            ControllerBuilder.InitializeControllers();

            var routePattern = WebUtils.ToUrl(options.RouteBasePath, "{**fastReportPath}");
            endpoints.Map(routePattern, HandleEndpointRequest)
                .WithDisplayName("FastReport")
                .AllowAnonymous()
                .ExcludeFromDescription();

            return endpoints;
        }

        /// <summary>
        /// WebApplication convenience overload for fluent startup configuration.
        /// </summary>
        public static WebApplication MapFastReport(this WebApplication app, Action<FastReportOptions> setupAction = null)
        {
            MapFastReport((IEndpointRouteBuilder)app, setupAction);
            return app;
        }

        private static async Task HandleEndpointRequest(HttpContext httpContext)
        {
            FastReportGlobal.FastReportOptions.RoutePathBaseRoot = httpContext.Request.PathBase;

            if (!await ControllerBuilder.Executor.ExecuteAsync(httpContext).ConfigureAwait(false))
            {
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            }
        }

        private static FastReportOptions SetupFastReport(Action<FastReportOptions> setupAction, IServiceProvider serviceProvider)
        {
            FastReportServicesCheck(serviceProvider);

            var options = new FastReportOptions();
            setupAction?.Invoke(options);

            FastReport.Utils.Config.WebMode = true;

            // because WebReport..ctor adds WebReport instances to WebReportCache without DI
            WebReportCache.Instance = serviceProvider.GetService<IWebReportCache>();

            // TODO: find better way to share global objects
            FastReportGlobal.HostingEnvironment = serviceProvider.GetService<IWebHostEnvironment>();
            WebReport.ResourceLoader = serviceProvider.GetService<IResourceLoader>();

            return options;
        }

        private static void FastReportServicesCheck(IServiceProvider serviceProvider)
        {
            const string HAVE_TO_REGISTER_SERVICES = "Please, register FastReport services in DI container. Use services.AddFastReport()";

            _ = serviceProvider.GetService<IReportService>() ?? throw new Exception(HAVE_TO_REGISTER_SERVICES);
        }

    }
}
#endif
