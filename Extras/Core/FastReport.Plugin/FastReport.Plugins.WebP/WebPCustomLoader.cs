using FastReport.Utils;
using SkiaSharp;
using System.IO;

namespace FastReport.Plugins
{
    public class WebPCustomLoader : IImageHelperDataLoader
    {
        public bool CanLoad(byte[] imageData)
        {
            return imageData is { Length: > 12 } &&
                imageData[0] == (byte)'R' && imageData[1] == (byte)'I' && imageData[2] == (byte)'F' && imageData[3] == (byte)'F' &&
                imageData[8] == (byte)'W' && imageData[9] == (byte)'E' && imageData[10] == (byte)'B' && imageData[11] == (byte)'P';
        }

        public bool CanLoad(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) && fileName.EndsWith(".webp", System.StringComparison.OrdinalIgnoreCase);
        }

        public bool TryLoad(byte[] imageData, out byte[] result)
        {
            try
            {
                using (var img = SKBitmap.Decode(imageData))
                {
                    if (img == null)
                    {
                        result = null;
                        return false;
                    }

                    result = ConvertToPng(img);
                    return result is { Length: > 0 };
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("WebPCustomLoader could not load image");
                result = null;
                return false;
            }

        }

        private static byte[] ConvertToPng(SKBitmap img)
        {
            var filters = SKPngEncoderFilterFlags.NoFilters;
            int compress = 0;
            var options = new SKPngEncoderOptions(filters, compress);

            using (var pixmap = img.PeekPixels())
            {
                if (pixmap == null)
                    return null;

                using (var data = pixmap.Encode(options))
                {
                    return data?.ToArray();
                }
            }
        }

        public bool TryLoad(string fileName, out byte[] result)
        {
            try
            {
                var bytes = File.ReadAllBytes(fileName);
                return TryLoad(bytes, out result);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("WebPCustomLoader could not load image");
                result = null;
                return false;
            }
        }
    }
}
