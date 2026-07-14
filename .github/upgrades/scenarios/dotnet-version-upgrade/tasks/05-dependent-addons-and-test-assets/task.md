# 05-dependent-addons-and-test-assets: Upgrade final dependent projects

Upgrade final dependency level projects: `FastReport.OpenSource.Data.Couchbase` and `FastReport.OpenSource.Export.PdfSimple.Tests`. These projects depend on already-upgraded lower levels and close the remaining project-level migration scope.

This task confirms all remaining dependency edges are resolved under the target framework and that late-stage dependent tests compile and execute successfully.

**Done when**: All level-3 projects are upgraded, build/test succeeds for this level, and no unresolved mandatory migration blockers remain.
