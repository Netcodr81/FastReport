using System;

using SkiaSharp;

namespace FastReport;

/// <summary>
/// The interface for unifying methods for drawing objects into different graphics
/// </summary>
public interface IGraphics : IDisposable
{
    #region Properties
    SKCanvas Graphics { get; }
    float DpiY { get; }
    SKSamplingOptions SamplingOptions { get; set; }
    SKMatrix Transform { get; set; }
    bool IsClipEmpty { get; }
    SKRegion Clip { get; set; }
    float DpiX { get; }
    #endregion

    #region Draw and measure text
    void DrawString(string text, SKFont font, SKPaint brush, float left, float top);
    void DrawString(string text, SKFont font, SKPaint brush, float left, float top, SKPaint format);
    // in this case if a baseline is needed, it will not be calculated
    void DrawString(string text, SKFont font, SKPaint brush, SKRect rectangleF);
    void DrawString(string text, SKFont font, SKPaint textBrush, SKRect textRect, SKPaint format);
    void DrawString(string s, SKFont font, SKPaint brush, SKPoint point, SKPaint format);
    SKRegion[] MeasureCharacterRanges(string text, SKFont font, SKRect textRect, SKPaint format);
    SKSize MeasureString(string text, SKFont font);
    SKSize MeasureString(string text, SKFont font, SKSize size);
    SKSize MeasureString(string text, SKFont font, int v, SKPaint format);
    void MeasureString(string text, SKFont font, SKSize size, SKPaint format, out int charsFit, out int linesFit);
    SKSize MeasureString(string text, SKFont font, SKSize layoutArea, SKPaint stringFormat);
    #endregion

    #region Draw images
    void DrawImage(SKImage image, float x, float y);
    void DrawImage(SKImage image, SKRect rect1, SKRect rect2);
    void DrawImage(SKImage image, SKRect rect);
    void DrawImage(SKImage image, float x, float y, float width, float height);
    void DrawImage(SKImage image, SKPoint[] points);
    void DrawImage(SKImage image, SKRectI destRect, int srcX, int srcY, int srcWidth, int srcHeight, SKPaint paint);
    void DrawImage(SKImage image, SKRectI destRect, float srcX, float srcY, float srcWidth, float srcHeight, SKPaint paint);
    void DrawImageUnscaled(SKImage image, SKRectI rect);
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
    void DrawPolygon(SKPaint pen, SKPointI[] points);
    void DrawRectangle(SKPaint pen, float left, float top, float width, float height);
    void DrawRectangle(SKPaint pen, SKRectI rectangle);
    #endregion

    #region Fill geometry
    void FillEllipse(SKPaint brush, float left, float top, float width, float height);
    void FillEllipse(SKPaint brush, SKRect rect);
    // Works with polygons only
    void FillPath(SKPaint brush, SKPath path);
    void FillPie(SKPaint brush, float x, float y, float width, float height, float startAngle, float sweepAngle);
    void FillPolygon(SKPaint brush, SKPoint[] points);
    void FillPolygon(SKPaint brush, SKPointI[] points);
    // Add rectangle to the graphics path
    void FillRectangle(SKPaint brush, SKRect rect);
    void FillRectangle(SKPaint brush, float left, float top, float width, float height);
    void FillRegion(SKPaint brush, SKRegion region);
    #endregion

    #region Fill and Draw

    void FillAndDrawPath(SKPaint pen, SKPaint brush, SKPath path);
    void FillAndDrawEllipse(SKPaint pen, SKPaint brush, SKRect rect);
    void FillAndDrawEllipse(SKPaint pen, SKPaint brush, float left, float top, float width, float height);
    void FillAndDrawPolygon(SKPaint pen, SKPaint brush, SKPointI[] points);
    void FillAndDrawPolygon(SKPaint pen, SKPaint brush, SKPoint[] points);
    void FillAndDrawRectangle(SKPaint pen, SKPaint brush, float left, float top, float width, float height);

    #endregion

    #region Transform
    void MultiplyTransform(SKMatrix matrix, bool prepend);
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
    void SetClip(SKRect rect, SKClipOperation combineMode);
    void SetClip(SKPath path, SKClipOperation combineMode);
    #endregion
}

/// <summary>
/// the interface for saving and restoring state
/// </summary>
public interface IGraphicsState
{

}