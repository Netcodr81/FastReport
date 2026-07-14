# 02.02-opensource-angular: Upgrade Angular demo project TFM and SPA package stack

# 02.02-opensource-angular

## Objective
Upgrade `FastReport.OpenSource.Angular` to .NET 10 and align SPA-related package dependencies based on assessment guidance, including replacement away from deprecated/legacy SPA services package usage where required.

## Scope
- Project: `Demos/OpenSource/SPA/FastReport.OpenSource.Angular/FastReport.OpenSource.Angular.csproj`
- Assessment signals: `Project.0002` (mandatory), `NuGet.0002` (2 package upgrade recommendations)
- Package focus: `Microsoft.AspNetCore.SpaProxy` and `Microsoft.AspNetCore.SpaServices.Extensions` modernization guidance

## Steps
1. Update target framework to net10.0 for the project.
2. Update/replace SPA-related package references as indicated by assessment and restore/build requirements.
3. Apply any minimal config/startup adjustments needed for package changes to compile.
4. Build and validate the project.

## Done when
- Project targets net10.0 as planned.
- SPA package references are updated to supported .NET 10-compatible setup.
- Restore/build succeeds with warnings addressed in touched scope.
