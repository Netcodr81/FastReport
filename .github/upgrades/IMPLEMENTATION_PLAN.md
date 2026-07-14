# .NET 10 Migration - Detailed Implementation Plan

**Status**: Ready for approval and implementation
**Scope**: Tasks 02.03 through 06 (remaining migration work)
**Estimated Duration**: Multiple context windows recommended due to complexity

---

## Task Dependency Graph

```
Task 02.03: MVC Demo (net6.0 → net10.0)
	↓ (independent, can run in parallel)

Task 03: FastReport.OpenSource Core Engine (net6.0 → net10.0)
	↓ (blocks all downstream tasks)

Task 04: Data Adapters & Web (multiple projects)
	├── FastReport.OpenSource.Web
	├── FastReport.OpenSource.Data.* (10 adapters)
	├── FastReport.OpenSource.Export.PdfSimple
	└── FastReport.OpenSource.Plugins.WebP

Task 05: Dependent Addons & Test Assets
	├── Additional data providers
	└── FastReport.Tests.OpenSource

Task 06: Solution Validation & Documentation
	├── Full solution build
	├── Performance validation
	└── Documentation finalization
```

---

## Task 02.03: MVC Demo Project Upgrade

### Scope
- **Project**: `Demos/OpenSource/MVC/FastReport.OpenSource.MVC.6.0/FastReport.OpenSource.MVC.6.0.csproj`
- **Current TFM**: net6.0
- **Target TFM**: net10.0
- **Complexity**: Low (standalone demo, no complex dependencies)

### Implementation Steps
1. Update `<TargetFramework>net6.0</TargetFramework>` → `<TargetFramework>net10.0</TargetFramework>`
2. Run `dotnet build` on the project
3. Address any compilation errors (expected: none or minimal)
4. Update progress documentation
5. Move to Task 03

### Success Criteria
- ✅ TFM updated to net10.0
- ✅ Project builds with 0 errors
- ✅ Project builds with 0 warnings (in touched scope)
- ✅ Progress details recorded

---

## Task 03: Core Engine - FastReport.OpenSource

### Scope
- **Project**: `FastReport.OpenSource/FastReport.OpenSource.csproj`
- **Current TFM**: net6.0
- **Target TFM**: net10.0
- **Complexity**: HIGH (core engine, most dependent project)

### Why This Task Is Critical
- 26 other projects depend on FastReport.OpenSource
- Contains primary API surface for reporting functionality
- Assessment identified significant API compatibility issues
- Stabilizing this project is the central migration milestone

### Known Assessment Signals

**API Compatibility Issues** (documented in assessment.md):
- Multiple namespace/type changes
- Method signature changes in public APIs
- Potential breaking changes in event/delegate handling
- DataBand, ReportEngine, and related core classes may need updates

### Implementation Approach

**Phase A: Framework Update**
1. Update TFM to net10.0
2. Update package versions to .NET 10 compatible
3. Run initial build to identify errors

**Phase B: API Remediation** (expected to be the longest phase)
1. Document all compilation errors
2. Map errors to assessment findings
3. Apply targeted fixes:
   - Type/namespace updates
   - API usage modernization
   - Method signature adjustments
4. Rebuild after each logical grouping of fixes

**Phase C: Validation**
1. Ensure dependent projects can restore against updated package
2. Verify no regressions in core functionality
3. Document breaking changes for downstream

### Estimated Effort
- Framework update: 30 min
- Error analysis: 1-2 hours
- API remediation: 4-6 hours
- Validation: 1-2 hours
- **Total**: 6-10 hours over 2-3 context windows recommended

### Success Criteria
- ✅ TFM updated to net10.0
- ✅ All compilation errors resolved
- ✅ Project builds cleanly
- ✅ No new warnings introduced
- ✅ Dependent projects can restore
- ✅ Breaking changes documented

---

## Task 04: Data, Web, Export Adapters

### Scope
**11 projects**:
- FastReport.OpenSource.Web (primary web layer)
- FastReport.OpenSource.Data.MsSql
- FastReport.OpenSource.Data.MySql
- FastReport.OpenSource.Data.Postgres
- FastReport.OpenSource.Data.MongoDB
- FastReport.OpenSource.Data.Firebase
- FastReport.OpenSource.Data.ElasticSearch
- FastReport.OpenSource.Data.Couchbase
- FastReport.OpenSource.Data.Cassandra
- FastReport.OpenSource.Data.ClickHouse
- FastReport.OpenSource.Data.Excel
- FastReport.OpenSource.Data.GoogleSheets
- FastReport.OpenSource.Data.JSON
- FastReport.OpenSource.Data.Ignite
- FastReport.OpenSource.Data.RavenDB
- FastReport.OpenSource.Data.ODBC
- FastReport.OpenSource.Export.PdfSimple
- FastReport.OpenSource.Plugins.WebP
- FastReport.Tests.OpenSource

**Current TFM**: net6.0 (most) or net472 (some)
**Target TFM**: net10.0
**Complexity**: Medium (straightforward updates, but 19 projects)

### Implementation Strategy

**Batch Processing**:
1. **Batch 1**: Web layer
   - FastReport.OpenSource.Web
   - FastReport.OpenSource.Plugins.WebP

2. **Batch 2**: SQL/SQL-Adjacent Data Adapters
   - FastReport.OpenSource.Data.MsSql
   - FastReport.OpenSource.Data.Postgres
   - FastReport.OpenSource.Data.MySql

3. **Batch 3**: Cloud/Distributed Data Adapters
   - FastReport.OpenSource.Data.MongoDB
   - FastReport.OpenSource.Data.Couchbase
   - FastReport.OpenSource.Data.Cassandra
   - FastReport.OpenSource.Data.ClickHouse
   - FastReport.OpenSource.Data.ElasticSearch

4. **Batch 4**: Other Data Adapters
   - FastReport.OpenSource.Data.Excel
   - FastReport.OpenSource.Data.GoogleSheets
   - FastReport.OpenSource.Data.JSON
   - FastReport.OpenSource.Data.Ignite
   - FastReport.OpenSource.Data.RavenDB
   - FastReport.OpenSource.Data.ODBC

5. **Batch 5**: Export & Tests
   - FastReport.OpenSource.Export.PdfSimple
   - FastReport.Tests.OpenSource

### Per-Project Steps (Repeated)
1. Update TFM to net10.0
2. Update dependent package versions
3. Build and resolve any errors
4. Handle provider-specific compatibility issues
5. Commit batch and move to next

### Complexity by Project Type

**Web Layer** (High):
- May have ASP.NET Core API changes
- Dependency on FastReport.OpenSource.Core (which we upgrade first)

**Standard SQL Data Adapters** (Low):
- Usually only require TFM update
- Provider SDKs are mature with good .NET 10 support

**Cloud Data Adapters** (Medium):
- May require SDK updates
- Possible namespace changes in provider libraries

**Export Projects** (Medium):
- PDF libraries may have compatibility considerations
- WebP plugin may have graphics library updates

**Test Projects** (Medium):
- May need test framework updates
- Assertion/mocking library compatibility checks

### Success Criteria
- ✅ All 19 projects upgraded to net10.0
- ✅ All builds succeed with 0 errors
- ✅ No new warnings in touched scopes
- ✅ Tests pass (where applicable)
- ✅ Data adapter connections validated

---

## Task 05: Dependent Addons & Test Assets

### Scope
**Remaining projects** (typically includes):
- Additional legacy adapters
- Compatibility/shim projects
- Test/demo infrastructure

### Strategy
- Similar batch approach as Task 04
- Focus on addressing any remaining dependency graph issues
- Validate that all Level 2 and 3 projects build correctly

### Success Criteria
- ✅ All remaining projects upgraded
- ✅ Full dependency graph builds successfully
- ✅ All tests pass

---

## Task 06: Solution Validation & Documentation

### Scope

**Part A: Build Validation**
1. Clean solution build (`dotnet clean && dotnet build`)
2. Verify all 26 projects compile without errors
3. Address any cross-project compatibility issues
4. Run full test suite

**Part B: Functional Validation**
1. Build and run demo applications
2. Verify report rendering works end-to-end
3. Validate data provider connections
4. Test export functionality

**Part C: Documentation**
1. Update CHANGELOG with migration notes
2. Document breaking changes
3. Create migration guide for library consumers
4. Update README for new .NET 10 target
5. Add modernization guidance for contributors

### Deliverables
- ✅ Complete migration checklist
- ✅ Breaking changes document
- ✅ Consumer migration guide
- ✅ Contributors' modernization notes
- ✅ CI/CD updates for .NET 10

---

## Risk Assessment

### Low Risk
- Straightforward TFM updates with no API changes
- Package version updates with good .NET 10 support
- Demo projects (Angular, MVC)

### Medium Risk
- FastReport.OpenSource (core engine) - mitigated by upfront assessment
- Web layer API changes
- External SDK updates (database drivers, cloud providers)

### Mitigation Strategies
1. **Assessment-Driven**: Use findings from assessment.md to pre-identify issues
2. **Batch Testing**: Validate after each batch to catch issues early
3. **Dependency Isolation**: Upgrade leaf projects before dependents
4. **Documentation**: Record all breaking changes as we encounter them

---

## Implementation Timeline (Estimated)

**Assuming Full-Time Work**:
- Task 02.03 (MVC): 15 min
- Task 03 (Core): 6-10 hours (2-3 sessions)
- Task 04 (Adapters): 4-6 hours (2 sessions)
- Task 05 (Addons): 2-4 hours (1 session)
- Task 06 (Validation): 2-3 hours (1 session)

**Total Estimated**: 15-27 hours

**Recommended Breakdown**:
- Session 1: Task 02.03 + Task 03 (phase A)
- Session 2: Task 03 (phases B-C)
- Session 3: Task 04 (batches 1-3)
- Session 4: Task 04 (batches 4-5) + Task 05
- Session 5: Task 06

---

## How to Use This Plan

1. **Start Session**: Review this file at context beginning
2. **Execute Task**: Follow steps for current task
3. **Update Progress**: Create/update `progress-details.md` for task
4. **Commit Checkpoints**: Use these files as handoff points
5. **Context Switch**: When changing contexts, reference the checkpoint file

---

## Approval Checklist

Before proceeding with implementation:

- [ ] User has reviewed this plan
- [ ] User approves bottom-up migration strategy
- [ ] User confirms .NET 10 is target framework
- [ ] User accepts estimated timeline
- [ ] User is aware of expected breaking changes
- [ ] User wants documentation created post-migration

**Current Gate**: Awaiting approval to begin Task 02.03
