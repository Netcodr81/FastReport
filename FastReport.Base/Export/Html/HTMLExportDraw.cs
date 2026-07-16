using System;
using System.IO;
using FastReport.Utils;
using System.Globalization;
using SkiaSharp;

namespace FastReport.Export.Html
{
    public partial class HTMLExport : ExportBase
    {
        private void HTMLFontStyle(FastString FFontDesc, SKFont font, float LineHeight)
        {
            // Extract font style from SKTypeface
            bool isBold = font.Typeface.FontWeight >= (int)SKFontStyleWeight.SemiBold;
            bool isItalic = font.Typeface.FontSlant != SKFontStyleSlant.Upright;

            FFontDesc.Append(isBold ? "font-weight:bold;" : String.Empty)
                     .Append(isItalic ? "font-style:italic;" : "font-style:normal;");

            // Note: SKFont doesn't have underline/strikeout as they're typically handled by text rendering
            // These would be managed at a higher level in FastReport

            FFontDesc.Append("font-family:").Append(font.Typeface.FamilyName).Append(";");
            FFontDesc.Append("font-size:").Append(Px(font.Size * 96 / 72));

            if (LineHeight > 0)
            {
                FFontDesc.Append("line-height:").Append(Px(LineHeight)).Append(";");
            }
            else
            {
                // SkiaSharp font metrics
                SKFontMetrics metrics = font.Metrics;
                float lineSpace = metrics.Descent - metrics.Ascent + metrics.Leading;
                float height = font.Size;
                FFontDesc.Append($"line-height: {Math.Round(lineSpace / height, 2).ToString(CultureInfo.InvariantCulture)};");
            }
        }

        private void HTMLPadding(FastString PaddingDesc, System.Windows.Forms.Padding padding, float ParagraphOffset)
        {
            PaddingDesc.Append("text-indent:").Append(Px(ParagraphOffset));
            PaddingDesc.Append("padding-left:").Append(Px(padding.Left));
            PaddingDesc.Append("padding-right:").Append(Px(padding.Right));
            PaddingDesc.Append("padding-top:").Append(Px(padding.Top));
            PaddingDesc.Append("padding-bottom:").Append(Px(padding.Bottom));
        }

        private string HTMLBorderStyle(BorderLine line)
        {
            switch (line.Style)
            {
                case LineStyle.Dash:
                case LineStyle.DashDot:
                case LineStyle.DashDotDot:
                    return "dashed";
                case LineStyle.Dot:
                    return "dotted";
                case LineStyle.Double:
                    return "double";
                default:
                    return "solid";
            }
        }

        private float HTMLBorderWidth(BorderLine line)
        {
            if (line.Style == LineStyle.Double)
                return (line.Width * 3 * Zoom);
            else
                return line.Width * Zoom;
        }

        private string HTMLBorderWidthPx(BorderLine line)
        {
            if (line.Style != LineStyle.Double && line.Width == 1 && Zoom == 1)
                return "1px;";
            float width;
            if (line.Style == LineStyle.Double)
                width = line.Width * 3 * Zoom;
            else if (layers)
                width = line.Width * Zoom;
            else
                width = line.Width;

            return ExportUtils.FloatToString(width) + "px;";
        }

        private void HTMLBorder(FastString BorderDesc, Border border)
        {
            if (!layers)
                BorderDesc.Append("border-collapse: separate;");
            if (border.Lines > 0)
            {
                // bottom
                if ((border.Lines & BorderLines.Bottom) > 0)
                    BorderDesc.Append("border-bottom-width:").
                        Append(HTMLBorderWidthPx(border.BottomLine)).
                        Append("border-bottom-color:").
                        Append(ExportUtils.HTMLColor(border.BottomLine.Color)).Append(";border-bottom-style:").
                        Append(HTMLBorderStyle(border.BottomLine)).Append(";");
                else
                    BorderDesc.Append("border-bottom:none;");
                // top
                if ((border.Lines & BorderLines.Top) > 0)
                    BorderDesc.Append("border-top-width:").
                        Append(HTMLBorderWidthPx(border.TopLine)).
                        Append("border-top-color:").
                        Append(ExportUtils.HTMLColor(border.TopLine.Color)).Append(";border-top-style:").
                        Append(HTMLBorderStyle(border.TopLine)).Append(";");
                else
                    BorderDesc.Append("border-top:none;");
                // left
                if ((border.Lines & BorderLines.Left) > 0)
                    BorderDesc.Append("border-left-width:").
                        Append(HTMLBorderWidthPx(border.LeftLine)).
                        Append("border-left-color:").
                        Append(ExportUtils.HTMLColor(border.LeftLine.Color)).Append(";border-left-style:").
                        Append(HTMLBorderStyle(border.LeftLine)).Append(";");
                else
                    BorderDesc.Append("border-left:none;");
                // right
                if ((border.Lines & BorderLines.Right) > 0)
                    BorderDesc.Append("border-right-width:").
                        Append(HTMLBorderWidthPx(border.RightLine)).
                        Append("border-right-color:").
                        Append(ExportUtils.HTMLColor(border.RightLine.Color)).Append(";border-right-style:").
                        Append(HTMLBorderStyle(border.RightLine)).Append(";");
                else
                    BorderDesc.Append("border-right:none;");
            }
            else
                BorderDesc.Append("border:none;");
        }

        private bool HTMLBorderWidthValues(ReportComponentBase obj, out float left, out float top, out float right, out float bottom)
        {
            Border border = obj.Border;
            left = 0;
            top = 0;
            right = 0;
            bottom = 0;

            if (border.Lines > 0)
            {
                if ((border.Lines & BorderLines.Left) > 0)
                    left += HTMLBorderWidth(border.LeftLine);

                if ((border.Lines & BorderLines.Right) > 0)
                    right += HTMLBorderWidth(border.RightLine);

                if ((border.Lines & BorderLines.Top) > 0)
                    top += HTMLBorderWidth(border.TopLine);

                if ((border.Lines & BorderLines.Bottom) > 0)
                    bottom += HTMLBorderWidth(border.BottomLine);

                return true;
            }

            return false;
        }

        private void HTMLAlign(FastString sb, HorzAlign horzAlign, VertAlign vertAlign, bool wordWrap)
        {
            sb.Append("text-align:");
            if (horzAlign == HorzAlign.Left)
                sb.Append("Left");
            else if (horzAlign == HorzAlign.Right)
                sb.Append("Right");
            else if (horzAlign == HorzAlign.Center)
                sb.Append("Center");
            else if (horzAlign == HorzAlign.Justify)
                sb.Append("Justify");
            sb.Append(";vertical-align:");
            if (vertAlign == VertAlign.Top)
                sb.Append("Top");
            else if (vertAlign == VertAlign.Bottom)
                sb.Append("Bottom");
            else if (vertAlign == VertAlign.Center)
                sb.Append("Middle");
            if (wordWrap)
                sb.Append(";word-wrap:break-word");
            sb.Append(";overflow:hidden;");
        }

        private void HTMLRtl(FastString sb, bool rtl)
        {
            if (rtl)
                sb.Append("direction:rtl;");
        }

        private string HTMLGetStylesHeader()
        {
            return "<style type=\"text/css\"><!-- ";
        }

        private void PrintPageStyle(FastString sb)
        {
            if (singlePage && pageBreaks)
            {
                string paperProps = "size: portrait; ";
                sb.AppendLine("<style type=\"text/css\" media=\"print\"><!--");
                sb.Append("div.").Append(pageStyleName)
                    .Append(" { break-after: always; page-break-inside: avoid; ");
                if (d.page.Landscape && !NotRotateLandscapePage)
                {
                    sb.Append("width:").Append(Px(maxHeight * Zoom).Replace(";", " !important;"))
                          .Append("transform: rotate(90deg); -webkit-transform: rotate(90deg)");
                }
                else if(d.page.Landscape)
                {
                    paperProps = "size: landscape; ";
                }

                sb.AppendLine("}").Append(" @page { " + paperProps + "margin: 0; }")
                      .AppendLine("--></style>");
            }
        }

        private string HTMLGetStyleHeader(long index, long subindex)
        {
            FastString header = new FastString();
            return header.Append(".").
                Append(stylePrefix).
                Append("s").
                Append(index.ToString()).
                Append((singlePage || layers) ? String.Empty : String.Concat("-", subindex.ToString())).
                Append(" { ").ToString();
        }

        private void HTMLGetStyle(FastString style, SKFont Font, SKColor TextColor, SKColor FillColor, HorzAlign HAlign, VertAlign VAlign,
            Border Border, System.Windows.Forms.Padding Padding, bool RTL, bool wordWrap, float LineHeight, float ParagraphOffset)
        {
            HTMLFontStyle(style, Font, LineHeight);
            style.Append("color:").Append(SKColorToHTML(TextColor)).Append(";");
            style.Append("background-color:");
            style.Append(FillColor.Alpha == 0 ? "transparent" : SKColorToHTML(FillColor)).Append(";");
            HTMLAlign(style, HAlign, VAlign, wordWrap);
            HTMLBorder(style, Border);
            HTMLPadding(style, Padding, ParagraphOffset);
            HTMLRtl(style, RTL);
            style.AppendLine("}");
        }

        private string SKColorToHTML(SKColor color)
        {
            if (color.Alpha < 255)
            {
                string alphaValue = (color.Alpha / 255.0).ToString("0.00", CultureInfo.InvariantCulture);
                return $"rgba({color.Red}, {color.Green}, {color.Blue}, {alphaValue})";
            }
            return $"rgb({color.Red}, {color.Green}, {color.Blue})";
        }

        private string HTMLGetStylesFooter()
        {
            return "--></style>";
        }

        private string HTMLGetTagsStub()
        {
            return "p { margin-block-start: initial; margin-block-end: initial; }";
        }

        private string HTMLGetAncor(string ancorName)
        {
            FastString ancor = new FastString();
            return ancor.Append("<a name=\"PageN").Append(ancorName).Append("\" id=\"PageN").Append(ancorName).Append("\" style=\"padding:0;margin:0;font-size:1px;\"></a>").ToString();
        }

        private string HTMLGetImageTag(string file, string styles, bool isSvg)
        {
            if(isSvg)
                return $"<div style=\"background: url({file}) no-repeat {styles};\" />";
            return $"<img src=\"{file}\" style=\"{styles}\" alt=\"/>";
        } 

        private string HTMLGetImage(int PageNumber, int CurrentPage, int ImageNumber, string hash, bool Base,
            SKImage Metafile, MemoryStream PictureStream, bool isSvg)
        {
            if (pictures)
            {
                SKEncodedImageFormat format = SKEncodedImageFormat.Bmp;
                if (imageFormat == ImageFormat.Png)
                    format = SKEncodedImageFormat.Png;
                else if (imageFormat == ImageFormat.Jpeg)
                    format = SKEncodedImageFormat.Jpeg;
                else if (imageFormat == ImageFormat.Gif)
                    format = SKEncodedImageFormat.Gif;
                string formatNm = isSvg ? "svg" : format.ToString().ToLower();

                string embedImgType = isSvg ? "svg+xml" : format.ToString();
                string embedPreffix = "data:image/" + embedImgType + ";base64,";

                FastString ImageFileNameBuilder = new FastString(48);
                string ImageFileName;
                if (!saveStreams)
                    ImageFileNameBuilder.Append(Path.GetFileName(targetFileName)).Append(".");
                ImageFileNameBuilder.Append(hash).
                    Append(".").Append(formatNm);

                ImageFileName = ImageFileNameBuilder.ToString();

                if (!webMode && !(preview || print))
                {
                    if (Base)
                    {
                        if (Metafile != null && !EmbedPictures)
                        {
                            if (saveStreams)
                            {
                                MemoryStream ImageFileStream = new MemoryStream();
                                using (SKData data = Metafile.Encode(format, 100))
                                {
                                    data.SaveTo(ImageFileStream);
                                }
                                GeneratedUpdate(targetPath + ImageFileName, ImageFileStream);
                            }
                            else
                            {
                                using (FileStream ImageFileStream =
                                    new FileStream(targetPath + ImageFileName, FileMode.Create))
                                using (SKData data = Metafile.Encode(format, 100))
                                {
                                    data.SaveTo(ImageFileStream);
                                }
                            }
                        }
                        else if (PictureStream != null && !EmbedPictures)
                        {
                            if (this.format == HTMLExportFormat.HTML)
                            {
                                string fileName = targetPath + ImageFileName;
                                FileInfo info = new FileInfo(fileName);
                                if (!(info.Exists && info.Length == PictureStream.Length))
                                {
                                    if (saveStreams)
                                    {
                                        GeneratedUpdate(targetPath + ImageFileName, PictureStream);
                                    }
                                    else
                                    {
                                        using (FileStream ImageFileStream =
                                        new FileStream(fileName, FileMode.Create))
                                            PictureStream.WriteTo(ImageFileStream);
                                    }
                                }
                            }
                            else
                            {
                                PicsArchiveItem item = new PicsArchiveItem(ImageFileName, PictureStream);
                                bool founded = false;
                                for (int i = 0; i < picsArchive.Count; i++)
                                    if (item.FileName == picsArchive[i].FileName)
                                    {
                                        founded = true;
                                        break;
                                    }
                                if (!founded)
                                    picsArchive.Add(item);
                            }
                        }
                        if (!saveStreams)
                            GeneratedFiles.Add(targetPath + ImageFileName);
                    }
                    if (EmbedPictures && PictureStream != null)
                    {
                        return embedPreffix + GetBase64Image(PictureStream, hash);
                    }
                    else if (subFolder && singlePage && !navigator)
                        return ExportUtils.HtmlURL(subFolderPath + ImageFileName);
                    else
                        return ExportUtils.HtmlURL(ImageFileName);
                }
                else
                {
                    if (EmbedPictures)
                    {
                        return embedPreffix + GetBase64Image(PictureStream, hash);
                    }
                    else
                    {
                        if (print || preview)
                        {
                            printPageData.Pictures.Add(PictureStream);
                            printPageData.Guids.Add(hash);
                        }
                        else if (Base)
                        {
                            pages[CurrentPage].Pictures.Add(PictureStream);
                            pages[CurrentPage].Guids.Add(hash);
                        }
                        return webImagePrefix + "=" + hash + webImageSuffix;
                    }
                }
            }
            else
                return String.Empty;
        }

        private string GetBase64Image(MemoryStream PictureStream, string hash)
        {
            string base64Image = String.Empty;
            if (!EmbeddedImages.TryGetValue(hash, out base64Image))
            {
                base64Image = Convert.ToBase64String(PictureStream.ToArray());
                EmbeddedImages.Add(hash, base64Image);
            }
            return base64Image;
        }
    }
}
