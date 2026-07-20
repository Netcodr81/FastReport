using System;
using System.Collections.Generic;
using System.IO;

using FastReport.Utils;

using SkiaSharp;

namespace FastReport.Export.Image;

/// <summary>
/// Specifies the image export format.
/// </summary>
public enum ImageExportFormat
{
    /// <summary>
    /// Specifies the .bmp format.
    /// </summary>
    Bmp,

    /// <summary>
    /// Specifies the .png format.
    /// </summary>
    Png,

    /// <summary>
    /// Specifies the .jpg format.
    /// </summary>
    Jpeg,

    /// <summary>
    /// Specifies the .gif format.
    /// </summary>
    Gif,

    /// <summary>
    /// Specifies the .tif format.
    /// </summary>
    Tiff,

    /// <summary>
    /// Specifies the .emf format. NOTE: Not supported in cross-platform version.
    /// </summary>
    [Obsolete("Metafile format is not supported in cross-platform builds. Use PNG or other raster formats.")]
    Metafile
}

/// <summary>
/// Represents the image export filter using SkiaSharp for cross-platform support.
/// </summary>
/// <remarks>
/// This version uses only SkiaSharp and does not support:
/// - Metafile (.emf) export (use PNG or PDF instead)
/// - Multi-frame TIFF (each page saves as separate TIFF)
/// - Monochrome TIFF compression options (saves as standard TIFF)
/// </remarks>
public partial class ImageExport : ExportBase
{
    private ImageExportFormat imageFormat;
    private bool separateFiles;
    private int resolutionX;
    private int resolutionY;
    private int jpegQuality;
    private int paddingNonSeparatePages;
    private int pageNumber;
    private string imageExtensionFormat;
    private string documentTitle;
    private bool saveStreams;

    // SkiaSharp rendering surfaces
    private SKSurface bigSurface;
    private SKImage bigImage;
    private SKCanvas bigGraphics;
    private SKSurface currentSurface;
    private SKImage currentImage;
    private SKCanvas g;

    // State tracking
    private float curOriginY;
    private bool firstPage;
    private int height;
    private int width;
    private string fileSuffix;
    private float zoomX;
    private float zoomY;
    private int savedState;

    const float DIVIDER = 0.75f;
    const float PAGE_DIVIDER = 2.8346400000000003f; // mm to point

    #region Properties
    /// <summary>
    /// Gets or sets the image format.
    /// </summary>
    public ImageExportFormat ImageFormat
    {
        get { return imageFormat; }
        set
        {
            if (value == ImageExportFormat.Metafile)
                throw new NotSupportedException("Metafile format is not supported in cross-platform builds. Use PNG, JPEG, or PDF export instead.");
            imageFormat = value;
        }
    }

    /// <summary>
    /// Gets or sets a value that determines whether to generate separate image file 
    /// for each exported page.
    /// </summary>
    /// <remarks>
    /// If this property is set to <b>false</b>, the export filter will produce one big image
    /// containing all exported pages. Be careful using this property with a big report
    /// because it may produce out of memory error.
    /// </remarks>
    public bool SeparateFiles
    {
        get { return separateFiles; }
        set { separateFiles = value; }
    }

    /// <summary>
    /// Gets or sets image resolution, in dpi.
    /// </summary>
    /// <remarks>
    /// By default this property is set to 96 dpi. Use bigger values (300-600 dpi)
    /// if you going to print the exported images.
    /// </remarks>
    public int Resolution
    {
        get { return resolutionX; }
        set
        {
            resolutionX = value;
            resolutionY = value;
        }
    }

    /// <summary>
    /// Gets or sets horizontal image resolution, in dpi.
    /// </summary>
    public int ResolutionX
    {
        get { return resolutionX; }
        set { resolutionX = value; }
    }

    /// <summary>
    /// Gets or sets vertical image resolution, in dpi.
    /// </summary>
    public int ResolutionY
    {
        get { return resolutionY; }
        set { resolutionY = value; }
    }

    /// <summary>
    /// Gets or sets the jpg image quality.
    /// </summary>
    /// <remarks>
    /// This property is used if <see cref="ImageFormat"/> is set to <b>Jpeg</b>. By default
    /// it is set to 100. Use lesser value to decrease the jpg file size.
    /// </remarks>
    public int JpegQuality
    {
        get { return jpegQuality; }
        set { jpegQuality = value; }
    }

    /// <summary>
    /// Gets or sets the value determines whether to produce multi-frame tiff file.
    /// </summary>
    /// <remarks>
    /// NOTE: Multi-frame TIFF is not supported in the cross-platform version.
    /// Each page will be saved as a separate TIFF file regardless of this setting.
    /// </remarks>
    [Obsolete("Multi-frame TIFF is not supported in cross-platform builds. Each page saves as separate TIFF.")]
    public bool MultiFrameTiff
    {
        get { return false; }
        set { /* Ignored - not supported */ }
    }

    /// <summary>
    /// Gets or sets a value that determines whether the Tiff export must produce monochrome image.
    /// </summary>
    /// <remarks>
    /// NOTE: Monochrome TIFF with compression is not supported in the cross-platform version.
    /// TIFF files will be saved as standard color images.
    /// </remarks>
    [Obsolete("Monochrome TIFF is not supported in cross-platform builds. Use standard color TIFF.")]
    public bool MonochromeTiff
    {
        get { return false; }
        set { /* Ignored - not supported */ }
    }

    /// <summary>
    /// Sets padding in non separate pages
    /// </summary>
    public int PaddingNonSeparatePages
    {
        get { return paddingNonSeparatePages; }
        set { paddingNonSeparatePages = value; }
    }

    /// <summary>
    /// Enable or disable saving streams in GeneratedStreams collection.
    /// </summary>
    public bool SaveStreams
    {
        get { return saveStreams; }
        set { saveStreams = value; }
    }

    #endregion

    #region Private Methods

    private void GeneratedUpdate(string filename, Stream stream)
    {
        int i = GeneratedFiles.IndexOf(filename);
        if (i == -1)
        {
            GeneratedFiles.Add(filename);
            GeneratedStreams.Add(stream);
        }
        else
        {
            GeneratedStreams[i] = stream;
        }
    }

    private SKSurface CreateSurface(int width, int height)
    {
        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        info = info.WithSize(width, height);
        return SKSurface.Create(info);
    }

    private void SaveImage(SKImage image, string suffix)
    {
        if (image == null)
            return;

        Stream stream;
        string targetFileName;

        if (saveStreams)
        {
            targetFileName = string.IsNullOrEmpty(suffix)
                ? Path.ChangeExtension(documentTitle, imageExtensionFormat)
                : Path.ChangeExtension(documentTitle + $" ({suffix})", imageExtensionFormat);
            stream = new MemoryStream();
        }
        else
        {
            string extension = Path.GetExtension(FileName);
            targetFileName = Path.ChangeExtension(FileName, suffix + extension);

            // empty suffix means that we should use the Stream that was created in the ExportBase
            stream = suffix == "" ? Stream : new FileStream(targetFileName, FileMode.Create);

            if (suffix != "")
            {
                GeneratedFiles.Add(targetFileName);
            }
        }

        // Determine SkiaSharp format
        SKEncodedImageFormat format = ImageFormat switch
        {
            ImageExportFormat.Png => SKEncodedImageFormat.Png,
            ImageExportFormat.Jpeg => SKEncodedImageFormat.Jpeg,
            ImageExportFormat.Bmp => SKEncodedImageFormat.Bmp,
            ImageExportFormat.Gif => SKEncodedImageFormat.Gif,
            ImageExportFormat.Tiff => SKEncodedImageFormat.Webp, // SkiaSharp doesn't support TIFF encoding, use WebP or PNG
            _ => SKEncodedImageFormat.Png
        };

        // Encode and save
        using (SKData data = image.Encode(format, jpegQuality))
        {
            if (data != null)
            {
                data.SaveTo(stream);
            }
        }

        if (saveStreams)
            GeneratedUpdate(targetFileName, stream);
        else if (suffix != "")
            stream.Dispose();
    }

    #endregion

    #region Protected Methods

    /// <inheritdoc/>
    protected override string GetFileFilter()
    {
        string filter = ImageFormat.ToString();
        return Res.Get("FileFilters," + filter + "File");
    }

    /// <inheritdoc/>
    protected override void Start()
    {
        base.Start();

        //init
        SeparateFiles = Stream is MemoryStream ? false : SeparateFiles;
        GeneratedStreams = new List<Stream>();
        pageNumber = 0;
        height = 0;
        width = 0;
        currentImage = null;
        currentSurface = null;
        g = null;
        zoomX = 1;
        zoomY = 1;
        savedState = 0;

        curOriginY = 0;
        firstPage = true;

        if (saveStreams)
        {
            imageExtensionFormat = ImageFormat.ToString().ToLowerInvariant();
            separateFiles = true;
            documentTitle = !String.IsNullOrEmpty(Report.ReportInfo.Name) ?
                Report.ReportInfo.Name : Path.GetFileNameWithoutExtension(Report.FileName);
        }

        if (!SeparateFiles)
        {
            // create one big image. To do this, calculate max width and sum of pages height
            float w = 0;
            float h = 0;

            foreach (int pageNo in Pages)
            {
                SKSize size = GetPageSize(pageNo);
                if (size.Width > w)
                    w = size.Width;
                h += size.Height + paddingNonSeparatePages * 2;
            }

            w += paddingNonSeparatePages * 2;

            bigSurface = CreateSurface((int)(w * ResolutionX / 96f), (int)(h * ResolutionY / 96f));
            bigGraphics = bigSurface.Canvas;
            bigGraphics.Clear(SKColors.Transparent);
        }
        pageNumber = 0;
    }

    private SKSize GetPageSize(int pageNo)
    {
        // Get page size from Report.PreparedPages
        // This is a helper method to get SKSize instead of System.Drawing.SizeF
        ReportPage page = Report.PreparedPages.GetPage(pageNo);
        if (page != null)
        {
            float widthMm = ExportUtils.GetPageWidth(page) * Units.Millimeters;
            float heightMm = ExportUtils.GetPageHeight(page) * Units.Millimeters;
            return new SKSize(widthMm, heightMm);
        }
        return new SKSize(210, 297); // A4 default
    }

    /// <inheritdoc/>
    protected override void ExportPageBegin(ReportPage page)
    {
        base.ExportPageBegin(page);
        zoomX = ResolutionX / 96f;
        zoomY = ResolutionY / 96f;
        width = (int)(ExportUtils.GetPageWidth(page) * Units.Millimeters * zoomX);
        height = (int)(ExportUtils.GetPageHeight(page) * Units.Millimeters * zoomY);
        int suffixDigits = Pages[Pages.Length - 1].ToString().Length;
        fileSuffix = firstPage ? "" : (pageNumber + 1).ToString("".PadLeft(suffixDigits, '0'));

        if (SeparateFiles)
        {
            currentSurface = CreateSurface(width, height);
            g = currentSurface.Canvas;
        }
        else
        {
            g = bigGraphics;
        }

        // Save graphics state
        savedState = g.Save();

        // Fill background
        g.Clear(SKColors.Transparent);

        // Draw page background if it has fill
        if (page.Fill != null && page.Fill is FastReport.SolidFill solidFill)
        {
            using (var paint = new SKPaint())
            {
                paint.Color = solidFill.Color;
                paint.Style = SKPaintStyle.Fill;
                g.DrawRect(0, 0, width, height, paint);
            }
        }

        // Apply transformations
        if (!SeparateFiles)
        {
            g.Translate(paddingNonSeparatePages, curOriginY + paddingNonSeparatePages);
        }

        g.Scale(zoomX, zoomY);

        // Draw bottom watermarks
        if (page.Watermark.Enabled && !page.Watermark.ShowImageOnTop)
            AddImageWatermark(page);
        if (page.Watermark.Enabled && !page.Watermark.ShowTextOnTop)
            AddTextWatermark(page);

        // Draw page borders
        if (page.Border.Lines != null && page.Border.Lines != BorderLines.None)
        {
            float pageWidth = ExportUtils.GetPageWidth(page) * Units.Millimeters;
            float pageHeight = ExportUtils.GetPageHeight(page) * Units.Millimeters;

            using (var paint = new SKPaint())
            {
                paint.Color = ConvertColor(page.Border.Color);
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = page.Border.Width;

                var rect = new SKRect(0, 0, pageWidth, pageHeight);
                g.DrawRect(rect, paint);
            }
        }

        firstPage = false;
    }

    private SKColor ConvertColor(System.Drawing.Color color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    private SKColor ConvertColor(SKColor color)
    {
        return color;
    }

    /// <inheritdoc/>
    protected override void ExportBand(BandBase band)
    {
        base.ExportBand(band);
        ExportObj(band);

        foreach (Base c in band.ForEachAllConvectedObjects(this))
        {
            // skip table internals
            if (!(c is Table.TableColumn || c is Table.TableCell || c is Table.TableRow))
                ExportObj(c);
        }
    }

    private void ExportObj(Base obj)
    {
        ReportComponentBase c = obj as ReportComponentBase;
        if (c != null && c.Exportable)
        {
            using var graphics = new GdiGraphics(g, false);
            var paintArgs = new FRPaintEventArgs(graphics, zoomX, zoomY, Report.GraphicCache);
            c.Draw(paintArgs);
        }
    }

    /// <inheritdoc/>
    protected override void ExportPageEnd(ReportPage page)
    {
        base.ExportPageEnd(page);

        // Draw top watermarks
        if (page.Watermark.Enabled && page.Watermark.ShowImageOnTop)
            AddImageWatermark(page);
        if (page.Watermark.Enabled && page.Watermark.ShowTextOnTop)
            AddTextWatermark(page);

        // Restore graphics state
        g.RestoreToCount(savedState);

        if (SeparateFiles)
        {
            // Save current page
            currentImage = currentSurface.Snapshot();
            SaveImage(currentImage, fileSuffix);

            // Cleanup
            currentImage?.Dispose();
            currentImage = null;
            g = null;
            currentSurface?.Dispose();
            currentSurface = null;
        }
        else
        {
            // Update position for next page in combined image
            curOriginY += height + paddingNonSeparatePages * 2;
        }

        pageNumber++;
    }

    private void AddImageWatermark(ReportPage page)
    {
        if (page.Watermark.Image != null)
        {
            using var graphics = new GdiGraphics(g, false);
            var rect = new SKRect(-page.LeftMargin * Units.Millimeters, -page.TopMargin * Units.Millimeters, width / zoomX, height / zoomY);
            page.Watermark.DrawImage(new FRPaintEventArgs(graphics, zoomX, zoomY, Report.GraphicCache), rect, page.Report, false);
        }
    }

    private void AddTextWatermark(ReportPage page)
    {
        if (!String.IsNullOrEmpty(page.Watermark.Text))
        {
            using var graphics = new GdiGraphics(g, false);
            var rect = new SKRect(-page.LeftMargin * Units.Millimeters, -page.TopMargin * Units.Millimeters, width / zoomX, height / zoomY);
            page.Watermark.DrawText(new FRPaintEventArgs(graphics, zoomX, zoomY, Report.GraphicCache), rect, page.Report, false);
        }
    }

    /// <inheritdoc/>
    protected override void Finish()
    {
        base.Finish();

        if (!SeparateFiles)
        {
            // Save the combined big image
            bigImage = bigSurface.Snapshot();
            SaveImage(bigImage, "");

            // Cleanup
            bigImage?.Dispose();
            bigGraphics = null;
            bigSurface?.Dispose();
        }
    }

    #endregion

    #region Public Methods

    /// <inheritdoc/>
    public override void Serialize(FRWriter writer)
    {
        base.Serialize(writer);
        writer.WriteValue("ImageFormat", ImageFormat);
        writer.WriteBool("SeparateFiles", SeparateFiles);
        writer.WriteInt("ResolutionX", ResolutionX);
        writer.WriteInt("ResolutionY", ResolutionY);
        writer.WriteInt("JpegQuality", JpegQuality);
    }

    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageExport"/> class.
    /// </summary>
    public ImageExport()
    {
        imageFormat = ImageExportFormat.Jpeg;
        separateFiles = true;
        resolutionX = 96;
        resolutionY = 96;
        jpegQuality = 100;
        saveStreams = false;
    }
}