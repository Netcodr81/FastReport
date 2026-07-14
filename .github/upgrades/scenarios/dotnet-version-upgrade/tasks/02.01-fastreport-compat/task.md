# 02.01-fastreport-compat: Upgrade FastReport.Compat target frameworks and API/package compatibility

# 02.01-fastreport-compat

## Objective
Upgrade `FastReport.Compat` as the primary level-0 compatibility library by adding .NET 10 targets in line with the selected multi-targeting approach and fixing mandatory/potential compatibility items required to build.

## Scope
- Project: `FastReport.Compat/FastReport.Compat/FastReport.Compat.csproj`
- Assessment signals: `Project.0002` (mandatory), `Api.0002` (495 potential)
- Technology focus: `System.Drawing` / GDI+ compatibility surface

## Research Findings

### Assessment details used for this subtask
- Mandatory issue: `Project.0002` requires TFM changes for `FastReport.Compat.csproj`.
- Potential issues are concentrated in 3 files:
  - `shared/TypeConverters/FontConverter.cs` (110 occurrences)
  - `shared/DotNetClasses/GdiGraphics.cs` (283 occurrences)
  - `shared/DotNetClasses/IGraphics.cs` (102 occurrences)
- Existing project is SDK-style and already multi-targeted.

### Project-file observations
- Current TFMs: `net462;net6.0` plus conditional `net6.0-windows7.0`.
- `System.Drawing.Common` is referenced for all non-`net462` targets, which will carry over to `net10.0` targets.
- `WindowsFormsReplacement` condition currently excludes only `net462` and `net6.0-windows7.0`; it must also include the new windows-specific TFM.

## Steps
1. Update TFMs to include `net10.0` and conditional `net10.0-windows7.0`.
2. Update project conditions that are TFM-specific to account for the new windows target.
3. Build the project and resolve any compile/package issues in scope.
4. Document outcomes and remaining compatibility follow-ups.

## Done when
- Project target framework configuration includes intended .NET 10 targets for migration stage.
- Restore/build succeeds for `FastReport.Compat` with warnings addressed in touched scope.
- Any required API/package compatibility fixes for this subtask scope are applied and documented.
