using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;

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
                g.DrawImageUnscaled(source, new Rectangle(0, 0, source.Width, source.Height));
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
                g.DrawImage(src, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    rect, GraphicsUnit.Pixel);
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
            Save(image, stream, image.GetImageFormat());
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
            if (image is Bitmap)
            {
                if (format == ImageFormat.Icon)
                    SaveAsIcon(image, stream, true);
                else
                    image.Save(stream, format);
            }
            else if (image is Metafile)
            {
                using (Metafile emf = CreateMetafile(stream))
                {
                    DrawImageToMetafile(image, emf);
                }
            }
        }

        internal static T CreateMetafileFromHdcContext<T>(Func<IntPtr, T> factory)
        {
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                IntPtr hdc = g.GetHdc();
                try
                {
                    return factory(hdc);
                }
                finally
                {
                    g.ReleaseHdc(hdc);
                }
            }
        }

        private static Metafile CreateMetafile(Stream stream)
        {
            return CreateMetafileFromHdcContext(hdc => new Metafile(stream, hdc));
        }

        private static void DrawImageToMetafile(Image image, Metafile metafile)
        {
            using (IGraphics g = FRPaintEventArgs.CreateGraphics(metafile))
            {
                g.DrawImage(image, 0, 0);
            }
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
                if (image is Bitmap)
                {
                    if (format == ImageFormat.MemoryBmp)
                        throw new Exception(Res.Get("Export,Image,ImageParceFormatException"));
                    image.Save(stream, format);
                    return true;
                }
                //from mf to bitmap
                using (Metafile metafile = image as Metafile)
                using (Bitmap bitmap = new Bitmap(image.Width, image.Height))
                {
                    bitmap.SetResolution(96F, 96F);
                    using (IGraphics g = FRPaintEventArgs.CreateGraphics(bitmap))
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.DrawImage(metafile, 0, 0, (float)image.Width, (float)image.Height);
                    }
                    bitmap.Save(stream, format);
                }
                return true;

            }
            else if (format == ImageFormat.Icon)
            {
                return SaveAsIcon(image, stream, true);
            }
            else if (format == ImageFormat.Wmf || format == ImageFormat.Emf)
            {
                if (image is Metafile)
                {
                    using (Metafile emf = CreateMetafile(stream))
                    {
                        DrawImageToMetafile(image, emf);
                    }
                    return true;
                }
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
                // TODO memory leaks image converter
                return Image.FromStream(new MemoryStream(bytes));
#else
                return new ImageConverter().ConvertFrom(bytes) as Image;
#endif
            }
            catch
            {
                return null;
            }
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

            ColorMatrix colorMatrix = new ColorMatrix();
            colorMatrix.Matrix33 = 1 - transparency;
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(
               colorMatrix,
               ColorMatrixFlag.Default,
               ColorAdjustType.Bitmap);

            int width = source.Width;
            int height = source.Height;
            Bitmap image = new Bitmap(width, height);
            image.SetResolution(source.HorizontalResolution, source.VerticalResolution);

            using (IGraphics g = FRPaintEventArgs.CreateGraphics(image))
            {
                g.Clear(Color.Transparent);
                g.DrawImage(
                  source,
                  new Rectangle(0, 0, width, height),
                  0, 0, width, height,
                  GraphicsUnit.Pixel,
                  imageAttributes);
            }
            return image;
        }

        internal static Bitmap GetGrayscaleBitmap(Image source)
        {
            Bitmap grayscaleBitmap = new Bitmap(source.Width, source.Height, source.PixelFormat);

            // Red should be converted to (R*.299)+(G*.587)+(B*.114)
            // Green should be converted to (R*.299)+(G*.587)+(B*.114)
            // Blue should be converted to (R*.299)+(G*.587)+(B*.114)
            // Alpha should stay the same.
            ColorMatrix grayscaleMatrix = new ColorMatrix(new float[][]{
                                                          new float[] {0.299f, 0.299f, 0.299f, 0, 0},
                                                          new float[] {0.587f, 0.587f, 0.587f, 0, 0},
                                                          new float[] {0.114f, 0.114f, 0.114f, 0, 0},
                                                          new float[] {     0,      0,      0, 1, 0},
                                                          new float[] {     0,      0,      0, 0, 1}});

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(grayscaleMatrix);

            // Use a Graphics object from the new image
            using (IGraphics graphics = FRPaintEventArgs.CreateGraphics(grayscaleBitmap))
            {
                // Draw the original image using the ImageAttributes we created
                graphics.DrawImage(source,
                    new Rectangle(0, 0, grayscaleBitmap.Width, grayscaleBitmap.Height),
                    0, 0, grayscaleBitmap.Width, grayscaleBitmap.Height,
                    GraphicsUnit.Pixel, attributes);
            }

            return grayscaleBitmap;
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
                return Image.FromFile(fileName);
            }
            catch (Exception ex)
            {
                if (TryLoadUsingLegacyCustomLoaders(fileName, out Image legacyResult))
                    return legacyResult;

                throw new ImageLoadException(ex);
            }
        }
    }

    /// <summary>
    /// Represents image extension methods.
    /// </summary>
    public static class ImageExtension
    {
        /// <summary>
        /// Returns an Image format.
        /// </summary>
        public static ImageFormat GetImageFormat(this Image bitmap)
        {
            if (bitmap == null || bitmap.RawFormat == null)
                return null;
            ImageFormat format = null;
            if (ImageFormat.Jpeg.Equals(bitmap.RawFormat))
            {
                format = ImageFormat.Jpeg;
            }
            else if (ImageFormat.Gif.Equals(bitmap.RawFormat))
            {
                format = ImageFormat.Gif;
            }
            else if (ImageFormat.Png.Equals(bitmap.RawFormat))
            {
                format = ImageFormat.Png;
            }
            else if (ImageFormat.Emf.Equals(bitmap.RawFormat))
            {
                format = ImageFormat.Emf;
            }
            else if (ImageFormat.Icon.Equals(bitmap.RawFormat))
            {
                format = ImageFormat.Icon;
            }
            else if (ImageFormat.Tiff.Equals(bitmap.RawFormat))
            {
                format = ImageFormat.Tiff;
            }
            else if (ImageFormat.Bmp.Equals(bitmap.RawFormat) || ImageFormat.MemoryBmp.Equals(bitmap.RawFormat)) // MemoryBmp format raises a GDI exception
            {
                format = ImageFormat.Bmp;
            }
            else if (ImageFormat.Wmf.Equals(bitmap.RawFormat))
            {
                format = ImageFormat.Wmf;
            }
            if (format != null)
                return format;
            return ImageFormat.Bmp;
        }
    }
}
