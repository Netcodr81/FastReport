using SkiaSharp;
using System;

namespace FastReport
{
    /// <summary>
    /// The interface for unifying methods for drawing objects into different graphics
    /// </summary>
    public interface IGraphics : IDisposable
    {
        #region Properties
        /// <summary>
        /// The underlying SkiaSharp canvas.
        /// </summary>
        SKCanvas Canvas { get; }

        /// <summary>
        /// Horizontal DPI of the render target.
        /// </summary>
        float DpiX { get; }

        /// <summary>
        /// Vertical DPI of the render target.
        /// </summary>
        float DpiY { get; }

        /// <summary>
        /// Current transformation matrix.
        /// </summary>
        SKMatrix Transform { get; set; }

        /// <summary>
        /// Indicates whether antialiasing is enabled.
        /// </summary>
        bool Antialias { get; set; }

        /// <summary>
        /// Image filtering quality.
        /// </summary>
        SKSamplingOptions FilterQuality { get; set; }

        /// <summary>
        /// Current clip bounds.
        /// </summary>
        SKRect ClipBounds { get; }

        /// <summary>
        /// True if the current clip region is empty.
        /// </summary>
        bool IsClipEmpty { get; }
        #endregion

        #region Draw and measure text
        // Contract-migration slice: text APIs without System.Drawing font/brush/string-format types.
        void DrawText(string text, TextPaint paint, float left, float top);
        SKSize MeasureText(string text, TextPaint paint);
        SKSize MeasureText(string text, TextPaint paint, float maxWidth);

        // Legacy text APIs kept temporarily for backward compatibility during migration.
        void DrawString(string text, SKFont font, SKPaint brush, float left, float top);
        void DrawString(string text, SKFont font, SKPaint brush, float left, float top, SKPaint format);
        // in this case if a baseline is needed, it will not be calculated
        void DrawString(string text, SKFont font, SKPaint brush, SKRect rectangleF);
        void DrawString(string text, SKFont font, SKPaint textBrush, SKRect textRect, SKPaint format);
        void DrawString(string s, SKFont font, SKPaint brush, SKPoint point, SKPaint format);
        SKRect[] MeasureCharacterRanges(string text, SKFont font, SKRect textRect, SKPaint format);
        SKSize MeasureString(string text, SKFont font);
        SKSize MeasureString(string text, SKFont font, SKSize size);
        SKSize MeasureString(string text, SKFont font, int v, SKPaint format);
        void MeasureString(string text, SKFont font, SKSize size, SKPaint format, out int charsFit, out int linesFit);
        SKSize MeasureString(string text, SKFont font, SKSize layoutArea, SKPaint stringFormat);
        #endregion

        #region Draw images
        // Contract-migration slice: image APIs wrapped in ImagePaint.
        void DrawImage(ImagePaint paint, float x, float y);
        void DrawImage(ImagePaint paint, SKRect destRect);
        void DrawImage(ImagePaint paint, SKRect destRect, SKRect srcRect);

        #endregion

        #region Draw geometry
        void DrawArc(SKPaint pen, float x, float y, float width, float height, float startAngle, float sweepAngle);
        void DrawCurve(SKPaint pen, SKPoint[] points, int offset, int numberOfSegments, float tension);
        void DrawEllipse(SKPaint pen, float left, float top, float width, float height);
        void DrawEllipse(SKPaint pen, SKRect rect);
        void DrawLine(SKPaint pen, float x1, float y1, float x2, float y2);
        void DrawLine(SKPaint pen, SKPoint p1, SKPoint p2);
        void DrawLines(SKPaint pen, SKPoint[] points);
        void DrawPath(SKPaint outlinePen, SKPath path);
        void DrawPie(SKPaint pen, float x, float y, float width, float height, float startAngle, float sweepAngle);
        void DrawPolygon(SKPaint pen, SKPoint[] points);
        void DrawRectangle(SKPaint pen, float left, float top, float width, float height);
        void DrawRectangle(SKPaint pen, SKRect rectangle);
        #endregion

        #region Fill geometry
        void FillEllipse(SKPaint brush, float left, float top, float width, float height);
        void FillEllipse(SKPaint brush, SKRect rect);
        // Works with polygons only
        void FillPath(SKPaint brush, SKPath path);
        void FillPie(SKPaint brush, float x, float y, float width, float height, float startAngle, float sweepAngle);
        void FillPolygon(SKPaint brush, SKPoint[] points);
        // Add rectangle to the graphics path
        void FillRectangle(SKPaint brush, SKRect rect);
        void FillRectangle(SKPaint brush, float left, float top, float width, float height);
        void FillRegion(SKPaint brush, SKRegion region);
        #endregion

        #region Fill and Draw

        void FillAndDrawPath(SKPaint pen, SKPaint brush, SKPath path);
        void FillAndDrawEllipse(SKPaint pen, SKPaint brush, SKRect rect);
        void FillAndDrawEllipse(SKPaint pen, SKPaint brush, float left, float top, float width, float height);
        void FillAndDrawPolygon(SKPaint pen, SKPaint brush, SKPoint[] points);
        void FillAndDrawRectangle(SKPaint pen, SKPaint brush, float left, float top, float width, float height);

        #endregion

        #region Transform
        void MultiplyTransform(SKMatrix matrix, MatrixOrder prepend);
        void RotateTransform(float angle);
        void ScaleTransform(float scaleX, float scaleY);
        void TranslateTransform(float left, float top);
        #endregion

        #region State
        void Restore(IGraphicsState state);
        IGraphicsState Save();
        #endregion

        #region Clip
        bool IsVisible(SKRect rect);

        void ResetClip();

        void SetClip(SKRect rect);

        void SetClip(SKRect rect, SKClipOperation operation);

        void SetClip(SKPath path);

        void SetClip(SKPath path, ClipOperation operation);
        #endregion

        #region Surface
        void Clear(SKColor color);
        bool TryGetNativeGraphics(out SKCanvas graphics);
        #endregion
    }

    public sealed class TextPaint
    {
        public SKFont Font { get; set; }

        public SKPaint Paint { get; set; }

        public TextAlignment Alignment { get; set; } = TextAlignment.Near;

        public TextTrimming Trimming { get; set; } = TextTrimming.None;

        public bool WordWrap { get; set; } = true;

        public bool RightToLeft { get; set; }

        public TextPaint()
        {
            Paint = new SKPaint
            {
                IsAntialias = true
            };
        }

        public TextPaint(
            SKFont font,
            SKPaint paint,
            TextAlignment alignment = TextAlignment.Near)
        {
            Font = font;
            Paint = paint;
            Alignment = alignment;
        }
    }

    public sealed class ImagePaint
    {
        public SKImage Image { get; set; }
        public SKRect SourceRect { get; set; }
        public bool HasSourceRect { get; set; }

        public ImagePaint()
        {
        }

        public ImagePaint(SKImage image)
        {
            Image = image;
            HasSourceRect = false;
            SourceRect = SKRect.Empty;
        }

        public ImagePaint(SKImage image, SKRect sourceRect)
        {
            Image = image;
            SourceRect = sourceRect;
            HasSourceRect = true;
        }
    }

    /// <summary>
    /// the interface for saving and restoring state
    /// </summary>
    public interface IGraphicsState
    {

    }

}

public enum ClipOperation
{
    Replace,
    Intersect,
    Exclude,
    Union,
    Xor,
    ReverseDifference
}

public enum MatrixOrder
{
    Prepend,
    Append
}

public enum TextAlignment
{
    Near,
    Center,
    Far
}

public enum TextTrimming
{
    None,
    Character,
    Word,
    EllipsisCharacter,
    EllipsisWord
}