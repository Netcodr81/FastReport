using SkiaSharp;

namespace FastReport.Matrix
{
    internal class MatrixStyleSheet : StyleSheet
    {
        public SKBitmap GetStyleBitmap(int index)
        {
            StyleCollection styleCollection = this[index];
            Style style = styleCollection[styleCollection.IndexOf("Header")];

            SKColor headerColor = SKColors.White;
            if (style.Fill is SolidFill solidHeaderFill)
                headerColor = solidHeaderFill.Color;
            else if (style.Fill is LinearGradientFill linearHeaderFill)
                headerColor = linearHeaderFill.StartColor;

            style = styleCollection[styleCollection.IndexOf("Body")];
            SKColor bodyColor = SKColors.White;
            if (style.Fill is SolidFill solidBodyFill)
                bodyColor = solidBodyFill.Color;
            else if (style.Fill is LinearGradientFill linearBodyFill)
                bodyColor = linearBodyFill.StartColor;

            SKBitmap result = new SKBitmap(16, 16, true);
            using SKCanvas canvas = new SKCanvas(result);
            using SKPaint fillPaint = new SKPaint { Style = SKPaintStyle.Fill, IsAntialias = false };
            using SKPaint borderPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.Silver, IsAntialias = false };

            fillPaint.Color = SKColors.White;
            canvas.DrawRect(0, 0, 16, 16, fillPaint);

            fillPaint.Color = headerColor;
            canvas.DrawRect(0, 0, 15, 8, fillPaint);

            fillPaint.Color = bodyColor;
            canvas.DrawRect(0, 8, 15, 8, fillPaint);

            canvas.DrawRect(0, 0, 14, 14, borderPaint);

            return result;
        }

    }
}
