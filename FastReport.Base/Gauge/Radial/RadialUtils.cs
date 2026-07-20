using System;

using FastReport.Utils;

using SkiaSharp;

namespace FastReport.Gauge.Radial;

internal class RadialUtils
{
    public static SKFont GetFont(FRPaintEventArgs e, GaugeObject gauge, SKFont font)
    {
        float size = gauge.IsPrinting ? font.Size : font.Size * e.ScaleX * 96f / DrawUtils.ScreenDpi;
        return new SKFont(font.Typeface, size);
    }

    public static SKSize GetStringSize(FRPaintEventArgs e, GaugeObject gauge, SKFont font, string text)
    {
        return e.Graphics.MeasureString(text, GetFont(e, gauge, font));
    }

    public static SKPoint[] RotateVector(SKPoint[] vector, double angle, SKPoint center)
    {
        SKPoint[] rotatedVector = new SKPoint[2];
        rotatedVector[0] = new SKPoint(
            (float)(center.X + (vector[0].X - center.X) * Math.Cos(angle) + (center.Y - vector[0].Y) * Math.Sin(angle)),
            (float)(center.Y + (vector[0].X - center.X) * Math.Sin(angle) + (vector[0].Y - center.Y) * Math.Cos(angle)));
        rotatedVector[1] = new SKPoint(
            (float)(center.X + (vector[1].X - center.X) * Math.Cos(angle) + (center.Y - vector[1].Y) * Math.Sin(angle)),
            (float)(center.Y + (vector[1].X - center.X) * Math.Sin(angle) + (vector[1].Y - center.Y) * Math.Cos(angle)));
        return rotatedVector;
    }

    public static bool IsTop(GaugeObject radialGauge)
    {
        return ((radialGauge as RadialGauge).Position & RadialGaugePosition.Top) != 0;
    }

    public static bool IsBottom(GaugeObject radialGauge)
    {
        return ((radialGauge as RadialGauge).Position & RadialGaugePosition.Bottom) != 0;
    }

    public static bool IsLeft(GaugeObject radialGauge)
    {
        return ((radialGauge as RadialGauge).Position & RadialGaugePosition.Left) != 0;
    }

    public static bool IsRight(GaugeObject radialGauge)
    {
        return ((radialGauge as RadialGauge).Position & RadialGaugePosition.Right) != 0;
    }

    public static bool IsSemicircle(GaugeObject radialGauge)
    {
        return ((radialGauge as RadialGauge).Type & RadialGaugeType.Semicircle) != 0;
    }

    public static bool IsQuadrant(GaugeObject radialGauge)
    {
        return ((radialGauge as RadialGauge).Type & RadialGaugeType.Quadrant) != 0;
    }
}