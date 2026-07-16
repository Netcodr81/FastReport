# ImageExport.cs - Side-by-Side Comparison

## Original vs Simplified Version Comparison

### File Headers

#### Original (System.Drawing)
```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using FastReport.Utils;
using SkiaSharp;  // Partially added
// Also needs: System.Drawing, System.Drawing.Imaging
```

#### Simplified (SkiaSharp Only)
```csharp
using FastReport.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
// No System.Drawing references
```

---

## Properties Comparison

### ImageFormat Property

#### Original
```csharp
public ImageExportFormat ImageFormat
{
	get { return imageFormat; }
	set { imageFormat = value; }
}
```

#### Simplified
```csharp
public ImageExportFormat ImageFormat
{
	get { return imageFormat; }
	set 
	{ 
		if (value == ImageExportFormat.Metafile)
			throw new NotSupportedException(
				"Metafile format is not supported in cross-platform builds.");
		imageFormat = value; 
	}
}
```
**Why**: Explicitly reject Metafile format with clear error message.

---

### MultiFrameTiff Property

#### Original
```csharp
public bool MultiFrameTiff
{
	get { return multiFrameTiff; }
	set { multiFrameTiff = value; }
}
```

#### Simplified
```csharp
[Obsolete("Multi-frame TIFF is not supported in cross-platform builds.")]
public bool MultiFrameTiff
{
	get { return false; }
	set { /* Ignored */ }
}
```
**Why**: SkiaSharp doesn't support multi-frame TIFF encoding.

---

### MonochromeTiff Property

#### Original
```csharp
public bool MonochromeTiff
{
	get { return monochromeTiff; }
	set { monochromeTiff = value; }
}

public EncoderValue MonochromeTiffCompression
{
	get { return monochromeTiffCompression; }
	set { monochromeTiffCompression = value; }
}
```

#### Simplified
```csharp
[Obsolete("Monochrome TIFF is not supported in cross-platform builds.")]
public bool MonochromeTiff
{
	get { return false; }
	set { /* Ignored */ }
}

// MonochromeTiffCompression property removed entirely
```
**Why**: Monochrome conversion requires System.Drawing bitmap manipulation.

---

## Field Declarations

### Original
```csharp
private EncoderValue monochromeTiffCompression;
private SKImage masterTiffImage;
private SKImage bigImage;
private SKCanvas bigGraphics;
private SKImage image;
private SKCanvas g;
private SKMatrix state;  // Problem: struct assigned null
private int widthK;
```

### Simplified
```csharp
// Removed: monochromeTiffCompression, masterTiffImage, widthK
private SKSurface bigSurface;      // NEW: Drawable surface
private SKImage bigImage;
private SKCanvas bigGraphics;
private SKSurface currentSurface;  // NEW: Per-page surface
private SKImage currentImage;      // Renamed from 'image'
private SKCanvas g;
private int savedState;            // Changed from SKMatrix
```

**Key Changes**:
- Added `SKSurface` for drawable surfaces
- Changed state tracking from `SKMatrix` to `int`
- Removed TIFF-specific fields
- Clearer naming (`currentImage` instead of `image`)

---

## Method Comparison

### CreateImage / CreateMetafile → CreateSurface

#### Original (Two Methods)
```csharp
private System.Drawing.Image CreateImage(int width, int height, string suffix)
{
	widthK = width;
	if (ImageFormat == ImageExportFormat.Metafile)
		return CreateMetafile(suffix);
	return new Bitmap(width, height);
}

private System.Drawing.Image CreateMetafile(string suffix)
{
	// 20+ lines of HDC manipulation
	using (Bitmap bmp = new Bitmap(1, 1))
	using (Graphics g = Graphics.FromImage(bmp))
	{
		IntPtr hdc = g.GetHdc();
		image = new Metafile(Stream, hdc);
		g.ReleaseHdc(hdc);
	}
	return image;
}
```

#### Simplified (One Method)
```csharp
private SKSurface CreateSurface(int width, int height)
{
	var info = new SKImageInfo(width, height, 
		SKColorType.Rgba8888, SKAlphaType.Premul);
	return SKSurface.Create(info);
}
```

**Reduction**: ~35 lines → 5 lines  
**Why**: SkiaSharp doesn't need HDC, no Metafile support

---

### ConvertToBitonal

#### Original
```csharp
private Bitmap ConvertToBitonal(Bitmap original)
{
	// 100 lines of pixel manipulation
	// - Convert to 32bpp ARGB if needed
	// - Lock bits
	// - Threshold conversion (RGB sum > 500 = white)
	// - Pack into 1-bit indexed
	// - Unlock bits
	return destination;
}
```

#### Simplified
```csharp
// REMOVED - Not supported
```

**Why**: 
- Requires detailed pixel manipulation
- SkiaSharp doesn't have 1-bit indexed format
- Rarely used feature
- Can be added later if needed using SKBitmap.GetPixels()

---

### SaveImage

#### Original (~100 lines)
```csharp
private void SaveImage(System.Drawing.Image image, string suffix)
{
	// Set bitmap resolution
	if (image is Bitmap bitmap)
	{
		bitmap.SetResolution(ResolutionX, ResolutionY);
	}

	// Multi-frame TIFF handling
	if (IsMultiFrameTiff)
	{
		ImageCodecInfo info = ExportUtils.GetCodec("image/tiff");
		EncoderParameters ep = new EncoderParameters(2);

		if (masterTiffImage == null)
		{
			// First frame
			ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, 
				(long)EncoderValue.MultiFrame);
			ep.Param[1] = new EncoderParameter(Encoder.Compression, 
				(long)EncoderValue.CompressionLZW);

			if (MonochromeTiff)
				image = ConvertToBitonal(image as Bitmap);

			masterTiffImage = image;
			masterTiffImage.Save(Stream, info, ep);
		}
		else
		{
			// Subsequent frames
			if (MonochromeTiff)
				image = ConvertToBitonal(image as Bitmap);

			ep.Param[1] = new EncoderParameter(Encoder.SaveFlag, 
				(long)EncoderValue.FrameDimensionPage);
			masterTiffImage.SaveAdd(image, ep);
		}
	}
	else
	{
		// Single image handling
		if (ImageFormat == ImageExportFormat.Jpeg)
		{
			ExportUtils.SaveJpeg(image, stream, JpegQuality);
		}
		else if (ImageFormat == ImageExportFormat.Tiff && MonochromeTiff)
		{
			// Monochrome TIFF with compression
			ImageCodecInfo info = ExportUtils.GetCodec("image/tiff");
			EncoderParameters ep = new EncoderParameters(1);
			ep.Param[0] = new EncoderParameter(Encoder.Compression, 
				(long)MonochromeTiffCompression);

			using (Bitmap bwImage = ConvertToBitonal(image as Bitmap))
			{
				bwImage.Save(stream, info, ep);
			}
		}
		else
		{
			// Standard formats
			ImageFormat format = System.Drawing.Imaging.ImageFormat.Bmp;
			// ... switch statement for format selection
			image.Save(stream, format);
		}
	}
}
```

#### Simplified (~30 lines)
```csharp
private void SaveImage(SKImage image, string suffix)
{
	if (image == null)
		return;

	// Determine file name and stream
	Stream stream;
	string targetFileName;

	if (saveStreams)
	{
		targetFileName = /* ... generate name ... */;
		stream = new MemoryStream();
	}
	else
	{
		string extension = Path.GetExtension(FileName);
		targetFileName = Path.ChangeExtension(FileName, suffix + extension);
		stream = suffix == "" ? Stream : new FileStream(targetFileName, FileMode.Create);
		if (suffix != "") GeneratedFiles.Add(targetFileName);
	}

	// Encode with SkiaSharp
	SKEncodedImageFormat format = ImageFormat switch
	{
		ImageExportFormat.Png => SKEncodedImageFormat.Png,
		ImageExportFormat.Jpeg => SKEncodedImageFormat.Jpeg,
		ImageExportFormat.Bmp => SKEncodedImageFormat.Bmp,
		ImageExportFormat.Gif => SKEncodedImageFormat.Gif,
		ImageExportFormat.Tiff => SKEncodedImageFormat.Webp, // Fallback
		_ => SKEncodedImageFormat.Png
	};

	using (SKData data = image.Encode(format, jpegQuality))
	{
		if (data != null)
			data.SaveTo(stream);
	}

	// Cleanup
	if (saveStreams)
		GeneratedUpdate(targetFileName, stream);
	else if (suffix != "")
		stream.Dispose();
}
```

**Key Simplifications**:
- No multi-frame TIFF → removed 40 lines
- No monochrome conversion → removed 30 lines
- No encoder parameters → removed 15 lines
- Simple SkiaSharp encoding → ~5 lines
- **Result**: 100 lines → 30 lines

---

### Start Method

#### Original
```csharp
protected override void Start()
{
	base.Start();
	// ... initialization ...

	if (!SeparateFiles && !IsMultiFrameTiff)
	{
		// Calculate dimensions
		foreach (int pageNo in Pages)
		{
			SizeF size = Report.PreparedPages.GetPageSize(pageNo);
			if (size.Width > w) w = size.Width;
			h += size.Height + padding;
		}

		// Create System.Drawing image
		bigImage = CreateImage((int)(w * ResolutionX / 96f), 
							   (int)(h * ResolutionY / 96f), "");
		bigGraphics = Graphics.FromImage(bigImage);
		bigGraphics.Clear(Color.Transparent);
	}
}
```

#### Simplified
```csharp
protected override void Start()
{
	base.Start();
	// ... initialization ...

	if (!SeparateFiles)
	{
		// Calculate dimensions
		foreach (int pageNo in Pages)
		{
			SKSize size = GetPageSize(pageNo);  // NEW helper
			if (size.Width > w) w = size.Width;
			h += size.Height + padding;
		}

		// Create SkiaSharp surface
		bigSurface = CreateSurface((int)(w * ResolutionX / 96f), 
								   (int)(h * ResolutionY / 96f));
		bigGraphics = bigSurface.Canvas;
		bigGraphics.Clear(SKColors.Transparent);
	}
}
```

**Changes**:
- `SizeF` → `SKSize` via helper method
- `CreateImage()` → `CreateSurface()`
- `Graphics.FromImage()` → `surface.Canvas`
- `Color.Transparent` → `SKColors.Transparent`

---

### ExportPageBegin Method

#### Original
```csharp
protected override void ExportPageBegin(ReportPage page)
{
	// ... setup width, height, zoom ...

	if (SeparateFiles || IsMultiFrameTiff)
	{
		image = CreateImage(width, height, fileSuffix);
		if (IsMultiFrameTiff && masterTiffImage == null)
			masterTiffImage = image;
	}

	if (image != null)
		g = Graphics.FromImage(image);
	else
		g = bigGraphics;

	state = g.Save();  // Returns GraphicsState object

	g.Clear(Color.Transparent);

	// Draw background
	g.FillRectangle(pageBack, 0, 0, width, height);

	// Transform
	if (!SeparateFiles)
		g.TranslateTransform(padding, curOriginY + padding);
	g.ScaleTransform(zoomX, zoomY);

	// Watermarks and borders...
}
```

#### Simplified
```csharp
protected override void ExportPageBegin(ReportPage page)
{
	// ... setup width, height, zoom ...

	if (SeparateFiles)
	{
		currentSurface = CreateSurface(width, height);
		g = currentSurface.Canvas;
	}
	else
	{
		g = bigGraphics;
	}

	savedState = g.Save();  // Returns int save count

	g.Clear(SKColors.Transparent);

	// Draw background
	using (var paint = new SKPaint())
	{
		paint.Color = ConvertColor(solidFill.Color);
		paint.Style = SKPaintStyle.Fill;
		g.DrawRect(0, 0, width, height, paint);
	}

	// Transform
	if (!SeparateFiles)
		g.Translate(padding, curOriginY + padding);
	g.Scale(zoomX, zoomY);

	// Watermarks and borders...
}
```

**Changes**:
- No multi-frame TIFF check
- `Graphics.FromImage()` → `surface.Canvas`
- `g.Save()` returns `int` instead of `GraphicsState`
- `g.Clear(Color)` → `g.Clear(SKColor)`
- `g.FillRectangle()` → `g.DrawRect()` with `SKPaint`
- `g.TranslateTransform()` → `g.Translate()`
- `g.ScaleTransform()` → `g.Scale()`

---

### ExportPageEnd Method

#### Original
```csharp
protected override void ExportPageEnd(ReportPage page)
{
	// ... watermarks ...

	if (state != null)  // Check for null
		g.Restore(state);

	if (image != null && image != bigImage)
	{
		g.Dispose();
		g = null;
	}

	if (SeparateFiles || IsMultiFrameTiff)
		SaveImage(image, fileSuffix);
	else
		curOriginY += height + padding * 2;

	if (image != masterTiffImage)
		image.Dispose();

	pageNumber++;
}
```

#### Simplified
```csharp
protected override void ExportPageEnd(ReportPage page)
{
	// ... watermarks ...

	g.RestoreToCount(savedState);  // No null check needed

	if (SeparateFiles)
	{
		currentImage = currentSurface.Snapshot();
		SaveImage(currentImage, fileSuffix);

		currentImage?.Dispose();
		currentImage = null;
		g = null;
		currentSurface?.Dispose();
		currentSurface = null;
	}
	else
	{
		curOriginY += height + padding * 2;
	}

	pageNumber++;
}
```

**Changes**:
- `g.Restore(state)` → `g.RestoreToCount(savedState)`
- No multi-frame TIFF handling
- Explicit surface cleanup for separate files
- Snapshot image before saving

---

### Finish Method

#### Original
```csharp
protected override void Finish()
{
	base.Finish();

	if (IsMultiFrameTiff)
	{
		// Flush multi-frame TIFF
		if (Stream != null)
			Stream.Flush();

		ImageCodecInfo info = ExportUtils.GetCodec("image/tiff");
		EncoderParameters ep = new EncoderParameters(1);
		ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, 
			(long)EncoderValue.Flush);
		masterTiffImage.SaveAdd(ep);
	}
	else if (!SeparateFiles && bigImage != null)
	{
		if (bigGraphics != null)
		{
			bigGraphics.Dispose();
			bigGraphics = null;
		}
		SaveImage(bigImage, "");
	}

	if (masterTiffImage != null)
		masterTiffImage.Dispose();
}
```

#### Simplified
```csharp
protected override void Finish()
{
	base.Finish();

	if (!SeparateFiles)
	{
		bigImage = bigSurface.Snapshot();
		SaveImage(bigImage, "");

		bigImage?.Dispose();
		bigGraphics = null;
		bigSurface?.Dispose();
	}
}
```

**Changes**:
- No multi-frame TIFF flush logic
- Simple snapshot and save for combined mode
- Cleaner disposal pattern

---

## Code Metrics

| Metric | Original | Simplified | Change |
|--------|----------|------------|--------|
| Total Lines | ~722 | ~450 | -38% |
| Methods | 16 | 13 | -3 |
| Properties | 11 | 8 | -3 |
| System.Drawing refs | ~150 | 0 | -100% |
| SkiaSharp refs | ~10 | ~50 | +400% |
| Error handling | Minimal | Better | ↑ |

---

## Benefits of Simplified Version

### ✅ Pros
1. **True cross-platform**: Works on Windows, Linux, macOS
2. **Simpler code**: 38% fewer lines
3. **Better maintainability**: One graphics stack (SkiaSharp)
4. **Modern API**: No legacy GDI+ code
5. **Memory management**: Clearer surface/image lifecycle
6. **Future-proof**: SkiaSharp actively maintained

### ⚠️ Cons
1. **Feature loss**: No Metafile, no multi-frame TIFF
2. **TIFF limited**: Maps to WebP
3. **Migration effort**: Need to update dependent code
4. **Testing needed**: Ensure rendering matches original

---

## Recommendations

### For New Projects
✅ **Use simplified version** 
- Cross-platform by design
- Cleaner codebase
- Fewer dependencies

### For Existing Projects
⚠️ **Conditional compilation**
```csharp
#if NETFRAMEWORK || WINDOWS
	// Use original with full features
#else
	// Use simplified for cross-platform
#endif
```

### For Migration
1. Start with simplified version
2. Test all export formats
3. Document feature changes
4. Update user documentation
5. Consider adding TIFF library if needed

---

**Document Version**: 1.0  
**Created**: .NET 10 Migration  
**Last Updated**: Migration phase
