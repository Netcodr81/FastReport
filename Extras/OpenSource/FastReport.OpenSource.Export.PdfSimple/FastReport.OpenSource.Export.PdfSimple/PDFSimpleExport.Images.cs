using FastReport.Export.PdfSimple.PdfCore;
using FastReport.Export.PdfSimple.PdfObjects;
using FastReport.Utils;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace FastReport.Export.PdfSimple
{
    partial class PDFSimpleExport
    {
        #region Private Fields

        private Dictionary<string, PdfIndirectObject> hashList;

        #endregion Private Fields

        #region Private Methods

        private string AppendPDFImage(SKBitmap image, int quality)
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

        private void DrawImage(SKRect rectangleF, SKBitmap image)
        {
            string imageLink = AppendPDFImage(image, JpegQuality);
            pageContent.Append("q").AppendLine();
            pageContent.Append(rectangleF.Width).Append(" 0 0 ").Append(rectangleF.Height).Append(" ").Append(rectangleF.Left).Append(" ").Append(rectangleF.Top).Append(" cm").AppendLine();
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

        private int[] GetRawBitmap(SKBitmap image)
        {
            int rawSize = image.Width * image.Height;
            int[] rawPicture = new int[rawSize];
            SKColor[] pixels = image.Pixels;
            for (int i = 0; i < rawSize; i++)
                rawPicture[i] = unchecked((int)pixels[i]);
            return rawPicture;
        }

        private void SaveJpeg(SKBitmap image, Stream buff, int quality)
        {
            using SKImage skImage = SKImage.FromBitmap(image);
            using SKData data = skImage.Encode(SKEncodedImageFormat.Jpeg, quality);
            data?.SaveTo(buff);
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