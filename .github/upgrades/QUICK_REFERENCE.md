# .NET 10 Migration - Quick Reference Guide

**Use this guide during implementation for quick navigation and reminders.**

---

## Current Status
- **Overall Progress**: 25% (2/8 tasks complete)
- **Next Task**: 02.03 (MVC Demo) - Ready for implementation
- **Blocked Tasks**: None
- **Branch**: `upgrade-dotnet-10`

---

## Essential Commands

### Build Current Project
```powershell
dotnet build "Demos/OpenSource/MVC/FastReport.OpenSource.MVC.6.0/FastReport.OpenSource.MVC.6.0.csproj" -c Debug
```

### Build Entire Solution
```powershell
dotnet build "FastReport.OpenSource.slnx" -c Debug
```

### Run Tests
```powershell
dotnet test "FastReport.OpenSource.slnx"
```

### Clean and Rebuild
```powershell
dotnet clean "FastReport.OpenSource.slnx"
dotnet build "FastReport.OpenSource.slnx" -c Debug
```

### Check Project Framework
```powershell
dotnet --info
```

---

## File Structure Quick Map

```
FastReport Repository Root (C:\Repositories\FastReport\)
│
├── FastReport.OpenSource.slnx          ← Main solution file
│
├── .github/
│   ├── copilot-instructions.md         ← Modernization standards (REFERENCE)
│   └── upgrades/
│       ├── MIGRATION_STATUS_CHECKPOINT.md   ← Current status (THIS FILE's companion)
│       ├── IMPLEMENTATION_PLAN.md           ← Detailed plan for all tasks
│       └── scenarios/dotnet-version-upgrade/
│           ├── plan.md                      ← Overall upgrade strategy
│           ├── tasks.md                     ← Progress tracker
│           ├── assessment.md                ← Assessment findings
│           ├── scenario-instructions.md     ← Execution rules
│           │
│           └── tasks/
│               ├── 01-prerequisites/        ✅ COMPLETE
│               ├── 02.01-fastreport-compat/ ✅ COMPLETE
│               ├── 02.02-opensource-angular/✅ COMPLETE
│               ├── 02.03-opensource-mvc-demo/🔄 NEXT
│               ├── 03-core-engine/          ⏳ PENDING
│               ├── 04-data-web-export-adapters/ ⏳ PENDING
│               ├── 05-dependent-addons-and-test-assets/ ⏳ PENDING
│               └── 06-solution-validation-and-docs/ ⏳ PENDING
│
├── Demos/
│   ├── OpenSource/
│   │   ├── MVC/
│   │   │   └── FastReport.OpenSource.MVC.6.0/  ← Task 02.03 target
│   │   └── SPA/
│   │       └── FastReport.OpenSource.Angular/  ✅ Done (net10.0)
│
├── FastReport.OpenSource/                       ← Task 03 target (core engine)
│
└── Extras/
	├── Core/
	│   └── FastReport.Data/
	│       ├── FastReport.OpenSource.Data.MsSql/    ← Task 04
	│       ├── FastReport.OpenSource.Data.MySql/    ← Task 04
	│       └── ... (10 more data adapters)
	└── OpenSource/
		├── FastReport.OpenSource.Web/               ← Task 04
		└── FastReport.OpenSource.Export.PdfSimple/  ← Task 04
```

---

## Key Modernization Standards (from copilot-instructions.md)

**Language Features**:
- ✅ Nullable Reference Types (enabled)
- ✅ File-scoped namespaces
- ✅ Implicit usings
- ✅ Primary constructors (where appropriate)
- ✅ Modern C# patterns & expressions

**Architecture**:
- ✅ .NET 10 exclusively
- ✅ Dependency Injection (Microsoft.Extensions.*)
- ✅ Async-first APIs
- ✅ Microsoft.Extensions.Logging only
- ✅ System.Text.Json for serialization

**NO**:
- ❌ .NET Framework compatibility
- ❌ Newtonsoft.Json (use System.Text.Json)
- ❌ Custom logging abstractions
- ❌ Service Locator pattern
- ❌ Task.Result, Task.Wait(), Thread.Sleep()

---

## Task Implementation Template

For each task, follow this pattern:

### 1. Review
```
- Read task.md
- Understand scope and objectives
- Review assessment signals (if any)
```

### 2. Modify Project File
```
- Update <TargetFramework> to net10.0
- Update package versions to .NET 10 compatible
- Follow modernization standards
```

### 3. Build
```
- Run: dotnet build "project.csproj" -c Debug
- Address any errors
- Verify 0 warnings in touched scope
```

### 4. Test
```
- Run relevant tests (if applicable)
- Validate demo apps (if applicable)
```

### 5. Document
```
- Update progress-details.md
- Record changes summary
- Document any issues encountered
```

### 6. Commit & Update Tracker
```
- Git commit changes
- Update tasks.md with status
- Move to next task
```

---

## Common Issues & Solutions

### Issue: Build fails with "TargetFramework not recognized"
**Solution**: Ensure .NET 10 SDK is installed
```powershell
dotnet --info
# Look for .NET SDK 10.0.x
```

### Issue: NuGet package not found for .NET 10
**Solution**: Verify package version supports .NET 10
```
Search NuGet.org for package, check "Frameworks" tab
Update to minimum supported .NET 10 version
```

### Issue: API changed in .NET 10 (e.g., method no longer exists)
**Solution**: 
1. Search assessment.md for API signal
2. Use find_symbol to locate all usages
3. Update API usage to modern equivalent
4. Document breaking change

### Issue: Third-party SDK not compatible
**Solution**:
1. Check vendor's .NET 10 compatibility matrix
2. Update to latest compatible version
3. If no .NET 10 support exists, escalate and document

---

## Task Progress Quick Status

### Task 02.03: MVC Demo
- **File**: `Demos/OpenSource/MVC/FastReport.OpenSource.MVC.6.0/FastReport.OpenSource.MVC.6.0.csproj`
- **Current TFM**: net6.0
- **Target TFM**: net10.0
- **Changes Required**: 1 line (TFM only)
- **Estimated Time**: 15 minutes
- **Status**: 🔄 Ready to implement

### Task 03: Core Engine
- **File**: `FastReport.OpenSource/FastReport.OpenSource.csproj`
- **Complexity**: HIGH
- **Status**: ⏳ Blocked until 02.03 complete
- **Expected Duration**: 6-10 hours

### Tasks 04-06
- **Status**: ⏳ Blocked until Task 03 complete

---

## Assessment Findings (For Reference)

**For complete details, see**: `.github/upgrades/scenarios/dotnet-version-upgrade/assessment.md`

### Key Signals
- **Project.0002**: Mandatory TFM updates needed
- **NuGet.0002**: Package upgrade recommendations
- **Api.0003**: Potential behavioral/API changes (FastReport.OpenSource mainly)

### Affected Areas
- Core reporting engine (FastReport.OpenSource)
- Data provider APIs
- Web layer integration
- Export functionality

---

## Approval Gates

✋ **Current Gate**: Task 02.03 Implementation Approval

Before starting any task:
1. Confirm scope with user
2. Review this quick reference
3. Follow task implementation template
4. Document progress
5. Request approval for next task (if needed)

---

## How to Hand Off to Next Context

Create a new checkpoint by:

1. Updating `MIGRATION_STATUS_CHECKPOINT.md`
   - Update progress percentage
   - Record completed tasks
   - Note any blockers

2. Updating `.github/upgrades/scenarios/dotnet-version-upgrade/tasks.md`
   - Change task status symbols
   - Update progress indicator

3. Creating `progress-details.md` for completed task
   - List files modified
   - Record build results
   - Document issues encountered

4. Committing these changes to `upgrade-dotnet-10` branch

**Example Handoff Note**:
```
## Next Context Window

Continue with Task 03 (Core Engine Upgrade)
- File: FastReport.OpenSource/FastReport.OpenSource.csproj
- Current TFM: net6.0
- Target TFM: net10.0
- Expected complexity: HIGH

Refer to:
1. .github/upgrades/MIGRATION_STATUS_CHECKPOINT.md (current status)
2. .github/upgrades/IMPLEMENTATION_PLAN.md (task details)
3. .github/upgrades/scenarios/dotnet-version-upgrade/assessment.md (API issues)

Start by running build to identify compilation errors.
```

---

## Support & References

**Official Docs**:
- [.NET 10 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [Breaking Changes in .NET 10](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [ASP.NET Core 10 Migration Guide](https://learn.microsoft.com/en-us/aspnet/core/migration/10.0-to-11.0)

**Project Standards**:
- `.github/copilot-instructions.md` - Modernization principles
- `.github/upgrades/scenarios/dotnet-version-upgrade/scenario-instructions.md` - Execution rules
- `.github/upgrades/scenarios/dotnet-version-upgrade/assessment.md` - Detailed findings

**Useful Commands**:
```powershell
# Find all usages of a symbol
grep -r "SymbolName" . --include="*.cs"

# Check project references
dotnet list "project.csproj" reference

# Validate solution integrity
dotnet sln "FastReport.OpenSource.slnx" list
```

---

**Generated**: $(date)  
**Status**: Ready for implementation  
**Next Step**: User approval to begin Task 02.03
