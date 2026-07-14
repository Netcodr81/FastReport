# 02-foundation-and-standalone-apps: Upgrade dependency-root projects

Upgrade the dependency-root projects first: `FastReport.Compat`, `FastReport.OpenSource.Angular`, and `FastReport.OpenSource.MVC.6.0`. This stage establishes migration compatibility at the graph base and clears mandatory TFM changes for projects that do not depend on upgraded internal projects.

This task includes package and API updates required by these projects and validates that these projects compile cleanly on the target framework with selected migration options.

**Done when**: All level-0 projects target the intended framework configuration, restore/build succeeds for these projects, and associated tests (if present) pass.
