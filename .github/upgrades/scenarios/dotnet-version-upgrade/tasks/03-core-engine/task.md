# 03-core-engine: Upgrade core reporting engine project

Upgrade `FastReport.OpenSource` after level-0 completion. This project has the largest compatibility surface and is a dependency for most downstream projects, so stabilizing it is the central milestone in the migration.

The task addresses mandatory framework migration, package alignment, and the high volume of API compatibility issues identified in the assessment before dependents are upgraded.

## Research Findings

### Projects Affected
- `FastReport.OpenSource/FastReport.OpenSource.csproj` — direct package updates for SourceLink and SkiaSharp.
- `UsedPackages.version` — shared package version source used by core/open-source reporting projects.

### Packages to Update
| Package | Current | Target | Notes |
|---------|---------|--------|-------|
| Microsoft.SourceLink.GitHub | 1.1.1 | 10.0.301 | Latest stable package is MIT-licensed; no commercial license change detected. |
| SkiaSharp | 2.88.9 | 4.150.1 | Latest stable package is MIT-licensed; no commercial license change detected. |

### License Check
- Both updated packages remain open-source under MIT.
- No package needed to be pinned back to a last-open-source version for licensing reasons.
- Preview packages were explicitly avoided per user preference.

### Implementation Notes
- `FastReport.Base/Graphics/SkiaSharpGraphicsProvider.cs` was updated for SkiaSharp 4.x text/image APIs and its backend adapter types were made internal to keep the package surface stable.
- `FastReport.OpenSource` now references the stable SourceLink release and consumes the stable SkiaSharp version through `UsedPackages.version`.

**Done when**: `FastReport.OpenSource` is upgraded and builds cleanly with resolved mandatory compatibility items, and downstream projects can restore against the upgraded output.
