# ImageExport.cs Migration - Dependency Checklist

This checklist identifies all files that may need updates to support the simplified SkiaSharp-only ImageExport.

## Status Legend
- ✅ **Ready**: File already supports SkiaSharp
- ⚠️ **Needs Review**: File may need updates
- ❌ **Blocked**: File must be updated before ImageExport works
- ❓ **Unknown**: File status needs investigation

---

## Critical Dependencies (Must Fix)

### 1. FRPaintEventArgs.cs
**Status**: ❌ **BLOCKED**

**Location**: `FastReport.Base\Utils\FRPaintEventArgs.cs` (estimated)

**Issue**: The ImageExport code calls:
```csharp
var paintArgs = new FRPaintEventArgs(g, zoomX, zoomY, Report.GraphicCache);
c.Draw(paintArgs);
```

**Required Changes**:
```csharp
// OLD (System.Drawing)
public class FRPaintEventArgs
{
	public System.Drawing.Graphics Graphics { get; }

	public FRPaintEventArgs(Graphics graphics, float scaleX, float scaleY, ...)
	{
		Graphics = graphics;
		// ...
	}
}

// NEW (SkiaSharp) - Must support both or only SKCanvas
public class FRPaintEventArgs
{
	public SKCanvas Canvas { get; }  // or Graphics property returns IGraphics wrapper

	public FRPaintEventArgs(SKCanvas canvas, float scaleX, float scaleY, ...)
	{
		Canvas = canvas;
		// ...
	}
}
```

**Action Items**:
- [ ] Find FRPaintEventArgs.cs file
- [ ] Check constructor signature
- [ ] Update to accept SKCanvas
- [ ] Update all Draw() method implementations
- [ ] Test with ImageExport

---

### 2. ReportComponentBase.Draw()
**Status**: ❌ **BLOCKED**

**Location**: `FastReport.Base\ReportComponentBase.cs`

**Issue**: All report components must implement:
```csharp
public virtual void Draw(FRPaintEventArgs e)
```

**Required**: Draw() must work with SKCanvas instead of System.Drawing.Graphics

**Action Items**:
- [ ] Verify Draw() method accepts updated FRPaintEventArgs
- [ ] Check if components use Graphics-specific APIs
- [ ] Update painting code to use SKCanvas
- [ ] Test common components: TextObject, PictureObject, ShapeObject

---

### 3. ExportUtils.cs
**Status**: ⚠️ **NEEDS REVIEW**

**Location**: `FastReport.Base\Export\ExportUtils.cs`

**Used By ImageExport**:
```csharp
float width = ExportUtils.GetPageWidth(page) * Units.Millimeters * zoomX;
float height = ExportUtils.GetPageHeight(page) * Units.Millimeters * zoomY;
```

**Potential Issues**:
- `GetPageWidth/Height()` might return System.Drawing types
- `SaveJpeg()` method (if it exists) needs SKImage parameter

**Action Items**:
- [ ] Check GetPageWidth() return type
- [ ] Check GetPageHeight() return type
- [ ] Verify SaveJpeg() signature (if used)
- [ ] Update to return/accept SkiaSharp types if needed

---

### 4. Report.PreparedPages
**Status**: ⚠️ **NEEDS REVIEW**

**Location**: `FastReport.Base\Report.cs` or separate PreparedPages class

**Used By ImageExport**:
```csharp
// In GetPageSize() helper
ReportPage page = Report.PreparedPages.GetPage(pageNo);
```

**Potential Issues**:
- GetPage() might return null or incompatible type
- Page dimensions might be in System.Drawing.SizeF

**Action Items**:
- [ ] Verify GetPage() method exists and signature
- [ ] Check ReportPage dimension properties
- [ ] Ensure page size retrieval works with SkiaSharp

---

## Secondary Dependencies (Should Review)

### 5. Watermark.DrawImage() and DrawText()
**Status**: ⚠️ **NEEDS REVIEW**

**Location**: `FastReport.Base\Watermark.cs` (estimated)

**Used By ImageExport**:
```csharp
page.Watermark.DrawImage(new FRPaintEventArgs(g, ...), rect, Report, false);
page.Watermark.DrawText(new FRPaintEventArgs(g, ...), rect, Report, false);
```

**Required**:
- Methods must accept FRPaintEventArgs with SKCanvas
- Drawing code must use SkiaSharp

**Action Items**:
- [ ] Check DrawImage() signature and implementation
- [ ] Check DrawText() signature and implementation
- [ ] Verify watermark rendering with SkiaSharp
- [ ] Test image watermarks
- [ ] Test text watermarks with various fonts

---

### 6. ReportPage Properties
**Status**: ⚠️ **NEEDS REVIEW**

**Location**: `FastReport.Base\ReportPage.cs`

**Used By ImageExport**:
```csharp
page.Fill                // Background fill
page.Border.Lines        // Border settings
page.Border.Color        // Border color
page.Border.Width        // Border width
page.Watermark           // Watermark object
```

**Potential Issues**:
- Fill might be System.Drawing.Brush or Color
- Border.Color might be System.Drawing.Color

**Action Items**:
- [ ] Check Fill property type
- [ ] Check Border.Color type (needs SKColor conversion)
- [ ] Verify Border.Lines enum
- [ ] Test page backgrounds render correctly
- [ ] Test page borders render correctly

---

### 7. SolidFill Class
**Status**: ⚠️ **NEEDS REVIEW**

**Location**: `FastReport.Base\SolidFill.cs` (estimated)

**Used By ImageExport**:
```csharp
if (page.Fill is FastReport.SolidFill solidFill)
{
	paint.Color = ConvertColor(solidFill.Color);
}
```

**Required**:
- SolidFill.Color property
- May return System.Drawing.Color (needs conversion)

**Action Items**:
- [ ] Verify SolidFill exists and Color property
- [ ] Check if Color is System.Drawing.Color or custom type
- [ ] Test solid fill rendering

---

### 8. BorderLines Enum
**Status**: ✅ **PROBABLY READY**

**Used By ImageExport**:
```csharp
if (page.Border.Lines != null && page.Border.Lines != BorderLines.None)
```

**Likely**: Simple enum, should work as-is

**Action Items**:
- [ ] Quick verification only

---

### 9. GraphicCache
**Status**: ❓ **UNKNOWN**

**Location**: `FastReport.Base\GraphicCache.cs` (estimated)

**Used By ImageExport**:
```csharp
new FRPaintEventArgs(g, zoomX, zoomY, Report.GraphicCache)
```

**Potential Issues**:
- Might cache System.Drawing objects
- Needs to support SkiaSharp objects

**Action Items**:
- [ ] Find GraphicCache class
- [ ] Check what it caches (fonts, brushes, pens?)
- [ ] Update to cache SKFont, SKPaint, etc.
- [ ] Verify cache invalidation works

---

## Supporting Classes

### 10. Units Class
**Status**: ✅ **PROBABLY READY**

**Used By ImageExport**:
```csharp
width = (int)(ExportUtils.GetPageWidth(page) * Units.Millimeters * zoomX);
```

**Likely**: Constants for unit conversion

**Action Items**:
- [ ] Quick verification that Units.Millimeters exists

---

### 11. Res (Resource) Class
**Status**: ✅ **PROBABLY READY**

**Used By ImageExport**:
```csharp
return Res.Get("FileFilters," + filter + "File");
```

**Likely**: String resource localization

**Action Items**:
- [ ] Verify resource strings exist for image formats

---

### 12. FRWriter (Serialization)
**Status**: ✅ **PROBABLY READY**

**Used By ImageExport**:
```csharp
public override void Serialize(FRWriter writer)
{
	writer.WriteValue("ImageFormat", ImageFormat);
	// ...
}
```

**Likely**: Generic serialization, should work as-is

**Action Items**:
- [ ] Test serialization/deserialization

---

## Object Rendering Pipeline

### 13. TextObject.Draw()
**Status**: ❌ **BLOCKED**

**Location**: `FastReport.Base\TextObject.cs`

**Critical**: Most common object type

**Action Items**:
- [ ] Update Draw() to use SKCanvas
- [ ] Verify font rendering
- [ ] Test text alignment
- [ ] Test text rotation
- [ ] Test rich text formatting

---

### 14. PictureObject.Draw()
**Status**: ❌ **BLOCKED**

**Location**: `FastReport.Base\PictureObject.cs`

**Critical**: Image rendering

**Action Items**:
- [ ] Update Draw() to use SKCanvas
- [ ] Convert image data to SKImage or SKBitmap
- [ ] Test image sizing modes
- [ ] Test transparency

---

### 15. ShapeObject.Draw()
**Status**: ❌ **BLOCKED**

**Location**: `FastReport.Base\ShapeObject.cs`

**Critical**: Shapes (rectangles, ellipses, lines)

**Action Items**:
- [ ] Update Draw() to use SKCanvas
- [ ] Test shape rendering
- [ ] Test fill and stroke

---

### 16. BandBase and TableObject
**Status**: ❌ **BLOCKED**

**Location**: Various

**Used By ImageExport**:
```csharp
foreach (Base c in band.ForEachAllConvNoDup())
{
	if (c is TableColumn || c is TableCell || c is TableRow)
		continue;
	ExportObj(c);
}
```

**Action Items**:
- [ ] Verify ForEachAllConvNoDup() method
- [ ] Test band rendering
- [ ] Test table rendering

---

## Testing Plan

### Phase 1: Core Infrastructure
1. ✅ Create simplified ImageExport.cs
2. ⬜ Update FRPaintEventArgs to accept SKCanvas
3. ⬜ Update ReportComponentBase.Draw()
4. ⬜ Run basic compilation test

### Phase 2: Basic Rendering
5. ⬜ Update TextObject.Draw()
6. ⬜ Update PictureObject.Draw()
7. ⬜ Update ShapeObject.Draw()
8. ⬜ Test simple report export

### Phase 3: Advanced Features
9. ⬜ Update Watermark rendering
10. ⬜ Update page backgrounds/borders
11. ⬜ Test complex reports

### Phase 4: Format Testing
12. ⬜ Test PNG export
13. ⬜ Test JPEG export with quality settings
14. ⬜ Test BMP export
15. ⬜ Test combined vs separate pages
16. ⬜ Test resolution settings (96, 300 dpi)

---

## Quick Start Commands

### Find Critical Files
```powershell
# Find FRPaintEventArgs
Get-ChildItem -Recurse -Filter "*PaintEventArgs*" | Select-Object FullName

# Find ExportUtils
Get-ChildItem -Recurse -Filter "ExportUtils.cs" | Select-Object FullName

# Find ReportComponentBase
Get-ChildItem -Recurse -Filter "*ComponentBase.cs" | Select-Object FullName

# Find Watermark
Get-ChildItem -Recurse -Filter "Watermark.cs" | Select-Object FullName

# Search for Graphics usage
Select-String -Path "FastReport.Base\*.cs" -Pattern "System.Drawing.Graphics" -Recurse
```

### Check Build Status
```powershell
# Build just the export project
dotnet build FastReport.Base\FastReport.Base.csproj

# Check errors in ImageExport
dotnet build FastReport.Base\FastReport.Base.csproj 2>&1 | Select-String "ImageExport"
```

---

## Risk Assessment

### High Risk (Will Break)
- ❌ FRPaintEventArgs (all rendering depends on this)
- ❌ ReportComponentBase.Draw() (base method)
- ❌ TextObject/PictureObject/ShapeObject rendering

### Medium Risk (May Break)
- ⚠️ Watermark rendering
- ⚠️ GraphicCache
- ⚠️ ExportUtils helpers

### Low Risk (Probably OK)
- ✅ Units, Res, FRWriter
- ✅ BorderLines enum
- ✅ Basic page properties

---

## Success Criteria

### Minimum Viable Product
- [ ] ImageExport compiles without errors
- [ ] Can export simple single-page report to PNG
- [ ] Text renders correctly
- [ ] Basic shapes render correctly

### Full Feature Parity
- [ ] All object types render correctly
- [ ] PNG, JPEG, BMP formats work
- [ ] Resolution settings work (96, 300 dpi)
- [ ] Separate and combined page modes work
- [ ] Watermarks render correctly
- [ ] Page backgrounds and borders work
- [ ] Complex reports export successfully

### Performance
- [ ] Export speed comparable to original
- [ ] Memory usage acceptable
- [ ] No memory leaks

---

## Next Steps

1. **Immediate**: 
   - [ ] Search for and identify FRPaintEventArgs.cs
   - [ ] Check current constructor signature
   - [ ] Assess effort to update to SKCanvas

2. **Short Term**:
   - [ ] Update FRPaintEventArgs
   - [ ] Replace ImageExport.cs with simplified version
   - [ ] First compilation attempt

3. **Medium Term**:
   - [ ] Update all Draw() implementations
   - [ ] Test basic exports
   - [ ] Fix rendering issues

4. **Long Term**:
   - [ ] Complete testing
   - [ ] Performance optimization
   - [ ] Documentation updates

---

**Created**: .NET 10 Migration  
**Status**: Ready for execution  
**Priority**: High (blocks cross-platform rendering)
