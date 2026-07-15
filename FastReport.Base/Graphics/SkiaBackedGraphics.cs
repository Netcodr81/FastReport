using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace FastReport
{
    internal sealed class SkiaBackedGraphics : IGraphics
    {
        private readonly SKBitmap bitmapTarget;
        private readonly SKBitmap skBitmap;
        private readonly SKCanvas canvas;
        private bool disposed;

        private SkiaBackedGraphics(SKBitmap bitmap)
        {
            bitmapTarget = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            skBitmap = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            canvas = new SKCanvas(skBitmap);
            ReloadFromBitmap();
        }

        internal static IGraphics FromImage(SKImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (image is SKBitmap bitmap)
                return new SkiaBackedGraphics(bitmap);

            var target = new SKBitmap(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var graphics = new SkiaBackedGraphics(target);
            graphics.DrawImage(new ImagePaint(image), 0, 0);
            return graphics;
        }

        public SKCanvas Graphics => null;
        public float DpiY => bitmapTarget.VerticalResolution;
        public TextRenderingHint TextRenderingHint { get; set; } = TextRenderingHint.AntiAlias;
        public InterpolationMode InterpolationMode { get; set; } = InterpolationMode.HighQualityBicubic;
        public SmoothingMode SmoothingMode { get; set; } = SmoothingMode.AntiAlias;
        public System.Drawing.Drawing2D.Matrix Transform
        {
            get => new System.Drawing.Drawing2D.Matrix();
            set
            {
                if (value == null)
                    return;

                var elements = value.Elements;
                var matrix = ToSKMatrix(value);
                canvas.SetMatrix(matrix);
            }
        }

        public GraphicsUnit PageUnit { get; set; } = GraphicsUnit.Pixel;
        public bool IsClipEmpty => false;
        public Region Clip
        {
            get => null;
            set
            {
                if (value == null)
                {
                    ResetClip();
                    return;
                }

                SetClip(value.GetBounds(null), CombineMode.Replace);
            }
        }

        public float DpiX => bitmapTarget.HorizontalResolution;
        public CompositingQuality CompositingQuality { get; set; } = CompositingQuality.HighQuality;

        public void DrawText(string text, TextPaint paint, float left, float top)
        {
            if (string.IsNullOrEmpty(text) || paint?.Font == null || paint.Brush == null)
                return;

            var drawColor = paint.Brush is SolidBrush solidBrush ? solidBrush.Color : Color.Black;

            using var skPaint = new SKPaint
            {
                Color = ToSKColor(drawColor),
                IsAntialias = true
            };
            using var typeface = SKTypeface.FromFamilyName(paint.Font.FontFamily.Name,
                paint.Font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                paint.Font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
            using var skFont = new SKFont(typeface, paint.Font.Size * 96f / 72f);

            canvas.DrawText(text, left, top + skFont.Size, SKTextAlign.Left, skFont, skPaint);
        }

        public SizeF MeasureText(string text, TextPaint paint)
        {
            if (paint?.Font == null)
                return SizeF.Empty;

            return MeasureString(text, paint.Font);
        }

        public SizeF MeasureText(string text, TextPaint paint, float maxWidth)
        {
            if (paint?.Font == null)
                return SizeF.Empty;

            return MeasureString(text, paint.Font, (int)Math.Max(0, maxWidth), paint.Format ?? StringFormat.GenericDefault);
        }

        public void DrawString(string text, Font font, Brush brush, float left, float top)
        {
            DrawString(text, font, brush, left, top, null);
        }

        public void DrawString(string text, Font font, Brush brush, float left, float top, StringFormat format)
        {
            DrawText(text, new TextPaint(font, brush, format), left, top);
        }

        public void DrawString(string text, Font font, Brush brush, RectangleF rectangleF)
        {
            DrawString(text, font, brush, rectangleF.Left, rectangleF.Top, StringFormat.GenericDefault);
        }

        public void DrawString(string text, Font font, Brush textBrush, RectangleF textRect, StringFormat format)
        {
            DrawString(text, font, textBrush, textRect.Left, textRect.Top, format);
        }

        public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format)
        {
            DrawString(s, font, brush, point.X, point.Y, format);
        }

        public Region[] MeasureCharacterRanges(string text, Font font, RectangleF textRect, StringFormat format)
        {
            var size = MeasureString(text, font);
            return new[] { new Region(new RectangleF(textRect.Left, textRect.Top, size.Width, size.Height)) };
        }

        public SizeF MeasureString(string text, Font font)
        {
            if (string.IsNullOrEmpty(text) || font == null)
                return SizeF.Empty;

            using var typeface = SKTypeface.FromFamilyName(font.FontFamily.Name,
                font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
            using var skFont = new SKFont(typeface, font.Size * 96f / 72f);
            using var paint = new SKPaint { IsAntialias = true };
            float width = skFont.MeasureText(text, paint);
            var metrics = skFont.Metrics;
            float height = metrics.Descent - metrics.Ascent;
            return new SizeF(width, height);
        }

        public SizeF MeasureString(string text, Font font, SizeF size)
        {
            var measured = MeasureString(text, font);
            return new SizeF(Math.Min(measured.Width, size.Width), Math.Min(measured.Height, size.Height));
        }

        public SizeF MeasureString(string text, Font font, int v, StringFormat format)
        {
            var measured = MeasureString(text, font);
            return new SizeF(Math.Min(measured.Width, v), measured.Height);
        }

        public void MeasureString(string text, Font font, SizeF size, StringFormat format, out int charsFit, out int linesFit)
        {
            charsFit = 0;
            linesFit = 0;

            if (string.IsNullOrEmpty(text) || font == null || size.Width <= 0 || size.Height <= 0)
                return;

            var measured = MeasureString(text, font);
            if (measured.Width <= 0)
                return;

            charsFit = (int)Math.Min(text.Length, Math.Floor(size.Width / (measured.Width / text.Length)));
            linesFit = (int)Math.Max(1, Math.Floor(size.Height / measured.Height));
        }

        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat stringFormat)
        {
            return MeasureString(text, font, layoutArea);
        }

        public void DrawImage(ImagePaint paint, float x, float y)
        {
            if (paint?.Image == null)
                return;

            DrawImage(paint, new RectangleF(x, y, paint.Image.Width, paint.Image.Height));
        }

        public void DrawImage(ImagePaint paint, RectangleF destRect)
        {
            if (paint?.Image == null)
                return;

            using var src = ConvertImage(paint.Image);
            if (paint.HasSourceRect)
            {
                var sourceRect = new SKRect(paint.SourceRect.Left, paint.SourceRect.Top, paint.SourceRect.Right, paint.SourceRect.Bottom);
                var destinationRect = new SKRect(destRect.Left, destRect.Top, destRect.Right, destRect.Bottom);
                canvas.DrawBitmap(src, sourceRect, destinationRect);
            }
            else
            {
                var destinationRect = new SKRect(destRect.Left, destRect.Top, destRect.Right, destRect.Bottom);
                canvas.DrawBitmap(src, destinationRect);
            }
        }

        public void DrawImage(ImagePaint paint, RectangleF destRect, RectangleF srcRect)
        {
            if (paint?.Image == null)
                return;

            using var src = ConvertImage(paint.Image);
            var sourceRect = new SKRect(srcRect.Left, srcRect.Top, srcRect.Right, srcRect.Bottom);
            var destinationRect = new SKRect(destRect.Left, destRect.Top, destRect.Right, destRect.Bottom);
            canvas.DrawBitmap(src, sourceRect, destinationRect);
        }


        public void DrawArc(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            using var paint = CreateStrokePaint(pen);
            if (paint == null)
                return;

            canvas.DrawArc(new SKRect(x, y, x + width, y + height), startAngle, sweepAngle, false, paint);
        }

        public void DrawCurve(Pen pen, PointF[] points, int offset, int numberOfSegments, float tension)
        {
            if (points == null || points.Length < 2)
                return;

            int start = Math.Max(0, offset);
            int end = Math.Min(points.Length - 1, start + Math.Max(1, numberOfSegments));
            if (end <= start)
                return;

            using var paint = CreateStrokePaint(pen);
            if (paint == null)
                return;

            using var path = new SKPath();
            path.MoveTo(points[start].X, points[start].Y);
            for (int i = start + 1; i <= end; i++)
                path.LineTo(points[i].X, points[i].Y);
            canvas.DrawPath(path, paint);
        }

        public void DrawEllipse(Pen pen, float left, float top, float width, float height)
        {
            using var paint = CreateStrokePaint(pen);
            if (paint == null)
                return;

            canvas.DrawOval(new SKRect(left, top, left + width, top + height), paint);
        }

        public void DrawEllipse(Pen pen, RectangleF rect)
        {
            DrawEllipse(pen, rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            using var paint = CreateStrokePaint(pen);
            if (paint == null)
                return;

            canvas.DrawLine(x1, y1, x2, y2, paint);
        }

        public void DrawLine(Pen pen, PointF p1, PointF p2) => DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);

        public void DrawLines(Pen pen, PointF[] points)
        {
            if (points == null || points.Length < 2)
                return;

            using var paint = CreateStrokePaint(pen);
            if (paint == null)
                return;

            canvas.DrawPoints(SKPointMode.Polygon, ToSKPoints(points), paint);
        }

        public void DrawPath(Pen outlinePen, GraphicsPath path)
        {
            if (path == null)
                return;

            using var skPath = ConvertGraphicsPath(path);
            using var paint = CreateStrokePaint(outlinePen);
            if (paint == null)
                return;

            canvas.DrawPath(skPath, paint);
        }

        public void DrawPie(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            using var paint = CreateStrokePaint(pen);
            if (paint == null)
                return;

            using var path = new SKPath();
            var rect = new SKRect(x, y, x + width, y + height);
            path.MoveTo(rect.MidX, rect.MidY);
            path.ArcTo(rect, startAngle, sweepAngle, false);
            path.Close();
            canvas.DrawPath(path, paint);
        }

        public void DrawPolygon(Pen pen, PointF[] points)
        {
            if (points == null || points.Length < 2)
                return;

            using var paint = CreateStrokePaint(pen);
            if (paint == null)
                return;

            using var path = new SKPath();
            path.MoveTo(points[0].X, points[0].Y);
            for (int i = 1; i < points.Length; i++)
                path.LineTo(points[i].X, points[i].Y);
            path.Close();
            canvas.DrawPath(path, paint);
        }

        public void DrawPolygon(Pen pen, Point[] points)
        {
            if (points == null || points.Length < 2)
                return;

            DrawPolygon(pen, Array.ConvertAll(points, p => new PointF(p.X, p.Y)));
        }

        public void DrawRectangle(Pen pen, float left, float top, float width, float height)
        {
            using var paint = CreateStrokePaint(pen);
            if (paint == null)
                return;

            canvas.DrawRect(new SKRect(left, top, left + width, top + height), paint);
        }

        public void DrawRectangle(Pen pen, Rectangle rectangle)
        {
            DrawRectangle(pen, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
        }

        public void FillEllipse(Brush brush, float left, float top, float width, float height)
        {
            using var paint = CreateFillPaint(brush);
            if (paint == null)
                return;

            canvas.DrawOval(new SKRect(left, top, left + width, top + height), paint);
        }

        public void FillEllipse(Brush brush, RectangleF rect)
        {
            FillEllipse(brush, rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public void FillPath(Brush brush, GraphicsPath path)
        {
            if (path == null)
                return;

            using var skPath = ConvertGraphicsPath(path);
            using var paint = CreateFillPaint(brush);
            if (paint == null)
                return;

            canvas.DrawPath(skPath, paint);
        }

        public void FillPie(Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
        {
            using var paint = CreateFillPaint(brush);
            if (paint == null)
                return;

            using var path = new SKPath();
            var rect = new SKRect(x, y, x + width, y + height);
            path.MoveTo(rect.MidX, rect.MidY);
            path.ArcTo(rect, startAngle, sweepAngle, false);
            path.Close();
            canvas.DrawPath(path, paint);
        }

        public void FillPolygon(Brush brush, PointF[] points)
        {
            if (points == null || points.Length < 2)
                return;

            using var paint = CreateFillPaint(brush);
            if (paint == null)
                return;

            using var path = new SKPath();
            path.MoveTo(points[0].X, points[0].Y);
            for (int i = 1; i < points.Length; i++)
                path.LineTo(points[i].X, points[i].Y);
            path.Close();
            canvas.DrawPath(path, paint);
        }

        public void FillPolygon(Brush brush, Point[] points)
        {
            if (points == null || points.Length < 2)
                return;

            FillPolygon(brush, Array.ConvertAll(points, p => new PointF(p.X, p.Y)));
        }

        public void FillRectangle(Brush brush, RectangleF rect)
        {
            FillRectangle(brush, rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public void FillRectangle(Brush brush, float left, float top, float width, float height)
        {
            using var paint = CreateFillPaint(brush);
            if (paint == null)
                return;

            canvas.DrawRect(new SKRect(left, top, left + width, top + height), paint);
        }

        public void FillRegion(Brush brush, Region region)
        {
            if (region == null)
                return;

            FillRectangle(brush, region.GetBounds(null));
        }

        public void FillAndDrawPath(Pen pen, Brush brush, GraphicsPath path)
        {
            FillPath(brush, path);
            DrawPath(pen, path);
        }

        public void FillAndDrawEllipse(Pen pen, Brush brush, RectangleF rect)
        {
            FillEllipse(brush, rect);
            DrawEllipse(pen, rect);
        }

        public void FillAndDrawEllipse(Pen pen, Brush brush, float left, float top, float width, float height)
        {
            FillEllipse(brush, left, top, width, height);
            DrawEllipse(pen, left, top, width, height);
        }

        public void FillAndDrawPolygon(Pen pen, Brush brush, Point[] points)
        {
            FillPolygon(brush, points);
            DrawPolygon(pen, points);
        }

        public void FillAndDrawPolygon(Pen pen, Brush brush, PointF[] points)
        {
            FillPolygon(brush, points);
            DrawPolygon(pen, points);
        }

        public void FillAndDrawRectangle(Pen pen, Brush brush, float left, float top, float width, float height)
        {
            FillRectangle(brush, left, top, width, height);
            DrawRectangle(pen, left, top, width, height);
        }

        public void MultiplyTransform(System.Drawing.Drawing2D.Matrix matrix, MatrixOrder prepend)
        {
            if (matrix == null)
                return;

            var skMatrix = ToSKMatrix(matrix);
            canvas.Concat(skMatrix);
        }

        public void RotateTransform(float angle) => canvas.RotateDegrees(angle);
        public void ScaleTransform(float scaleX, float scaleY) => canvas.Scale(scaleX, scaleY);
        public void TranslateTransform(float left, float top) => canvas.Translate(left, top);

        public void Restore(IGraphicsState state)
        {
            if (state is SkiaGraphicsState skiaState)
                canvas.RestoreToCount(skiaState.SaveCount);
        }

        public IGraphicsState Save() => new SkiaGraphicsState(canvas.Save());
        public bool IsVisible(RectangleF rect) => true;

        public void ResetClip()
        {
            canvas.Restore();
            canvas.Save();
        }

        public void SetClip(RectangleF rect)
        {
            SetClip(rect, CombineMode.Replace);
        }

        public void SetClip(RectangleF rect, CombineMode combineMode)
        {
            var skRect = new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
            switch (combineMode)
            {
                case CombineMode.Exclude:
                    canvas.ClipRect(skRect, SKClipOperation.Difference, true);
                    break;
                case CombineMode.Intersect:
                case CombineMode.Replace:
                case CombineMode.Complement:
                case CombineMode.Xor:
                case CombineMode.Union:
                default:
                    canvas.ClipRect(skRect, SKClipOperation.Intersect, true);
                    break;
            }
        }

        public void SetClip(GraphicsPath path, CombineMode combineMode)
        {
            if (path == null)
                return;

            using var skPath = ConvertGraphicsPath(path);
            canvas.ClipPath(skPath, SKClipOperation.Intersect, true);
        }

        public void Clear(SKColor color)
        {
            canvas.Clear(color);
        }

        public bool TryGetNativeGraphics(out SKCanvas graphics)
        {
            graphics = null;
            return false;
        }

        public void Dispose()
        {
            if (disposed)
                return;

            FlushToBitmap();
            canvas.Dispose();
            skBitmap.Dispose();
            disposed = true;
        }

        private void FlushToBitmap()
        {
            var data = bitmapTarget.LockBits(new Rectangle(0, 0, bitmapTarget.Width, bitmapTarget.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            try
            {
                IntPtr src = skBitmap.GetPixels();
                if (src == IntPtr.Zero || data == null || data.Scan0 == IntPtr.Zero)
                    return;

                int bytes = data.Stride * data.Height;
                if (bytes <= 0)
                    return;

                byte[] buffer = new byte[bytes];
                Marshal.Copy(src, buffer, 0, bytes);
                Marshal.Copy(buffer, 0, data.Scan0, bytes);
            }
            finally
            {
                bitmapTarget.UnlockBits(data);
            }
        }

        private void ReloadFromBitmap()
        {
            var data = bitmapTarget.LockBits(new Rectangle(0, 0, bitmapTarget.Width, bitmapTarget.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
            try
            {
                IntPtr dst = skBitmap.GetPixels();
                if (dst == IntPtr.Zero || data == null || data.Scan0 == IntPtr.Zero)
                {
                    canvas.Clear(SKColors.Transparent);
                    return;
                }

                int bytes = data.Stride * data.Height;
                if (bytes <= 0)
                {
                    canvas.Clear(SKColors.Transparent);
                    return;
                }

                byte[] buffer = new byte[bytes];
                Marshal.Copy(data.Scan0, buffer, 0, bytes);
                Marshal.Copy(buffer, 0, dst, bytes);
            }
            finally
            {
                bitmapTarget.UnlockBits(data);
            }
        }

        private static SKBitmap ConvertBitmap(Bitmap bitmap)
        {
            var target = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
            try
            {
                IntPtr dst = target.GetPixels();
                if (dst == IntPtr.Zero || data == null || data.Scan0 == IntPtr.Zero)
                    return target;

                int bytes = data.Stride * data.Height;
                if (bytes <= 0)
                    return target;

                byte[] buffer = new byte[bytes];
                Marshal.Copy(data.Scan0, buffer, 0, bytes);
                Marshal.Copy(buffer, 0, dst, bytes);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return target;
        }

        private static SKBitmap ConvertImage(Image image)
        {
            if (image is Bitmap bitmap)
                return ConvertBitmap(bitmap);

            try
            {
                using var stream = new System.IO.MemoryStream();
                image.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                var decoded = SKBitmap.Decode(stream);
                if (decoded != null)
                    return decoded;
            }
            catch
            {
            }

            return new SKBitmap(Math.Max(1, image.Width), Math.Max(1, image.Height), SKColorType.Bgra8888, SKAlphaType.Premul);
        }

        private static SKColor ToSKColor(Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        private static SKMatrix ToSKMatrix(System.Drawing.Drawing2D.Matrix matrix)
        {
            var elements = matrix.Elements;
            return new SKMatrix
            {
                ScaleX = elements[0],
                SkewY = elements[1],
                SkewX = elements[2],
                ScaleY = elements[3],
                TransX = elements[4],
                TransY = elements[5],
                Persp0 = 0,
                Persp1 = 0,
                Persp2 = 1
            };
        }

        private static SKPoint[] ToSKPoints(PointF[] points)
        {
            var result = new SKPoint[points.Length];
            for (int i = 0; i < points.Length; i++)
                result[i] = new SKPoint(points[i].X, points[i].Y);
            return result;
        }

        private static SKPaint CreateStrokePaint(Pen pen)
        {
            if (pen == null)
                return null;

            return new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = pen.Width,
                Color = ToSKColor(pen.Color)
            };
        }

        private static SKPaint CreateFillPaint(Brush brush)
        {
            if (brush == null)
                return null;

            var color = brush is SolidBrush solidBrush ? solidBrush.Color : Color.Black;
            return new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = ToSKColor(color)
            };
        }

        private static SKPath ConvertGraphicsPath(GraphicsPath path)
        {
            var skPath = new SKPath();
            if (path.PointCount == 0)
                return skPath;

            var points = path.PathPoints;
            var types = path.PathTypes;
            int i = 0;
            while (i < points.Length)
            {
                var type = (byte)(types[i] & 0x07);
                var isClosed = (types[i] & (byte)PathPointType.CloseSubpath) != 0;

                switch ((PathPointType)type)
                {
                    case PathPointType.Start:
                        skPath.MoveTo(points[i].X, points[i].Y);
                        i++;
                        break;
                    case PathPointType.Line:
                        skPath.LineTo(points[i].X, points[i].Y);
                        i++;
                        break;
                    case PathPointType.Bezier3:
                        if (i + 2 < points.Length)
                        {
                            skPath.CubicTo(
                                points[i].X, points[i].Y,
                                points[i + 1].X, points[i + 1].Y,
                                points[i + 2].X, points[i + 2].Y);
                            i += 3;
                        }
                        else
                        {
                            skPath.LineTo(points[i].X, points[i].Y);
                            i++;
                        }
                        break;
                    default:
                        skPath.LineTo(points[i].X, points[i].Y);
                        i++;
                        break;
                }

                if (isClosed)
                    skPath.Close();
            }

            return skPath;
        }

        private sealed class SkiaGraphicsState : IGraphicsState
        {
            internal int SaveCount { get; }

            internal SkiaGraphicsState(int saveCount)
            {
                SaveCount = saveCount;
            }
        }
    }
}
