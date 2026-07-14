# Task 03 - Graphics Abstraction Strategy & Architectural Decision

**Status**: Proposal for review before implementation  
**Date**: 2024  
**Scope**: FastReport.OpenSource core engine .NET 10 migration  
**Critical Decision**: How to handle System.Drawing incompatibility with cross-platform requirements

---

## Problem Statement

### Current Situation
- **System.Drawing** is deeply embedded in FastReport.OpenSource (4,032 API uses across 50+ files)
- System.Drawing is **Windows-only in .NET 10** (not available on Linux/macOS)
- FastReport targets multiple frameworks: `net6.0;net462;net6.0-windows`
- Modernization instructions mandate: **"Avoid Windows-only APIs unless isolated behind interfaces"**
- Assessment indicates 3,419+ LOC requires modification

### Core Problem
FastReport.OpenSource mixes:
1. **Platform-agnostic** reporting logic (bands, data handling, structure)
2. **Windows-specific** rendering logic (System.Drawing graphics)

Upgrading to .NET 10 requires separating these concerns.

---

## Evaluation: Three Strategic Options

### Option A: Platform Detection with Conditional Compilation
**Approach**: Keep System.Drawing for Windows targets (net462, net6.0-windows), provide stub/no-op for cross-platform

**Pros**:
- ✅ Minimal immediate changes to existing code
- ✅ Uses existing CROSSPLATFORM compile constant
- ✅ Zero API breaking changes for Windows consumers

**Cons**:
- ❌ Reporting functionality disabled on Linux/macOS
- ❌ Violates "cross-platform" modernization goal
- ❌ Creates two maintenance paths
- ❌ Does not enable future Blazor/MAUI support
- ❌ Leaves technical debt

**Estimated Effort**: Low (2-4 hours)  
**Long-term Viability**: ⛔ Poor - locks out cross-platform strategy

---

### Option B: Custom Graphics Abstraction Layer
**Approach**: Create `IGraphicsProvider` interface with:
- Abstract graphics operations (Draw, FillRect, MeasureText, etc.)
- Windows implementation using System.Drawing
- Cross-platform implementations using SkiaSharp or System.Drawing.Common

**Pros**:
- ✅ Enables cross-platform rendering
- ✅ Maintains Windows performance (direct System.Drawing)
- ✅ Enables future Blazor/MAUI/desktop designer support
- ✅ Clean separation of concerns
- ✅ Testable (can mock graphics layer)
- ✅ Allows gradual migration to modern graphics APIs

**Cons**:
- ⚠️ Significant initial architecture work (10-20 hours)
- ⚠️ Requires systematic refactoring of rendering code
- ⚠️ API breaking changes (new IGraphicsProvider interface)
- ⚠️ Multiple implementations to maintain

**Estimated Effort**: Medium (10-20 hours)  
**Long-term Viability**: ✅ Excellent - enables full modernization roadmap

---

### Option C: Migrate to Modern Graphics Library (SkiaSharp or Microsoft.Maui.Graphics)
**Approach**: Replace System.Drawing entirely with:
- **SkiaSharp**: Cross-platform, mature, Google-backed
- **Microsoft.Maui.Graphics**: First-party, integrated with MAUI, design system support

**Pros**:
- ✅ Single cross-platform graphics implementation
- ✅ Better performance and features than System.Drawing
- ✅ Direct path to modern desktop (MAUI) and web (Blazor) UIs
- ✅ No conditional compilation needed
- ✅ Aligned with .NET 10 ecosystem direction
- ✅ Industry standard (SkiaSharp used in production widely)

**Cons**:
- ⚠️ Major code refactoring (20-30 hours for complete migration)
- ⚠️ Different API model (slightly different text measurement, coordinate system)
- ⚠️ Additional NuGet dependency
- ⚠️ Requires validation of rendering fidelity

**Estimated Effort**: High (20-30 hours)  
**Long-term Viability**: ✅ Excellent - future-proof architecture

---

## Recommendation: Option B (Custom Abstraction) First, Path to Option C

### Rationale per Modernization Instructions

1. **"Prefer modern graphics abstractions. Avoid direct GDI+ dependencies."**
   - Option B creates abstraction immediately ✅
   - Option C is the ultimate modern target ✅

2. **"Keep rendering engine UI independent. Rendering should not depend on UI frameworks."**
   - Option B isolates graphics layer clearly ✅
   - Option C follows same pattern with modern library ✅

3. **"Evaluate cross-platform support, performance, graphics, long-term investment."**
   - Option B: Platform detection + SkiaSharp (best of both worlds initially)
   - Option C: Eventual unified solution for maximum compatibility

4. **"Support ASP.NET Core, Blazor, MAUI, Console Applications"**
   - Option B: Enables path to all platforms
   - Option C: Necessary for Blazor Server/WebAssembly rendering

---

## Proposed Implementation Strategy

### Phase 1: Create Graphics Abstraction (this Task)
1. Define `IGraphicsProvider` interface with core operations:
   - `DrawString`, `MeasureString`
   - `DrawRectangle`, `FillRectangle`
   - `DrawImage`, `GetImageSize`
   - `DrawLine`, `DrawPath`
   - Font/Brush/Pen management

2. Create `WindowsGraphicsProvider` wrapper around System.Drawing
3. For cross-platform: use SkiaSharp or System.Drawing.Common (temporary)
4. Update core classes (BandBase, ComponentBase, rendering) to use `IGraphicsProvider`
5. Validate net10.0 targeting works for Windows builds

### Phase 2: Expand Cross-Platform (Future Task)
1. Implement full `SkiaSharpGraphicsProvider` (alternative implementation)
2. Add rendering validation tests
3. Enable full cross-platform targeting (linux-x64, osx-x64)

### Phase 3: Modernize to Option C (Long-term)
1. Once rendering abstraction is solid, consider full migration to Microsoft.Maui.Graphics
2. Align with MAUI desktop designer initiative (per modernization instructions)
3. Remove System.Drawing dependency entirely

---

## Implementation Scope for Task 03 (This Context)

**Goal**: Update to net10.0 with abstraction layer foundation

**Steps**:
1. ✅ Create `IGraphicsProvider` interface definition
2. ✅ Create `WindowsGraphicsProvider` implementation (thin wrapper over System.Drawing)
3. ✅ Identify and abstract key rendering entry points in BandBase, ComponentBase, etc.
4. ⏭️ Update TFM to net10.0
5. ⏭️ Refactor for IGraphicsProvider usage (systemic but scoped)
6. ⏭️ Build and validate Windows target
7. ⏭️ Document architecture for downstream projects

**NOT In Scope**:
- Full Linux/macOS testing (pending SkiaSharp implementation)
- Complete rendering fidelity validation (can use existing test reports)
- Blazor/MAUI integration (Phase 2)

---

## Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| Abstraction too fine-grained | Review API surface with rendering team; validate call patterns |
| Performance degradation | Profile WindowsGraphicsProvider against baseline; optimize marshaling |
| Rendering fidelity issues | Test against existing report files; validate output pixel-perfect |
| Downstream breakage | Major version bump; clear migration guide for consumers |

---

## Breaking Changes & Consumer Communication

This migration introduces **binary breaking changes**:
- Consumers cannot pass System.Drawing objects directly to FastReport
- Must depend on new rendering interface (if custom renderers exist)

**Mitigation**: 
- Major version bump (1.x → 2.x)
- Clear migration guide for consumers
- Deprecation notices in previous release
- Shim classes for common System.Drawing → IGraphicsProvider conversions

---

## Next Immediate Actions

### User Approval Required
Before proceeding with implementation:

1. ✅ **Confirm Strategy**: Is Option B (Abstraction Layer) acceptable as .NET 10 target?
2. ✅ **Confirm Scope**: Proceed with Phase 1 (foundation) in this task?
3. ✅ **Confirm Timeline**: This work will span this and 1-2 future context windows?

### Requested Decisions
- [ ] Approve Option B strategy (IGraphicsProvider abstraction)
- [ ] Confirm Phase 1 scope for this context
- [ ] Confirm next context window assignment if needed
- [ ] Approve breaking changes / major version bump communication

---

## References

**Modernization Instructions**: `.github/copilot-instructions.md`
- Graphics section emphasizes: "Prefer modern graphics abstractions. Avoid direct GDI+ dependencies."
- "Keep rendering engine UI independent. Rendering should not depend on UI frameworks."

**Assessment Details**: `.github/upgrades/scenarios/dotnet-version-upgrade/assessment.md`
- 4,032 source-incompatible APIs (mostly System.Drawing)
- 3,419 LOC estimated to modify

**.NET 10 Breaking Changes**: https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0
- System.Drawing not available on non-Windows platforms for .NET 10

---

## Files to Create/Modify

1. **New File**: `FastReport.Base/Graphics/IGraphicsProvider.cs` - Interface definition
2. **New File**: `FastReport.Base/Graphics/WindowsGraphicsProvider.cs` - System.Drawing wrapper
3. **Modified Files**: BandBase, ComponentBase, and rendering entry points (to inject IGraphicsProvider)
4. **Project File**: FastReport.OpenSource.csproj - Update TFM and add conditional graphics provider registration

**Estimated Lines of Code**:
- IGraphicsProvider interface: 100-150 lines
- WindowsGraphicsProvider: 200-300 lines
- Refactoring existing code: 1000-1500 lines (spreading across multiple files)
- **Total for Phase 1**: ~1500-2000 lines

---

**Status**: ⏸️ Awaiting approval to proceed with Phase 1 implementation
