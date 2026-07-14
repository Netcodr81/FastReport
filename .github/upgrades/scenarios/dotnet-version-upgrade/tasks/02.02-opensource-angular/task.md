# 02.02-opensource-angular: Upgrade Angular demo project TFM and SPA package stack

# 02.02-opensource-angular

## Objective
Upgrade `FastReport.OpenSource.Angular` to .NET 10 and align SPA-related package dependencies based on assessment guidance, including replacement away from deprecated/legacy SPA services package usage where required.

## Scope
- Project: `Demos/OpenSource/SPA/FastReport.OpenSource.Angular/FastReport.OpenSource.Angular.csproj`
- Assessment signals: `Project.0002` (mandatory), `NuGet.0002` (2 package upgrade recommendations)
- Package focus: `Microsoft.AspNetCore.SpaProxy` and `Microsoft.AspNetCore.SpaServices.Extensions` modernization guidance

## Research Findings

### Assessment/package findings
- TFM change required from `net6.0` to `net10.0`.
- Package upgrade recommendations detected for:
  - `Microsoft.AspNetCore.SpaProxy` (`6.0.11` → `10.0.9`)
  - `Microsoft.AspNetCore.SpaServices.Extensions` (`6.0.1` → `10.0.9`)

### Source/config findings
- `Program.cs` already uses endpoint fallback (`MapFallbackToFile("index.html")`) and does not use legacy `UseSpa()` middleware.
- `launchSettings.json` already contains `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=Microsoft.AspNetCore.SpaProxy` for both profiles.
- `ClientApp/package.json` and `angular.json` already contain SpaProxy-compatible dev/proxy configuration.
- Post-restart environment check confirmed `node --version` is available, enabling DebugEnsureNodeEnv to run successfully.

## Steps
1. Update project TFM to net10.0.
2. Upgrade SPA package references to .NET 10-compatible versions.
3. Build and validate project compile/restore behavior.
4. Record completion details and any remaining follow-up notes.

## Done when
- Project targets net10.0 as planned.
- SPA package references are updated to supported .NET 10-compatible setup.
- Restore/build succeeds with warnings addressed in touched scope.
