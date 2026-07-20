using FastReport.Utils;

using SkiaSharp;

namespace FastReport.Barcode;

/// <summary>
/// The base class for 2D-barcodes such as PDF417 and Datamatrix.
/// </summary>
public abstract class Barcode2DBase : BarcodeBase
{
    internal float FontHeight => Font.Size * DrawUtils.ScreenDpiFX * 18 / 13; // 18/13 to be more or less compatible with old behavior (Arial,8 with hardcoded 18px height)

    private void DrawBarcode(IGraphics g, float width, float height)
    {
        SKSize originalSize = CalcBounds();
        float kx = width / originalSize.Width;
        float ky = height / originalSize.Height;

        Draw2DBarcode(g, kx, ky);

        //If swiss qr, draw the swiss cross
        if (text.StartsWith("SPC"))
        {
            float top = showText ? height - 21 : height;

            using (var whitePaint = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill })
            using (var blackPaint = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Fill })
            {
                g.FillRectangle(whitePaint, width / 2 - width / 100f * 7, top / 2 - top / 100 * 7, width / 100f * 14, top / 100 * 14);
                g.FillRectangle(blackPaint, width / 2 - width / 100f * 6, top / 2 - top / 100 * 6, width / 100f * 12, top / 100 * 12);
                g.FillRectangle(whitePaint, width / 2 - width / 100f * 4, top / 2 - top / 100 * 1.5f, width / 100f * 8, top / 100 * 3);
                g.FillRectangle(whitePaint, width / 2 - width / 100f * 1.5f, top / 2 - top / 100 * 4, width / 100f * 3, top / 100 * 8);
            }
        }
        if (showMarker && text.StartsWith("ST"))
        {
            using (var blackPen = new SKPaint
            {
                Color = SKColors.Black,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = (kx * 4 * zoom) / 2
            })
            {
                g.DrawLine(blackPen, width - 2, height / 2, width - 2, height - 2);
                g.DrawLine(blackPen, width / 2, height - 2, width - 2, height - 2);
            }
        }
        // draw the text.
        if (showText)
        {
            string data = StripControlCodes(text);
            if (data.Length > 0)
            {
                // When we print, .Net automatically scales the font. However, we need to handle this process.
                // Downscale the font to the screen resolution, then scale by required value (ky).
                float fontHeight = FontHeight;

                // Use the font from BarcodeBase
                var measureSize = g.MeasureString(data, Font);
                float fontZoom = fontHeight / measureSize.Height * ky;

                using (var drawFont = new SKFont(SKTypeface.FromFamilyName(FontFamilyName), FontSize * fontZoom))
                using (var textPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    var textRect = new SKRect(0, height - fontHeight * ky, width, height);
                    g.DrawString(data, drawFont, textPaint, textRect, null);
                }
            }
        }
    }

    internal virtual void Draw2DBarcode(IGraphics g, float kx, float ky)
    {
    }

    /// <inheritdoc/>
    public override void DrawBarcode(IGraphics g, SKRect displayRect)
    {
        float width = angle == 90 || angle == 270 ? displayRect.Height : displayRect.Width;
        float height = angle == 90 || angle == 270 ? displayRect.Width : displayRect.Height;
        IGraphicsState state = g.Save();
        try
        {
            // rotate
            g.TranslateTransform(displayRect.Left, displayRect.Top);
            g.RotateTransform(angle);

            switch (angle)
            {
                case 90:
                    g.TranslateTransform(0, -displayRect.Width);
                    break;

                case 180:
                    g.TranslateTransform(-displayRect.Width, -displayRect.Height);
                    break;

                case 270:
                    g.TranslateTransform(-displayRect.Height, 0);
                    break;
            }

            DrawBarcode(g, width, height);
        }
        finally
        {
            g.Restore(state);
        }
    }
}