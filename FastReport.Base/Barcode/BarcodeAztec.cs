using FastReport.Barcode.Aztec;
using FastReport.Utils;
using SkiaSharp;
using System.ComponentModel;

namespace FastReport.Barcode
{
    /// <summary>
    /// Generates the 2D Aztec barcode.
    /// </summary>
    public class BarcodeAztec : Barcode2DBase
    {
        BitMatrix matrix;
        int errorCorrectionPercent;
        const int PIXEL_SIZE = 4;

        /// <summary>
        /// Gets or sets the error correction percent.
        /// </summary>
        [DefaultValue(33)]
        public int ErrorCorrectionPercent
        {
            get { return errorCorrectionPercent; }
            set { errorCorrectionPercent = (value < 5) ? 5 : ((value > 95) ? 95 : value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeAztec"/> class with default settings.
        /// </summary>
        public BarcodeAztec()
        {
            ErrorCorrectionPercent = 33;
        }

        internal override void Initialize(string text, bool showText, int angle, float zoom, bool showMarker)
        {
            base.Initialize(text, showText, angle, zoom, showMarker);

            matrix = Encoder.encode(System.Text.Encoding.ASCII.GetBytes(text), ErrorCorrectionPercent, 0).Matrix;
        }

        internal override SKSize CalcBounds()
        {
            int textAdd = showText ? (int)(FontHeight) : 0;
            return new SKSize(matrix.Width * PIXEL_SIZE, matrix.Height * PIXEL_SIZE + textAdd);
        }

        internal override void Draw2DBarcode(IGraphics g, float kx, float ky)
        {
            using (SKPaint light = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill })
            using (SKPaint dark = new SKPaint { Color = Color, Style = SKPaintStyle.Fill })
            {
                for (int y = 0; y < matrix.Height; y++)
                {
                    for (int x = 0; x < matrix.Width; x++)
                    {
                        bool b = matrix.getRow(y, null)[x];

                        SKPaint brush = /*b == true ?*/ dark /*: light*/;
                        if (b == true)
                            g.FillRectangle(brush, x * PIXEL_SIZE * kx, y * PIXEL_SIZE * ky,
                                                   PIXEL_SIZE * kx, PIXEL_SIZE * ky);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Assign(BarcodeBase source)
        {
            base.Assign(source);
            BarcodeAztec src = source as BarcodeAztec;

            ErrorCorrectionPercent = src.ErrorCorrectionPercent;
        }

        internal override void Serialize(FastReport.Utils.FRWriter writer, string prefix, BarcodeBase diff)
        {
            base.Serialize(writer, prefix, diff);
            BarcodeAztec c = diff as BarcodeAztec;

            if (c == null || ErrorCorrectionPercent != c.ErrorCorrectionPercent)
                writer.WriteInt(prefix + "ErrorCorrection", ErrorCorrectionPercent);
        }
    }
}
