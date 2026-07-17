using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using Bitmap = SkiaSharp.SKBitmap;
using Image = SkiaSharp.SKBitmap;
using RectangleF = SkiaSharp.SKRect;

namespace FastReport.Utils
{
    public enum ImageFormat
    {
        Bmp,
        Png,
        Jpeg,
        Gif,
        Tiff,
        Icon,
        MemoryBmp,
        Wmf,
        Emf
    }

    /// <summary>
    /// Interface allows to load images with custom format or custom type
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IImageHelperLoader
    {
        bool CanLoad(byte[] imageData);
        bool CanLoad(string fileName);
        bool TryLoad(byte[] imageData, out Image result);
        bool TryLoad(string fileName, out Image result);
    }

    /// <summary>
    /// Internal calss for image processing
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class ImageHelper
    {
        private readonly static object _customLoadersLocker = new object();
        private readonly static List<IImageHelperLoader> _customLoaders = new List<IImageHelperLoader>();

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

        internal static Bitmap CloneBitmap(Image source)
        {
            if (source == null)
                return null;

            Bitmap image = CreateBitmap(source.Width, source.Height, source.ColorType, source.AlphaType, source.ColorSpace);
            using SKCanvas canvas = new SKCanvas(image);
            canvas.DrawBitmap(source, 0, 0);
            return image;
        }

        internal static Bitmap CutImage(Bitmap src, RectangleF rect)
        {
            if (src == null)
                return null;

            SKRectI subset = new SKRectI((int)rect.Left, (int)rect.Top, (int)Math.Ceiling(rect.Right), (int)Math.Ceiling(rect.Bottom));
            Bitmap bitmap = CreateBitmap(Math.Max(1, subset.Width), Math.Max(1, subset.Height), src.ColorType, src.AlphaType, src.ColorSpace);
            if (!src.ExtractSubset(bitmap, subset))
            {
                using SKCanvas canvas = new SKCanvas(bitmap);
                canvas.Clear(SKColors.Transparent);
                canvas.DrawBitmap(src, -subset.Left, -subset.Top);
            }
            return bitmap;
        }

        internal static byte[] ToByteArray(Image image, ImageFormat format)
        {
            using MemoryStream ms = new MemoryStream();
            Save(image, ms, format);
            return ms.ToArray();
        }

        internal static void Save(Image image, Stream stream)
        {
            Save(image, stream, image.GetImageFormat());
        }

        internal static void Save(Image image, string fileName, ImageFormat format)
        {
            using FileStream stream = new FileStream(fileName, FileMode.Create);
            Save(image, stream, format);
        }

        internal static void Save(Image image, Stream stream, ImageFormat format)
        {
            if (image == null || stream == null)
                return;

            if (format == ImageFormat.Icon)
            {
                SaveAsIcon(image, stream, true);
                return;
            }

            if (TryEncode(image, stream, format))
                return;

            TryEncode(image, stream, ImageFormat.Png);
        }

        internal static bool SaveAndConvert(Image image, Stream stream, ImageFormat format)
        {
            if (image == null || stream == null)
                return false;

            if (format == ImageFormat.MemoryBmp)
                throw new Exception(Res.Get("Export,Image,ImageParceFormatException"));

            if (format == ImageFormat.Icon)
                return SaveAsIcon(image, stream, true);

            if (format == ImageFormat.Wmf || format == ImageFormat.Emf || format == ImageFormat.Tiff)
                return false;

            return TryEncode(image, stream, format);
        }

        internal static byte[] Load(string fileName)
        {
            if (!String.IsNullOrEmpty(fileName))
                return File.ReadAllBytes(fileName);
            return null;
        }

        public static Image Load(Stream stream)
        {
            if (stream == null)
                return null;

            using MemoryStream memory = new MemoryStream();
            stream.CopyTo(memory);
            return Load(memory.ToArray());
        }

        /// <summary>
        /// Load the image from bytes, Internal only method
        /// </summary>
        public static Image Load(byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                try
                {
                    Bitmap bitmap = SKBitmap.Decode(bytes);
                    if (bitmap != null)
                        return bitmap;
                }
                catch
                {
                }

                if (_customLoaders.Count > 0)
                {
                    lock (_customLoadersLocker)
                    {
                        foreach (var loader in _customLoaders)
                        {
                            if (loader.CanLoad(bytes) && loader.TryLoad(bytes, out Image result))
                                return result;
                        }
                    }
                }

                Bitmap errorBmp = CreateBitmap(10, 10, SKColorType.Bgra8888, SKAlphaType.Premul, null);
                using SKCanvas canvas = new SKCanvas(errorBmp);
                using SKPaint paint = new SKPaint { Color = SKColors.Red, StrokeWidth = 1, IsAntialias = true };
                canvas.DrawLine(0, 0, 10, 10, paint);
                canvas.DrawLine(0, 10, 10, 0, paint);
                return errorBmp;
            }
            return null;
        }

        internal static byte[] LoadURL(string url)
        {
            if (!String.IsNullOrEmpty(url))
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)(0xc0 | 0x300 | 0xc00);
#pragma warning disable SYSLIB0014
                using WebClient web = new WebClient();
#pragma warning restore SYSLIB0014
                return web.DownloadData(url);
            }
            return null;
        }

        internal static Bitmap GetTransparentBitmap(Image source, float transparency)
        {
            if (source == null)
                return null;

            byte alpha = (byte)Math.Clamp((1f - transparency) * 255f, 0f, 255f);
            Bitmap image = CreateBitmap(source.Width, source.Height, source.ColorType, source.AlphaType, source.ColorSpace);
            using SKCanvas canvas = new SKCanvas(image);
            using SKPaint paint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, alpha),
                BlendMode = SKBlendMode.Modulate,
                IsAntialias = true
            };
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(source, 0, 0, paint);
            return image;
        }

        internal static Bitmap GetGrayscaleBitmap(Image source)
        {
            if (source == null)
                return null;

            Bitmap grayscaleBitmap = CreateBitmap(source.Width, source.Height, source.ColorType, source.AlphaType, source.ColorSpace);
            float[] grayscaleMatrix =
            {
                0.299f, 0.299f, 0.299f, 0, 0,
                0.587f, 0.587f, 0.587f, 0, 0,
                0.114f, 0.114f, 0.114f, 0, 0,
                0, 0, 0, 1, 0
            };
            using SKCanvas canvas = new SKCanvas(grayscaleBitmap);
            using SKPaint paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateColorMatrix(grayscaleMatrix)
            };
            canvas.DrawBitmap(source, 0, 0, paint);
            return grayscaleBitmap;
        }

        internal static bool SaveAsIcon(Image image, Stream output, bool preserveAspectRatio = false)
        {
            if (image == null || output == null)
                return false;

            int size = 256;
            float width = size;
            float height = size;
            if (preserveAspectRatio)
            {
                if (image.Width > image.Height)
                    height = ((float)image.Height / image.Width) * size;
                else
                    width = ((float)image.Width / image.Height) * size;
            }

            using Bitmap newBitmap = ResizeBitmap(image, Math.Max(1, (int)width), Math.Max(1, (int)height));
            if (newBitmap == null)
                return false;

            using MemoryStream memoryStream = new MemoryStream();
            if (!TryEncode(newBitmap, memoryStream, ImageFormat.Png))
                return false;

            BinaryWriter iconWriter = new BinaryWriter(output);
            iconWriter.Write((byte)0);
            iconWriter.Write((byte)0);
            iconWriter.Write((short)1);
            iconWriter.Write((short)1);
            iconWriter.Write((byte)(width >= 256 ? 0 : width));
            iconWriter.Write((byte)(height >= 256 ? 0 : height));
            iconWriter.Write((byte)0);
            iconWriter.Write((byte)0);
            iconWriter.Write((short)0);
            iconWriter.Write((short)32);
            iconWriter.Write((int)memoryStream.Length);
            iconWriter.Write(6 + 16);
            iconWriter.Write(memoryStream.ToArray());
            iconWriter.Flush();
            return true;
        }

        internal static Image LoadFromFile(string fileName)
        {
            try
            {
                Bitmap bitmap = SKBitmap.Decode(fileName);
                if (bitmap != null)
                    return bitmap;
                throw new InvalidOperationException($"Cannot load image '{fileName}'.");
            }
            catch (Exception ex)
            {
                if (_customLoaders.Count > 0)
                {
                    lock (_customLoadersLocker)
                    {
                        foreach (var loader in _customLoaders)
                        {
                            if (loader.CanLoad(fileName) && loader.TryLoad(fileName, out Image result))
                                return result;
                        }
                    }
                }

                throw new ImageLoadException(ex);
            }
        }

        private static Bitmap CreateBitmap(int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorSpace)
        {
            return new Bitmap(new SKImageInfo(width, height, colorType, alphaType, colorSpace));
        }

        private static Bitmap ResizeBitmap(Bitmap image, int width, int height)
        {
            Bitmap resized = CreateBitmap(width, height, image.ColorType, image.AlphaType, image.ColorSpace);
            using SKCanvas canvas = new SKCanvas(resized);
            using SKPaint paint = new SKPaint { IsAntialias = true, IsDither = true };
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(image, new SKRect(0, 0, width, height), paint);
            return resized;
        }

        private static bool TryEncode(Bitmap image, Stream stream, ImageFormat format)
        {
            SKEncodedImageFormat? encodedFormat = ToEncodedFormat(format);
            if (encodedFormat == null)
                return false;

            using SKImage skImage = SKImage.FromBitmap(image);
            using SKData data = skImage.Encode(encodedFormat.Value, 100);
            if (data == null)
                return false;

            data.SaveTo(stream);
            return true;
        }

        private static SKEncodedImageFormat? ToEncodedFormat(ImageFormat format)
        {
            return format switch
            {
                ImageFormat.Bmp => SKEncodedImageFormat.Bmp,
                ImageFormat.MemoryBmp => SKEncodedImageFormat.Bmp,
                ImageFormat.Png => SKEncodedImageFormat.Png,
                ImageFormat.Jpeg => SKEncodedImageFormat.Jpeg,
                ImageFormat.Gif => SKEncodedImageFormat.Gif,
                _ => null
            };
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
            return bitmap == null ? ImageFormat.Bmp : ImageFormat.Png;
        }
    }
}
