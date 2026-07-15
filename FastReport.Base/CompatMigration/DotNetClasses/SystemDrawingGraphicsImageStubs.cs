using System;
using System.IO;

namespace System.Drawing
{
    public class Region : IDisposable
    {
        private RectangleF rect;
        public Region(RectangleF rectangle) => rect = rectangle;
        public RectangleF GetBounds(Graphics g) => rect;
        public void Dispose() { }
    }

    public abstract class Image : IDisposable
    {
        public virtual int Width { get; protected set; }
        public virtual int Height { get; protected set; }
        public virtual float HorizontalResolution { get; protected set; } = 96f;
        public virtual float VerticalResolution { get; protected set; } = 96f;
        public virtual Imaging.PixelFormat PixelFormat => Imaging.PixelFormat.Format32bppArgb;
        public virtual Imaging.ImageFormat RawFormat { get; protected set; } = Imaging.ImageFormat.Png;

        public static Image FromStream(Stream stream) => new Bitmap(1, 1);
        public static Image FromFile(string fileName) => new Bitmap(1, 1);

        public virtual void Save(Stream stream, Imaging.ImageFormat format) { }
        public virtual void Save(string fileName, Imaging.ImageFormat format) { }
        public virtual void Save(Stream stream, Imaging.ImageCodecInfo encoder, Imaging.EncoderParameters encoderParams) { }
        public virtual void SaveAdd(Image image, Imaging.EncoderParameters encoderParams) { }
        public virtual void SaveAdd(Imaging.EncoderParameters encoderParams) { }

        public virtual object Clone() => MemberwiseClone();
        public virtual void Dispose() { }
    }

    public class Bitmap : Image
    {
        public Bitmap(int width, int height) { Width = width; Height = height; }
        public Bitmap(int width, int height, Imaging.PixelFormat pixelFormat) : this(width, height) { }
        public Bitmap(Image original) : this(original?.Width ?? 1, original?.Height ?? 1) { }
        public Bitmap(Image original, Size newSize) : this(newSize.Width, newSize.Height) { }
        public void SetResolution(float xDpi, float yDpi) { HorizontalResolution = xDpi; VerticalResolution = yDpi; }
        public void MakeTransparent(Color transparentColor) { }
        public Imaging.BitmapData LockBits(Rectangle rect, Imaging.ImageLockMode flags, Imaging.PixelFormat format) => new Imaging.BitmapData();
        public void UnlockBits(Imaging.BitmapData bitmapdata) { }
        public Color GetPixel(int x, int y) => Color.Empty;
        public void SetPixel(int x, int y, Color color) { }
    }

    public sealed class Metafile : Image
    {
        public Metafile(Stream stream, IntPtr referenceHdc) { }
        public Metafile(string filename, IntPtr referenceHdc) { }
    }

    public class Graphics : IDisposable
    {
        public float DpiX { get; set; } = 96f;
        public float DpiY { get; set; } = 96f;

        public Text.TextRenderingHint TextRenderingHint { get; set; }
        public Drawing2D.InterpolationMode InterpolationMode { get; set; }
        public Drawing2D.SmoothingMode SmoothingMode { get; set; }
        public Drawing2D.Matrix Transform { get; set; } = new Drawing2D.Matrix();
        public GraphicsUnit PageUnit { get; set; }
        public bool IsClipEmpty => false;
        public Region Clip { get; set; }
        public Drawing2D.CompositingQuality CompositingQuality { get; set; }
        public Drawing2D.CompositingMode CompositingMode { get; set; }
        public Drawing2D.PixelOffsetMode PixelOffsetMode { get; set; }

        public static Graphics FromImage(Image image) => new Graphics();
        public static Graphics FromHdc(IntPtr hdc) => new Graphics();

        public void DrawImage(Image image, float x, float y, float width, float height) { }
        public void DrawImage(Image image, float x, float y) { }
        public void DrawImage(Image image, PointF point) { }
        public void DrawImage(Image image, RectangleF rect) { }
        public void DrawImage(Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit) { }
        public void DrawImage(Image image, PointF[] points) { }
        public void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, Imaging.ImageAttributes imageAttr) { }
        public void DrawImage(Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, Imaging.ImageAttributes imageAttrs) { }
        public void DrawImageUnscaled(Image image, Rectangle rect) { }

        public void DrawString(string s, Font font, Brush brush, PointF point, StringFormat format) { }
        public void DrawString(string text, Font font, Brush brush, float x, float y) { }
        public void DrawString(string text, Font font, Brush brush, float x, float y, StringFormat format) { }
        public void DrawString(string text, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format) { }

        public SizeF MeasureString(string text, Font font) => SizeF.Empty;
        public SizeF MeasureString(string text, Font font, int width) => SizeF.Empty;
        public SizeF MeasureString(string text, Font font, SizeF size) => SizeF.Empty;
        public SizeF MeasureString(string text, Font font, int width, StringFormat format) => SizeF.Empty;
        public SizeF MeasureString(string text, Font font, SizeF layoutArea, StringFormat format) => SizeF.Empty;
        public void MeasureString(string text, Font font, SizeF size, StringFormat format, out int charsFit, out int linesFit) { charsFit = 0; linesFit = 0; }
        public Region[] MeasureCharacterRanges(string text, Font font, RectangleF rect, StringFormat format) => Array.Empty<Region>();

        public void DrawArc(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle) { }
        public void DrawCurve(Pen pen, PointF[] points, int offset, int numberOfSegments, float tension) { }
        public void DrawEllipse(Pen pen, float left, float top, float width, float height) { }
        public void DrawEllipse(Pen pen, RectangleF rect) { }
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2) { }
        public void DrawLine(Pen pen, PointF p1, PointF p2) { }
        public void DrawLines(Pen pen, PointF[] points) { }
        public void DrawPath(Pen pen, Drawing2D.GraphicsPath path) { }
        public void DrawPie(Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle) { }
        public void DrawPolygon(Pen pen, PointF[] points) { }
        public void DrawPolygon(Pen pen, Point[] points) { }
        public void DrawRectangle(Pen pen, float left, float top, float width, float height) { }
        public void DrawRectangle(Pen pen, Rectangle rectangle) { }

        public void FillEllipse(Brush brush, float left, float top, float width, float height) { }
        public void FillEllipse(Brush brush, RectangleF rect) { }
        public void FillPath(Brush brush, Drawing2D.GraphicsPath path) { }
        public void FillPie(Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle) { }
        public void FillPolygon(Brush brush, PointF[] points) { }
        public void FillPolygon(Brush brush, Point[] points) { }
        public void FillRectangle(Brush brush, RectangleF rect) { }
        public void FillRectangle(Brush brush, float left, float top, float width, float height) { }
        public void FillRegion(Brush brush, Region region) { }

        public void MultiplyTransform(Drawing2D.Matrix matrix, Drawing2D.MatrixOrder order) { }
        public void RotateTransform(float angle) { }
        public void ScaleTransform(float sx, float sy) { }
        public void TranslateTransform(float dx, float dy) { }

        public Drawing2D.GraphicsState Save() => new Drawing2D.GraphicsState();
        public void Restore(Drawing2D.GraphicsState state) { }

        public bool IsVisible(RectangleF rect) => true;
        public void ResetClip() { }
        public void SetClip(RectangleF rect) { }
        public void SetClip(RectangleF rect, Drawing2D.CombineMode mode) { }
        public void SetClip(Drawing2D.GraphicsPath path, Drawing2D.CombineMode mode) { }
        public void Clear(Color color) { }

        public IntPtr GetHdc() => IntPtr.Zero;
        public void ReleaseHdc(IntPtr hdc) { }
        public void Dispose() { }
    }
}