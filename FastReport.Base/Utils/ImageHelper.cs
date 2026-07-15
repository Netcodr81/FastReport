using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace FastReport.Utils
{
    /// <summary>
    /// Interface allows to load images with custom format or custom type
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IImageHelperLoader
    {
        /// <summary>
        /// Returns true if image can be loaded
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        bool CanLoad(byte[] imageData);
        /// <summary>
        /// Returns true if image can be loaded
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        bool CanLoad(string fileName);
        /// <summary>
        /// Try to load the image, must not throw exception!
        /// </summary>
        /// <param name="imageData"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryLoad(byte[] imageData, out Image result);
        /// <summary>
        /// Try to load the image, must not throw exception!
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryLoad(string fileName, out Image result);
    }

    /// <summary>
    /// Interface allows to decode custom image formats to cross-platform image bytes.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IImageHelperDataLoader
    {
        bool CanLoad(byte[] imageData);
        bool CanLoad(string fileName);
        bool TryLoad(byte[] imageData, out byte[] result);
        bool TryLoad(string fileName, out byte[] result);
    }

    /// <summary>
    /// Internal calss for image processing
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class ImageHelper
    {
        private readonly static object _customLoadersLocker = new object();
        private readonly static List<IImageHelperLoader> _customLoaders = new List<IImageHelperLoader>();
        private readonly static List<IImageHelperDataLoader> _customDataLoaders = new List<IImageHelperDataLoader>();
        private readonly static HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Register a new custom loader
        /// </summary>
        /// <param name="imageHelperLoader"></param>
        public static void Register(IImageHelperLoader imageHelperLoader)
        {
            lock (_customLoadersLocker)
            {
                foreach (var loader in _customLoaders)
                    if (loader == imageHelperLoader)
                        return;

                _customLoaders.Add(imageHelperLoader);
            }
        }

        /// <summary>
        /// Register a new custom image data loader.
        /// </summary>
        /// <param name="imageHelperDataLoader"></param>
        public static void Register(IImageHelperDataLoader imageHelperDataLoader)
        {
            lock (_customLoadersLocker)
            {
                foreach (var loader in _customDataLoaders)
                    if (loader == imageHelperDataLoader)
                        return;

                _customDataLoaders.Add(imageHelperDataLoader);
            }
        }
        internal static Bitmap CloneBitmap(Image source)
        {
            if (source == null)
                return null;

            Bitmap image = new Bitmap(source.Width, source.Height);
            if (!Config.IsRunningOnMono) // mono fw bug workaround
                image.SetResolution(source.HorizontalResolution, source.VerticalResolution);
            using (IGraphics g = FRPaintEventArgs.CreateGraphics(image))
            {
                g.DrawImage(new ImagePaint(source), new RectangleF(0, 0, source.Width, source.Height));
            }
            return image;

            // this can throw OutOfMemory when creating a grayscale image from a cloned bitmap
            //      return source.Clone() as Bitmap;
        }

        internal static Bitmap CutImage(Bitmap src, RectangleF rect)
        {
            var bitmap = new Bitmap((int)rect.Width, (int)rect.Height);
            using (IGraphics g = FRPaintEventArgs.CreateGraphics(bitmap))
            {
                g.DrawImage(new ImagePaint(src), new RectangleF(0, 0, bitmap.Width, bitmap.Height), rect);
            }
            return bitmap;
        }

        internal static byte[] ToByteArray(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }

        internal static void Save(Image image, Stream stream)
        {
            Save(image, stream, ImageFormat.Png);
        }

        internal static void Save(Image image, string fileName, ImageFormat format)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                Save(image, stream, format);
            }
        }

        internal static void Save(Image image, Stream stream, ImageFormat format)
        {
            if (image == null)
                return;

            if (format == ImageFormat.Icon)
                SaveAsIcon(image, stream, true);
            else
                image.Save(stream, format);
        }


        internal static bool SaveAndConvert(Image image, Stream stream, ImageFormat format)
        {
            if (image == null)
                return false;

            if (format == ImageFormat.Jpeg || format == ImageFormat.Gif
                || format == ImageFormat.Tiff || format == ImageFormat.Bmp
                || format == ImageFormat.Png
                || format == ImageFormat.MemoryBmp)
            {
                if (format == ImageFormat.MemoryBmp)
                    throw new Exception(Res.Get("Export,Image,ImageParceFormatException"));

                image.Save(stream, format);
                return true;
            }
            else if (format == ImageFormat.Icon)
            {
                return SaveAsIcon(image, stream, true);
            }
            else if (format == ImageFormat.Wmf || format == ImageFormat.Emf)
            {
                // Cross-platform fallback: vector formats are emitted as PNG.
                image.Save(stream, ImageFormat.Png);
                return true;
            }

            //throw new Exception(Res.Get("Export,Image,ImageParceFormatException")); // we cant convert image to exif or from bitmap to mf 
            return false;
        }

        internal static byte[] Load(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
                return File.ReadAllBytes(fileName);
            return null;
        }

        /// <summary>
        /// Create a blank bitmap image using the current cross-platform drawing backend.
        /// </summary>
        public static Image CreateBitmap(int width, int height)
        {
            return new Bitmap(Math.Max(1, width), Math.Max(1, height), PixelFormat.Format32bppPArgb);
        }

        /// <summary>
        /// Load the image from bytes, Internal only method
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Image Load(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                var image = LoadCore(bytes);
                if (image != null)
                    return image;

                if (TryLoadUsingCustomDataLoaders(bytes, out image))
                    return image;

                if (TryLoadUsingLegacyCustomLoaders(bytes, out image))
                    return image;

                Bitmap errorBmp = new Bitmap(10, 10);
                using (IGraphics g = FRPaintEventArgs.CreateGraphics(errorBmp))
                {
                    g.DrawLine(Pens.Red, 0, 0, 10, 10);
                    g.DrawLine(Pens.Red, 0, 10, 10, 0);
                }
                return errorBmp;
            }
            return null;
        }

        private static Image LoadCore(byte[] bytes)
        {
            try
            {
#if CROSSPLATFORM
                using var decoded = SKBitmap.Decode(bytes);
                if (decoded == null)
                    return null;

                return CreateBitmapFromSkia(decoded);
#else
                return new ImageConverter().ConvertFrom(bytes) as Image;
#endif
            }
            catch
            {
                return null;
            }
        }

        private static Bitmap CreateBitmapFromSkia(SKBitmap skBitmap)
        {
            var bitmap = new Bitmap(skBitmap.Width, skBitmap.Height, PixelFormat.Format32bppPArgb);
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
            try
            {
                IntPtr srcPtr = skBitmap.GetPixels();
                if (data == null || data.Scan0 == IntPtr.Zero || srcPtr == IntPtr.Zero)
                    return bitmap;

                int srcStride = skBitmap.RowBytes;
                int dstStride = data.Stride;
                int rows = Math.Min(bitmap.Height, skBitmap.Height);
                int copyBytesPerRow = Math.Min(Math.Abs(srcStride), Math.Abs(dstStride));

                if (copyBytesPerRow <= 0 || rows <= 0)
                    return bitmap;

                byte[] rowBuffer = new byte[copyBytesPerRow];
                for (int y = 0; y < rows; y++)
                {
                    IntPtr srcRow = IntPtr.Add(srcPtr, y * srcStride);
                    IntPtr dstRow = IntPtr.Add(data.Scan0, y * dstStride);
                    Marshal.Copy(srcRow, rowBuffer, 0, copyBytesPerRow);
                    Marshal.Copy(rowBuffer, 0, dstRow, copyBytesPerRow);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            return bitmap;
        }

        private static bool TryLoadUsingCustomDataLoaders(byte[] bytes, out Image result)
        {
            result = null;

            lock (_customLoadersLocker)
            {
                foreach (var loader in _customDataLoaders)
                {
                    if (!loader.CanLoad(bytes) || !loader.TryLoad(bytes, out byte[] resultData) || resultData == null || resultData.Length == 0)
                        continue;

                    result = LoadCore(resultData);
                    if (result != null)
                        return true;
                }
            }

            return false;
        }

        private static bool TryLoadUsingCustomDataLoaders(string fileName, out Image result)
        {
            result = null;

            lock (_customLoadersLocker)
            {
                foreach (var loader in _customDataLoaders)
                {
                    if (!loader.CanLoad(fileName) || !loader.TryLoad(fileName, out byte[] resultData) || resultData == null || resultData.Length == 0)
                        continue;

                    result = LoadCore(resultData);
                    if (result != null)
                        return true;
                }
            }

            return false;
        }

        private static bool TryLoadUsingLegacyCustomLoaders(byte[] bytes, out Image result)
        {
            result = null;

            lock (_customLoadersLocker)
            {
                foreach (var loader in _customLoaders)
                {
                    if (loader.CanLoad(bytes) && loader.TryLoad(bytes, out result))
                        return true;
                }
            }

            return false;
        }

        private static bool TryLoadUsingLegacyCustomLoaders(string fileName, out Image result)
        {
            result = null;

            lock (_customLoadersLocker)
            {
                foreach (var loader in _customLoaders)
                {
                    if (loader.CanLoad(fileName) && loader.TryLoad(fileName, out result))
                        return true;
                }
            }

            return false;
        }

        internal static byte[] LoadURL(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                return _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
            }
            return null;
        }

        internal static Bitmap GetTransparentBitmap(Image source, float transparency)
        {
            if (source == null)
                return null;

            int width = source.Width;
            int height = source.Height;
            var image = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            image.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (IGraphics g = FRPaintEventArgs.CreateGraphics(image))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(new ImagePaint(source), new RectangleF(0, 0, width, height), new RectangleF(0, 0, width, height));
            }

            float clampedTransparency = transparency < 0 ? 0 : (transparency > 1 ? 1 : transparency);
            byte alphaScale = (byte)Math.Round((1f - clampedTransparency) * 255f);
            ApplyAlphaScale(image, alphaScale);
            return image;
        }

        internal static Bitmap GetGrayscaleBitmap(Image source)
        {
            if (source == null)
                return null;

            var grayscaleBitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppPArgb);
            grayscaleBitmap.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (IGraphics graphics = FRPaintEventArgs.CreateGraphics(grayscaleBitmap))
            {
                graphics.DrawImage(new ImagePaint(source), new RectangleF(0, 0, grayscaleBitmap.Width, grayscaleBitmap.Height), new RectangleF(0, 0, grayscaleBitmap.Width, grayscaleBitmap.Height));
            }

            ApplyGrayscale(grayscaleBitmap);
            return grayscaleBitmap;
        }

        private static void ApplyAlphaScale(Bitmap bitmap, byte alphaScale)
        {
            if (bitmap == null)
                return;

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            try
            {
                if (data == null || data.Scan0 == IntPtr.Zero)
                    return;

                int bytes = data.Stride * data.Height;
                if (bytes <= 0)
                    return;

                byte[] buffer = new byte[bytes];
                Marshal.Copy(data.Scan0, buffer, 0, bytes);

                for (int i = 0; i < bytes; i += 4)
                {
                    int scaledAlpha = buffer[i + 3] * alphaScale;
                    buffer[i + 3] = (byte)(scaledAlpha / 255);
                }

                Marshal.Copy(buffer, 0, data.Scan0, bytes);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        private static void ApplyGrayscale(Bitmap bitmap)
        {
            if (bitmap == null)
                return;

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppPArgb);
            try
            {
                if (data == null || data.Scan0 == IntPtr.Zero)
                    return;

                int bytes = data.Stride * data.Height;
                if (bytes <= 0)
                    return;

                byte[] buffer = new byte[bytes];
                Marshal.Copy(data.Scan0, buffer, 0, bytes);

                for (int i = 0; i < bytes; i += 4)
                {
                    byte b = buffer[i];
                    byte g = buffer[i + 1];
                    byte r = buffer[i + 2];
                    byte gray = (byte)Math.Clamp((int)Math.Round(r * 0.299 + g * 0.587 + b * 0.114), 0, 255);

                    buffer[i] = gray;
                    buffer[i + 1] = gray;
                    buffer[i + 2] = gray;
                }

                Marshal.Copy(buffer, 0, data.Scan0, bytes);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }

        /// <summary>
        /// Converts a PNG image to a icon (ico)
        /// </summary>
        /// <param name="image">The input image</param>
        /// <param name="output">The output stream</param>
        /// <param name="preserveAspectRatio">Preserve the aspect ratio</param>
        /// <returns>Wether or not the icon was succesfully generated</returns>
        internal static bool SaveAsIcon(Image image, Stream output, bool preserveAspectRatio = false)
        {
            int size = 256;
            float width = size, height = size;
            if (preserveAspectRatio)
            {
                if (image.Width > image.Height)
                    height = ((float)image.Height / image.Width) * size;
                else
                    width = ((float)image.Width / image.Height) * size;
            }

            var newBitmap = new Bitmap(image, new Size((int)width, (int)height));
            if (newBitmap == null)
                return false;

            // save the resized png into a memory stream for future use
            using (MemoryStream memoryStream = new MemoryStream())
            {
                newBitmap.Save(memoryStream, ImageFormat.Png);

                var iconWriter = new BinaryWriter(output);
                if (output == null || iconWriter == null)
                    return false;

                // 0-1 reserved, 0
                iconWriter.Write((byte)0);
                iconWriter.Write((byte)0);

                // 2-3 image type, 1 = icon, 2 = cursor
                iconWriter.Write((short)1);

                // 4-5 number of images
                iconWriter.Write((short)1);

                // image entry 1
                // 0 image width
                iconWriter.Write((byte)width);
                // 1 image height
                iconWriter.Write((byte)height);

                // 2 number of colors
                iconWriter.Write((byte)0);

                // 3 reserved
                iconWriter.Write((byte)0);

                // 4-5 color planes
                iconWriter.Write((short)0);

                // 6-7 bits per pixel
                iconWriter.Write((short)32);

                // 8-11 size of image data
                iconWriter.Write((int)memoryStream.Length);

                // 12-15 offset of image data
                iconWriter.Write((int)(6 + 16));

                // write image data
                // png data must contain the whole png data file
                iconWriter.Write(memoryStream.ToArray());

                iconWriter.Flush();
            }

            return true;
        }

        internal static Image LoadFromFile(string fileName)
        {
            if (TryLoadUsingCustomDataLoaders(fileName, out Image customDataResult))
                return customDataResult;

            try
            {
                if (!File.Exists(fileName))
                    throw new FileNotFoundException("Image file was not found.", fileName);

                var fileBytes = File.ReadAllBytes(fileName);
                var loaded = Load(fileBytes);
                if (loaded != null)
                    return loaded;

                throw new InvalidOperationException("Unable to load image bytes.");
            }
            catch (Exception ex)
            {
                if (TryLoadUsingLegacyCustomLoaders(fileName, out Image legacyResult))
                    return legacyResult;

                throw new ImageLoadException(ex);
            }
        }
    }
}
