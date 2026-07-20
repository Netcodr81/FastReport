using System;
using System.ComponentModel;

using FastReport.Utils;

using SkiaSharp;

namespace FastReport.Gauge.Radial;

#region Enums

/// <summary>
/// Radial Gauge types
/// </summary>
[Flags]
public enum RadialGaugeType
{
    /// <summary>
    /// Full sized gauge
    /// </summary>
    Circle = 1,

    /// <summary>
    /// Half of the radial gauge
    /// </summary>
    Semicircle = 2,

    /// <summary>
    /// Quarter of the radial gauge
    /// </summary>
    Quadrant = 4
}

/// <summary>
/// Radial Gauge position types
/// </summary>
[Flags]
[TypeConverter(typeof(FastReport.TypeConverters.FlagConverter))]
public enum RadialGaugePosition
{
    /// <summary>
    /// None
    /// </summary>
    None = 0,

    /// <summary>
    /// Top
    /// </summary>
    Top = 1,

    /// <summary>
    /// Bottom
    /// </summary>
    Bottom = 2,

    /// <summary>
    /// Left
    /// </summary>
    Left = 4,

    /// <summary>
    /// Right
    /// </summary>
    Right = 8
}

#endregion // Enums

/// <summary>
/// Represents a linear gauge.
/// </summary>
public partial class RadialGauge : GaugeObject
{
    private const double RAD = Math.PI / 180.0;
    private SKPoint center;
    private RadialGaugeType type;
    private RadialGaugePosition position;
    private float semicircleOffsetRatio;

    #region Properties
    /// <inheritdoc/>
    public override float Width
    {
        get { return base.Width; }
        set
        {
            base.Width = value;
            if (base.Height != base.Width)
            {
                base.Height = Width;
            }
        }
    }

    /// <inheritdoc/>
    public override float Height
    {
        get { return base.Height; }
        set
        {
            base.Height = value;
            if (base.Width != base.Height)
            {
                base.Width = Height;
            }
        }
    }

    /// <summary>
    /// Returns centr of the gauge
    /// </summary>
    [Browsable(false)]
    public SKPoint Center
    {
        get { return center; }
        set { center = value; }
    }

    /// <summary>
    /// The number of radians in one degree
    /// </summary>
    public static double Radians
    {
        get { return RAD; }
    }

    /// <summary>
    /// Gets or sets the Radial Gauge type
    /// </summary>
    [Browsable(true)]
    [Category("Appearance")]
    public RadialGaugeType Type
    {
        get { return type; }
        set
        {
            if (value == RadialGaugeType.Circle)
            {
                position = RadialGaugePosition.None;
                type = value;
            }
            if (value == RadialGaugeType.Semicircle &&
                !(Position == RadialGaugePosition.Bottom ||
                Position == RadialGaugePosition.Left ||
                Position == RadialGaugePosition.Right ||
                Position == RadialGaugePosition.Top))
            {
                position = RadialGaugePosition.Top;
                type = value;
            }
            else if (value == RadialGaugeType.Quadrant &&
                !(
                ((Position & RadialGaugePosition.Left) != 0 && (Position & RadialGaugePosition.Top) != 0 &&
                (Position & RadialGaugePosition.Right) == 0 && (Position & RadialGaugePosition.Bottom) == 0) ||

                ((Position & RadialGaugePosition.Right) != 0 && (Position & RadialGaugePosition.Top) != 0 &&
                (Position & RadialGaugePosition.Left) == 0 && (Position & RadialGaugePosition.Bottom) == 0) ||

                ((Position & RadialGaugePosition.Left) != 0 && (Position & RadialGaugePosition.Bottom) != 0 &&
                (Position & RadialGaugePosition.Right) == 0 && (Position & RadialGaugePosition.Top) == 0) ||

                ((Position & RadialGaugePosition.Right) != 0 && (Position & RadialGaugePosition.Bottom) != 0 &&
                (Position & RadialGaugePosition.Left) == 0 && (Position & RadialGaugePosition.Top) == 0)
                ))
            {
                position = RadialGaugePosition.Top | RadialGaugePosition.Left;
                type = value;
            }

        }
    }

    /// <summary>
    /// Gats or sets the Radial Gauge position. Doesn't work for Full Radial Gauge.
    /// </summary>
    [Category("Appearance")]
    public RadialGaugePosition Position
    {
        get { return position; }
        set
        {
            if (Type == RadialGaugeType.Semicircle &&
                (value == RadialGaugePosition.Bottom ||
                value == RadialGaugePosition.Left ||
                value == RadialGaugePosition.Right ||
                value == RadialGaugePosition.Top))
                position = value;
            else if (Type == RadialGaugeType.Quadrant &&
                (
                ((value & RadialGaugePosition.Left) != 0 && (value & RadialGaugePosition.Top) != 0 &&
                (value & RadialGaugePosition.Right) == 0 && (value & RadialGaugePosition.Bottom) == 0) ||

                ((value & RadialGaugePosition.Right) != 0 && (value & RadialGaugePosition.Top) != 0 &&
                (value & RadialGaugePosition.Left) == 0 && (value & RadialGaugePosition.Bottom) == 0) ||

                ((value & RadialGaugePosition.Left) != 0 && (value & RadialGaugePosition.Bottom) != 0 &&
                (value & RadialGaugePosition.Right) == 0 && (value & RadialGaugePosition.Top) == 0) ||

                ((value & RadialGaugePosition.Right) != 0 && (value & RadialGaugePosition.Bottom) != 0 &&
                (value & RadialGaugePosition.Left) == 0 && (value & RadialGaugePosition.Top) == 0)
                ))
                position = value;
            else if (Type == RadialGaugeType.Circle)
                position = 0;

        }
    }

    /// <summary>
    /// Gets or sets the semicircles offset
    /// </summary>
    [Category("Appearance")]
    public float SemicircleOffsetRatio
    {
        get { return semicircleOffsetRatio; }
        set { semicircleOffsetRatio = value; }
    }
    #endregion // Properties

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RadialGauge"/> class.
    /// </summary>
    public RadialGauge() : base()
    {
        InitializeComponent();
        Scale = new RadialScale(this);
        Pointer = new RadialPointer(this, Scale as RadialScale);
        Label = new RadialLabel(this);
        Height = 4.0f * Units.Centimeters;
        Width = 4.0f * Units.Centimeters;
        semicircleOffsetRatio = type == RadialGaugeType.Semicircle &&
            (position == RadialGaugePosition.Left || position == RadialGaugePosition.Right) ? 1.5f : 1;
        Type = RadialGaugeType.Circle;
        Border.Lines = BorderLines.None;
    }

    #endregion // Constructor

    #region Public Methods

    /// <inheritdoc/>
    public override void Assign(Base source)
    {
        base.Assign(source);
        RadialGauge src = source as RadialGauge;
        Type = src.Type;
        Position = src.Position;
    }

    /// <inheritdoc/>
    public override void Draw(FRPaintEventArgs e)
    {
        IGraphics g = e.Graphics;

        if (Report != null && Report.SmoothGraphics)
        {
            g.SamplingOptions = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
        }

        float x = (AbsLeft + Border.Width / 2) * e.ScaleX;
        float y = (AbsTop + Border.Width / 2) * e.ScaleY;
        float dx = (Width - Border.Width) * e.ScaleX - 1;
        float dy = (Height - Border.Width) * e.ScaleY - 1;

        center = new SKPoint(x + dx / 2, y + dy / 2);

        using SKPaint pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = Border.Color,
            StrokeWidth = Border.Width * e.ScaleX,
            IsAntialias = true
        };

        using SKPaint brush = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = Export.ExportUtils.GetColorFromFill(Fill),
            IsAntialias = true
        };

        if (type == RadialGaugeType.Circle)
        {
            g.FillAndDrawEllipse(pen, brush, x, y, dx, dy);
        }
        else if (type == RadialGaugeType.Semicircle)
        {
            float semiOffset = (Width / 16f / 2f + 2f) * semicircleOffsetRatio * e.ScaleY;
            SKPoint[] points = new SKPoint[4];
            if (position == RadialGaugePosition.Top)
            {
                g.FillPie(brush, x, y, dx, dy, -180, 180);
                g.DrawArc(pen, x, y, dx, dy, -180, 180);

                SKPoint startPoint = new SKPoint(center.X + dx / 2, center.Y);

                points[0] = new SKPoint(startPoint.X, startPoint.Y - 1 * e.ScaleY);
                points[1] = new SKPoint(startPoint.X, startPoint.Y + semiOffset);
                points[2] = new SKPoint(startPoint.X + dx, startPoint.Y + semiOffset);
                points[3] = new SKPoint(startPoint.X + dx, startPoint.Y - 1 * e.ScaleY);
            }
            else if (position == RadialGaugePosition.Bottom)
            {
                g.FillPie(brush, x, y, dx, dy, 0, 180);
                g.DrawArc(pen, x, y, dx, dy, 0, 180);

                SKPoint startPoint = new SKPoint(center.X - dx / 2, center.Y);

                points[0] = new SKPoint(startPoint.X, startPoint.Y + 1 * e.ScaleY);
                points[1] = new SKPoint(startPoint.X, startPoint.Y - semiOffset);
                points[2] = new SKPoint(startPoint.X - dx, startPoint.Y - semiOffset);
                points[3] = new SKPoint(startPoint.X - dx, startPoint.Y + 1 * e.ScaleY);
            }
            else if (position == RadialGaugePosition.Left)
            {
                g.FillPie(brush, x, y, dx, dy, 90, 180);
                g.DrawArc(pen, x, y, dx, dy, 90, 180);

                SKPoint startPoint = new SKPoint(center.X, center.Y + dy / 2);

                points[0] = new SKPoint(startPoint.X - 1 * e.ScaleX, startPoint.Y);
                points[1] = new SKPoint(startPoint.X + semiOffset, startPoint.Y);
                points[2] = new SKPoint(startPoint.X + semiOffset, startPoint.Y - dy);
                points[3] = new SKPoint(startPoint.X - 1 * e.ScaleX, startPoint.Y - dy);
            }
            else if (position == RadialGaugePosition.Right)
            {
                g.FillPie(brush, x, y, dx, dy, -90, 180);
                g.DrawArc(pen, x, y, dx, dy, -90, 180);

                SKPoint startPoint = new SKPoint(center.X, center.Y + dy / 2);

                points[0] = new SKPoint(startPoint.X + 1 * e.ScaleX, startPoint.Y);
                points[1] = new SKPoint(startPoint.X - semiOffset, startPoint.Y);
                points[2] = new SKPoint(startPoint.X - semiOffset, startPoint.Y - dy);
                points[3] = new SKPoint(startPoint.X + 1 * e.ScaleX, startPoint.Y - dy);
            }

            if (position != RadialGaugePosition.None)
            {
                g.FillAndDrawPolygon(pen, brush, points);
            }
        }
        else if (type == RadialGaugeType.Quadrant)
        {
            float semiOffset = (Width / 16f / 2f + 2f) * semicircleOffsetRatio * e.ScaleY;
            if (RadialUtils.IsTop(this) && RadialUtils.IsLeft(this))
            {
                g.FillPie(brush, x, y, dx, dy, -180, 90);
                g.DrawArc(pen, x, y, dx, dy, -180, 90);

                SKPoint startPoint = new SKPoint(center.X + dx / 2, center.Y);

                SKPoint[] points = new SKPoint[5];
                points[0] = new SKPoint(startPoint.X, startPoint.Y - 1 * e.ScaleY);
                points[1] = new SKPoint(startPoint.X, startPoint.Y + semiOffset);
                points[2] = new SKPoint(startPoint.X + dx / 2 + semiOffset, startPoint.Y + semiOffset);
                points[3] = new SKPoint(startPoint.X + dx / 2 + semiOffset, y);
                points[4] = new SKPoint(startPoint.X + dx / 2 - 1 * e.ScaleX, y);
                g.FillAndDrawPolygon(pen, brush, points);
            }
            else if (RadialUtils.IsBottom(this) && RadialUtils.IsLeft(this))
            {
                g.FillPie(brush, x, y, dx, dy, -270, 90);
                g.DrawArc(pen, x, y, dx, dy, -270, 90);

                SKPoint startPoint = new SKPoint(center.X + dx / 2, center.Y);
                SKPoint[] points = new SKPoint[5];
                points[0] = new SKPoint(startPoint.X, startPoint.Y + 1 * e.ScaleY);
                points[1] = new SKPoint(startPoint.X, startPoint.Y - semiOffset);
                points[2] = new SKPoint(startPoint.X + dx / 2 + semiOffset, startPoint.Y - semiOffset);
                points[3] = new SKPoint(startPoint.X + dx / 2 + semiOffset, y + dy);
                points[4] = new SKPoint(x + dx / 2 - 1 * e.ScaleX, y + dy);
                g.FillAndDrawPolygon(pen, brush, points);
            }
            else if (RadialUtils.IsTop(this) && RadialUtils.IsRight(this))
            {
                g.FillPie(brush, x, y, dx, dy, -90, 90);
                g.DrawArc(pen, x, y, dx, dy, -90, 90);

                SKPoint startPoint = new SKPoint(center.X - dx / 2, center.Y);

                SKPoint[] points = new SKPoint[5];
                points[0] = new SKPoint(startPoint.X, startPoint.Y - 1 * e.ScaleY);
                points[1] = new SKPoint(startPoint.X, startPoint.Y + semiOffset);
                points[2] = new SKPoint(startPoint.X - dx / 2 - semiOffset, startPoint.Y + semiOffset);
                points[3] = new SKPoint(x + dx / 2 - semiOffset, y);
                points[4] = new SKPoint(x + dx / 2 + 1 * e.ScaleX, y);
                g.FillAndDrawPolygon(pen, brush, points);
            }
            else if (RadialUtils.IsBottom(this) && RadialUtils.IsRight(this))
            {
                g.FillPie(brush, x, y, dx, dy, 0, 90);
                g.DrawArc(pen, x, y, dx, dy, 0, 90);

                SKPoint startPoint = new SKPoint(center.X - dx / 2, center.Y);

                SKPoint[] points = new SKPoint[5];
                points[0] = new SKPoint(startPoint.X, startPoint.Y + 1 * e.ScaleY);
                points[1] = new SKPoint(startPoint.X, startPoint.Y - semiOffset);
                points[2] = new SKPoint(x + dx / 2 - semiOffset, startPoint.Y - semiOffset);
                points[3] = new SKPoint(x + dx / 2 - semiOffset, y + dy);
                points[4] = new SKPoint(x + dx / 2 + 1 * e.ScaleX, y + dy);
                g.FillAndDrawPolygon(pen, brush, points);
            }
        }

        Scale.Draw(e);
        Pointer.Draw(e);
        Label.Draw(e);
        DrawMarkers(e);
    }

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
        RadialGauge c = writer.DiffObject as RadialGauge;
        base.Serialize(writer);
        if (Type != c.Type)
        {
            writer.WriteValue("Type", Type);
        }
        if (Position != c.Position)
        {
            writer.WriteValue("Position", Position);
        }
    }

    #endregion // Public Methods
}