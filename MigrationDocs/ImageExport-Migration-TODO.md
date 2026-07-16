# ImageExport.cs Migration to SkiaSharp - TODO

## Overview
The `FastReport.Base\Export\Image\ImageExport.cs` file requires extensive changes to migrate from System.Drawing to SkiaSharp. This file is approximately 722 lines and heavily uses System.Drawing APIs.

## Current Status
- **Total Errors**: 70+ compilation errors
- **Main Issue**: File was partially updated to use SKImage/SKCanvas but still has System.Drawing throughout

## Critical Type Replacements Needed

### 1. EncoderValue Enum
**Current**: `System.Drawing.Imaging.EncoderValue`
**Replacement**: Need to create custom enum or use SKEncodedImageFormat
```csharp
// Replace EncoderValue with custom enum:
public enum TiffCompressionMode
{
	None,
	LZW,
	Rle,
	CCITT3,
	CCITT4
}
```

### 2. Image Type
**Current**: Mixed use of `System.Drawing.Image` and `SKImage`
**Issue**: Fields declared as SKImage but methods return System.Drawing.Image
**Fix**: All image operations must use SKImage consistently

### 3. Graphics/Canvas
**Current**: `System.Drawing.Graphics` with methods like:
- `Graphics.FromImage()`
- `TranslateTransform()`
- `ScaleTransform()`
- `Save()` / `Restore()`
- `FillRegion()`

**Replacement**: `SKCanvas` with:
- Create canvas from SKSurface
- `SKCanvas.Translate()`
- `SKCanvas.Scale()`
- `SKCanvas.Save()` / `SKCanvas.Restore()` (returns int, not state object)
- `SKCanvas.DrawRect()` instead of FillRegion

### 4. Bitmap Operations
**Current**: System.Drawing.Bitmap with:
- `new Bitmap(width, height)`
- `Bitmap.SetResolution()`
- `Bitmap.LockBits()` / `UnlockBits()`

**Replacement**: SKBitmap with:
- `new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul)`
- Resolution handling through SKEncodedOriginExtensions
- `SKBitmap.GetPixels()` for direct pixel access

### 5. Metafile Support
**Current**: `System.Drawing.Imaging.Metafile`
**Issue**: SkiaSharp doesn't support EMF/WMF metafiles natively
**Options**:
- Remove Metafile export support
- Use SVG export instead
- Keep System.Drawing for Metafile format only

### 6. Image Format/Codec
**Current**:
- `System.Drawing.Imaging.ImageFormat`
- `ImageCodecInfo`
- `EncoderParameters`

**Replacement**:
- `SKEncodedImageFormat` (Bmp, Png, Jpeg, Gif, Webp)
- Direct encoding: `SKImage.Encode(SKEncodedImageFormat, quality)`

### 7. Geometry Types
**Current**: `System.Drawing.RectangleF`, `System.Drawing.SizeF`, `System.Drawing.Rectangle`
**Replacement**: `SKRect`, `SKSize`, `SKRectI`

### 8. State Management
**Current**: `GraphicsState state = g.Save();` then `g.Restore(state);`
**Replacement**: `int saveCount = canvas.Save();` then `canvas.RestoreToCount(saveCount);`

## Major Method Conversions Needed

### CreateImage()
```csharp
// OLD: Returns System.Drawing.Image
private System.Drawing.Image CreateImage(int width, int height, string suffix)

// NEW: Return SKImage or SKSurface
private SKSurface CreateSurface(int width, int height)
{
	var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
	return SKSurface.Create(info);
}
```

### ConvertToBitonal()
**Complete rewrite needed** using SKBitmap pixel manipulation:
```csharp
private SKBitmap ConvertToBitonal(SKBitmap original)
{
	// Use SKBitmap.GetPixels() for direct access
	// Convert to 1-bit indexed color
	// This is complex and may require custom dithering
}
```

### SaveImage()
Needs to use SkiaSharp encoding:
```csharp
private void SaveImage(SKImage image, string suffix)
{
	SKEncodedImageFormat format = imageFormat switch
	{
		ImageExportFormat.Png => SKEncodedImageFormat.Png,
		ImageExportFormat.Jpeg => SKEncodedImageFormat.Jpeg,
		ImageExportFormat.Bmp => SKEncodedImageFormat.Bmp,
		ImageExportFormat.Gif => SKEncodedImageFormat.Gif,
		_ => SKEncodedImageFormat.Png
	};

	using (SKData data = image.Encode(format, jpegQuality))
	{
		// Save to stream or file
	}
}
```

## Multi-Frame TIFF Support
**Major Issue**: SkiaSharp doesn't support multi-frame TIFF directly
**Options**:
1. Use third-party library (e.g., ImageSharp, LibTiff.NET)
2. Remove multi-frame TIFF support
3. Keep System.Drawing for TIFF only

## FRPaintEventArgs Issue
**Problem**: `FRPaintEventArgs` expects `IGraphics`, not `SKCanvas`
**Solution**: Need to create IGraphics wrapper around SKCanvas or update FRPaintEventArgs

## Recommended Migration Strategy

### Phase 1: Remove Metafile Support
- Document that Metafile export is not supported in cross-platform version
- Remove all Metafile-related code
- Update ImageFormat enum or handle gracefully

### Phase 2: Replace Core Types
- Replace System.Drawing.Image with SKImage
- Replace Graphics with SKCanvas
- Update all geometry types (RectangleF → SKRect)

### Phase 3: Rewrite Pixel Operations
- ConvertToBitonal using SKBitmap
- Handle resolution metadata

### Phase 4: Handle TIFF
- Decide on TIFF multi-frame strategy
- Implement alternative or remove feature

### Phase 5: Test Thoroughly
- Test all image formats
- Test resolution settings
- Test monochrome TIFF
- Test multi-page exports

## Files That May Need Updates
- `FastReport.Base\Utils\FRPaintEventArgs.cs` - May need to support SKCanvas
- `FastReport.Base\Export\ExportUtils.cs` - Image codec utilities
- `FastReport.Base\Utils\DrawUtils.cs` - Drawing utilities

## Estimated Complexity
**Very High** - This is one of the most complex files to migrate due to:
- Deep System.Drawing integration
- Pixel-level operations
- Multi-frame TIFF handling
- Metafile support
- Graphics state management

## Next Steps
1. Decide on Metafile and multi-frame TIFF support
2. Create IGraphics implementation for SKCanvas
3. Begin systematic replacement of types
4. Rewrite pixel manipulation code
5. Extensive testing of all image export scenarios
