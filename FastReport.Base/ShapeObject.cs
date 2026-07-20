using System;
using System.ComponentModel;

using FastReport.Utils;

using SkiaSharp;

namespace FastReport;

/// <summary>
/// Specifies a kind of the shape.
/// </summary>
public enum ShapeKind
{
    /// <summary>
    /// Specifies a rectangle shape.
    /// </summary>
    Rectangle,

    /// <summary>
    /// Specifies a round rectangle shape.
    /// </summary>
    RoundRectangle,

    /// <summary>
    /// Specifies an ellipse shape.
    /// </summary>
    Ellipse,

    /// <summary>
    /// Specifies a triangle shape.
    /// </summary>
    Triangle,

    /// <summary>
    /// Specifies a diamond shape.
    /// </summary>
    Diamond
}

/// <summary>
/// Represents a shape object.
/// </summary>
/// <remarks>
/// Use the <see cref="ShapeKind"/> property to specify a shape. To set the width, style and color of the
/// shape's border, use the <b>Border.Width</b>, <b>Border.Style</b> and <b>Border.Color</b> properties.
/// </remarks>
public partial class ShapeObject : ReportComponentBase
{
    #region Fields
    private ShapeKind shape;
    private float curve;
    private FloatCollection dashPattern;
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets collection of values for custom dash pattern.
    /// </summary>
    /// <remarks>
    /// Each element should be a non-zero positive number. 
    /// If the number is negative or zero, that number is replaced by one.
    /// </remarks>
    [Category("Appearance")]
    public FloatCollection DashPattern
    {
        get { return dashPattern; }
        set { dashPattern = value; }
    }

    /// <summary>
    /// Gets or sets a shape kind.
    /// </summary>
    [DefaultValue(ShapeKind.Rectangle)]
    [Category("Appearance")]
    public ShapeKind Shape
    {
        get { return shape; }
        set { shape = value; }
    }

    /// <summary>
    /// Gets or sets a shape curvature if <see cref="ShapeKind"/> is <b>RoundRectangle</b>.
    /// </summary>
    /// <remarks>
    /// 0 value means automatic curvature.
    /// </remarks>
    [DefaultValue(0f)]
    [Category("Appearance")]
    public float Curve
    {
        get { return curve; }
        set { curve = value; }
    }

    #endregion

    #region Private Methods
    private SKPath GetRoundRectPath(float x, float y, float x1, float y1, float radius)
    {
        SKPath gp = new SKPath();
        if (radius < 1)
            radius = 1;
        gp.AddArc(new SKRect(x1 - radius, y, x1, y + radius), 270, 90);
        gp.AddArc(new SKRect(x1 - radius, y1 - radius, x1, y1), 0, 90);
        gp.AddArc(new SKRect(x, y1 - radius, x + radius, y1), 90, 90);
        gp.AddArc(new SKRect(x, y, x + radius, y + radius), 180, 90);
        gp.Close();
        return gp;
    }
    #endregion

    #region Public Methods
    /// <inheritdoc/>
    public override void Assign(Base source)
    {
        base.Assign(source);

        ShapeObject src = source as ShapeObject;
        Shape = src.Shape;
        Curve = src.Curve;
        DashPattern.Assign(src.DashPattern);
    }

    /// <inheritdoc/>
    public override void Draw(FRPaintEventArgs e)
    {
        if (Math.Abs(Width) < 1 || Math.Abs(Height) < 1)
            return;

        IGraphics g = e.Graphics;
        float x = (AbsLeft + Border.Width / 2) * e.ScaleX;
        float y = (AbsTop + Border.Width / 2) * e.ScaleY;
        float dx = (Width - Border.Width) * e.ScaleX - 1;
        float dy = (Height - Border.Width) * e.ScaleY - 1;
        float x1 = x + dx;
        float y1 = y + dy;

        bool smooth = Report != null && Report.SmoothGraphics && Shape != ShapeKind.Rectangle;
        float strokeWidth = Border.Width * e.ScaleX;

        using SKPaint pen = new SKPaint
        {
            Color = Border.Color,
            StrokeWidth = strokeWidth,
            Style = SKPaintStyle.Stroke,
            IsAntialias = smooth
        };

        if (DashPattern?.Count > 0)
        {
            float[] intervals = new float[DashPattern.Count];
            for (int i = 0; i < DashPattern.Count; i++)
                intervals[i] = (DashPattern[i] <= 0 ? 1 : DashPattern[i]) * strokeWidth;
            pen.PathEffect = SKPathEffect.CreateDash(intervals, 0);
        }
        else
        {
            float w = strokeWidth;
            switch (Border.DashStyle)
            {
                case DashStyle.Dash:
                    pen.PathEffect = SKPathEffect.CreateDash(new[] { 3 * w, 1 * w }, 0);
                    break;
                case DashStyle.Dot:
                    pen.PathEffect = SKPathEffect.CreateDash(new[] { 1 * w, 1 * w }, 0);
                    break;
                case DashStyle.DashDot:
                    pen.PathEffect = SKPathEffect.CreateDash(new[] { 3 * w, 1 * w, 1 * w, 1 * w }, 0);
                    break;
                case DashStyle.DashDotDot:
                    pen.PathEffect = SKPathEffect.CreateDash(new[] { 3 * w, 1 * w, 1 * w, 1 * w, 1 * w, 1 * w }, 0);
                    break;
            }
        }

        SKColor fillColor;
        if (Fill is SolidFill solidFill)
            fillColor = solidFill.Color;
        else
        {
            FastReport.Brush fb = Fill.CreateBrush(new SKRect(x, y, x + dx, y + dy), e.ScaleX, e.ScaleY);
            fillColor = (fb as SolidBrush)?.Color ?? SKColors.Transparent;
            fb.Dispose();
        }

        using SKPaint brush = new SKPaint
        {
            Color = fillColor,
            Style = SKPaintStyle.Fill,
            IsAntialias = smooth
        };

        switch (Shape)
        {
            case ShapeKind.Rectangle:
                g.FillAndDrawRectangle(pen, brush, x, y, dx, dy);
                break;

            case ShapeKind.RoundRectangle:
                float min = Math.Min(dx, dy);
                if (curve == 0)
                    min = min / 4;
                else
                    min = Math.Min(min, curve * e.ScaleX * 10);
                using (SKPath gp = GetRoundRectPath(x, y, x1, y1, min))
                    g.FillAndDrawPath(pen, brush, gp);
                break;

            case ShapeKind.Ellipse:
                g.FillAndDrawEllipse(pen, brush, x, y, dx, dy);
                break;

            case ShapeKind.Triangle:
                SKPoint[] triPoints = {
        new SKPoint(x1, y1), new SKPoint(x, y1), new SKPoint(x + dx / 2, y), new SKPoint(x1, y1) };
                g.FillAndDrawPolygon(pen, brush, triPoints);
                break;

            case ShapeKind.Diamond:
                SKPoint[] diaPoints = {
        new SKPoint(x + dx / 2, y), new SKPoint(x1, y + dy / 2), new SKPoint(x + dx / 2, y1),
        new SKPoint(x, y + dy / 2) };
                g.FillAndDrawPolygon(pen, brush, diaPoints);
                break;
        }

        DrawDesign(e);
    }

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
        Border.SimpleBorder = true;
        base.Serialize(writer);
        ShapeObject c = writer.DiffObject as ShapeObject;

        if (Shape != c.Shape)
            writer.WriteValue("Shape", Shape);
        if (Curve != c.Curve)
            writer.WriteFloat("Curve", Curve);
        if (DashPattern?.Count > 0)
            writer.WriteValue("DashPattern", DashPattern);
    }
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ShapeObject"/> class with default settings.
    /// </summary>
    public ShapeObject()
    {
        shape = ShapeKind.Rectangle;
        FlagSimpleBorder = true;
        dashPattern = new FloatCollection();
    }
}