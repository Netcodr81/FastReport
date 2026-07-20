using System.ComponentModel;

using FastReport.Utils;

using SkiaSharp;

namespace FastReport.Gauge.Radial;

#if !DEBUG
[DesignTimeVisible(false)]
#endif
class RadialLabel : GaugeLabel
{
    public RadialLabel(GaugeObject parent) : base(parent)
    {
        Parent = parent as RadialGauge;
    }

    private SKFont CreateFont(FRPaintEventArgs e)
    {
        float size = Parent.IsPrinting ? Font.Size : Font.Size * e.ScaleX * 96f / DrawUtils.ScreenDpi;
        SKTypeface typeface = Font?.Typeface ?? SKTypeface.Default;
        SKFontStyle style = new SKFontStyle(typeface.FontWeight, typeface.FontWidth, typeface.FontSlant);
        return new SKFont(SKTypeface.FromFamilyName(typeface.FamilyName, style) ?? SKTypeface.Default, size);
    }

    public override void Draw(FRPaintEventArgs e)
    {
        if ((Parent as RadialGauge).Type == RadialGaugeType.Circle)
        {
            base.Draw(e);
            float x = (Parent.AbsLeft + Parent.Border.Width / 2) * e.ScaleX;
            float y = (Parent.AbsTop + Parent.Border.Width / 2) * e.ScaleY;
            float dx = (Parent.Width - Parent.Border.Width) * e.ScaleX - 1;
            float dy = (Parent.Height - Parent.Border.Width) * e.ScaleY - 1;

            SKPoint lblPt = new SKPoint(x + dx / 2, y + dy - ((Parent.Scale as RadialScale).AvrTick.Y - y));
            using SKFont font = CreateFont(e);
            using SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = Color,
                IsAntialias = true
            };

            SKSize txtSize = e.Graphics.MeasureString(Text, font);
            e.Graphics.DrawString(Text, font, paint, lblPt.X - txtSize.Width / 2, lblPt.Y - txtSize.Height / 2);
        }
    }
}