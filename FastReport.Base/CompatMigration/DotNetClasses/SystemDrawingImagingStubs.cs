using System;
using System.IO;

namespace System.Drawing.Imaging
{
    public enum PixelFormat
    {
        Undefined = 0,
        Format1bppIndexed = 196865,
        Format24bppRgb = 137224,
        Format32bppArgb = 2498570,
        Format32bppPArgb = 925707
    }

    public enum ImageLockMode { ReadOnly = 1, WriteOnly = 2, ReadWrite = 3, UserInputBuffer = 4 }

    public sealed class BitmapData
    {
        public IntPtr Scan0 { get; set; }
        public int Stride { get; set; }
        public int Height { get; set; }
    }

    public class ImageAttributes : IDisposable
    {
        public void SetColorMatrix(ColorMatrix newColorMatrix, ColorMatrixFlag mode = ColorMatrixFlag.Default, ColorAdjustType type = ColorAdjustType.Default) { }
        public void SetWrapMode(System.Drawing.Drawing2D.WrapMode mode) { }
        public void Dispose() { }
    }

    public class ColorMatrix
    {
        public float Matrix33 { get; set; }
        public ColorMatrix() { }
        public ColorMatrix(float[][] newColorMatrix) { }
    }

    public enum ColorMatrixFlag { Default = 0, SkipGrays = 1, AltGrays = 2 }
    public enum ColorAdjustType { Default = 0, Bitmap = 1 }

    public sealed class ImageFormat
    {
        private readonly Guid guid;
        private ImageFormat(Guid guid) { this.guid = guid; }

        public static ImageFormat Bmp { get; } = new ImageFormat(Guid.NewGuid());
        public static ImageFormat Emf { get; } = new ImageFormat(Guid.NewGuid());
        public static ImageFormat Exif { get; } = new ImageFormat(Guid.NewGuid());
        public static ImageFormat Gif { get; } = new ImageFormat(Guid.NewGuid());
        public static ImageFormat Icon { get; } = new ImageFormat(Guid.NewGuid());
        public static ImageFormat Jpeg { get; } = new ImageFormat(Guid.NewGuid());
        public static ImageFormat MemoryBmp { get; } = new ImageFormat(Guid.NewGuid());
        public static ImageFormat Png { get; } = new ImageFormat(Guid.NewGuid());
        public static ImageFormat Tiff { get; } = new ImageFormat(Guid.NewGuid());
        public static ImageFormat Wmf { get; } = new ImageFormat(Guid.NewGuid());

        public override bool Equals(object obj) => obj is ImageFormat other && other.guid == guid;
        public override int GetHashCode() => guid.GetHashCode();
    }


    public sealed class Encoder
    {
        private Encoder() { }
        public static Encoder Compression { get; } = new Encoder();
        public static Encoder SaveFlag { get; } = new Encoder();
        public static Encoder Quality { get; } = new Encoder();
    }

    public enum EncoderValue
    {
        CompressionNone = 6,
        CompressionLZW = 2,
        CompressionRle = 5,
        CompressionCCITT3 = 3,
        CompressionCCITT4 = 4,
        MultiFrame = 18,
        FrameDimensionPage = 23,
        Flush = 20
    }

    public sealed class EncoderParameter : IDisposable
    {
        public EncoderParameter(Encoder encoder, long value) { }
        public void Dispose() { }
    }

    public sealed class EncoderParameters : IDisposable
    {
        public EncoderParameter[] Param { get; }
        public EncoderParameters() : this(1) { }
        public EncoderParameters(int count) => Param = new EncoderParameter[count];
        public void Dispose() { }
    }

    public sealed class ImageCodecInfo
    {
        public Guid FormatID { get; set; }
        public string MimeType { get; set; } = string.Empty;
        public static ImageCodecInfo[] GetImageEncoders() => Array.Empty<ImageCodecInfo>();
    }
}

namespace System.Drawing
{
    public static class SystemFonts
    {
        public static Font DefaultFont { get; } = new Font("SansSerif", 8f);
    }
}
