using SkiaSharp;
using System;

namespace FastReport
{
    /// <summary>
    /// Drawing objects using SkiaSharp
    /// </summary>
    public class GdiGraphics : IGraphics
    {
        private SKCanvas graphics;
        private readonly bool haveToDispose;
        private SKSamplingOptions samplingOptions;
        private SKMatrix transform;
        private SKRegion clipRegion;

       #region Properties
        public SKCanvas Graphics
        {
            get { return graphics; }
        }

        float IGraphics.DpiX => 96f; // Default DPI for SkiaSharp
        float IGraphics.DpiY => 96f; // Default DPI for SkiaSharp

        SKSamplingOptions IGraphics.SamplingOptions 
        { 
            get => samplingOptions; 
            set => samplingOptions = value; 
        }

        SKMatrix IGraphics.Transform 
        { 
            get => graphics.TotalMatrix; 
            set 
            {
                graphics.SetMatrix(value);
                transform = value;
            }
        }

        bool IGraphics.IsClipEmpty => clipRegion?.IsEmpty ?? true;

        SKRegion IGraphics.Clip 
        { 
            get => clipRegion; 
            set => clipRegion = value; 
        }
        #endregion

        public GdiGraphics(SKBitmap bitmap)
            : this(new SKCanvas(bitmap), true)
        {
        }

        public GdiGraphics(SKCanvas graphics, bool haveToDispose)
        {
            this.graphics = graphics;
            this.haveToDispose = haveToDispose;
            this.samplingOptions = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
            this.transform = SKMatrix.Identity;
            this.clipRegion = new SKRegion();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (graphics != null && haveToDispose)
                        graphics.Dispose();
                    graphics = null;
                    clipRegion?.Dispose();
                    clipRegion = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Draw and measure text
        public void DrawString(string text, SKFont font, SKPaint brush, float left, float top)
        {
            graphics.DrawText(text, left, top, font, brush);
        }

        public void DrawString(string text, SKFont font, SKPaint brush, float left, float top, SKPaint format)
        {
            // Merge format properties if provided
            var paint = brush.Clone();
            if (format != null && format.IsAntialias)
            {
                paint.IsAntialias = format.IsAntialias;
            }
            graphics.DrawText(text, left, top, font, paint);
        }

        public void DrawString(string text, SKFont font, SKPaint brush, SKRect rectangleF)
        {
            graphics.DrawText(text, rectangleF.Left, rectangleF.Top + font.Size, font, brush);
        }

        public void DrawString(string text, SKFont font, SKPaint textBrush, SKRect textRect, SKPaint format)
        {
            var paint = textBrush.Clone();

            // Draw text with wrapping if needed
            var lines = text.Split('\n');
            float y = textRect.Top + font.Size;
            float lineHeight = font.Spacing;

            foreach (var line in lines)
            {
                if (y > textRect.Bottom) break;

                float x = textRect.Left;
                graphics.DrawText(line, x, y, font, paint);
                y += lineHeight;
            }
        }

        void IGraphics.DrawString(string s, SKFont font, SKPaint brush, SKPoint point, SKPaint format)
        {
            var paint = brush.Clone();
            graphics.DrawText(s, point.X, point.Y, font, paint);
        }

        public SKRegion[] MeasureCharacterRanges(string text, SKFont font, SKRect rect, SKPaint format)
        {
            // SkiaSharp doesn't have direct equivalent, return empty array
            return new SKRegion[0];
        }

        public SKSize MeasureString(string text, SKFont font, SKSize size)
        {
            var bounds = new SKRect();
            using (var paint = new SKPaint())
            {
                font.MeasureText(text, out bounds, paint);
                return new SKSize(bounds.Width, font.Size);
            }
        }

        public SKSize MeasureString(string text, SKFont font, int width, SKPaint format)
        {
            var bounds = new SKRect();
            using (var paint = format?.Clone() ?? new SKPaint())
            {
                font.MeasureText(text, out bounds, paint);
                return new SKSize(bounds.Width, font.Size);
            }
        }

        public void MeasureString(string text, SKFont font, SKSize size, SKPaint format, out int charsFit, out int linesFit)
        {
            var bounds = new SKRect();
            using (var paint = format?.Clone() ?? new SKPaint())
            {
                font.MeasureText(text, out bounds, paint);

                charsFit = text.Length;
                linesFit = 1;

                if (bounds.Width > size.Width)
                {
                    charsFit = (int)(text.Length * (size.Width / bounds.Width));
                }
            }
        }

        public SKSize MeasureString(string text, SKFont font)
        {
            var bounds = new SKRect();
            using (var paint = new SKPaint())
            {
                font.MeasureText(text, out bounds, paint);
                return new SKSize(bounds.Width, font.Size);
            }
        }

        public SKSize MeasureString(string text, SKFont font, SKSize layoutArea, SKPaint format)
        {
            var bounds = new SKRect();
            using (var paint = format?.Clone() ?? new SKPaint())
            {
                font.MeasureText(text, out bounds, paint);
                return new SKSize(Math.Min(bounds.Width, layoutArea.Width), font.Size);
            }
        }
        #endregion

        #region Draw images
        public void DrawImage(SKImage image, float x, float y)
        {
            graphics.DrawImage(image, x, y);
        }

        public void DrawImage(SKImage image, SKRect destRect, SKRect srcRect)
        {
            graphics.DrawImage(image, destRect, samplingOptions);
        }

        public void DrawImage(SKImage image, SKRect rect)
        {
            graphics.DrawImage(image, rect, samplingOptions);
        }

        public void DrawImage(SKImage image, float x, float y, float width, float height)
        {
            var destRect = new SKRect(x, y, x + width, y + height);
            graphics.DrawImage(image, destRect, samplingOptions);
        }

        public void DrawImage(SKImage image, SKPoint[] points)
        {
            // SkiaSharp doesn't support arbitrary quad drawing directly
            // We'll use a simple rectangle for the first 3 points
            if (points.Length >= 3)
            {
                var destRect = new SKRect(
                    Math.Min(points[0].X, Math.Min(points[1].X, points[2].X)),
                    Math.Min(points[0].Y, Math.Min(points[1].Y, points[2].Y)),
                    Math.Max(points[0].X, Math.Max(points[1].X, points[2].X)),
                    Math.Max(points[0].Y, Math.Max(points[1].Y, points[2].Y))
                );
                graphics.DrawImage(image, destRect, samplingOptions);
            }
        }

        public void DrawImage(SKImage image, SKRectI destRect, int srcX, int srcY, int srcWidth, int srcHeight, SKPaint paint)
        {
            var srcRect = new SKRect(srcX, srcY, srcX + srcWidth, srcY + srcHeight);
            var dest = SKRect.Create(destRect.Left, destRect.Top, destRect.Width, destRect.Height);
            graphics.DrawImage(image, srcRect, dest, samplingOptions, paint);
        }

        public void DrawImage(SKImage image, SKRectI destRect, float srcX, float srcY, float srcWidth, float srcHeight, SKPaint paint)
        {
            var srcRect = new SKRect(srcX, srcY, srcX + srcWidth, srcY + srcHeight);
            var dest = SKRect.Create(destRect.Left, destRect.Top, destRect.Width, destRect.Height);
            graphics.DrawImage(image, srcRect, dest, samplingOptions, paint);
        }

        public void DrawImageUnscaled(SKImage image, SKRectI rect)
        {
            var destRect = SKRect.Create(rect.Left, rect.Top, rect.Width, rect.Height);
            graphics.DrawImage(image, destRect, samplingOptions);
        }
        #endregion

        #region Draw geometry
        public void DrawArc(SKPaint pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            using (var path = new SKPath())
            {
                var rect = new SKRect(x, y, x + width, y + height);
                path.AddArc(rect, startAngle, sweepAngle);
                graphics.DrawPath(path, pen);
            }
        }

        public void DrawCurve(SKPaint pen, SKPoint[] points, int offset, int numberOfSegments, float tension)
        {
            if (points.Length < 2) return;

            using (var path = new SKPath())
            {
                path.MoveTo(points[offset]);

                for (int i = offset; i < offset + numberOfSegments && i < points.Length - 1; i++)
                {
                    // Simple curve implementation
                    path.LineTo(points[i + 1]);
                }

                graphics.DrawPath(path, pen);
            }
        }

        public void DrawEllipse(SKPaint pen, float left, float top, float width, float height)
        {
            var rect = new SKRect(left, top, left + width, top + height);
            graphics.DrawOval(rect, pen);
        }

        public void DrawEllipse(SKPaint pen, SKRect rect)
        {
            graphics.DrawOval(rect, pen);
        }

        public void DrawLine(SKPaint pen, float x1, float y1, float x2, float y2)
        {
            graphics.DrawLine(x1, y1, x2, y2, pen);
        }

        public void DrawLine(SKPaint pen, SKPoint p1, SKPoint p2)
        {
            graphics.DrawLine(p1, p2, pen);
        }

        public void DrawLines(SKPaint pen, SKPoint[] points)
        {
            if (points.Length < 2) return;

            for (int i = 0; i < points.Length - 1; i++)
            {
                graphics.DrawLine(points[i], points[i + 1], pen);
            }
        }

        public void DrawPath(SKPaint pen, SKPath path)
        {
            graphics.DrawPath(path, pen);
        }

        public void DrawPie(SKPaint pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            using (var path = new SKPath())
            {
                var rect = new SKRect(x, y, x + width, y + height);
                var center = new SKPoint(rect.MidX, rect.MidY);
                path.MoveTo(center);
                path.ArcTo(rect, startAngle, sweepAngle, false);
                path.Close();
                graphics.DrawPath(path, pen);
            }
        }

        public void DrawPolygon(SKPaint pen, SKPoint[] points)
        {
            graphics.DrawPoints(SKPointMode.Polygon, points, pen);
        }

        public void DrawPolygon(SKPaint pen, SKPointI[] points)
        {
            var floatPoints = new SKPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                floatPoints[i] = new SKPoint(points[i].X, points[i].Y);
            }
            graphics.DrawPoints(SKPointMode.Polygon, floatPoints, pen);
        }

        public void DrawRectangle(SKPaint pen, float left, float top, float width, float height)
        {
            graphics.DrawRect(left, top, width, height, pen);
        }

        public void DrawRectangle(SKPaint pen, SKRectI rectangle)
        {
            graphics.DrawRect(SKRect.Create(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height), pen);
        }
        #endregion

        #region Fill geometry
        public void FillEllipse(SKPaint brush, float left, float top, float width, float height)
        {
            var rect = new SKRect(left, top, left + width, top + height);
            var fillPaint = brush.Clone();
            fillPaint.Style = SKPaintStyle.Fill;
            graphics.DrawOval(rect, fillPaint);
        }

        public void FillEllipse(SKPaint brush, SKRect rect)
        {
            var fillPaint = brush.Clone();
            fillPaint.Style = SKPaintStyle.Fill;
            graphics.DrawOval(rect, fillPaint);
        }

        public void FillPath(SKPaint brush, SKPath path)
        {
            var fillPaint = brush.Clone();
            fillPaint.Style = SKPaintStyle.Fill;
            graphics.DrawPath(path, fillPaint);
        }

        public void FillPie(SKPaint brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            using (var path = new SKPath())
            {
                var rect = new SKRect(x, y, x + width, y + height);
                var center = new SKPoint(rect.MidX, rect.MidY);
                path.MoveTo(center);
                path.ArcTo(rect, startAngle, sweepAngle, false);
                path.Close();

                var fillPaint = brush.Clone();
                fillPaint.Style = SKPaintStyle.Fill;
                graphics.DrawPath(path, fillPaint);
            }
        }

        public void FillPolygon(SKPaint brush, SKPoint[] points)
        {
            using (var path = new SKPath())
            {
                if (points.Length > 0)
                {
                    path.MoveTo(points[0]);
                    for (int i = 1; i < points.Length; i++)
                    {
                        path.LineTo(points[i]);
                    }
                    path.Close();

                    var fillPaint = brush.Clone();
                    fillPaint.Style = SKPaintStyle.Fill;
                    graphics.DrawPath(path, fillPaint);
                }
            }
        }

        public void FillPolygon(SKPaint brush, SKPointI[] points)
        {
            var floatPoints = new SKPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                floatPoints[i] = new SKPoint(points[i].X, points[i].Y);
            }
            FillPolygon(brush, floatPoints);
        }

        public void FillRectangle(SKPaint brush, SKRect rect)
        {
            var fillPaint = brush.Clone();
            fillPaint.Style = SKPaintStyle.Fill;
            graphics.DrawRect(rect, fillPaint);
        }

        public void FillRectangle(SKPaint brush, float left, float top, float width, float height)
        {
            var fillPaint = brush.Clone();
            fillPaint.Style = SKPaintStyle.Fill;
            graphics.DrawRect(left, top, width, height, fillPaint);
        }

        public void FillRegion(SKPaint brush, SKRegion region)
        {
            // SkiaSharp doesn't have direct region fill, use path instead
            using (var path = region.GetBoundaryPath())
            {
                if (path != null)
                {
                    var fillPaint = brush.Clone();
                    fillPaint.Style = SKPaintStyle.Fill;
                    graphics.DrawPath(path, fillPaint);
                }
            }
        }
        #endregion

        #region Fill and Draw
        public void FillAndDrawPath(SKPaint pen, SKPaint brush, SKPath path)
        {
            var fillPaint = brush.Clone();
            fillPaint.Style = SKPaintStyle.Fill;
            graphics.DrawPath(path, fillPaint);

            var strokePaint = pen.Clone();
            strokePaint.Style = SKPaintStyle.Stroke;
            graphics.DrawPath(path, strokePaint);
        }

        public void FillAndDrawEllipse(SKPaint pen, SKPaint brush, SKRect rect)
        {
            var fillPaint = brush.Clone();
            fillPaint.Style = SKPaintStyle.Fill;
            graphics.DrawOval(rect, fillPaint);

            var strokePaint = pen.Clone();
            strokePaint.Style = SKPaintStyle.Stroke;
            graphics.DrawOval(rect, strokePaint);
        }

        public void FillAndDrawEllipse(SKPaint pen, SKPaint brush, float left, float top, float width, float height)
        {
            var rect = new SKRect(left, top, left + width, top + height);
            FillAndDrawEllipse(pen, brush, rect);
        }

        public void FillAndDrawPolygon(SKPaint pen, SKPaint brush, SKPointI[] points)
        {
            var floatPoints = new SKPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                floatPoints[i] = new SKPoint(points[i].X, points[i].Y);
            }
            FillAndDrawPolygon(pen, brush, floatPoints);
        }

        public void FillAndDrawPolygon(SKPaint pen, SKPaint brush, SKPoint[] points)
        {
            using (var path = new SKPath())
            {
                if (points.Length > 0)
                {
                    path.MoveTo(points[0]);
                    for (int i = 1; i < points.Length; i++)
                    {
                        path.LineTo(points[i]);
                    }
                    path.Close();

                    var fillPaint = brush.Clone();
                    fillPaint.Style = SKPaintStyle.Fill;
                    graphics.DrawPath(path, fillPaint);

                    var strokePaint = pen.Clone();
                    strokePaint.Style = SKPaintStyle.Stroke;
                    graphics.DrawPath(path, strokePaint);
                }
            }
        }

        public void FillAndDrawRectangle(SKPaint pen, SKPaint brush, float left, float top, float width, float height)
        {
            var fillPaint = brush.Clone();
            fillPaint.Style = SKPaintStyle.Fill;
            graphics.DrawRect(left, top, width, height, fillPaint);

            var strokePaint = pen.Clone();
            strokePaint.Style = SKPaintStyle.Stroke;
            graphics.DrawRect(left, top, width, height, strokePaint);
        }
        #endregion

        #region Transform
        public void MultiplyTransform(SKMatrix matrix, bool prepend)
        {
            var currentMatrix = graphics.TotalMatrix;
            SKMatrix result;

            if (prepend)
            {
                result = matrix.PreConcat(currentMatrix);
            }
            else
            {
                result = currentMatrix.PreConcat(matrix);
            }

            graphics.SetMatrix(result);
            transform = result;
        }

        public void RotateTransform(float angle)
        {
            graphics.RotateDegrees(angle);
            transform = graphics.TotalMatrix;
        }

        public void ScaleTransform(float scaleX, float scaleY)
        {
            graphics.Scale(scaleX, scaleY);
            transform = graphics.TotalMatrix;
        }

        public void TranslateTransform(float left, float top)
        {
            graphics.Translate(left, top);
            transform = graphics.TotalMatrix;
        }
        #endregion

        #region State
        public void Restore(IGraphicsState state)
        {
            if (state is SkiaGraphicsState skiaState)
            {
                graphics.RestoreToCount(skiaState.SaveCount);
            }
        }

        public IGraphicsState Save()
        {
            int saveCount = graphics.Save();
            return new SkiaGraphicsState(saveCount);
        }

        private class SkiaGraphicsState : IGraphicsState
        {
            public int SaveCount { get; }

            public SkiaGraphicsState(int saveCount)
            {
                SaveCount = saveCount;
            }
        }
        #endregion

        #region Clip
        public bool IsVisible(SKRect rect)
        {
            return graphics.QuickReject(rect) == false;
        }

        public void ResetClip()
        {
            // SkiaSharp doesn't have a direct ResetClip, so we restore to no clip
            graphics.Save();
            clipRegion = new SKRegion();
        }

        public void SetClip(SKRect rect)
        {
            graphics.ClipRect(rect);
            clipRegion.SetRect(SKRectI.Round(rect));
        }

        public void SetClip(SKRect rect, SKClipOperation combineMode)
        {
            graphics.ClipRect(rect, combineMode);

            // Update region for tracking
            if (combineMode == SKClipOperation.Intersect)
            {
                var rectRegion = new SKRegion();
                rectRegion.SetRect(SKRectI.Round(rect));
                clipRegion.Op(rectRegion, SKRegionOperation.Intersect);
            }
        }

        public void SetClip(SKPath path, SKClipOperation combineMode)
        {
            graphics.ClipPath(path, combineMode);

            // Update region for tracking
            if (combineMode == SKClipOperation.Intersect)
            {
                var pathRegion = new SKRegion();
                pathRegion.SetPath(path);
                clipRegion.Op(pathRegion, SKRegionOperation.Intersect);
            }
        }
        #endregion
    }
}
