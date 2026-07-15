# System.Drawing.Bitmap Equivalent in SkiaSharp

The closest equivalent to `System.Drawing.Bitmap` in SkiaSharp is
**`SKBitmap`**.

## Type Mapping

  System.Drawing   SkiaSharp                Notes
  ---------------- ------------------------ -----------------------------------
  `Bitmap`         `SKBitmap`               In-memory bitmap image
  `Graphics`       `SKCanvas`               Drawing surface
  `Image`          `SKImage`                Immutable image
  `Pen`            `SKPaint`                Used for drawing lines and shapes
  `Brush`          `SKPaint`                Configure `Style = Fill`
  `Color`          `SKColor`                Color structure
  `Point`          `SKPoint`                Floating-point coordinates
  `PointF`         `SKPoint`                Same
  `Rectangle`      `SKRectI`                Integer rectangle
  `RectangleF`     `SKRect`                 Floating-point rectangle
  `Size`           `SKSizeI`                Integer size
  `SizeF`          `SKSize`                 Floating-point size
  `Font`           `SKFont`                 Font information
  `ImageFormat`    `SKEncodedImageFormat`   PNG, JPEG, WebP, etc.

## Creating a Bitmap

### System.Drawing

``` csharp
using var bitmap = new Bitmap(800, 600);
using var graphics = Graphics.FromImage(bitmap);

graphics.Clear(Color.White);
graphics.DrawLine(Pens.Black, 0, 0, 100, 100);

bitmap.Save("test.png", ImageFormat.Png);
```

### SkiaSharp

``` csharp
using var bitmap = new SKBitmap(800, 600);
using var canvas = new SKCanvas(bitmap);

canvas.Clear(SKColors.White);

using var paint = new SKPaint
{
    Color = SKColors.Black,
    StrokeWidth = 2,
    Style = SKPaintStyle.Stroke
};

canvas.DrawLine(0, 0, 100, 100, paint);

using var image = SKImage.FromBitmap(bitmap);
using var data = image.Encode(SKEncodedImageFormat.Png, 100);

File.WriteAllBytes("test.png", data.ToArray());
```

## SKBitmap vs SKImage

### SKBitmap

-   Mutable
-   Pixel buffer you can modify.
-   Best for rendering and editing.
-   Closest replacement for `Bitmap`.

``` csharp
var bitmap = new SKBitmap(width, height);
```

### SKImage

-   Immutable.
-   Optimized for display and encoding.
-   Similar to a read-only `Image`.

``` csharp
using var image = SKImage.FromBitmap(bitmap);
```

Typical rendering pipeline:

``` text
SKBitmap
    ↓
SKCanvas
    ↓ draw
SKImage
    ↓
Encode()
```

## Accessing Pixels

### System.Drawing

``` csharp
Color color = bitmap.GetPixel(x, y);
bitmap.SetPixel(x, y, Color.Red);
```

### SkiaSharp

``` csharp
SKColor color = bitmap.GetPixel(x, y);
bitmap.SetPixel(x, y, SKColors.Red);
```

For high-performance pixel access:

``` csharp
IntPtr pixels = bitmap.GetPixels();
```

## Loading Images

### From a Stream

``` csharp
using var stream = File.OpenRead("image.png");
using var bitmap = SKBitmap.Decode(stream);
```

### From a Byte Array

``` csharp
byte[] bytes = File.ReadAllBytes("image.png");
using var bitmap = SKBitmap.Decode(bytes);
```

## Recommended Migration Strategy for .NET 10

When migrating from `System.Drawing` to a cross-platform .NET 10
application:

1.  Replace `Bitmap` with `SKBitmap`.
2.  Replace `Graphics` with `SKCanvas`.
3.  Replace drawing operations with the `SKCanvas` API.
4.  Replace image saving with `SKImage.Encode()`.
5.  Replace GDI+ fonts with `SKFont` and `SKTypeface`.
6.  Remove the dependency on `System.Drawing.Common` entirely.

This results in a fully cross-platform rendering pipeline that works on
Windows, Linux, macOS, .NET MAUI, Avalonia, Uno Platform, and server
environments.
