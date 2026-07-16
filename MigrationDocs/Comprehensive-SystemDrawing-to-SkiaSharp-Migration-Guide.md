# Comprehensive System.Drawing to SkiaSharp Migration Guide (.NET 10)

## Overview

`System.Drawing.Common` is Windows-only in modern .NET and should not be
used for new cross-platform applications. SkiaSharp is the recommended
low-level 2D graphics library for cross-platform desktop, server,
mobile, and web rendering.

------------------------------------------------------------------------

# Type Mapping

  System.Drawing   SkiaSharp              Notes
  ---------------- ---------------------- -----------------------
  Bitmap           SKBitmap               Mutable bitmap
  Image            SKImage                Immutable image
  Graphics         SKCanvas               Drawing surface
  GraphicsPath     SKPath                 Vector paths
  Pen              SKPaint                Stroke configuration
  SolidBrush       SKPaint                Fill configuration
  Brush            SKPaint                Fill paint
  Font             SKFont                 Font metrics
  FontFamily       SKTypeface             Font family
  Color            SKColor                RGBA color
  Point / PointF   SKPoint                Float coordinates
  Rectangle        SKRectI                Integer rectangle
  RectangleF       SKRect                 Float rectangle
  Size             SKSizeI                Integer size
  SizeF            SKSize                 Float size
  Matrix           SKMatrix               Transform matrix
  Region           SKRegion               Clipping region
  ImageFormat      SKEncodedImageFormat   PNG, JPEG, WebP, etc.

------------------------------------------------------------------------

# Rendering Pipeline

``` text
SKBitmap
    ↓
SKCanvas
    ↓ Draw
SKImage
    ↓ Encode
File/Stream
```

------------------------------------------------------------------------

# Creating Images

``` csharp
using var bitmap = new SKBitmap(800,600);
using var canvas = new SKCanvas(bitmap);

canvas.Clear(SKColors.White);
```

------------------------------------------------------------------------

# Drawing Lines

System.Drawing

``` csharp
graphics.DrawLine(Pens.Black,0,0,100,100);
```

SkiaSharp

``` csharp
using var paint = new SKPaint
{
    Color = SKColors.Black,
    Style = SKPaintStyle.Stroke,
    StrokeWidth = 2
};

canvas.DrawLine(0,0,100,100,paint);
```

------------------------------------------------------------------------

# Drawing Shapes

``` csharp
canvas.DrawRect(10,10,200,100,paint);
canvas.DrawCircle(100,100,50,paint);
canvas.DrawOval(new SKRect(20,20,120,80),paint);
```

------------------------------------------------------------------------

# Filling Shapes

``` csharp
paint.Style = SKPaintStyle.Fill;
paint.Color = SKColors.CornflowerBlue;

canvas.DrawRect(10,10,100,50,paint);
```

------------------------------------------------------------------------

# Text Rendering

``` csharp
using var typeface = SKTypeface.FromFamilyName("Arial");
using var font = new SKFont(typeface,18);

paint.Color = SKColors.Black;

canvas.DrawText("Hello",20,40,font,paint);
```

Unlike System.Drawing, fonts and paint are separate objects.

------------------------------------------------------------------------

# Images

Load

``` csharp
using var bitmap = SKBitmap.Decode("photo.png");
```

Stream

``` csharp
using var bitmap = SKBitmap.Decode(stream);
```

Save

``` csharp
using var image = SKImage.FromBitmap(bitmap);
using var data = image.Encode(SKEncodedImageFormat.Png,100);

File.WriteAllBytes("output.png",data.ToArray());
```

------------------------------------------------------------------------

# Pixel Access

``` csharp
var pixel = bitmap.GetPixel(x,y);
bitmap.SetPixel(x,y,SKColors.Red);
```

For high-performance editing:

``` csharp
IntPtr ptr = bitmap.GetPixels();
```

------------------------------------------------------------------------

# Transforms

``` csharp
canvas.Translate(50,50);
canvas.Scale(2);
canvas.RotateDegrees(45);
```

Save/Restore

``` csharp
canvas.Save();

// transform

canvas.Restore();
```

------------------------------------------------------------------------

# Clipping

``` csharp
canvas.ClipRect(new SKRect(0,0,200,200));
canvas.ClipPath(path);
```

------------------------------------------------------------------------

# Gradients

``` csharp
paint.Shader = SKShader.CreateLinearGradient(
    new SKPoint(0,0),
    new SKPoint(200,0),
    new[]{SKColors.Blue,SKColors.Red},
    null,
    SKShaderTileMode.Clamp);
```

------------------------------------------------------------------------

# Paths

``` csharp
using var path = new SKPath();

path.MoveTo(0,0);
path.LineTo(100,100);
path.Close();

canvas.DrawPath(path,paint);
```

------------------------------------------------------------------------

# Common Replacements

  System.Drawing       SkiaSharp
  -------------------- ----------------------
  Graphics.FromImage   new SKCanvas(bitmap)
  DrawImage            DrawImage
  DrawString           DrawText
  FillRectangle        DrawRect(Fill)
  DrawRectangle        DrawRect(Stroke)
  DrawEllipse          DrawOval
  FillEllipse          DrawOval(Fill)
  DrawPolygon          DrawPoints/DrawPath
  MeasureString        SKFont.MeasureText

------------------------------------------------------------------------

# Performance Tips

-   Reuse `SKPaint`, `SKFont`, and `SKTypeface`.
-   Dispose all SkiaSharp objects promptly.
-   Avoid repeated image encoding.
-   Prefer `GetPixels()` for large pixel operations.
-   Cache immutable `SKImage` objects when possible.

------------------------------------------------------------------------

# Migration Strategy

1.  Remove `System.Drawing.Common`.
2.  Replace drawing code with `SKCanvas`.
3.  Convert bitmap operations to `SKBitmap`.
4.  Replace `GraphicsPath` with `SKPath`.
5.  Convert fonts to `SKFont`/`SKTypeface`.
6.  Replace `Image.Save()` with `SKImage.Encode()`.
7.  Test rendering on Windows, macOS, and Linux.

------------------------------------------------------------------------

# Common Pitfalls

-   `SKImage` is immutable.
-   `SKBitmap` is mutable.
-   `SKPaint` replaces both Pen and Brush.
-   Text measurement differs from GDI+.
-   Font fallback behavior is platform dependent.
-   Always dispose unmanaged SkiaSharp resources.

------------------------------------------------------------------------

# FastReport Migration Notes

For a large System.Drawing-based rendering engine such as FastReport:

-   Create an abstraction over drawing operations (canvas, bitmap, font,
    image).
-   Implement a SkiaSharp backend first.
-   Replace rendering primitives incrementally.
-   Keep rendering logic independent of the graphics library.
-   Add visual regression tests comparing rendered pages.
-   Consider SVG and PDF output using SkiaSharp where appropriate.

This incremental approach minimizes risk while producing a fully
cross-platform rendering engine targeting .NET 10.
