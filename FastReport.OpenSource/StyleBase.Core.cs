using FastReport.Utils;
using SkiaSharp;

namespace FastReport
{
    partial class StyleBase
    {
        #region Private Methods

        private SKFont GetDefaultFontInternal()
        {
            return DrawUtils.DefaultFont;
        }

        #endregion Private Methods
    }
}
