using FastReport.Utils;
using SkiaSharp;
using System.IO;

namespace FastReport.Plugins
{
    public class WebPCustomLoader : IImageHelperLoader
    {
        public bool CanLoad(byte[] imageData)
        {
            return imageData.Length > 12 &&
                imageData[0] == (byte)'R' && imageData[1] == (byte)'I' && imageData[2] == (byte)'F' && imageData[3] == (byte)'F' &&
                imageData[8] == (byte)'W' && imageData[9] == (byte)'E' && imageData[10] == (byte)'B' && imageData[11] == (byte)'P';
        }

        public bool CanLoad(string fileName)
        {
            return fileName.EndsWith(".webp", System.StringComparison.OrdinalIgnoreCase);
        }

        public bool TryLoad(byte[] imageData, out SKBitmap result)
        {
            try
            {
                result = SKBitmap.Decode(imageData);
                return result != null;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("WebPCustomLoader could not load image");
                result = null;
                return false;
            }

        }

        public bool TryLoad(string fileName, out SKBitmap result)
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
