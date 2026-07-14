using FastReport.Export.PdfSimple.PdfCore;
using FastReport.Export.PdfSimple.PdfObjects;
using FastReport.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace FastReport.Export.PdfSimple
{
    partial class PDFSimpleExport
    {
        #region Private Fields

        private Dictionary<string, PdfIndirectObject> hashList;

        #endregion Private Fields

        #region Private Methods

        private string AppendPDFImage(Bitmap image, int quality)
        {
            int[] rawBitmap = GetRawBitmap(image);
            string hash = CalculateHash(rawBitmap);
            PdfIndirectObject imageLink = GetImageByHash(hash);

            if (imageLink == null)
            {
                using (MemoryStream imageStream = new MemoryStream())
                using (MemoryStream maskStream = GetMask(rawBitmap))
                {
                    SaveJpeg(image, imageStream, quality);
                    imageStream.Position = 0;
                    imageLink = WriteImage(imageStream, maskStream, image.Width, image.Height);
                }

                SetImageByHash(hash, imageLink);
            }

            return pdfPage.AddImage(imageLink);
        }

        private string CalculateHash(int[] raw_picture)
        {
            byte[] raw_picture_byte = new byte[raw_picture.Length * sizeof(int)];
            Buffer.BlockCopy(raw_picture, 0, raw_picture_byte, 0, raw_picture_byte.Length);
            byte[] hash = new Murmur3().ComputeHash(raw_picture_byte);
            return Convert.ToBase64String(hash);
        }

        private void DrawImage(float left, float top, float width, float height, Bitmap image)
        {
            string imageLink = AppendPDFImage(image, JpegQuality);
            pageContent.Append("q").AppendLine();
            pageContent.Append(width).Append(" 0 0 ").Append(height).Append(" ").Append(left).Append(" ").Append(top).Append(" cm").AppendLine();
            pageContent.Append(imageLink).Append(" Do").AppendLine();
            pageContent.Append("Q").AppendLine();
        }


        private PdfIndirectObject GetImageByHash(string hash)
        {
            PdfIndirectObject result;
            if (hashList.TryGetValue(hash, out result))
                return result;
            else
                return null;
        }

        private MemoryStream GetMask(int[] raw_pixels)
        {
            MemoryStream mask_stream = new MemoryStream(raw_pixels.Length);

            bool alpha = false;
            byte pixel;

            for (int i = 0; i < raw_pixels.Length; i++)
            {
                pixel = (byte)(((UInt32)raw_pixels[i]) >> 24);
                if (!alpha && pixel != 0xff)
                    alpha = true;
                mask_stream.WriteByte(pixel);
            }

            if (alpha)
            {
                mask_stream.Position = 0;
                return mask_stream;
            }

            return null;
        }

        private int[] GetRawBitmap(Bitmap image)
        {
            using (SKBitmap bitmap = ConvertToSkBitmap(image))
            {
                if (bitmap == null)
                    return Array.Empty<int>();

                int rawSize = bitmap.Width * bitmap.Height;
                int[] rawPicture = new int[rawSize];
                int index = 0;

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        SKColor color = bitmap.GetPixel(x, y);
                        rawPicture[index++] = (color.Alpha << 24) | (color.Red << 16) | (color.Green << 8) | color.Blue;
                    }
                }

                return rawPicture;
            }
        }

        private void SaveJpeg(Bitmap image, Stream buff, int quality)
        {
            using (SKBitmap bitmap = ConvertToSkBitmap(image))
            {
                if (bitmap == null)
                    return;

                using (SKImage skImage = SKImage.FromBitmap(bitmap))
                using (SKData data = skImage.Encode(SKEncodedImageFormat.Jpeg, Math.Max(0, Math.Min(100, quality))))
                {
                    data?.SaveTo(buff);
                }
            }
        }

        private SKBitmap ConvertToSkBitmap(Bitmap image)
        {
            if (image == null)
                return null;

            SKBitmap bitmap = new SKBitmap(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    bitmap.SetPixel(x, y, new SKColor(pixel.R, pixel.G, pixel.B, pixel.A));
                }
            }

            return bitmap;
        }

        private void SetImageByHash(string hash, PdfIndirectObject obj)
        {
            hashList.Add(hash, obj);
        }

        private PdfIndirectObject WriteImage(MemoryStream image, MemoryStream mask, int width, int height)
        {
            if (image == null || image.Length == 0)
                return null;

            PdfImage pdfImage = new PdfImage();
            pdfImage.Width = width;
            pdfImage.Height = height;
            pdfImage.Stream = image.ToArray();

            if (mask != null && mask.Length > 0)
            {
                PdfMask pdfMask = new PdfMask();
                pdfMask.Width = width;
                pdfMask.Height = height;
                pdfMask.Stream = mask.ToArray();

                pdfImage["SMask"] = pdfWriter.Write(pdfMask);
            }

            return pdfWriter.Write(pdfImage);
        }

        #endregion Private Methods
    }
}