using FastReport.Utils;
using SkiaSharp;

namespace FastReport
{
    partial class BandBase
    {
        /// <inheritdoc/>
        public override void Draw(FRPaintEventArgs e)
        {
            DrawBackground(e);
            Border.Draw(e, new SKRect(AbsLeft, AbsTop, AbsLeft + Width, AbsTop + Height));
        }
    }
}
