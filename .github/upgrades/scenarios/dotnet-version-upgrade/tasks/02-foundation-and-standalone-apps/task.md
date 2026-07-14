# 02-foundation-and-standalone-apps: Upgrade dependency-root projects

Upgrade the dependency-root projects first: `FastReport.Compat`, `FastReport.OpenSource.Angular`, and `FastReport.OpenSource.MVC.6.0`. This stage establishes migration compatibility at the graph base and clears mandatory TFM changes for projects that do not depend on upgraded internal projects.

This task includes package and API updates required by these projects and validates that these projects compile cleanly on the target framework with selected migration options.

## Research Findings

### Scope Inventory
- Projects affected:
  - `FastReport.Compat/FastReport.Compat/FastReport.Compat.csproj`
  - `Demos/OpenSource/SPA/FastReport.OpenSource.Angular/FastReport.OpenSource.Angular.csproj`
  - `Demos/OpenSource/MVC/FastReport.OpenSource.MVC.6.0/FastReport.OpenSource.MVC.6.0.csproj`
- Distinct concerns:
  - Multi-target framework transition for `FastReport.Compat` with significant API compatibility surface (`Api.0002`, 495 potential issues).
  - AspNetCore SPA package modernization in Angular demo (`NuGet.0002`, `Microsoft.AspNetCore.SpaServices.Extensions` replacement guidance).
  - TFM bump and behavioral check for MVC demo (`Api.0003` potential behavioral change).
- Issue signals from assessment:
  - `FastReport.Compat`: 496 issues (1 mandatory TFM + 495 potential API issues), GDI+/System.Drawing-heavy.
  - `FastReport.OpenSource.Angular`: 3 issues (1 mandatory TFM + 2 package upgrade recommendations).
  - `FastReport.OpenSource.MVC.6.0`: 2 issues (1 mandatory TFM + 1 behavioral change signal).

### Decomposition Decision
This parent task should be decomposed into project-scoped subtasks because each project has different migration mechanics, package/API risk profiles, and independent validation boundaries.

**Done when**: All level-0 projects target the intended framework configuration, restore/build succeeds for these projects, and associated tests (if present) pass.
