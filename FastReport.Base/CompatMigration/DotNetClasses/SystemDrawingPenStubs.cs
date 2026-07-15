using System;

namespace System.Drawing
{
    [Flags]
    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8
    }

    public class FontFamily
    {
        public static FontFamily GenericSansSerif { get; } = new FontFamily("SansSerif");
        public static FontFamily[] Families { get; } = new[] { GenericSansSerif };
        public string Name { get; }
        public FontFamily(string name) => Name = name ?? string.Empty;
        public int GetLineSpacing(FontStyle style) => 1000;
        public int GetCellDescent(FontStyle style) => 200;
        public int GetCellAscent(FontStyle style) => 800;
        public int GetEmHeight(FontStyle style) => 1000;
    }

    public enum GraphicsUnit
    {
        World = 0,
        Display = 1,
        Pixel = 2,
        Point = 3,
        Inch = 4,
        Document = 5,
        Millimeter = 6
    }

    public class Font : IDisposable
    {
        public FontFamily FontFamily { get; }
        public float Size { get; }
        public FontStyle Style { get; }
        public GraphicsUnit Unit { get; }
        public byte GdiCharSet { get; }
        public bool GdiVerticalFont { get; }
        public string Name => FontFamily?.Name ?? string.Empty;
        public int Height => (int)Math.Ceiling(Size);
        public bool Bold => (Style & FontStyle.Bold) != 0;
        public bool Italic => (Style & FontStyle.Italic) != 0;
        public bool Underline => (Style & FontStyle.Underline) != 0;

        public Font(string familyName, float emSize)
            : this(new FontFamily(familyName), emSize, FontStyle.Regular, GraphicsUnit.Point) { }

        public Font(string familyName, float emSize, FontStyle style)
            : this(new FontFamily(familyName), emSize, style, GraphicsUnit.Point) { }

        public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit)
            : this(new FontFamily(familyName), emSize, style, unit) { }

        public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
            : this(new FontFamily(familyName), emSize, style, unit, gdiCharSet, gdiVerticalFont) { }

        public Font(FontFamily family, float emSize, FontStyle style = FontStyle.Regular, GraphicsUnit unit = GraphicsUnit.Point)
            : this(family, emSize, style, unit, 1, false)
        {
        }

        public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont)
        {
            FontFamily = family ?? FontFamily.GenericSansSerif;
            Size = emSize;
            Style = style;
            Unit = unit;
            GdiCharSet = gdiCharSet;
            GdiVerticalFont = gdiVerticalFont;
        }

        public Font(Font prototype, FontStyle style)
            : this(prototype?.FontFamily ?? FontFamily.GenericSansSerif,
                  prototype?.Size ?? 10f,
                  style,
                  prototype?.Unit ?? GraphicsUnit.Point,
                  prototype?.GdiCharSet ?? 1,
                  prototype?.GdiVerticalFont ?? false) { }

        public float GetHeight() => Size;
        public float GetHeight(Graphics g) => Size;
        public void Dispose() { }
    }

    public class Pen : IDisposable
    {
        public Color Color { get; set; }
        public float Width { get; set; }
        public Drawing2D.DashStyle DashStyle { get; set; } = Drawing2D.DashStyle.Solid;
        public Drawing2D.LineJoin LineJoin { get; set; } = Drawing2D.LineJoin.Miter;
        public Drawing2D.LineCap StartCap { get; set; } = Drawing2D.LineCap.Flat;
        public Drawing2D.LineCap EndCap { get; set; } = Drawing2D.LineCap.Flat;
        public float DashOffset { get; set; }
        public float[] DashPattern { get; set; } = Array.Empty<float>();

        public Pen(Color color, float width = 1f)
        {
            Color = color;
            Width = width;
        }

        public Pen(Brush brush, float width = 1f)
            : this(brush is SolidBrush sb ? sb.Color : Color.Black, width) { }

        public void Dispose() { }
    }

    public static class Pens
    {
        public static Pen Black { get; } = new Pen(Color.Black);
        public static Pen Red { get; } = new Pen(Color.Red);
        public static Pen Silver { get; } = new Pen(Color.Silver);
    }

    [Flags]
    public enum ContentAlignment
    {
        TopLeft = 1,
        TopCenter = 2,
        TopRight = 4,
        MiddleLeft = 16,
        MiddleCenter = 32,
        MiddleRight = 64,
        BottomLeft = 256,
        BottomCenter = 512,
        BottomRight = 1024
    }
}

namespace System.Drawing.Drawing2D
{
    public enum DashStyle { Solid = 0, Dash = 1, Dot = 2, DashDot = 3, DashDotDot = 4, Custom = 5 }
    public enum LineJoin { Miter = 0, Bevel = 1, Round = 2, MiterClipped = 3 }
    public enum LineCap { Flat = 0, Square = 1, Round = 2, Triangle = 3 }
}
