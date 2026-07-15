# Step 6 - Compiler and TypeConverter Migration Off Compat

## Current Dependency Surface

### Compiler shims consumed by FastReport.Base
- `FastReport.Code.CodeDom.Compiler` (CodeDomProvider, CompilerParameters, CompilerResults, TempFileCollection, etc.)
- `FastReport.Code.CSharp.CSharpCodeProvider`
- `FastReport.Code.VisualBasic.VBCodeProvider`

Primary consumers:
- `FastReport.Base/Code/Ms/MsAssemblyDescriptor.cs`
- `FastReport.Base/Code/Ms/MsAssemblyDescriptor.Async.cs`
- `FastReport.Base/Utils/CompileHelper.cs`
- `FastReport.Base/Utils/CompilerSettings.cs`
- `FastReport.Base/Base.cs`

### TypeConverter shims consumed by FastReport.Base
- `FastReport.TypeConverters.FontConverter`
- `System.Drawing.ColorConverter` fallback from compat type converter layer

Primary consumers:
- `FastReport.Base/Utils/Converter.cs`
- `FastReport.Base/Utils/FontManager.Internals.cs`

## Migration Constraint Identified
A direct duplicate-copy migration into `FastReport` while `FastReport.Compat` is still referenced creates type duplication risk across shared namespaces and blocks incremental cutover.

## Safe Migration Strategy (Compiler/TypeConverters)
1. Introduce **new namespaces** in `FastReport.Base` for replacement implementations:
   - `FastReport.Code.Compilation.*` (compiler abstractions)
   - `FastReport.Base.TypeConverters.*` (converter abstractions)
2. Add **adapter wrappers** in `FastReport.Base` that map old contracts to new implementations.
3. Switch consumer files (`MsAssemblyDescriptor*`, `CompileHelper`, `CompilerSettings`, `Converter`) to new namespaces first.
4. Keep old compat namespaces available until all call sites move.
5. Remove compat compiler/type-converter source inclusion only after zero references remain.

## Required Follow-up for Full Step Completion
- Migrate `MsAssemblyDescriptor*` and `CompileHelper` to `FastReport.Code.Compilation.*`.
- Migrate `Converter.cs` and font matcher usage to `FastReport.Base.TypeConverters.*`.
- Add regression tests for script compilation and font conversion serialization.
