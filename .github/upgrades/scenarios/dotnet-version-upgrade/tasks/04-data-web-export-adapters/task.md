# 04-data-web-export-adapters: Upgrade dependent integration projects

Upgrade the level-2 dependent projects that consume the core engine, including data providers, web layer, export extensions, plugin adapters, and top-level test host projects. This task modernizes the integration surface once the core engine baseline is stable.

The scope includes compatibility updates in projects with known API and package signals (for example Web, Odbc/MsSql/GoogleSheets providers, PdfSimple export, and related test/integration projects).

**Done when**: All level-2 projects are upgraded with required package/API changes, targeted build validation passes, and migration issues are reduced to expected remaining level-3 scope.
