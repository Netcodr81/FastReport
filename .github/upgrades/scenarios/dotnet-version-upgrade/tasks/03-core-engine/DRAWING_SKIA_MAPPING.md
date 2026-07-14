# System.Drawing → SkiaSharp API Mapping Reference

**Purpose**: Systematic guide for migrating FastReport.OpenSource from System.Drawing to SkiaSharp  
**Scope**: Core graphics APIs used in FastReport rendering  
**Audience**: Implementation team for Phase C (systematic migration)

---

## Core Graphics Object Mapping

### Canvas/Context
```csharp
// System.Drawing
Graphics g = Graphics.FromImage(bitmap);
g.DrawString(...);
g.FillRectangle(...);
g.Dispose();

// SkiaSharp
using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
{
	var canvas = surface.Canvas;
	canvas.DrawText(...);
	canvas.DrawRect(...);
}
```

**Key Differences**:
- Graphics is imperative; SKCanvas is functional
- SkiaSharp uses `using` pattern (surface/canvas)
- No explicit `.Dispose()` needed (handle IDisposable pattern)

---

### Font Handling

#### System.Drawing Font
```csharp
var font = new Font("Arial", 12, FontStyle.Bold | FontStyle.Italic);
var size = g.MeasureString("Hello", font);
g.DrawString("Hello", font, brush, x, y);
```

#### SkiaSharp Equivalent
```csharp
var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold | SKFontStyle.Italic);
var paint = new SKPaint
{
	Typeface = typeface,
	TextSize = 12 * scaleFactor, // SkiaSharp uses absolute size
	TextEncoding = SKTextEncoding.Utf8
};

var metrics = paint.FontMetrics;
var width = paint.MeasureText("Hello");
canvas.DrawText("Hello", x, y, paint);
```

**Key Differences**:
- Font → Typeface + SKPaint combo
- FontStyle is same enum (can reuse)
- Text size is absolute (not relative like GDI+)
- Text baseline is explicit (must add metrics.Top offset)
- `MeasureString` → `MeasureText` (returns width only, not size)

**Mapping Table**:
| GDI+ | SkiaSharp |
|-----|----------|
| `new Font(family, size, style)` | `SKTypeface.FromFamilyName(family, (SKFontStyle)style)` + `SKPaint` |
| `font.Name` | `typeface.FamilyName` |
| `font.Size` | `paint.TextSize` |
| `font.Style` | `paint.Typeface.Style` |
| `g.MeasureString(text, font)` | `paint.MeasureText(text)` (width), then calculate height from metrics |

---

### Color & Brush Handling

#### System.Drawing Color/Brush
```csharp
var color = Color.FromArgb(255, 128, 0);
var brush = new SolidBrush(color);
var pen = new Pen(color, 2);
g.FillRectangle(brush, rect);
g.DrawRectangle(pen, rect);
```

#### SkiaSharp Equivalent
```csharp
var color = new SKColor(255, 128, 0, 255); // ARGB order (A, R, G, B)
var paint = new SKPaint
{
	Color = color,
	Style = SKPaintStyle.Fill
};
canvas.DrawRect(rect, paint);

var strokePaint = new SKPaint
{
	Color = color,
	Style = SKPaintStyle.Stroke,
	StrokeWidth = 2
};
canvas.DrawRect(rect, strokePaint);
```

**Key Differences**:
- No separate Brush class; use SKPaint with Style
- SKPaint is stateful, reusable object
- Fill vs Stroke is a property, not different classes
- Color order is ARGB (consistent with GDI+)

**Mapping Table**:
| GDI+ | SkiaSharp |
|-----|----------|
| `Color.FromArgb(a, r, g, b)` | `new SKColor(r, g, b, a)` or `SKColors.{Name}` |
| `new SolidBrush(color)` | `SKPaint { Color = color, Style = SKPaintStyle.Fill }` |
| `new Pen(color, width)` | `SKPaint { Color = color, StrokeWidth = width, Style = SKPaintStyle.Stroke }` |
| `g.FillRectangle(brush, rect)` | `canvas.DrawRect(rect, fillPaint)` |
| `g.DrawRectangle(pen, rect)` | `canvas.DrawRect(rect, strokePaint)` |

---

### Shapes & Drawing

#### System.Drawing Path
```csharp
var path = new GraphicsPath();
path.AddArc(rect, startAngle, sweepAngle);
path.AddLine(x1, y1, x2, y2);
g.DrawPath(pen, path);
g.FillPath(brush, path);
```

#### SkiaSharp Equivalent
```csharp
var path = new SKPath();
path.AddArc(rect, startAngle, sweepAngle);
path.LineTo(x2, y2); // MoveTo first if needed
canvas.DrawPath(path, strokePaint);
canvas.DrawPath(path, fillPaint);
```

**Key Differences**:
- GraphicsPath → SKPath (similar API, but additive)
- AddLine requires implicit MoveTo first
- DrawPath for both stroke and fill (use paint style)

**Mapping Table**:
| GDI+ | SkiaSharp |
|-----|----------|
| `new GraphicsPath()` | `new SKPath()` |
| `path.AddArc(...)` | `path.AddArc(...)` (same) |
| `path.AddLine(x1,y1, x2,y2)` | `path.MoveTo(x1,y1); path.LineTo(x2,y2)` |
| `path.AddRectangle(rect)` | `path.AddRect(rect)` |
| `g.DrawPath(pen, path)` | `canvas.DrawPath(path, strokePaint)` |
| `g.FillPath(brush, path)` | `canvas.DrawPath(path, fillPaint)` |

---

### Image Handling

#### System.Drawing Image/Bitmap
```csharp
var bitmap = new Bitmap(width, height);
var image = Image.FromFile("file.png");
g.DrawImageUnscaled(image, x, y);
g.DrawImage(image, destRect, sourceRect, GraphicsUnit.Pixel);
```

#### SkiaSharp Equivalent
```csharp
var bitmap = new SKBitmap(width, height);
var image = SKImage.FromEncodedData("file.png");
canvas.DrawImage(image, x, y);
canvas.DrawImage(image, destRect, sourceRect);
```

**Key Differences**:
- Image → SKImage (immutable) or SKBitmap (mutable)
- DrawImageUnscaled → DrawImage at position
- Similar rect-based drawing API

**Mapping Table**:
| GDI+ | SkiaSharp |
|-----|----------|
| `new Bitmap(w, h)` | `new SKBitmap(new SKImageInfo(w, h))` |
| `Image.FromFile(path)` | `SKImage.FromEncodedData(path)` |
| `g.DrawImageUnscaled(img, x, y)` | `canvas.DrawImage(img, x, y)` |
| `g.DrawImage(...)` with rects | `canvas.DrawImage(...)` (same rect API) |
| `bitmap.GetPixel(x, y)` | `bitmap.GetPixel(x, y)` (same API!) |

---

### Text Formatting

#### System.Drawing StringFormat
```csharp
var format = new StringFormat();
format.Alignment = StringAlignment.Center;
format.LineAlignment = StringAlignment.Middle;
g.DrawString(text, font, brush, rect, format);
```

#### SkiaSharp Equivalent
```csharp
var paint = new SKPaint { ... };
paint.TextAlign = SKTextAlign.Center; // Horizontal center
// Vertical center: manually offset by metrics.Height/2
var metrics = paint.FontMetrics;
var y = rect.MidY + (metrics.Bottom - metrics.Top) / 2;
canvas.DrawText(text, x, y, paint);
```

**Key Differences**:
- StringFormat properties spread across SKPaint properties
- TextAlign covers horizontal; vertical requires manual metric calculation
- No built-in StringFormat replacement (must handle each property)

**Mapping Table**:
| GDI+ | SkiaSharp |
|-----|----------|
| `StringAlignment.Center` | `SKTextAlign.Center` (paint.TextAlign) |
| `StringAlignment.Right` | `SKTextAlign.Right` |
| `StringTrimming.EllipsisCharacter` | `paint.TextScaleX = ...` (manual) |
| Line Alignment | Calculated via `paint.FontMetrics` |

---

### Transformations

#### System.Drawing Graphics Transforms
```csharp
g.TranslateTransform(x, y);
g.RotateTransform(angle);
g.ScaleTransform(sx, sy);
g.Clear(Color.White);
```

#### SkiaSharp Equivalent
```csharp
var matrix = SKMatrix.CreateTranslation(x, y);
matrix = SKMatrix.Concat(matrix, SKMatrix.CreateRotationDegrees(angle));
canvas.SetMatrix(matrix);
canvas.DrawColor(SKColors.White);
```

**Key Differences**:
- Transforms are matrices (more explicit)
- Must use `SetMatrix` instead of implicit transform stack
- Rotation angle in degrees (same as GDI+)
- `Clear` → `DrawColor`

---

## High-Frequency Replacements (Phase C Priority)

These are the most common replacements in FastReport rendering:

### #1: Graphics → SKCanvas
```csharp
// Before: void RenderBand(Graphics g, Rectangle bounds)
// After:  void RenderBand(SKCanvas canvas, SKRect bounds)
```

### #2: Font Initialization
```csharp
// Before: Font font = new Font("Arial", 12);
// After:  SKTypeface typeface = SKTypeface.FromFamilyName("Arial");
//         SKPaint paint = new SKPaint { Typeface = typeface, TextSize = 12 };
```

### #3: DrawString → DrawText
```csharp
// Before: g.DrawString("Hello", font, brush, x, y);
// After:  canvas.DrawText("Hello", x, y + paint.FontMetrics.Bottom, paint);
```

### #4: FillRectangle/DrawRectangle
```csharp
// Before: g.FillRectangle(brush, rect);
//         g.DrawRectangle(pen, rect);
// After:  canvas.DrawRect(rect, fillPaint);
//         canvas.DrawRect(rect, strokePaint);
```

### #5: MeasureString → MeasureText
```csharp
// Before: SizeF size = g.MeasureString(text, font);
// After:  float width = paint.MeasureText(text);
//         float height = paint.FontMetrics.Bottom - paint.FontMetrics.Top;
```

---

## Class Structure for IGraphicsProvider

Based on these mappings, IGraphicsProvider should abstract:

```csharp
public interface IGraphicsProvider
{
	// Canvas operations
	void Clear(IColor color);

	// Text
	void DrawText(string text, IFont font, IBrush brush, float x, float y, StringFormat format = null);
	SizeF MeasureText(string text, IFont font);

	// Shapes
	void DrawRectangle(IPen pen, float x, float y, float width, float height);
	void FillRectangle(IBrush brush, float x, float y, float width, float height);
	void DrawLine(IPen pen, float x1, float y1, float x2, float y2);
	void DrawEllipse(IPen pen, float x, float y, float width, float height);
	void FillEllipse(IBrush brush, float x, float y, float width, float height);

	// Images
	void DrawImage(IImage image, float x, float y);
	void DrawImage(IImage image, RectangleF destRect, RectangleF srcRect);

	// Paths
	IGraphicsPath CreatePath();
	void DrawPath(IPen pen, IGraphicsPath path);
	void FillPath(IBrush brush, IGraphicsPath path);

	// Transforms
	void TranslateTransform(float x, float y);
	void RotateTransform(float angle);
	void ScaleTransform(float sx, float sy);
}

// Supporting interfaces
public interface IFont { }
public interface IBrush { }
public interface IPen { }
public interface IColor { }
public interface IImage { }
public interface IGraphicsPath { }
```

---

## Migration Strategy by File Type

### Files with High System.Drawing Density
**Estimated Replacements per File**:
- `BandBase.cs`: 50-100 replacements
- `ComponentBase.cs`: 100-200 replacements
- `Fills.cs`: 100-150 replacements
- `Border.cs`: 30-50 replacements
- Export files (HTML, Image, PDF wrapper): 50-100 each

### Batching Strategy
**Batch 1**: Core rendering (BandBase, ComponentBase) - 200-300 replacements
**Batch 2**: Styling (Fills, Border, CapSettings) - 200-300 replacements
**Batch 3**: Shapes and specialized objects - 150-200 replacements
**Batch 4**: Export and utilities - 200-300 replacements
**Batch 5**: Barcode and specialized rendering - 100-150 replacements
**Batch 6**: Remaining and cleanup - 100-150 replacements

**Total Estimated**: ~1000-1500 replacements across ~50 files

---

## Validation & Testing Strategy

### Text Measurement Validation
Create test to verify text metrics:
```csharp
// Render same text in System.Drawing and SkiaSharp
// Compare dimensions (allow small DPI-based variance)
// Verify baseline handling
```

### Rendering Output Validation
```csharp
// Generate sample report with both backends
// Compare pixel output (use image diff library)
// Validate layout consistency
```

### Performance Validation
```csharp
// Benchmark System.Drawing rendering
// Benchmark SkiaSharp rendering
// Document performance delta
```

---

## Special Cases & Gotchas

### Text Baseline Handling
**GDI+**: Draws text from baseline (implicit)  
**SkiaSharp**: Position is explicit (can be top or baseline)

**Solution**: Always calculate offset:
```csharp
y = y + paint.FontMetrics.Top; // To draw from top
y = y + paint.FontMetrics.Descent; // To draw from baseline
```

### DPI Scaling
**GDI+**: Automatic DPI scaling
**SkiaSharp**: Manual DPI handling

**Solution**: Apply DPI factor to all measurements:
```csharp
float dpiScale = systemDpi / 96f; // Windows default 96 DPI
textSize = textSize * dpiScale;
```

### Angle Units
**Both use degrees** (good news - no change needed)

### Color Component Order
**GDI+**: ARGB (A, R, G, B)  
**SkiaSharp**: ARGB (R, G, B, A) in constructor, but `SKColor` handles both

**Solution**: Use SKColor properly:
```csharp
// GDI+ Color.FromArgb(alpha, red, green, blue)
// SkiaSharp SKColor(red, green, blue, alpha)
var color = new SKColor(r, g, b, a);
```

### StringFormat Edge Cases
StringFormat properties not directly supported in SkiaSharp:
- `FormatFlags.DirectionRightToLeft` → Use RTL text input
- `Trimming` → Manual string truncation
- `HotkeyPrefix` → Manual character marking

**Solution**: Handle in SkiaSharpGraphicsProvider wrapper layer

---

## References

- SkiaSharp Official Docs: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/
- System.Drawing Migration Guide: https://docs.microsoft.com/en-us/dotnet/core/compatibility/drawing
- SkiaSharp GitHub: https://github.com/mono/SkiaSharp

---

**Status**: Ready for Phase B implementation  
**Next**: Add SkiaSharp NuGet package and create project structure
