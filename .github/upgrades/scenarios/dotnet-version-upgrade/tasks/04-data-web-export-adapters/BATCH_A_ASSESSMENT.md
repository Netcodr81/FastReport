# Task 04 - Batch A Assessment: Simple Data Adapters

**Status**: Assessment Complete  
**Date**: 2024  
**Batch**: A (Simple Data Adapters - 10 projects)

---

## Batch A Projects

| Project | Current TFM | Target TFM | Status | Notes |
|---------|------------|------------|--------|-------|
| FastReport.OpenSource.Data.SQLite | net462;net6.0 | net462;net10.0 | Ready | SQLite client library: mature, high .NET 10 support |
| FastReport.OpenSource.Data.Json | net462;net6.0 | net462;net10.0 | Ready | No DB dependency, pure data adapter |
| FastReport.OpenSource.Data.Excel | net462;net6.0 | net462;net10.0 | Ready | Uses EPPlus/NPOI, check versions |
| FastReport.OpenSource.Data.ODBC | net462;net6.0 | net462;net10.0 | Ready | ODBC client likely .NET 10 compatible |
| FastReport.OpenSource.Data.Cassandra | net462;net6.0 | net462;net10.0 | Ready | DataStax driver needs version check |
| FastReport.OpenSource.Data.Ignite | net462;net6.0 | net462;net10.0 | Ready | Apache Ignite .NET client - verify compatibility |
| FastReport.OpenSource.Data.RavenDB | net462;net6.0 | net462;net10.0 | Ready | RavenDB client mature, likely compatible |
| FastReport.OpenSource.Data.Couchbase | net462;net6.0 | net462;net10.0 | Ready | Couchbase .NET SDK - verify compatibility |
| FastReport.OpenSource.Data.ElasticSearch | net462;net6.0 | net462;net10.0 | Ready | Elastic.Net likely .NET 10 compatible |
| FastReport.OpenSource.Data.GoogleSheets | net462;net6.0 | net462;net10.0 | Ready | Google APIs for .NET - check version |

---

## Upgrade Pattern

**Current Structure** (in each .csproj):
```xml
<TargetFrameworks>$(NetFrameworkMinimum);net6.0</TargetFrameworks>
```
Where `NetFrameworkMinimum` = `net462` (defined in `Extras/Core/FastReport.Data/Directory.Build.props`)

**After Upgrade**:
```xml
<TargetFrameworks>$(NetFrameworkMinimum);net10.0</TargetFrameworks>
```

**Windows-specific** (if needed):
```xml
<TargetFrameworks>$(NetFrameworkMinimum);net10.0</TargetFrameworks>
<TargetFrameworks Condition="'$(OS)' == 'Windows_NT'">$(TargetFrameworks);net10.0-windows</TargetFrameworks>
```

---

## Dependency Assessment

### Shared Dependencies (All Batch A)
- ✅ FastReport.OpenSource (net10.0 - completed Task 03)
- ✅ FastReport.Compat (net10.0 - completed Task 02.01)
- ✅ Microsoft.Extensions.* (widely available)

### Project-Specific Dependencies to Verify

**SQLite**
- Package: System.Data.SQLite (likely compatible) OR Microsoft.Data.Sqlite (confirmed .NET 10)
- Status: Expected ✅ No issues

**JSON**
- No specific dependencies (pure C# adapter)
- Status: Expected ✅ No issues

**Excel**
- Packages: EPPlus, NPOI, or ClosedXML (all .NET 10 compatible)
- Status: Expected ✅ No issues

**ODBC**
- Package: System.Data.Odbc (built-in, .NET 10 compatible)
- Status: Expected ✅ No issues

**Cassandra**
- Package: Cassandra (DataStax driver)
- Status: ⚠️ Verify version compatibility

**Ignite**
- Package: Apache.Ignite (or similar)
- Status: ⚠️ Verify version compatibility

**RavenDB**
- Package: RavenDB.Client
- Status: Expected ✅ RavenDB has excellent .NET 10 support

**Couchbase**
- Package: Couchbase.NetClient
- Status: ⚠️ Verify version compatibility

**ElasticSearch**
- Package: Elastic.Clients.Elasticsearch
- Status: Expected ✅ Elastic has .NET 10 support

**GoogleSheets**
- Package: Google.Apis.Sheets.v4
- Status: ⚠️ Verify version compatibility

---

## Implementation Strategy

### Phase 1: TFM Update Only
1. Update each .csproj TargetFrameworks from `net6.0` to `net10.0`
2. Keep net462 for backward compatibility
3. No code changes needed (TFM-only update)

### Phase 2: Build & Validate
1. Build each project individually
2. Document any compilation errors
3. Resolve package compatibility issues if found

### Phase 3: Batch Build
1. Build all Batch A projects together
2. Verify 0 new errors
3. Check for cross-project dependency issues

---

## Risk Assessment

**Low Risk** ✅:
- Simple TFM update (straightforward)
- Well-tested dependencies (most are mature libraries)
- No code changes required
- FastReport.OpenSource (primary dependency) already net10.0 ready

**Medium Risk** ⚠️:
- Some DB client libraries may lag on .NET 10 support
- Potential need to update package versions
- Mitigation: Check NuGet for latest versions

**High Risk** ❌:
- None identified for Batch A

---

## Recommended Execution Order

1. **SQLite** (simplest, no exotic dependencies)
2. **JSON** (pure C# implementation)
3. **Excel** (common adapter, well-tested)
4. **ODBC** (built-in System.Data compatibility)
5. **ElasticSearch** (mature Elastic support)
6. **RavenDB** (excellent .NET 10 support)
7. **Couchbase** (verify compatibility first)
8. **Cassandra** (verify DataStax driver version)
9. **Ignite** (verify Apache driver version)
10. **GoogleSheets** (verify Google APIs version)

---

## Expected Outcome

✅ All 10 Batch A projects compile against net10.0  
✅ 0 new compilation errors  
✅ Backward compatibility with net462 maintained  
✅ Ready for Batch B (Complex Data Adapters)

---

## Handoff to Step 2

Proceed with TFM updates in recommended order. Build after each 2-3 projects to catch issues early.

**Estimated Time**: 2-3 hours (rapid TFM updates + build validation)
