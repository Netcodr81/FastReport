using FastReport.Utils;
using SkiaSharp;
using System;
using System.ComponentModel;
using System.IO;
using GraphicsPath = SkiaSharp.SKPath;
using Image = SkiaSharp.SKBitmap;
using RectangleF = SkiaSharp.SKRect;
using PointF = SkiaSharp.SKPoint;

namespace FastReport
{
    /// <summary>
    /// Represents a Picture object that can display pictures.
    /// </summary>
    /// <remarks>
    /// The Picture object can display the following kind of pictures:
    /// <list type="bullet">
    ///   <item>
    ///     <description>picture that is embedded in the report file. Use the <see cref="Image"/>
    ///     property to do this;</description>
    ///   </item>
    ///   <item>
    ///     <description>picture that is stored in the database BLOb field. Use the <see cref="PictureObjectBase.DataColumn"/>
    ///     property to specify the name of data column you want to show;</description>
    ///   </item>
    ///   <item>
    ///     <description>picture that is stored in the local disk file. Use the <see cref="PictureObjectBase.ImageLocation"/>
    ///     property to specify the name of the file;</description>
    ///   </item>
    ///   <item>
    ///     <description>picture that is stored in the Web. Use the <see cref="PictureObjectBase.ImageLocation"/>
    ///     property to specify the picture's URL.</description>
    ///   </item>
    /// </list>
    /// <para/>Use the <see cref="PictureObjectBase.SizeMode"/> property to specify a size mode. The <see cref="PictureObjectBase.MaxWidth"/>
    /// and <see cref="PictureObjectBase.MaxHeight"/> properties can be used to restrict the image size if <b>SizeMode</b>
    /// is set to <b>AutoSize</b>.
    /// <para/>The <see cref="TransparentColor"/> property can be used to display an image with
    /// transparent background. Use the <see cref="Transparency"/> property if you want to display
    /// semi-transparent image.
    /// </remarks>
    public partial class PictureObject : PictureObjectBase
    {
        #region Fields
        private SKBitmap image;

        private int imageIndex;

        private SKColor transparentColor;
        private float transparency;
        private bool tile;
        private SKBitmap transparentImage;
        private byte[] imageData;
        private bool shouldDisposeImage;
        private SKBitmap grayscaleBitmap;
        private int grayscaleHash;
        private ImageFormat imageFormat;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <remarks>
        /// By default, image that you assign to this property is never disposed - you should
        /// take care about it. If you want to dispose the image when this <b>PictureObject</b> is disposed,
        /// set the <see cref="ShouldDisposeImage"/> property to <b>true</b> right after you assign an image:
        /// <code>
        /// myPictureObject.Image = new Bitmap("file.bmp");
        /// myPictureObject.ShouldDisposeImage = true;
        /// </code>
        /// </remarks>
        [Category("Data")]
        public virtual SKBitmap Image
        {
            get { return image; }
            set
            {
                image = value;
                imageData = null;
                UpdateAutoSize();
                UpdateTransparentImage();
                ResetImageIndex();
                imageFormat = CheckImageFormat();
                ShouldDisposeImage = false;
            }
        }

        /// <summary>
        /// Gets the raw image data as a byte array. 
        /// </summary>
        internal byte[] ImageData
        {
            get { return imageData; }
        }

        /// <summary>
        /// Gets or sets the extension of image.
        /// </summary>
        [Category("Data")]
        public virtual ImageFormat ImageFormat
        {
            get { return imageFormat; }
            set
            {
                if (image == null)
                    return;
                bool wasC = false;
                using (MemoryStream stream = new MemoryStream())
                {
                    wasC = ImageHelper.SaveAndConvert(Image, stream, value);
                    imageData = stream.ToArray();
                }
                if (!wasC)
                    return;
                ForceLoadImage();
                imageFormat = CheckImageFormat();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating that the image should be displayed in grayscale mode.
        /// </summary>
        [DefaultValue(false)]
        [Category("Appearance")]
        public override bool Grayscale
        {
            get { return base.Grayscale; }
            set
            {
                base.Grayscale = value;
                if (!value && grayscaleBitmap != null)
                {
                    grayscaleBitmap.Dispose();
                    grayscaleBitmap = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets a hash of grayscale svg image
        /// </summary>
        [Browsable(false)]
        public int GrayscaleHash
        {
            get { return grayscaleHash; }
            set { grayscaleHash = value; }
        }

        /// <summary>
        /// Gets or sets the color of the image that will be treated as transparent.
        /// </summary>
        [Category("Appearance")]
        public SKColor TransparentColor
        {
            get { return transparentColor; }
            set
            {
                transparentColor = value;
                UpdateTransparentImage();
            }
        }

        /// <summary>
        /// Gets or sets the transparency of the PictureObject.
        /// </summary>
        /// <remarks>
        /// Valid range of values is 0..1. Default value is 0.
        /// </remarks>
        [DefaultValue(0f)]
        [Category("Appearance")]
        public float Transparency
        {
            get { return transparency; }
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 1)
                    value = 1;
                transparency = value;
                UpdateTransparentImage();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating that the image should be tiled.
        /// </summary>
        [DefaultValue(false)]
        [Category("Appearance")]
        public bool Tile
        {
            get { return tile; }
            set { tile = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating that the image stored in the <see cref="Image"/> 
        /// property should be disposed when this object is disposed.
        /// </summary>
        /// <remarks>
        /// By default, image assigned to the <see cref="Image"/> property is never disposed - you should
        /// take care about it. If you want to dispose the image when this <b>PictureObject</b> is disposed,
        /// set this property to <b>true</b> right after you assign an image to the <see cref="Image"/> property.
        /// </remarks>
        [Browsable(false)]
        public bool ShouldDisposeImage
        {
            get { return shouldDisposeImage; }
            set { shouldDisposeImage = value; }
        }

        /// <summary>
        /// Gets or sets a bitmap transparent image
        /// </summary>
        [Browsable(false)]
        public SKBitmap TransparentImage
        {
            get { return transparentImage; }
            set { transparentImage = value; }
        }

        /// <inheritdoc/>
        [Browsable(false)]
        protected override float ImageWidth
        {
            get
            {
                if (Image == null) return 0;
                return Image.Width;
            }
        }

        /// <inheritdoc/>
        [Browsable(false)]
        protected override float ImageHeight
        {
            get
            {
                if (Image == null) return 0;
                return Image.Height;
            }
        }
        #endregion

        #region Private Methods
        private ImageFormat CheckImageFormat()
        {
            return ImageFormat.Bmp;
        }

        private void UpdateTransparentImage()
        {
            if (transparentImage != null)
                transparentImage.Dispose();
            transparentImage = null;

            if (Image == null)
                return;

            if (TransparentColor != SKColors.Transparent)
                transparentImage = CreateTransparentColorBitmap(Image, TransparentColor);
            else if (Transparency != 0)
                transparentImage = ImageHelper.GetTransparentBitmap(Image, Transparency);
        }

        private static SKBitmap CreateTransparentColorBitmap(SKBitmap source, SKColor transparentColor)
        {
            if (source == null)
                return null;

            SKBitmap result = ImageHelper.CloneBitmap(source);
            for (int y = 0; y < result.Height; y++)
            {
                for (int x = 0; x < result.Width; x++)
                {
                    SKColor pixel = result.GetPixel(x, y);
                    if (pixel.Red == transparentColor.Red && pixel.Green == transparentColor.Green && pixel.Blue == transparentColor.Blue)
                        result.SetPixel(x, y, pixel.WithAlpha(0));
                }
            }
            return result;
        }

        private GraphicsPath GetRoundRectPath(RectangleF rectangleF, float radius)
        {
            if (radius < 1)
                radius = 1;

            GraphicsPath gp = new GraphicsPath();
            gp.AddRoundRect(rectangleF, radius, radius);
            return gp;
        }
        #endregion

        #region Protected Methods
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DisposeImage();
            base.Dispose(disposing);
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Assign(Base source)
        {
            base.Assign(source);

            PictureObject src = source as PictureObject;
            if (src != null)
            {
                TransparentColor = src.TransparentColor;
                Transparency = src.Transparency;
                Tile = src.Tile;
                Image = src.Image == null ? null : ImageHelper.CloneBitmap(src.Image);
                if (src.Image == null && src.imageData != null)
                    imageData = src.imageData;
                ShouldDisposeImage = true;
                ImageFormat = src.ImageFormat;
            }
        }

        /// <summary>
        /// Draws the image.
        /// </summary>
        /// <param name="e">Paint event args.</param>
        public override void DrawImage(FRPaintEventArgs e)
        {
            IGraphics g = e.Graphics;
            if (Image == null)
                ForceLoadImage();

            if (Image == null)
            {
                DrawErrorImage(g, e);
                return;
            }

            float drawLeft = (AbsLeft + Padding.Left) * e.ScaleX;
            float drawTop = (AbsTop + Padding.Top) * e.ScaleY;
            float drawWidth = (Width - Padding.Horizontal) * e.ScaleX;
            float drawHeight = (Height - Padding.Vertical) * e.ScaleY;

            RectangleF drawRect = new RectangleF(
              drawLeft,
              drawTop,
              drawLeft + drawWidth,
              drawTop + drawHeight);

            GraphicsPath path = new GraphicsPath();
            IGraphicsState state = g.Save();
            try
            {
                g.ResetClip();

                EstablishImageForm(path, drawLeft, drawTop, drawWidth, drawHeight);

                g.SetClip(path, SKClipOperation.Intersect);

                if (!Tile)
                {
                    using SKImage skImage = SKImage.FromBitmap(transparentImage ?? Image);
                    g.DrawImage(skImage, drawRect);
                }
                else
                {
                    using SKImage skImage = SKImage.FromBitmap(transparentImage ?? Image);
                    float y = drawRect.Top;
                    float width = Image.Width * e.ScaleX;
                    float height = Image.Height * e.ScaleY;
                    while (y < drawRect.Bottom)
                    {
                        float x = drawRect.Left;
                        while (x < drawRect.Right)
                        {
                            g.DrawImage(skImage, x, y, width, height);
                            x += width;
                        }
                        y += height;
                    }
                }
            }
            finally
            {
                g.Restore(state);
                g.ResetClip();
                path.Dispose();
            }

            if (IsPrinting)
            {
                DisposeImage();
            }
        }

        /// <inheritdoc/>
        protected override void DrawImageInternal2(IGraphics graphics, PointF upperLeft, PointF upperRight, PointF lowerLeft)
        {
            Image image = transparentImage != null ? transparentImage : Image;
            if (image == null)
                return;
            if (Grayscale)
            {
                if (grayscaleHash != image.GetHashCode() || grayscaleBitmap == null)
                {
                    if (grayscaleBitmap != null)
                        grayscaleBitmap.Dispose();
                    grayscaleBitmap = ImageHelper.GetGrayscaleBitmap(image);
                    grayscaleHash = image.GetHashCode();
                }

                image = grayscaleBitmap;
            }

            DrawImage3Points(graphics, image, upperLeft, upperRight, lowerLeft);
        }

        // This is analogue of graphics.DrawImage(image, PointF[] points) method. 
        // The original gdi+ method does not work properly in mono on linux/macos.
        private void DrawImage3Points(IGraphics g, Image image, PointF p0, PointF p1, PointF p2)
        {
            if (image == null || image.Width == 0 || image.Height == 0)
                return;
            if (p0 == p1 || p0 == p2)
                return;

            using SKImage skImage = SKImage.FromBitmap(image);
            g.DrawImage(skImage, new[] { p0, p1, p2 });
        }

        /// <summary>
        /// Sets image data to FImageData
        /// </summary>
        /// <param name="data"></param>
        public void SetImageData(byte[] data)
        {
            imageData = data;
            // if autosize is on, load the image.
            if (SizeMode.ToString() == "AutoSize")
                ForceLoadImage();
        }

        /// <inheritdoc/>
        public override void Serialize(FRWriter writer)
        {
            PictureObject c = writer.DiffObject as PictureObject;
            base.Serialize(writer);

if (TransparentColor != c.TransparentColor)
                writer.WriteValue("TransparentColor", TransparentColor);
            if (FloatDiff(Transparency, c.Transparency))
                writer.WriteFloat("Transparency", Transparency);
            if (Tile != c.Tile)
                writer.WriteBool("Tile", Tile);
            if (ImageFormat != c.ImageFormat)
                writer.WriteValue("ImageFormat", ImageFormat);
            // store image data
            if (writer.SerializeTo != SerializeTo.SourcePages)
            {
                if (writer.SerializeTo == SerializeTo.Preview ||
                    (String.IsNullOrEmpty(ImageLocation) && String.IsNullOrEmpty(DataColumn)) ||

                    // Next condition should work when serializing to Undo and only when designing the prepared page.
                    (writer.SerializeTo == SerializeTo.Undo && IsDesigningInPreviewPageDesigner())
                   )
                {
                    if (writer.BlobStore != null)
                    {
                        // check FImageIndex >= writer.BlobStore.Count is needed when we close the designer
                        // and run it again, the BlobStore is empty, but FImageIndex is pointing to
                        // previous BlobStore item and is not -1.
                        if (imageIndex == -1 || imageIndex >= writer.BlobStore.Count)
                        {
                            byte[] bytes = imageData;
                            if (bytes == null)
                            {
                                using (MemoryStream stream = new MemoryStream())
                                {
                                    ImageHelper.Save(Image, stream, imageFormat);
                                    bytes = stream.ToArray();
                                }
                            }
                            if (bytes != null)
                            {
                                string imgHash = BitConverter.ToString(new Murmur3().ComputeHash(bytes));
                                imageIndex = writer.BlobStore.AddOrUpdate(bytes, imgHash);
                            }
                        }
                    }
                    else
                    {
                        if (Image == null && imageData != null)
                            writer.WriteStr("Image", Convert.ToBase64String(imageData));
                        else if (!writer.AreEqual(Image, c.Image))
                            writer.WriteValue("Image", Image);
                    }

                    if (writer.BlobStore != null || writer.SerializeTo == SerializeTo.Undo)
                        writer.WriteInt("ImageIndex", imageIndex);
                }
            }
        }

        /// <inheritdoc/>
        public override void Deserialize(FRReader reader)
        {
            base.Deserialize(reader);
            if (reader.HasProperty("ImageIndex"))
            {
                imageIndex = reader.ReadInt("ImageIndex");
                if (reader.BlobStore != null && imageIndex != -1)
                {
                    //int saveIndex = FImageIndex;
                    //Image = ImageHelper.Load(reader.BlobStore.Get(FImageIndex));
                    //FImageIndex = saveIndex;
                    SetImageData(reader.BlobStore.Get(imageIndex));
                }
            }
        }

        /// <summary>
        /// Loads image
        /// </summary>
        public override void LoadImage()
        {
            if (!String.IsNullOrEmpty(ImageLocation))
            {
                // 
                try
                {
                    Uri uri = CalculateUri();
                    if (uri.IsFile)
                        SetImageData(ImageHelper.Load(uri.LocalPath));
                    else
                        SetImageData(ImageHelper.LoadURL(uri.ToString()));
                }
                catch
                {
                    Image = null;
                }

                ShouldDisposeImage = true;
            }
        }

        /// <summary>
        /// Disposes image
        /// </summary>
        public void DisposeImage()
        {
            if (Image != null && ShouldDisposeImage)
                Image.Dispose();
            if (grayscaleBitmap != null)
                grayscaleBitmap.Dispose();
            grayscaleBitmap = null;
            Image = null;
        }

        /// <inheritdoc/>
        protected override void ResetImageIndex()
        {
            imageIndex = -1;
        }

        /// <summary>
        /// The shape of the image is set using GraphicsPath
        /// </summary>
        /// <param name="path"></param>
        /// <param name="drawLeft"></param>
        /// <param name="drawTop"></param>
        /// <param name="drawWidth"></param>
        /// <param name="drawHeight"></param>
        public void EstablishImageForm(GraphicsPath path, float drawLeft, float drawTop, float drawWidth, float drawHeight)
        {
            RectangleF drawRect = new RectangleF(
              drawLeft,
              drawTop,
              drawLeft + drawWidth,
              drawTop + drawHeight);

            switch (Shape)
            {
                case ShapeKind.Rectangle:
                    path.AddRect(drawRect);
                    break;
                case ShapeKind.RoundRectangle:
                    float min = Math.Min(drawWidth, drawHeight) / 4;
                    path.AddPath(GetRoundRectPath(drawRect, min));
                    break;
                case ShapeKind.Ellipse:
                    path.AddOval(drawRect);
                    break;
                case ShapeKind.Triangle:
                    SKPoint[] triPoints =
                    {
                        new SKPoint(drawLeft + drawWidth, drawTop + drawHeight),
                        new SKPoint(drawLeft, drawTop + drawHeight),
                        new SKPoint(drawLeft + drawWidth / 2, drawTop)
                    };
                    path.AddPoly(triPoints, true);
                    break;
                case ShapeKind.Diamond:
                    SKPoint[] diaPoints =
                    {
                        new SKPoint(drawLeft + drawWidth / 2, drawTop),
                        new SKPoint(drawLeft + drawWidth, drawTop + drawHeight / 2),
                        new SKPoint(drawLeft + drawWidth / 2, drawTop + drawHeight),
                        new SKPoint(drawLeft, drawTop + drawHeight / 2)
                    };
                    path.AddPoly(diaPoints, true);
                    break;
            }
        }
        #endregion

        #region Report Engine


        /// <inheritdoc/>
        public override void InitializeComponent()
        {
            base.InitializeComponent();
            ResetImageIndex();
        }

        /// <inheritdoc/>
        public override void FinalizeComponent()
        {
            base.FinalizeComponent();
            ResetImageIndex();
        }



        /// <inheritdoc/>
        public override void GetData()
        {
            base.GetData();

            if (!String.IsNullOrEmpty(DataColumn))
            {
                // reset the image
                Image = null;
                imageData = null;

                object data = Report.GetColumnValueNullable(DataColumn);
                if (data is byte[])
                {
                    SetImageData((byte[])data);
                    return;
                }
                else if (data is Image)
                {
                    Image = data as Image;
                    return;
                }
                else if (data is string dataStr)
                {
                    SetImageLocation(dataStr, true);
                }
            }
            else
            {
                // no other data received
                UpdateImageLocation();
            }
        }

        /// <summary>
        /// Forces loading the image from a data column.
        /// </summary>
        /// <remarks>
        /// Call this method in the <b>AfterData</b> event handler to force loading an image 
        /// into the <see cref="Image"/> property. Normally, the image is stored internally as byte[] array 
        /// and never loaded into the <b>Image</b> property, to save the time. The side effect is that you 
        /// can't analyze the image properties such as width and height. If you need this, call this method
        /// before you access the <b>Image</b> property. Note that this will significantly slow down the report.
        /// </remarks>
        public void ForceLoadImage()
        {
            if (imageData == null)
                return;

            byte[] saveImageData = imageData;
            // FImageData will be reset after this line, keep it
            Image = ImageHelper.Load(imageData);
            imageData = saveImageData;
            ShouldDisposeImage = true;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureObject"/> class with default settings.
        /// </summary>
        public PictureObject()
        {
            transparentColor = SKColors.Transparent;
            SetFlags(Flags.HasSmartTag, true);
            ResetImageIndex();
        }

    }
}