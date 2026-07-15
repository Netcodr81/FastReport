# Compat Removal Design (Step 3)

## Goal
Define replacement modules and ownership for removing `FastReport.Compat` safely.

## Replacement Modules

### 1) Graphics Abstraction Module
- **Target location:** `FastReport.Base/Graphics` (expand existing graphics abstractions)
- **Replaces:** `FastReport.Compat/shared/DotNetClasses/IGraphics.cs`, `GdiGraphics.cs`
- **Owner:** Rendering/Export maintainers
- **Notes:**
  - Keep adapter seam in `FRPaintEventArgs` as transition point.
  - Migrate call sites to `IGraphics` implementations in `FastReport.Base`.
  - Keep optional native Graphics fallback probe only for transition.

### 2) UI Primitives Module (WinForms-like types)
- **Target location:** `FastReport.Base/Primitives` (new)
- **Replaces:** Compat-provided replacements for `AnchorStyles`, `DockStyle`, `Padding`, `PictureBoxSizeMode`, `Cursor` usage assumptions.
- **Owner:** Core model/layout maintainers
- **Notes:**
  - Introduce neutral enums/structs with explicit conversion helpers.
  - Update `ComponentBase`, `BandBase`, `ContainerObject`, `TextObjectBase`, `PictureObjectBase` first.

### 3) Compiler Abstraction Module
- **Target location:** `FastReport.Base/Code/Compilation` (new)
- **Replaces:** `FastReport.Code.CodeDom.Compiler` wrappers from Compat
- **Owner:** Scripting/compilation maintainers
- **Notes:**
  - Keep Roslyn-first APIs.
  - Provide thin compatibility layer for existing `MsAssemblyDescriptor` contracts.

### 4) Type Converter Module
- **Target location:** `FastReport.Base/TypeConverters` (new or consolidate under `Utils`)
- **Replaces:** Compat `TypeConverters` fallback implementations
- **Owner:** Serialization/designer maintainers
- **Notes:**
  - Move only required converters used by .NET 10 runtime paths.
  - Drop legacy-target-only branches.

## Migration Order
1. Graphics abstraction migration (least surface break due to existing adapter seam).
2. UI primitives migration in core layout/model objects.
3. Compiler abstraction migration for script pipeline.
4. Type converters migration.
5. Remove `FastReport.OpenSource -> FastReport.Compat` reference.
6. Remove `FastReport.Compat` project from solution and packaging references.

## Acceptance Criteria
- `FastReport.OpenSource.slnx` builds without `FastReport.Compat` reference.
- PdfSimple + OpenSource tests pass.
- No `FastReport.Code.CodeDom.Compiler` or `System.Windows.Forms` dependency remains in runtime projects.
- Packaging/build scripts no longer mention `FastReport.Compat`.