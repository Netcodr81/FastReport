# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade the FastReport.OpenSource solution to `net10.0` while maintaining migration stability across mixed .NET Framework and modern .NET projects.
**Scope**: Large solution modernization (26 assessed projects) with a 4-level dependency graph and significant API compatibility remediation.

### Selected Strategy
**Bottom-Up (Dependency-First)** — Upgrade from dependency leaves to top-level dependents in strict order.
**Rationale**: .NET Framework projects are present in a multi-project graph, so tiered dependency validation is required.

### Dependency Graph
```text
Level 3: FastReport.OpenSource.Data.Couchbase, FastReport.OpenSource.Export.PdfSimple.Tests
   ↓
Level 2: Data providers, Web, PdfSimple, Plugins.WebP, FastReport.Tests.OpenSource
   ↓
Level 1: FastReport.OpenSource
   ↓
Level 0: FastReport.Compat, FastReport.OpenSource.Angular, FastReport.OpenSource.MVC.6.0
```

## Tasks

### 01-prerequisites: Validate toolchain and migration constraints

Confirm the migration environment before touching project files: verify .NET 10 SDK availability, confirm global.json compatibility, and lock execution constraints from scenario-instructions.md (guided flow, bottom-up ordering, and no implementation changes without explicit approval). This task establishes the baseline guardrails for all subsequent tasks.

**Done when**: .NET 10 SDK readiness is verified, baseline constraints are documented in task context, and prerequisite blockers are either resolved or explicitly recorded.

---

### 02-foundation-and-standalone-apps: Upgrade dependency-root projects

Upgrade the dependency-root projects first: `FastReport.Compat`, `FastReport.OpenSource.Angular`, and `FastReport.OpenSource.MVC.6.0`. This stage establishes migration compatibility at the graph base and clears mandatory TFM changes for projects that do not depend on upgraded internal projects.

This task includes package and API updates required by these projects and validates that these projects compile cleanly on the target framework with selected migration options.

**Done when**: All level-0 projects target the intended framework configuration, restore/build succeeds for these projects, and associated tests (if present) pass.

---

### 03-core-engine: Upgrade core reporting engine project

Upgrade `FastReport.OpenSource` after level-0 completion. This project has the largest compatibility surface and is a dependency for most downstream projects, so stabilizing it is the central milestone in the migration.

The task addresses mandatory framework migration, package alignment, and the high volume of API compatibility issues identified in the assessment before dependents are upgraded.

**Done when**: `FastReport.OpenSource` is upgraded and builds cleanly with resolved mandatory compatibility items, and downstream projects can restore against the upgraded output.

---

### 04-data-web-export-adapters: Upgrade dependent integration projects

Upgrade the level-2 dependent projects that consume the core engine, including data providers, web layer, export extensions, plugin adapters, and top-level test host projects. This task modernizes the integration surface once the core engine baseline is stable.

The scope includes compatibility updates in projects with known API and package signals (for example Web, Odbc/MsSql/GoogleSheets providers, PdfSimple export, and related test/integration projects).

**Done when**: All level-2 projects are upgraded with required package/API changes, targeted build validation passes, and migration issues are reduced to expected remaining level-3 scope.

---

### 05-dependent-addons-and-test-assets: Upgrade final dependent projects

Upgrade final dependency level projects: `FastReport.OpenSource.Data.Couchbase` and `FastReport.OpenSource.Export.PdfSimple.Tests`. These projects depend on already-upgraded lower levels and close the remaining project-level migration scope.

This task confirms all remaining dependency edges are resolved under the target framework and that late-stage dependent tests compile and execute successfully.

**Done when**: All level-3 projects are upgraded, build/test succeeds for this level, and no unresolved mandatory migration blockers remain.

---

### 06-solution-validation-and-docs: Validate solution and document post-migration actions

Run full-solution validation after all upgrade tasks complete: restore/build/tests across the solution, confirm no mandatory upgrade issues remain, and record deferred modernization follow-ups (including post-migration CPM adoption and cross-platform replacement plan for Windows-specific APIs).

Update migration documentation in `docs/migration` with final status, decisions, and resumable context.

**Done when**: Full-solution validation passes, migration documentation is updated, and post-migration deferred recommendations are explicitly recorded.
