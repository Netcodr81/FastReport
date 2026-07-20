#if !WASM
using System;
using System.Collections.Generic;

using FastReport.Web.Cache;
using FastReport.Web.Services;

using Microsoft.AspNetCore.Hosting;

namespace FastReport.Web.Application.Infrastructure;

internal static class FastReportGlobal
{
    internal static IWebHostEnvironment HostingEnvironment = null;
    internal static FastReportOptions FastReportOptions = new FastReportOptions();

#if !WASM
    internal static EmailExportOptions InternalEmailExportOptions = null;
#endif

}
#endif