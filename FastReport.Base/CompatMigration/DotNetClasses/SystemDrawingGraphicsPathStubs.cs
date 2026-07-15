using System;

namespace System.Drawing.Drawing2D
{
    public enum MatrixOrder { Prepend = 0, Append = 1 }
    public enum PathPointType { Start = 0, Line = 1, Bezier = 3, Bezier3 = 3, CloseSubpath = 128 }
    public enum CombineMode { Replace = 0, Intersect = 1, Union = 2, Xor = 3, Exclude = 4, Complement = 5 }
    public enum SmoothingMode { Invalid = -1, Default = 0, HighSpeed = 1, HighQuality = 2, None = 3, AntiAlias = 4 }
    public enum InterpolationMode { Invalid = -1, Default = 0, Low = 1, High = 2, Bilinear = 3, Bicubic = 4, NearestNeighbor = 5, HighQualityBilinear = 6, HighQualityBicubic = 7 }
    public enum CompositingQuality { Invalid = -1, Default = 0, HighSpeed = 1, HighQuality = 2, GammaCorrected = 3, AssumeLinear = 4 }
    public enum PixelOffsetMode { Invalid = -1, Default = 0, HighSpeed = 1, HighQuality = 2 }
    public enum CompositingMode { SourceOver = 0, SourceCopy = 1 }
    public enum WrapMode { Tile = 0, TileFlipX = 1, TileFlipY = 2, TileFlipXY = 3, Clamp = 4 }

    public sealed class GraphicsState { }

    public class Matrix : IDisposable
    {
        public float[] Elements => new[] { 1f, 0f, 0f, 1f, OffsetX, OffsetY };
        public float OffsetX { get; private set; }
        public float OffsetY { get; private set; }

        public Matrix()
        {
        }

        public Matrix(float m11, float m12, float m21, float m22, float dx, float dy)
        {
            OffsetX = dx;
            OffsetY = dy;
        }

        public void Translate(float dx, float dy)
        {
            OffsetX += dx;
            OffsetY += dy;
        }

        public void Scale(float sx, float sy)
        {
        }

        public void Rotate(float angle)
        {
        }

        public void Dispose() { }
    }

    public class GraphicsPath : IDisposable
    {
        public System.Drawing.PointF[] PathPoints => Array.Empty<System.Drawing.PointF>();
        public byte[] PathTypes => Array.Empty<byte>();
        public int PointCount => PathPoints.Length;

        public GraphicsPath()
        {
        }

        public GraphicsPath(System.Drawing.PointF[] points, byte[] types)
        {
        }

        public void StartFigure() { }
        public void AddEllipse(float x, float y, float width, float height) { }
        public void AddEllipse(System.Drawing.RectangleF rect) { }
        public void AddRectangle(System.Drawing.RectangleF rect) { }
        public void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle) { }
        public void AddLine(float x1, float y1, float x2, float y2) { }
        public void AddLine(System.Drawing.PointF pt1, System.Drawing.PointF pt2) { }
        public void AddLines(System.Drawing.PointF[] points) { }
        public void AddPolygon(System.Drawing.PointF[] points) { }
        public void AddPath(GraphicsPath path, bool connect) { }
        public void Transform(Matrix matrix) { }
        public System.Drawing.RectangleF GetBounds() => System.Drawing.RectangleF.Empty;
        public void AddString(string s, System.Drawing.FontFamily family, int style, float emSize, System.Drawing.PointF origin, System.Drawing.StringFormat format) { }
        public void AddString(string s, System.Drawing.FontFamily family, int style, float emSize, System.Drawing.RectangleF layoutRect, System.Drawing.StringFormat format) { }
        public void CloseFigure() { }
        public void CloseAllFigures() { }
        public void Dispose() { }
    }

    public class LinearGradientBrush : System.Drawing.Brush
    {
        public LinearGradientBrush(System.Drawing.RectangleF rect, System.Drawing.Color color1, System.Drawing.Color color2, int angle) { }
        public void SetSigmaBellShape(float focus, float scale) { }
    }

    public class PathGradientBrush : System.Drawing.Brush
    {
        public PathGradientBrush(GraphicsPath path) { }
        public System.Drawing.Color CenterColor { get; set; }
        public System.Drawing.Color[] SurroundColors { get; set; } = Array.Empty<System.Drawing.Color>();
    }
}