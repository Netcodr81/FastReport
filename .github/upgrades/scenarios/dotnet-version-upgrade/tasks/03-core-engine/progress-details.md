## Files Modified
- `FastReport.OpenSource/FastReport.OpenSource.csproj`
- `UsedPackages.version`
- `FastReport.Base/Graphics/SkiaSharpGraphicsProvider.cs`
- `.github/upgrades/scenarios/dotnet-version-upgrade/tasks/03-core-engine/task.md`
- `.github/upgrades/scenarios/dotnet-version-upgrade/scenario-instructions.md`

## Build Result
- Errors: 0
- Warnings: 66
- Projects built: `FastReport.OpenSource` (which also built `FastReport.Base` and `FastReport.Compat`)

## Test Result
- Tests run: 81
- Passed: 81
- Failed: 0
- Test project: `Tools/FastReport.Tests.OpenSource/FastReport.Tests.OpenSource.csproj`
- Warning observed during test restore/build: `NU1903` for `Newtonsoft.Json 9.0.1` in the test project

## Changes Summary
- Updated `Microsoft.SourceLink.GitHub` to stable `10.0.301`.
- Updated shared `SkiaSharpVersion` to stable `4.150.1`.
- Migrated `FastReport.Base/Graphics/SkiaSharpGraphicsProvider.cs` to the SkiaSharp 4.x text/image APIs.
- Kept the SkiaSharp backend implementation internal to avoid exposing a brittle public surface.
- Saved the user preference to avoid preview package versions.

## Issues Encountered
- SkiaSharp 4.x removed text members from `SKPaint`, requiring `SKFont`-based text rendering and measurement updates.
- The solution still emits pre-existing warnings in `FastReport.Base` and the test project, mainly XML-doc and analyzer warnings; the build and tests succeeded regardless.