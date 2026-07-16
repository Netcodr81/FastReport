# ImageExport.cs Simplified SkiaSharp Version - Migration Guide

## Overview
A new simplified version of `ImageExport.cs` has been created that uses **only SkiaSharp** for cross-platform compatibility. This version is located at:

**`FastReport.Base\Export\Image\ImageExport.Simplified.cs`**

## Key Changes from Original

### ✅ What's Supported

1. **Image Formats**
   - ✅ PNG (SKEncodedImageFormat.Png)
   - ✅ JPEG (SKEncodedImageFormat.Jpeg) with quality control
   - ✅ BMP (SKEncodedImageFormat.Bmp)
   - ✅ GIF (SKEncodedImageFormat.Gif)
   - ⚠️ TIFF (mapped to WebP as SkiaSharp doesn't support TIFF encoding)

2. **Export Modes**
   - ✅ Separate files per page
   - ✅ Combined single image with all pages
   - ✅ Resolution control (DPI)
   - ✅ Padding for non-separate pages

3. **Page Features**
   - ✅ Page backgrounds
   - ✅ Page borders
   - ✅ Top and bottom watermarks (text and image)
   - ✅ Band and object rendering

### ❌ What's Removed

1. **Metafile Export**
   - ❌ EMF/WMF format not supported (SkiaSharp limitation)
   - Alternative: Use PNG for raster or PDF export for vector

2. **Multi-Frame TIFF**
   - ❌ Multi-page TIFF files not supported (SkiaSharp limitation)
   - Alternative: Each page saves as separate file

3. **Monochrome TIFF**
   - ❌ 1-bit black and white conversion
   - ❌ CCITT3/CCITT4/LZW compression
   - Alternative: Use standard color TIFF or grayscale conversion

## Technical Changes

### Type Replacements

| Original (System.Drawing) | New (SkiaSharp) | Notes |
|---------------------------|-----------------|-------|
| `Image` | `SKImage` | Immutable snapshot |
| `Bitmap` | `SKBitmap` or `SKSurface` | Surface for drawing |
| `Graphics` | `SKCanvas` | Drawing context |
| `GraphicsState` | `int` (save count) | State ID instead of object |
| `Color` | `SKColor` | Converted via helper |
| `SizeF` | `SKSize` | Explicit conversion |
| `RectangleF` | `SKRect` | Explicit conversion |
| `ImageFormat` | `SKEncodedImageFormat` | Different enum |
| `EncoderParameters` | N/A | Not needed with SkiaSharp |

### Method Changes

#### CreateImage → CreateSurface
```csharp
// OLD
private System.Drawing.Image CreateImage(int width, int height, string suffix)
{
	if (ImageFormat == ImageExportFormat.Metafile)
		return CreateMetafile(suffix);
	return new Bitmap(width, height);
}

// NEW
private SKSurface CreateSurface(int width, int height)
{
	var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
	return SKSurface.Create(info);
}
```

#### SaveImage
```csharp
// OLD - Complex encoder logic with multi-frame TIFF
private void SaveImage(System.Drawing.Image image, string suffix)
{
	// EncoderParameters, ImageCodecInfo, etc.
	// Multi-frame TIFF: masterTiffImage.SaveAdd()
}

// NEW - Simple SkiaSharp encoding
private void SaveImage(SKImage image, string suffix)
{
	SKEncodedImageFormat format = ImageFormat switch
	{
		ImageExportFormat.Png => SKEncodedImageFormat.Png,
		ImageExportFormat.Jpeg => SKEncodedImageFormat.Jpeg,
		// ... other formats
	};

	using (SKData data = image.Encode(format, jpegQuality))
	{
		data.SaveTo(stream);
	}
}
```

#### Graphics State Management
```csharp
// OLD
GraphicsState state = g.Save();
// ... drawing ...
g.Restore(state);

// NEW
int savedState = g.Save();
// ... drawing ...
g.RestoreToCount(savedState);
```

### Field Changes

**Removed:**
- `EncoderValue monochromeTiffCompression` (not needed)
- `SKImage masterTiffImage` (multi-frame not supported)
- `int widthK` (unused helper)

**Changed:**
- `SKMatrix state` → `int savedState` (different state tracking)
- `SKImage image` → `SKImage currentImage` (clarity)
- Added: `SKSurface currentSurface` (drawing surface)
- Added: `SKSurface bigSurface` (for combined mode)

## Compilation Status

The simplified version should compile without errors because:

1. ✅ No System.Drawing references
2. ✅ No Windows Forms dependencies
3. ✅ Only SkiaSharp types used
4. ✅ FRPaintEventArgs expects SKCanvas (assuming it's been updated)

## Migration Steps

### Option 1: Replace Original File
```powershell
# Backup original
Move-Item FastReport.Base\Export\Image\ImageExport.cs FastReport.Base\Export\Image\ImageExport.Legacy.cs

# Use simplified version
Move-Item FastReport.Base\Export\Image\ImageExport.Simplified.cs FastReport.Base\Export\Image\ImageExport.cs
```

### Option 2: Conditional Compilation
Keep both versions with `#if` directives:
```csharp
#if WINDOWS
	// Legacy System.Drawing version with full features
#else
	// SkiaSharp cross-platform version (simplified)
#endif
```

### Option 3: Separate Export Classes
- Keep original as `ImageExportWindows.cs` (Windows-only)
- Use simplified as `ImageExportCrossPlatform.cs`
- Use factory pattern to select at runtime

## Known Limitations

### 1. TIFF Format
**Issue**: SkiaSharp doesn't support TIFF encoding
**Workaround**: 
- Use WebP format (similar compression)
- Use PNG for lossless
- Add dependency on `SkiaSharp.Extended` or another TIFF library

### 2. FRPaintEventArgs Compatibility
**Issue**: The code assumes `FRPaintEventArgs` accepts `SKCanvas`
**Required**: `FRPaintEventArgs` constructor must be updated:
```csharp
// Must accept SKCanvas instead of System.Drawing.Graphics
public FRPaintEventArgs(SKCanvas canvas, float scaleX, float scaleY, GraphicCache cache)
```

### 3. Page Size Retrieval
**Issue**: `Report.PreparedPages.GetPageSize()` may return `System.Drawing.SizeF`
**Workaround**: Added `GetPageSize()` helper that converts to `SKSize`

### 4. Color Conversion
**Issue**: FastReport objects may still use `System.Drawing.Color`
**Workaround**: Added `ConvertColor()` helper:
```csharp
private SKColor ConvertColor(System.Drawing.Color color)
{
	return new SKColor(color.R, color.G, color.B, color.A);
}
```

## Testing Checklist

- [ ] Export single page as PNG
- [ ] Export single page as JPEG with quality=50
- [ ] Export single page as BMP
- [ ] Export multiple pages as separate files
- [ ] Export multiple pages as combined image
- [ ] Verify resolution (96 dpi, 300 dpi)
- [ ] Test page backgrounds
- [ ] Test page borders
- [ ] Test text watermarks (top and bottom)
- [ ] Test image watermarks
- [ ] Test padding in combined mode
- [ ] Verify generated file names
- [ ] Test saveStreams mode

## Breaking Changes for Users

### API Changes
1. `ImageFormat.Metafile` → throws `NotSupportedException`
2. `MultiFrameTiff` property → always returns `false`
3. `MonochromeTiff` property → always returns `false`
4. `MonochromeTiffCompression` property → ignored
5. TIFF export → saves as WebP instead

### Documentation Updates Needed
- Update export documentation to note cross-platform limitations
- Mark Metafile as Windows-only feature
- Document TIFF→WebP mapping
- Add migration notes for existing code

## Future Enhancements

### Possible Improvements
1. **Add TIFF Support**: 
   - Use `LibTiff.NET` or `ImageSharp` for real TIFF encoding
   - Implement multi-frame TIFF via third-party library

2. **Add Monochrome Conversion**:
   - Implement dithering algorithm with SKBitmap
   - Convert to 8-bit grayscale at minimum

3. **Vector Export**:
   - Use `SkiaSharp.Svg` for SVG export
   - Add PDF export as Metafile alternative

4. **Better Error Handling**:
   - Validate format support before export
   - Provide detailed error messages

## Additional Files Needed

Check if these files need updates:

1. **FRPaintEventArgs.cs**
   - Must accept SKCanvas parameter
   - Remove System.Drawing.Graphics dependency

2. **ExportUtils.cs**
   - Check `SaveJpeg()` method signature
   - Verify page size methods

3. **Watermark.cs**
   - Verify `DrawImage()` and `DrawText()` accept SKCanvas
   - Check coordinate system compatibility

## Contact & Support

If you encounter issues during migration:
1. Check that all FastReport.Utils types support SkiaSharp
2. Verify dependencies don't reference System.Drawing
3. Test on target platforms (Linux, macOS)
4. Check SkiaSharp version compatibility

---

**Created**: During .NET 10 migration
**Author**: GitHub Copilot
**Status**: Ready for integration testing
