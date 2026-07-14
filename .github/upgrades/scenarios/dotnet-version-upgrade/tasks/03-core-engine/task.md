# 03-core-engine: Upgrade core reporting engine project

Upgrade `FastReport.OpenSource` after level-0 completion. This project has the largest compatibility surface and is a dependency for most downstream projects, so stabilizing it is the central milestone in the migration.

The task addresses mandatory framework migration, package alignment, and the high volume of API compatibility issues identified in the assessment before dependents are upgraded.

**Done when**: `FastReport.OpenSource` is upgraded and builds cleanly with resolved mandatory compatibility items, and downstream projects can restore against the upgraded output.
