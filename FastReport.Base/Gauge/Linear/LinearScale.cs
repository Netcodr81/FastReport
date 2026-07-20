using System;
using System.ComponentModel;

using FastReport.Utils;

using SkiaSharp;

namespace FastReport.Gauge.Linear;

/// <summary>
/// Represents a linear scale.
/// </summary>
#if !DEBUG
[DesignTimeVisible(false)]
#endif
public class LinearScale : GaugeScale
{
    #region Fields

    private float left;
    private float top;
    private float height;
    private float width;
    private int majorTicksNum;

    #endregion // Fields

    #region Properties



    #endregion // Properties

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearScale"/> class.
    /// </summary>
    /// <param name="parent">The parent gauge object.</param>
    public LinearScale(GaugeObject parent) : base(parent)
    {
        MajorTicks = new ScaleTicks(10, 2, SKColors.Black);
        MinorTicks = new ScaleTicks(6, 1, SKColors.Black);
        majorTicksNum = 6;
    }

    #endregion // Constructors

    #region Private Methods

    private SKFont CreateFont(float scaleX)
    {
        float size = Parent.IsPrinting ? Font.Size : Font.Size * scaleX * 96f / DrawUtils.ScreenDpi;
        SKTypeface typeface = Font?.Typeface ?? SKTypeface.Default;
        SKFontStyle style = new SKFontStyle(typeface.FontWeight, typeface.FontWidth, typeface.FontSlant);
        return new SKFont(SKTypeface.FromFamilyName(typeface.FamilyName, style) ?? SKTypeface.Default, size);
    }

    private SKPaint CreateTextPaint()
    {
        return new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = Export.ExportUtils.GetColorFromFill(TextFill),
            IsAntialias = true
        };
    }

    private void DrawMajorTicksHorz(FRPaintEventArgs e)
    {
        IGraphics g = e.Graphics;
        using SKPaint pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = MajorTicks.Color,
            StrokeWidth = MajorTicks.Width * e.ScaleX,
            IsAntialias = true
        };
        using SKPaint brush = CreateTextPaint();
        using SKFont font = CreateFont(e.ScaleX);

        float x = left;
        float y1 = top;
        float y2 = top + height;
        float step = width / (majorTicksNum - 1);
        int textStep = (int)((Parent.Maximum - Parent.Minimum) / (majorTicksNum - 1));
        string text = Parent.Minimum.ToString();
        float y3 = y1 - 0.4f * Units.Centimeters * e.ScaleY;
        if ((Parent as LinearGauge).Inverted)
        {
            y3 = y2 - g.MeasureString(text, font).Height + 0.4f * Units.Centimeters * e.ScaleY;
        }
        for (int i = 0; i < majorTicksNum; i++)
        {
            g.DrawLine(pen, x, y1, x, y2);
            SKSize strSize = g.MeasureString(text, font);
            g.DrawString(text, font, brush, x - strSize.Width / 2 * e.ScaleX / (DrawUtils.ScreenDpi / 96f), y3);
            text = Convert.ToString(textStep * (i + 1) + Parent.Minimum);
            x += step;
        }
    }

    private void DrawMinorTicksHorz(FRPaintEventArgs e)
    {
        IGraphics g = e.Graphics;
        using SKPaint pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = MinorTicks.Color,
            StrokeWidth = MinorTicks.Width * e.ScaleX,
            IsAntialias = true
        };
        float x = left;
        float y1 = top + height * 0.2f;
        float y2 = top + height - height * 0.2f;
        float step = width / (majorTicksNum - 1) / 4;
        for (int i = 0; i < majorTicksNum - 1; i++)
        {
            x += step;
            for (int j = 0; j < 3; j++)
            {
                g.DrawLine(pen, x, y1, x, y2);
                x += step;
            }
        }
    }

    private void DrawMajorTicksVert(FRPaintEventArgs e)
    {
        IGraphics g = e.Graphics;
        using SKPaint pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = MajorTicks.Color,
            StrokeWidth = MajorTicks.Width * e.ScaleX,
            IsAntialias = true
        };
        using SKPaint brush = CreateTextPaint();
        using SKFont font = CreateFont(e.ScaleX);

        float y = top + height;
        float x1 = left;
        float x2 = left + width;
        float step = height / (majorTicksNum - 1);
        int textStep = (int)((Parent.Maximum - Parent.Minimum) / (majorTicksNum - 1));
        string text = Parent.Minimum.ToString();
        for (int i = 0; i < majorTicksNum; i++)
        {
            g.DrawLine(pen, x1, y, x2, y);
            SKSize strSize = g.MeasureString(text, font);
            float x3 = x1 - strSize.Width * e.ScaleX / (DrawUtils.ScreenDpi / 96f) - 0.04f * Units.Centimeters * e.ScaleX;
            if ((Parent as LinearGauge).Inverted)
            {
                x3 = x2 + 0.04f * Units.Centimeters * e.ScaleX;
            }
            g.DrawString(text, font, brush, x3, y - strSize.Height / 2 * e.ScaleY / (DrawUtils.ScreenDpi / 96f));
            text = Convert.ToString(textStep * (i + 1) + Parent.Minimum);
            y -= step;
        }
    }

    private void DrawMinorTicksVert(FRPaintEventArgs e)
    {
        IGraphics g = e.Graphics;
        using SKPaint pen = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = MinorTicks.Color,
            StrokeWidth = MinorTicks.Width * e.ScaleX,
            IsAntialias = true
        };
        float y = top + height;
        float x1 = left + width * 0.2f;
        float x2 = left + width - width * 0.2f;
        float step = height / (majorTicksNum - 1) / 4;
        for (int i = 0; i < majorTicksNum - 1; i++)
        {
            y -= step;
            for (int j = 0; j < 3; j++)
            {
                g.DrawLine(pen, x1, y, x2, y);
                y -= step;
            }
        }
    }

    #endregion // Private Methods

    #region Public Methods

    /// <inheritdoc/>
    public override void Assign(GaugeScale src)
    {
        base.Assign(src);

        LinearScale s = src as LinearScale;
        MajorTicks.Assign(s.MajorTicks);
        MinorTicks.Assign(s.MinorTicks);
    }

    /// <inheritdoc/>
    public override void Draw(FRPaintEventArgs e)
    {
        base.Draw(e);

        if (Parent.Vertical)
        {
            left = (Parent.AbsLeft + 0.7f * Units.Centimeters) * e.ScaleX;
            top = (Parent.AbsTop + 0.5f * Units.Centimeters) * e.ScaleY;
            height = (Parent.Height - 1.0f * Units.Centimeters) * e.ScaleY;
            width = (Parent.Width - 1.4f * Units.Centimeters) * e.ScaleX;

            DrawMajorTicksVert(e);
            DrawMinorTicksVert(e);
        }
        else
        {
            left = (Parent.AbsLeft + 0.5f * Units.Centimeters) * e.ScaleX;
            top = (Parent.AbsTop + 0.6f * Units.Centimeters) * e.ScaleY;
            height = (Parent.Height - 1.2f * Units.Centimeters) * e.ScaleY;
            width = (Parent.Width - 1.0f * Units.Centimeters) * e.ScaleX;

            DrawMajorTicksHorz(e);
            DrawMinorTicksHorz(e);
        }
    }

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer, string prefix, GaugeScale diff)
    {
        base.Serialize(writer, prefix, diff);

        LinearScale dc = diff as LinearScale;
        MajorTicks.Serialize(writer, prefix + ".MajorTicks", dc.MajorTicks);
        MinorTicks.Serialize(writer, prefix + ".MinorTicks", dc.MinorTicks);
    }

    #endregion // Public Methods
}