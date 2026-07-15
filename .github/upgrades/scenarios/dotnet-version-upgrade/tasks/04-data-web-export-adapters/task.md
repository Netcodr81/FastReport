# 04-data-web-export-adapters: Upgrade dependent integration projects

Upgrade the level-2 dependent projects that consume the core engine, including data providers, web layer, export extensions, plugin adapters, and top-level test host projects. This task modernizes the integration surface once the core engine baseline is stable.

The scope includes compatibility updates in projects with known API and package signals (for example Web, Odbc/MsSql/GoogleSheets providers, PdfSimple export, and related test/integration projects).

## Research Findings (current slice)
- `Extras/Core/FastReport.Data/FastReport.Data.MsSql/FastReport.OpenSource.Data.MsSql.csproj` already references `Microsoft.Data.SqlClient`.
- `Extras/Core/FastReport.Data/FastReport.Data.MsSql/MsSqlDataConnection.cs` still used `System.Data.SqlClient` namespace and needed alignment to `Microsoft.Data.SqlClient`.
- `Extras/Core/FastReport.Data/FastReport.Data.MsSql/Shared.props` still referenced `System.Data.SqlClient` and needed removal to avoid mixed provider packages.
- xUnit test projects in scope were on v2 package ids (`xunit`) and required migration to stable v3 package id (`xunit.v3`).
- `FastReport.Core.Web/FastReport.OpenSource.Web.csproj` already targets `net10.0`; the modernization need is API shape and hosting-model flexibility, not TFM change.
- `FastReport.Core.Web/Application/WebReport.Backend.cs` only exposes `Task<HtmlString> Render()` and `HtmlString RenderSync()`, which is view-oriented and not ideal as a reusable framework-agnostic component contract.
- `FastReport.Core.Web/Application/Infrastructure/FastReportBuilderExtensions.cs` currently exposes middleware-only `UseFastReport(...)`; a `MapFastReport(...)` endpoint-routing option exists only in a disabled `#if false` block.
- Introduce additive APIs for .NET 10 web hosts:
  - `WebReport.RenderHtmlAsync()` / `RenderHtml()` for string-based rendering contracts
  - `WebReportComponent` + `IWebReportComponentFactory` for DI-friendly reusable component creation and rendering
  - `MapFastReport(...)` endpoint mapping extension for endpoint routing (works with Razor Pages, MVC, and Minimal APIs in ASP.NET Core)
- Scope decision: desktop-framework-specific integrations are not expanded; this slice focuses on web-only reusable abstractions without adding WinForms/WPF dependencies.

**Done when**: All level-2 projects are upgraded with required package/API changes, targeted build validation passes, and migration issues are reduced to expected remaining level-3 scope.
