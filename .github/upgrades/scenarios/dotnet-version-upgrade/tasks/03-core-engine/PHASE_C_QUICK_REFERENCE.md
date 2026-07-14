# Phase C Quick Reference - System.Drawing → IGraphicsProvider Cheat Sheet

**Use this when refactoring rendering files in Phase C.**

---

## Import Changes

```csharp
// Add
using FastReport.Rendering;

// Keep (Rectangle, Point, etc. are still needed)
using System.Drawing;
```

---

## Method Signature Changes

### Before (System.Drawing.Graphics)
```csharp
public void Render(Graphics g)
public void Paint(Graphics graphics, Rectangle bounds)
private void DrawContent(Graphics g, RectangleF area)
```

### After (IGraphicsProvider)
```csharp
public void Render(IGraphicsProvider graphics)
public void Paint(IGraphicsProvider graphics, Rectangle bounds)
private void DrawContent(IGraphicsProvider graphics, RectangleF area)
```

---

## Common API Replacements

| System.Drawing | IGraphicsProvider | Notes |
|---|---|---|
| `g.Clear(color)` | `graphics.Clear(color)` | Same API |
| `g.FillRectangle(brush, rect)` | `graphics.FillRectangle(brush, x, y, w, h)` | Rect → params |
| `g.DrawRectangle(pen, rect)` | `graphics.DrawRectangle(pen, x, y, w, h)` | Rect → params |
| `g.DrawString(text, font, brush, x, y)` | `graphics.DrawText(text, font, brush, x, y)` | DrawString → DrawText |
| `g.MeasureString(text, font)` | `graphics.MeasureText(text, font)` | Returns same SizeF |
| `g.DrawImage(img, x, y)` | `graphics.DrawImage(img, x, y)` | Same API |
| `g.TranslateTransform(x, y)` | `graphics.TranslateTransform(x, y)` | Same API |
| `g.RotateTransform(angle)` | `graphics.RotateTransform(angle)` | Same API |
| `g.DrawLine(pen, x1, y1, x2, y2)` | `graphics.DrawLine(pen, x1, y1, x2, y2)` | Same API |
| `g.DrawEllipse(pen, rect)` | `graphics.DrawEllipse(pen, x, y, w, h)` | Rect → params |
| `g.FillEllipse(brush, rect)` | `graphics.FillEllipse(brush, x, y, w, h)` | Rect → params |
| `new Font(name, size)` | `new SkiaSharpFont(name, size)` | Same constructor |
| `new Pen(color, width)` | `new SkiaSharpPen(color, width)` | Same constructor |
| `new SolidBrush(color)` | `new SkiaSharpBrush(color)` | Same constructor |
| `new GraphicsPath()` | `graphics.CreatePath()` | Factory method |

---

## Rectangle Handling

Rectangle in System.Drawing is a single type. IGraphicsProvider expects separate x, y, width, height:

```csharp
// Before
var rect = new Rectangle(10, 20, 100, 50);
g.FillRectangle(brush, rect);

// After
var rect = new Rectangle(10, 20, 100, 50);
graphics.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);

// Or decomposed
graphics.FillRectangle(brush, 10, 20, 100, 50);
```

---

## RectangleF (Float) Handling

Same pattern as Rectangle, but with float values:

```csharp
// Before
var rect = new RectangleF(10.5f, 20.5f, 100.5f, 50.5f);
g.DrawRectangle(pen, rect);

// After
graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
```

---

## Brush/Pen/Font Handling

Old System.Drawing way (resources):
```csharp
// Before
var brush = new SolidBrush(Color.Red);
g.FillRectangle(brush, rect);
brush.Dispose(); // Manual disposal
```

New way (same pattern, different types):
```csharp
// After
using var brush = new SkiaSharpBrush(Color.Red);
graphics.FillRectangle(brush, x, y, w, h);
// Auto-disposed via using
```

---

## Color Handling

Good news: System.Drawing.Color works directly!

```csharp
// Before
var color = Color.Red;
var brush = new SolidBrush(color);

// After
var color = Color.Red;
var brush = new SkiaSharpBrush(color);
// IGraphicsProvider wrapper handles conversion
```

---

## Text Rendering

### Simple Text
```csharp
// Before
g.DrawString("Hello", font, brush, x, y);

// After
graphics.DrawText("Hello", font, brush, x, y);
```

### Text with Format (if needed)
```csharp
// Before
var format = new StringFormat { Alignment = StringAlignment.Center };
g.DrawString("Hello", font, brush, rect, format);

// After
using var format = new SkiaSharpStringFormat { Alignment = StringAlignment.Center };
graphics.DrawText("Hello", font, brush, rect.X, rect.Y, rect.Width, rect.Height, format);
```

---

## Path Operations

### Before
```csharp
var path = new GraphicsPath();
path.AddArc(rect, 0, 360);
path.AddLine(x1, y1, x2, y2);
g.DrawPath(pen, path);
g.FillPath(brush, path);
path.Dispose();
```

### After
```csharp
using var path = graphics.CreatePath();
path.AddArc(rect.X, rect.Y, rect.Width, rect.Height, 0, 360);
path.MoveTo(x1, y1);
path.LineTo(x2, y2);
graphics.DrawPath(pen, path);
graphics.FillPath(brush, path);
// Auto-disposed via using
```

---

## Save/Restore Pattern

Good news: Same pattern!

```csharp
// Before
g.Save();
g.TranslateTransform(10, 10);
// ... drawing ...
g.Restore();

// After
graphics.Save();
graphics.TranslateTransform(10, 10);
// ... drawing ...
graphics.Restore();
```

---

## Things That Changed

### DrawString → DrawText
- Same purpose, clearer name
- `text, font, brush, x, y` → `text, font, brush, x, y`

### MeasureString → MeasureText
- Returns `SizeF` (same type)
- Signature: `MeasureText(text, font)` → `MeasureText(text, font)`

### GraphicsPath → IGraphicsPath
- No direct constructor
- Use: `graphics.CreatePath()`
- Same operations: MoveTo, LineTo, AddArc, etc.

### Dispose Pattern
- All new types support `using` pattern
- No change needed to calling code

---

## Gotchas to Watch For

### 1. Rectangle to Coordinates
The most common mistake:
```csharp
// ❌ Wrong
graphics.FillRectangle(brush, rect); // Won't compile

// ✅ Correct
graphics.FillRectangle(brush, rect.X, rect.Y, rect.Width, rect.Height);
```

### 2. Font/Pen/Brush Types
```csharp
// ❌ Wrong - using System.Drawing types
var font = new Font("Arial", 12); // System.Drawing.Font
graphics.DrawText(text, font, brush, x, y); // Compile error

// ✅ Correct - using FastReport.Rendering types
var font = new SkiaSharpFont("Arial", 12);
graphics.DrawText(text, font, brush, x, y);
```

### 3. Graphics Parameter Passing
When calling rendering methods, ensure you pass `IGraphicsProvider`, not `Graphics`:

```csharp
// ❌ Wrong - still using Graphics
void RenderBand(Graphics g) { ... }

// ✅ Correct - using IGraphicsProvider
void RenderBand(IGraphicsProvider graphics) { ... }
```

### 4. StringFormat Details
Not all StringFormat features map 1:1. Most common ones do:
- Alignment ✅
- LineAlignment ✅
- Some flags ⚠️ (may need manual handling)

---

## Refactoring Checklist for Each File

- [ ] Add `using FastReport.Rendering;`
- [ ] Find all `void Render(Graphics g)` methods
- [ ] Change to `void Render(IGraphicsProvider graphics)`
- [ ] Replace all `g.Clear(...)` → `graphics.Clear(...)`
- [ ] Replace all `g.FillRectangle(...)` → `graphics.FillRectangle(..., x, y, w, h)`
- [ ] Replace all `g.DrawRectangle(...)` → `graphics.DrawRectangle(..., x, y, w, h)`
- [ ] Replace all `g.DrawString(...)` → `graphics.DrawText(...)`
- [ ] Replace all `g.MeasureString(...)` → `graphics.MeasureText(...)`
- [ ] Replace all `new Font(...)` → `new SkiaSharpFont(...)`
- [ ] Replace all `new Pen(...)` → `new SkiaSharpPen(...)`
- [ ] Replace all `new SolidBrush(...)` → `new SkiaSharpBrush(...)`
- [ ] Replace all `new GraphicsPath()` → `graphics.CreatePath()`
- [ ] Build & verify no compile errors
- [ ] Visually verify rendering looks correct (if UI testing available)

---

## Performance Notes

✅ **SkiaSharp uses native Skia library (C++ underneath)**
- Generally performs better than System.Drawing
- GPU acceleration available (no code change needed)
- No performance regression expected

---

## Build After Each File

After updating each rendering file:
```powershell
dotnet build FastReport.OpenSource/FastReport.OpenSource.csproj -c Debug
```

Should see:
- ✅ 0 errors
- ⚠️ ~502 warnings (pre-existing - ignore)
- ✅ Compile time: ~9 seconds

---

## Need Help?

**Reference Documents**:
- `DRAWING_SKIA_MAPPING.md` - Detailed API mappings
- `IGraphicsProvider.cs` - Full interface definition
- `SkiaSharpGraphicsProvider.cs` - Implementation reference

**Common Pattern Examples**:
- `BandBase.cs` (first file to refactor - will have most examples)

---

**Status**: Ready for Phase C refactoring  
**Confidence**: High - systematic, well-understood patterns  
**Timeline**: 3-5 hours per batch × 5 batches = 15-25 hours total Phase C

Go systematically, build frequently, verify visually. You've got this! 💪
