using FastReport.Utils;
using System;
using System.Collections.Generic;
using SkiaSharp;
using GdiGraphics = FastReport.GdiGraphics;
using RectangleF = SkiaSharp.SKRect;
using Bitmap = SkiaSharp.SKBitmap;

namespace FastReport
{
    public partial class LineObject
    {
        /// <summary>
        /// Converts it to an picture object if it has cap.
        /// </summary>
        /// <returns>PictureObject</returns>

        public override IEnumerable<Base> GetConvertedObjects()
        {
            PictureObject pictObj = new PictureObject();
            pictObj.SetReport(Report);
            pictObj.Assign(this);
            pictObj.SetParentCore(this.Parent);

            SKRect skRect;
            CreatePath().GetBounds(out skRect);
            skRect.Left -= Border.Width / 2;
            skRect.Top -= Border.Width / 2;
            skRect.Right += Border.Width / 2;
            skRect.Bottom += Border.Width / 2;

            int width = (int)Math.Ceiling(skRect.Width);
            int height = (int)Math.Ceiling(skRect.Height);
            using SKBitmap bitmap = new SKBitmap(width, height);
            using (SKCanvas canvas = new SKCanvas(bitmap))
            using (GdiGraphics g = new GdiGraphics(canvas, false))
            {
                g.TranslateTransform(-skRect.Left, -skRect.Top);
                Draw(new FRPaintEventArgs(g, 1, 1, Report.GraphicCache));
                canvas.Flush();
            }

            using (SKImage image = SKImage.FromBitmap(bitmap))
            using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                pictObj.SetImageData(data.ToArray());
            }

            pictObj.Left += skRect.Left - pictObj.AbsLeft;
            pictObj.Top += skRect.Top - pictObj.AbsTop;
            pictObj.Width = skRect.Width;
            pictObj.Height = skRect.Height;

            yield return pictObj;
        }
    }
}