using System;
using System.Drawing;

namespace FastReport.Rendering
{
    /// <summary>
    /// Abstracts graphics rendering operations for cross-platform compatibility.
    /// Provides a unified interface for rendering via different backends (System.Drawing, SkiaSharp, etc.).
    /// </summary>
    public interface IGraphicsProvider : IDisposable
    {
        /// <summary>
        /// Gets the current DPI scale factor (96 = 100%, 120 = 125%, etc.)
        /// </summary>
        float DpiScale { get; }

        #region Canvas Operations

        /// <summary>
        /// Clears the canvas with the specified color.
        /// </summary>
        void Clear(IColor color);

        /// <summary>
        /// Saves the current graphics state.
        /// </summary>
        void Save();

        /// <summary>
        /// Restores the previously saved graphics state.
        /// </summary>
        void Restore();

        #endregion

        #region Transformations

        /// <summary>
        /// Translates the graphics coordinate system by the specified amounts.
        /// </summary>
        void TranslateTransform(float dx, float dy);

        /// <summary>
        /// Rotates the graphics coordinate system by the specified angle in degrees.
        /// </summary>
        void RotateTransform(float angle);

        /// <summary>
        /// Rotates the graphics coordinate system by the specified angle in degrees around a point.
        /// </summary>
        void RotateTransform(float angle, float centerX, float centerY);

        /// <summary>
        /// Scales the graphics coordinate system by the specified factors.
        /// </summary>
        void ScaleTransform(float scaleX, float scaleY);

        /// <summary>
        /// Resets the graphics coordinate system to identity.
        /// </summary>
        void ResetTransform();

        #endregion

        #region Text Drawing

        /// <summary>
        /// Draws text at the specified location using the specified font and brush.
        /// </summary>
        void DrawText(string text, IFont font, IBrush brush, float x, float y, IStringFormat format = null);

        /// <summary>
        /// Draws text within the specified rectangle.
        /// </summary>
        void DrawText(string text, IFont font, IBrush brush, float x, float y, float width, float height, IStringFormat format = null);

        /// <summary>
        /// Measures the size of the specified text.
        /// </summary>
        SizeF MeasureText(string text, IFont font);

        #endregion

        #region Lines

        /// <summary>
        /// Draws a line between two points.
        /// </summary>
        void DrawLine(IPen pen, float x1, float y1, float x2, float y2);

        #endregion

        #region Rectangles

        /// <summary>
        /// Draws the outline of a rectangle.
        /// </summary>
        void DrawRectangle(IPen pen, float x, float y, float width, float height);

        /// <summary>
        /// Fills the interior of a rectangle.
        /// </summary>
        void FillRectangle(IBrush brush, float x, float y, float width, float height);

        /// <summary>
        /// Draws the outline of a rounded rectangle.
        /// </summary>
        void DrawRoundedRectangle(IPen pen, float x, float y, float width, float height, float cornerRadius);

        /// <summary>
        /// Fills the interior of a rounded rectangle.
        /// </summary>
        void FillRoundedRectangle(IBrush brush, float x, float y, float width, float height, float cornerRadius);

        #endregion

        #region Ellipses

        /// <summary>
        /// Draws the outline of an ellipse.
        /// </summary>
        void DrawEllipse(IPen pen, float x, float y, float width, float height);

        /// <summary>
        /// Fills the interior of an ellipse.
        /// </summary>
        void FillEllipse(IBrush brush, float x, float y, float width, float height);

        #endregion

        #region Arcs

        /// <summary>
        /// Draws an arc representing a portion of an ellipse.
        /// </summary>
        void DrawArc(IPen pen, float x, float y, float width, float height, float startAngle, float sweepAngle);

        #endregion

        #region Paths

        /// <summary>
        /// Creates a new graphics path.
        /// </summary>
        IGraphicsPath CreatePath();

        /// <summary>
        /// Draws the outline of a path.
        /// </summary>
        void DrawPath(IPen pen, IGraphicsPath path);

        /// <summary>
        /// Fills the interior of a path.
        /// </summary>
        void FillPath(IBrush brush, IGraphicsPath path);

        #endregion

        #region Images

        /// <summary>
        /// Draws an image at the specified location.
        /// </summary>
        void DrawImage(IImage image, float x, float y);

        /// <summary>
        /// Draws an image at the specified location with the specified size.
        /// </summary>
        void DrawImage(IImage image, float x, float y, float width, float height);

        /// <summary>
        /// Draws a portion of an image at the specified location and size.
        /// </summary>
        void DrawImage(IImage image, float destX, float destY, float destWidth, float destHeight,
                      float srcX, float srcY, float srcWidth, float srcHeight);

        #endregion

        #region Clipping

        /// <summary>
        /// Sets the clipping region to the specified rectangle.
        /// </summary>
        void SetClipRectangle(float x, float y, float width, float height);

        /// <summary>
        /// Resets the clipping region to the entire canvas.
        /// </summary>
        void ResetClip();

        #endregion
    }

    /// <summary>
    /// Represents a font for text rendering.
    /// </summary>
    public interface IFont : IDisposable
    {
        string Name { get; }
        float Size { get; }
        FontStyle Style { get; }
    }

    /// <summary>
    /// Represents a brush for filling shapes.
    /// </summary>
    public interface IBrush : IDisposable
    {
    }

    /// <summary>
    /// Represents a pen for drawing outlines.
    /// </summary>
    public interface IPen : IDisposable
    {
        float Width { get; set; }
        IColor Color { get; set; }
        DashStyle DashStyle { get; set; }
    }

    /// <summary>
    /// Represents a color with ARGB values.
    /// </summary>
    public interface IColor
    {
        byte A { get; }
        byte R { get; }
        byte G { get; }
        byte B { get; }
    }

    /// <summary>
    /// Represents an image for rendering operations.
    /// </summary>
    public interface IImage : IDisposable
    {
        int Width { get; }
        int Height { get; }
    }

    /// <summary>
    /// Represents a graphics path for drawing complex shapes.
    /// </summary>
    public interface IGraphicsPath : IDisposable
    {
        /// <summary>
        /// Clears all figures from the path.
        /// </summary>
        void Reset();

        /// <summary>
        /// Starts a new figure at the specified point.
        /// </summary>
        void MoveTo(float x, float y);

        /// <summary>
        /// Draws a line from the current point to the specified point.
        /// </summary>
        void LineTo(float x, float y);

        /// <summary>
        /// Draws a cubic bezier curve from the current point.
        /// </summary>
        void CubicTo(float x1, float y1, float x2, float y2, float x3, float y3);

        /// <summary>
        /// Draws a quadratic bezier curve from the current point.
        /// </summary>
        void QuadTo(float x1, float y1, float x2, float y2);

        /// <summary>
        /// Adds a rectangle to the path.
        /// </summary>
        void AddRectangle(float x, float y, float width, float height);

        /// <summary>
        /// Adds an ellipse to the path.
        /// </summary>
        void AddEllipse(float x, float y, float width, float height);

        /// <summary>
        /// Adds an arc to the path.
        /// </summary>
        void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle);

        /// <summary>
        /// Closes the current figure and starts a new one.
        /// </summary>
        void CloseFigure();

        /// <summary>
        /// Gets a value indicating whether the path is empty.
        /// </summary>
        bool IsEmpty { get; }
    }

    /// <summary>
    /// Represents text formatting options.
    /// </summary>
    public interface IStringFormat : IDisposable
    {
        StringAlignment Alignment { get; set; }
        StringAlignment LineAlignment { get; set; }
        StringFormatFlags FormatFlags { get; set; }
        StringTrimming Trimming { get; set; }
    }

    #region Supporting Enums

    /// <summary>
    /// Specifies the style of a font.
    /// </summary>
    [Flags]
    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8
    }

    /// <summary>
    /// Specifies the style of a dash line.
    /// </summary>
    public enum DashStyle
    {
        Solid = 0,
        Dash = 1,
        Dot = 2,
        DashDot = 3,
        DashDotDot = 4
    }

    /// <summary>
    /// Specifies text alignment relative to a point or rectangle.
    /// </summary>
    public enum StringAlignment
    {
        Near = 0,
        Center = 1,
        Far = 2
    }

    /// <summary>
    /// Specifies text trimming behavior.
    /// </summary>
    public enum StringTrimming
    {
        None = 0,
        Character = 1,
        Word = 2,
        EllipsisCharacter = 3,
        EllipsisWord = 4,
        EllipsisPath = 5
    }

    /// <summary>
    /// Specifies format flags for text rendering.
    /// </summary>
    [Flags]
    public enum StringFormatFlags
    {
        DirectionRightToLeft = 1,
        DirectionVertical = 2,
        NoClip = 4,
        DisplayFormatControl = 32,
        NoFontFallback = 1024,
        MeasureTrailingSpaces = 2048
    }

    #endregion
}
