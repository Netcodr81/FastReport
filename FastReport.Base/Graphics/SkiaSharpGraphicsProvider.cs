using System;
using System.Collections.Generic;
using System.Drawing;
using SkiaSharp;

namespace FastReport.Rendering
{
    /// <summary>
    /// Graphics provider implementation using SkiaSharp for cross-platform rendering.
    /// Wraps SkiaSharp's SKCanvas to provide a unified graphics interface.
    /// </summary>
    internal class SkiaSharpGraphicsProvider : IGraphicsProvider
    {
        private readonly SKCanvas _canvas;
        private readonly Stack<SKAutoCanvasRestore> _saveStack;
        private float _dpiScale;

        /// <summary>
        /// Initializes a new instance of the SkiaSharpGraphicsProvider.
        /// </summary>
        /// <param name="canvas">The SKCanvas to wrap.</param>
        /// <param name="dpiScale">The DPI scale factor (96 = 100%, 120 = 125%, etc.). Default is 1.0f.</param>
        public SkiaSharpGraphicsProvider(SKCanvas canvas, float dpiScale = 1.0f)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
            _dpiScale = dpiScale;
            _saveStack = new Stack<SKAutoCanvasRestore>();
        }

        public float DpiScale => _dpiScale;

        #region Canvas Operations

        public void Clear(IColor color)
        {
            if (color is SkiaSharpColor skiaColor)
            {
                _canvas.Clear(skiaColor.Value);
            }
            else
            {
                _canvas.Clear(new SKColor(color.R, color.G, color.B, color.A));
            }
        }

        public void Save()
        {
            _saveStack.Push(new SKAutoCanvasRestore(_canvas));
        }

        public void Restore()
        {
            if (_saveStack.Count > 0)
            {
                _saveStack.Pop().Dispose();
            }
        }

        #endregion

        #region Transformations

        public void TranslateTransform(float dx, float dy)
        {
            _canvas.Translate(dx * _dpiScale, dy * _dpiScale);
        }

        public void RotateTransform(float angle)
        {
            _canvas.RotateDegrees(angle);
        }

        public void RotateTransform(float angle, float centerX, float centerY)
        {
            _canvas.RotateDegrees(angle, centerX * _dpiScale, centerY * _dpiScale);
        }

        public void ScaleTransform(float scaleX, float scaleY)
        {
            _canvas.Scale(scaleX, scaleY);
        }

        public void ResetTransform()
        {
            _canvas.ResetMatrix();
        }

        #endregion

        #region Text Drawing

        public void DrawText(string text, IFont font, IBrush brush, float x, float y, IStringFormat format = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var skiaFont = font as SkiaSharpFont ?? throw new ArgumentException("Font must be a SkiaSharpFont instance.");
            var skiaBrush = brush as SkiaSharpBrush ?? throw new ArgumentException("Brush must be a SkiaSharpBrush instance.");
            var align = SKTextAlign.Left;

            if (format is SkiaSharpStringFormat skiaFormat)
            {
                align = skiaFormat.Alignment switch
                {
                    StringAlignment.Center => SKTextAlign.Center,
                    StringAlignment.Far => SKTextAlign.Right,
                    _ => SKTextAlign.Left
                };

                // Note: Vertical alignment requires manual offset calculation using font metrics.
            }

            using var skFont = new SKFont(skiaFont.Typeface, skiaFont.Size * _dpiScale);
            _canvas.DrawText(text, x * _dpiScale, y * _dpiScale, align, skFont, skiaBrush.Paint);
        }

        public void DrawText(string text, IFont font, IBrush brush, float x, float y, float width, float height, IStringFormat format = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            // For now, delegate to simple DrawText with format alignment handling
            // In production, would need full text wrapping implementation
            DrawText(text, font, brush, x, y, format);
        }

        public SizeF MeasureText(string text, IFont font)
        {
            if (string.IsNullOrEmpty(text))
                return SizeF.Empty;

            var skiaFont = font as SkiaSharpFont ?? throw new ArgumentException("Font must be a SkiaSharpFont instance.");

            using var skFont = new SKFont(skiaFont.Typeface, skiaFont.Size * _dpiScale);
            using var paint = new SKPaint();
            var width = skFont.MeasureText(text, paint);
            var metrics = skFont.Metrics;
            var height = metrics.Bottom - metrics.Top;

            return new SizeF(width / _dpiScale, height / _dpiScale);
        }

        #endregion

        #region Lines

        public void DrawLine(IPen pen, float x1, float y1, float x2, float y2)
        {
            var skiaPen = pen as SkiaSharpPen ?? throw new ArgumentException("Pen must be a SkiaSharpPen instance.");
            _canvas.DrawLine(x1 * _dpiScale, y1 * _dpiScale, x2 * _dpiScale, y2 * _dpiScale, skiaPen.Paint);
        }

        #endregion

        #region Rectangles

        public void DrawRectangle(IPen pen, float x, float y, float width, float height)
        {
            var skiaPen = pen as SkiaSharpPen ?? throw new ArgumentException("Pen must be a SkiaSharpPen instance.");
            var rect = new SKRect(x * _dpiScale, y * _dpiScale, (x + width) * _dpiScale, (y + height) * _dpiScale);
            _canvas.DrawRect(rect, skiaPen.Paint);
        }

        public void FillRectangle(IBrush brush, float x, float y, float width, float height)
        {
            var skiaBrush = brush as SkiaSharpBrush ?? throw new ArgumentException("Brush must be a SkiaSharpBrush instance.");
            var rect = new SKRect(x * _dpiScale, y * _dpiScale, (x + width) * _dpiScale, (y + height) * _dpiScale);
            _canvas.DrawRect(rect, skiaBrush.Paint);
        }

        public void DrawRoundedRectangle(IPen pen, float x, float y, float width, float height, float cornerRadius)
        {
            var skiaPen = pen as SkiaSharpPen ?? throw new ArgumentException("Pen must be a SkiaSharpPen instance.");
            var rect = new SKRect(x * _dpiScale, y * _dpiScale, (x + width) * _dpiScale, (y + height) * _dpiScale);
            _canvas.DrawRoundRect(rect, cornerRadius * _dpiScale, cornerRadius * _dpiScale, skiaPen.Paint);
        }

        public void FillRoundedRectangle(IBrush brush, float x, float y, float width, float height, float cornerRadius)
        {
            var skiaBrush = brush as SkiaSharpBrush ?? throw new ArgumentException("Brush must be a SkiaSharpBrush instance.");
            var rect = new SKRect(x * _dpiScale, y * _dpiScale, (x + width) * _dpiScale, (y + height) * _dpiScale);
            _canvas.DrawRoundRect(rect, cornerRadius * _dpiScale, cornerRadius * _dpiScale, skiaBrush.Paint);
        }

        #endregion

        #region Ellipses

        public void DrawEllipse(IPen pen, float x, float y, float width, float height)
        {
            var skiaPen = pen as SkiaSharpPen ?? throw new ArgumentException("Pen must be a SkiaSharpPen instance.");
            var rect = new SKRect(x * _dpiScale, y * _dpiScale, (x + width) * _dpiScale, (y + height) * _dpiScale);
            _canvas.DrawOval(rect, skiaPen.Paint);
        }

        public void FillEllipse(IBrush brush, float x, float y, float width, float height)
        {
            var skiaBrush = brush as SkiaSharpBrush ?? throw new ArgumentException("Brush must be a SkiaSharpBrush instance.");
            var rect = new SKRect(x * _dpiScale, y * _dpiScale, (x + width) * _dpiScale, (y + height) * _dpiScale);
            _canvas.DrawOval(rect, skiaBrush.Paint);
        }

        #endregion

        #region Arcs

        public void DrawArc(IPen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            var skiaPen = pen as SkiaSharpPen ?? throw new ArgumentException("Pen must be a SkiaSharpPen instance.");
            var rect = new SKRect(x * _dpiScale, y * _dpiScale, (x + width) * _dpiScale, (y + height) * _dpiScale);
            _canvas.DrawArc(rect, startAngle, sweepAngle, false, skiaPen.Paint);
        }

        #endregion

        #region Paths

        public IGraphicsPath CreatePath()
        {
            return new SkiaSharpPath();
        }

        public void DrawPath(IPen pen, IGraphicsPath path)
        {
            var skiaPen = pen as SkiaSharpPen ?? throw new ArgumentException("Pen must be a SkiaSharpPen instance.");
            var skiaPath = path as SkiaSharpPath ?? throw new ArgumentException("Path must be a SkiaSharpPath instance.");
            _canvas.DrawPath(skiaPath.Path, skiaPen.Paint);
        }

        public void FillPath(IBrush brush, IGraphicsPath path)
        {
            var skiaBrush = brush as SkiaSharpBrush ?? throw new ArgumentException("Brush must be a SkiaSharpBrush instance.");
            var skiaPath = path as SkiaSharpPath ?? throw new ArgumentException("Path must be a SkiaSharpPath instance.");
            _canvas.DrawPath(skiaPath.Path, skiaBrush.Paint);
        }

        #endregion

        #region Images

        public void DrawImage(IImage image, float x, float y)
        {
            var skiaImage = image as SkiaSharpImage ?? throw new ArgumentException("Image must be a SkiaSharpImage instance.");
            using var paint = new SKPaint();
            _canvas.DrawImage(skiaImage.Image, x * _dpiScale, y * _dpiScale, SKSamplingOptions.Default, paint);
        }

        public void DrawImage(IImage image, float x, float y, float width, float height)
        {
            var skiaImage = image as SkiaSharpImage ?? throw new ArgumentException("Image must be a SkiaSharpImage instance.");
            var destRect = new SKRect(x * _dpiScale, y * _dpiScale, (x + width) * _dpiScale, (y + height) * _dpiScale);
            using var paint = new SKPaint();
            _canvas.DrawImage(skiaImage.Image, destRect, SKSamplingOptions.Default, paint);
        }

        public void DrawImage(IImage image, float destX, float destY, float destWidth, float destHeight,
                              float srcX, float srcY, float srcWidth, float srcHeight)
        {
            var skiaImage = image as SkiaSharpImage ?? throw new ArgumentException("Image must be a SkiaSharpImage instance.");
            var srcRect = new SKRect(srcX, srcY, srcX + srcWidth, srcY + srcHeight);
            var destRect = new SKRect(destX * _dpiScale, destY * _dpiScale, (destX + destWidth) * _dpiScale, (destY + destHeight) * _dpiScale);
            using var paint = new SKPaint();
            _canvas.DrawImage(skiaImage.Image, srcRect, destRect, SKSamplingOptions.Default, paint);
        }

        #endregion

        #region Clipping

        public void SetClipRectangle(float x, float y, float width, float height)
        {
            var rect = new SKRect(x * _dpiScale, y * _dpiScale, (x + width) * _dpiScale, (y + height) * _dpiScale);
            _canvas.ClipRect(rect, SKClipOperation.Intersect, true);
        }

        public void ResetClip()
        {
            _canvas.Save();
            _canvas.RestoreToCount(0);
        }

        #endregion

        public void Dispose()
        {
            // Canvas disposal is handled by caller (surface owner)
        }
    }

    #region Supporting Classes

    /// <summary>
    /// SkiaSharp implementation of IFont.
    /// </summary>
    internal class SkiaSharpFont : IFont
    {
        private SKTypeface _typeface;

        public string Name { get; }
        public float Size { get; }
        public FontStyle Style { get; }

        public SKTypeface Typeface => _typeface ??= SKTypeface.FromFamilyName(Name, ConvertFontStyle(Style));

        private static SKFontStyle ConvertFontStyle(FontStyle style)
        {
            var weight = (style & FontStyle.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
            var slant = (style & FontStyle.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
            return new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);
        }

        public SkiaSharpFont(string name, float size, FontStyle style = FontStyle.Regular)
        {
            Name = name ?? "Arial";
            Size = size;
            Style = style;
        }

        public void Dispose()
        {
            _typeface?.Dispose();
        }
    }

    /// <summary>
    /// SkiaSharp implementation of IBrush.
    /// </summary>
    internal class SkiaSharpBrush : IBrush
    {
        public SKPaint Paint { get; }

        public SkiaSharpBrush(IColor color)
        {
            Paint = new SKPaint
            {
                Color = new SKColor(color.R, color.G, color.B, color.A),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        public SkiaSharpBrush(SKColor color)
        {
            Paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        public void Dispose()
        {
            Paint?.Dispose();
        }
    }

    /// <summary>
    /// SkiaSharp implementation of IPen.
    /// </summary>
    internal class SkiaSharpPen : IPen
    {
        private DashStyle _dashStyle;
        public SKPaint Paint { get; }

        public float Width
        {
            get => Paint.StrokeWidth;
            set => Paint.StrokeWidth = value;
        }

        public IColor Color
        {
            get => new SkiaSharpColor(Paint.Color);
            set => Paint.Color = new SKColor(value.R, value.G, value.B, value.A);
        }

        public DashStyle DashStyle
        {
            get => _dashStyle;
            set
            {
                _dashStyle = value;
                Paint.PathEffect = value switch
                {
                    DashStyle.Dash => SKPathEffect.CreateDash(new[] { 5f, 5f }, 0f),
                    DashStyle.Dot => SKPathEffect.CreateDash(new[] { 1f, 3f }, 0f),
                    DashStyle.DashDot => SKPathEffect.CreateDash(new[] { 5f, 5f, 1f, 5f }, 0f),
                    DashStyle.DashDotDot => SKPathEffect.CreateDash(new[] { 5f, 5f, 1f, 5f, 1f, 5f }, 0f),
                    _ => null
                };
            }
        }

        public SkiaSharpPen(IColor color, float width = 1.0f)
        {
            Paint = new SKPaint
            {
                Color = new SKColor(color.R, color.G, color.B, color.A),
                StrokeWidth = width,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
            _dashStyle = DashStyle.Solid;
        }

        public void Dispose()
        {
            Paint?.Dispose();
        }
    }

    /// <summary>
    /// SkiaSharp implementation of IColor.
    /// </summary>
    internal class SkiaSharpColor : IColor
    {
        public SKColor Value { get; }

        public byte A => Value.Alpha;
        public byte R => Value.Red;
        public byte G => Value.Green;
        public byte B => Value.Blue;

        public SkiaSharpColor(SKColor color)
        {
            Value = color;
        }

        public SkiaSharpColor(byte r, byte g, byte b, byte a = 255)
        {
            Value = new SKColor(r, g, b, a);
        }
    }

    /// <summary>
    /// SkiaSharp implementation of IImage.
    /// </summary>
    internal class SkiaSharpImage : IImage
    {
        public SKImage Image { get; }

        public int Width => Image.Width;
        public int Height => Image.Height;

        public SkiaSharpImage(SKImage image)
        {
            Image = image ?? throw new ArgumentNullException(nameof(image));
        }

        public void Dispose()
        {
            Image?.Dispose();
        }
    }

    /// <summary>
    /// SkiaSharp implementation of IGraphicsPath.
    /// </summary>
    internal class SkiaSharpPath : IGraphicsPath
    {
        public SKPath Path { get; private set; }

        public bool IsEmpty => Path.IsEmpty;

        public SkiaSharpPath()
        {
            Path = new SKPath();
        }

        public void Reset() => Path.Reset();
        public void MoveTo(float x, float y) => ReplacePath(BuildPath(builder => builder.MoveTo(x, y)));
        public void LineTo(float x, float y) => ReplacePath(BuildPath(builder => builder.LineTo(x, y)));
        public void CubicTo(float x1, float y1, float x2, float y2, float x3, float y3) =>
            ReplacePath(BuildPath(builder => builder.CubicTo(x1, y1, x2, y2, x3, y3)));
        public void QuadTo(float x1, float y1, float x2, float y2) =>
            ReplacePath(BuildPath(builder => builder.QuadTo(x1, y1, x2, y2)));
        public void AddRectangle(float x, float y, float width, float height) =>
            ReplacePath(BuildPath(builder => builder.AddRect(new SKRect(x, y, x + width, y + height))));
        public void AddEllipse(float x, float y, float width, float height) =>
            ReplacePath(BuildPath(builder => builder.AddOval(new SKRect(x, y, x + width, y + height))));
        public void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle) =>
            ReplacePath(BuildPath(builder => builder.AddArc(new SKRect(x, y, x + width, y + height), startAngle, sweepAngle)));
        public void CloseFigure() => ReplacePath(BuildPath(builder => builder.Close()));

        private void ReplacePath(SKPath newPath)
        {
            Path?.Dispose();
            Path = newPath ?? throw new ArgumentNullException(nameof(newPath));
        }

        private static SKPath BuildPath(Action<SKPathBuilder> buildAction)
        {
            var builder = new SKPathBuilder();
            buildAction(builder);
            return builder.Detach();
        }

        public void Dispose()
        {
            Path?.Dispose();
        }
    }

    /// <summary>
    /// SkiaSharp implementation of IStringFormat.
    /// </summary>
    internal class SkiaSharpStringFormat : IStringFormat
    {
        public StringAlignment Alignment { get; set; }
        public StringAlignment LineAlignment { get; set; }
        public StringFormatFlags FormatFlags { get; set; }
        public StringTrimming Trimming { get; set; }

        public SkiaSharpStringFormat()
        {
            Alignment = StringAlignment.Near;
            LineAlignment = StringAlignment.Near;
            FormatFlags = StringFormatFlags.NoClip;
            Trimming = StringTrimming.None;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }

    #endregion
}
