using FastReport.Utils;
using SkiaSharp;
using System.ComponentModel;

namespace FastReport.Gauge.Radial
{
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
            SKFontStyleWeight weight = (Font.Style & SKFontStyle.Bold) != 0
                ? SKFontStyleWeight.Bold
                : SKFontStyleWeight.Normal;
            SKFontStyleSlant slant = (Font.Style & SKFontStyle.Italic) != 0
                ? SKFontStyleSlant.Italic
                : SKFontStyleSlant.Upright;
            SKTypeface typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name, new SKFontStyle(weight, SKFontStyleWidth.Normal, slant));
            return new SKFont(typeface, size);
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
                    Color = new SKColor(Color.R, Color.G, Color.B, Color.A),
                    IsAntialias = true
                };

                SKSize txtSize = e.Graphics.MeasureString(Text, font);
                e.Graphics.DrawString(Text, font, paint, lblPt.X - txtSize.Width / 2, lblPt.Y - txtSize.Height / 2);
            }
        }
    }
}
