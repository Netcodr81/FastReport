using SkiaSharp;

namespace FastReport.Utils
{
    partial class ObjectInfo
    {
        #region Private Methods

        partial void UpdateDesign(SKBitmap image, int imageIndex, int buttonIndex = -1);

        /// <summary>
        /// Does nothing.
        /// </summary>
        partial void UpdateDesign(int flags, bool multiInsert, SKBitmap image, int imageIndex, int buttonIndex = -1);

        #endregion Private Methods
    }
}
