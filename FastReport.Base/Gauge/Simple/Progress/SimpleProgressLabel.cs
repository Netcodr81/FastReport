using System;
using System.ComponentModel;

using FastReport.Gauge.Radial;
using FastReport.Utils;

using SkiaSharp;

namespace FastReport.Gauge.Simple.Progress;

/// <inheritdoc />
#if !DEBUG
[DesignTimeVisible(false)]
#endif
public class SimpleProgressLabel : GaugeLabel
{
    private int decimals;

    /// <summary>
    /// Gets or sets the number of fractional digits
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int Decimals
    {
        get { return decimals; }
        set
        {
            if (value < 0)
                decimals = 0;
            else if (value > 15)
                decimals = 15;
            else
                decimals = value;
        }
    }

    /// <inheritdoc />
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
        get { return base.Text; }
        set { base.Text = value; }
    }

    /// <inheritdoc />
    public SimpleProgressLabel(GaugeObject parent) : base(parent)
    {
        Parent = parent as SimpleProgressGauge;
        decimals = 0;
    }

    private SKFont CreateFont(FRPaintEventArgs e)
    {
        float size = Parent.IsPrinting ? Font.Size : Font.Size * e.ScaleX * 96f / DrawUtils.ScreenDpi;
        SKTypeface typeface = Font?.Typeface ?? SKTypeface.Default;
        SKFontStyle style = new SKFontStyle(typeface.FontWeight, typeface.FontWidth, typeface.FontSlant);
        return new SKFont(SKTypeface.FromFamilyName(typeface.FamilyName, style) ?? SKTypeface.Default, size);
    }

    /// <inheritdoc />
    public override void Draw(FRPaintEventArgs e)
    {
        base.Draw(e);
        float x = (Parent.AbsLeft + Parent.Border.Width / 2) * e.ScaleX;
        float y = (Parent.AbsTop + Parent.Border.Width / 2) * e.ScaleY;
        float dx = (Parent.Width - Parent.Border.Width) * e.ScaleX;
        float dy = (Parent.Height - Parent.Border.Width) * e.ScaleY;

        SKPoint lblPt = new SKPoint(x + dx / 2, y + dy / 2);
        Text = Math.Round((Parent.Value - Parent.Minimum) / (Parent.Maximum - Parent.Minimum) * 100, decimals) + "%";

        using SKFont font = CreateFont(e);
        using SKPaint brush = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = Color,
            IsAntialias = true
        };

        SKSize txtSize = e.Graphics.MeasureString(Text, font);
        e.Graphics.DrawString(Text, font, brush, lblPt.X - txtSize.Width / 2, lblPt.Y - txtSize.Height / 2);
    }
}