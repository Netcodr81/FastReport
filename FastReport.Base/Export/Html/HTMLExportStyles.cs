using FastReport.Table;
using FastReport.Utils;
using SkiaSharp;
using System;

namespace FastReport.Export.Html
{
    public partial class HTMLExport : ExportBase
    {

        // TODO:
        private bool InlineStyles { get; set; }


        private string GetStyleFromObject(ReportComponentBase obj)
        {
            string style;
            if (obj is TextObject)
            {
                TextObject textObj = obj as TextObject;

                SKFont skFont = null;
                if (textObj.Font != null)
                {
                    var srcFont = textObj.Font;
                    SKTypeface srcTypeface = srcFont.Typeface ?? SKTypeface.Default;
                    SKFontStyle fontStyle = new SKFontStyle(srcTypeface.FontWeight, srcTypeface.FontWidth, srcTypeface.FontSlant);
                    SKTypeface typeface = SKTypeface.FromFamilyName(srcTypeface.FamilyName, fontStyle) ?? SKTypeface.Default;
                    skFont = new SKFont(typeface, srcFont.Size * (Zoom != 1 ? Zoom : 1));
                }

                SKColor textColor = textObj.TextColor;

                style = GetStyle(skFont, textColor, textObj.FillColor,
                    textObj.RightToLeft, textObj.HorzAlign, textObj.Border, textObj.WordWrap, textObj.LineHeight,
                    textObj.Width, textObj.Height, textObj.Clip);

                skFont?.Dispose();
            }
            else if (obj is HtmlObject)
            {
                HtmlObject htmlObj = obj as HtmlObject;

                SKFont skFont = null;
                var srcFont = DrawUtils.DefaultTextObjectFont;
                if (srcFont != null)
                {
                    SKTypeface srcTypeface = srcFont.Typeface ?? SKTypeface.Default;
                    SKFontStyle fontStyle = new SKFontStyle(srcTypeface.FontWeight, srcTypeface.FontWidth, srcTypeface.FontSlant);
                    SKTypeface typeface = SKTypeface.FromFamilyName(srcTypeface.FamilyName, fontStyle) ?? SKTypeface.Default;
                    skFont = new SKFont(typeface, srcFont.Size * (Zoom != 1 ? Zoom : 1));
                }

                style = GetStyle(skFont, SKColors.Black, htmlObj.FillColor,
                    false, HorzAlign.Left, htmlObj.Border, true, 0, htmlObj.Width, htmlObj.Height, false);

                skFont?.Dispose();
            }
            else
                style = GetStyle(null, SKColors.White, obj.FillColor, false, HorzAlign.Center, obj.Border, false, 0, obj.Width, obj.Height, false);
            return style;
        }

        private string GetStyle(SKFont font, SKColor textColor, SKColor fillColor,
            bool RTL, HorzAlign HAlign, Border Border, bool WordWrap, float LineHeight, float Width, float Height, bool Clip)
        {
            FastString style = new FastString(256);

            if (font != null)
            {
                HTMLFontStyle(style, font, LineHeight);
            }
            style.Append("text-align:");
            if (HAlign == HorzAlign.Left)
                style.Append(RTL ? "right" : "left");
            else if (HAlign == HorzAlign.Right)
                style.Append(RTL ? "left" : "right");
            else if (HAlign == HorzAlign.Center)
                style.Append("center");
            else
                style.Append("justify");
            style.Append(";");

            if (WordWrap)
                style.Append("word-wrap:break-word;");

            if (Clip)
                style.Append("overflow:hidden;");

            // Both colors are now SKColor
            style.Append("position:absolute;color:").
                Append(SKColorToHTML(textColor)).
                Append(";background-color:").
                Append(fillColor.Alpha == 0 ? "transparent" : SKColorToHTML(fillColor)).
                Append(";").Append(RTL ? "direction:rtl;" : String.Empty);

            Border newBorder = Border;
            HTMLBorder(style, newBorder);
            style.Append("width:").Append(Px(Math.Abs(Width) * Zoom)).Append("height:").Append(Px(Math.Abs(Height) * Zoom));
            return style.ToString();
        }

        private int UpdateCSSTable(ReportComponentBase obj)
        {
            var style = GetStyleFromObject(obj);
            return UpdateCSSTable(style);
        }

        private int UpdateCSSTable(string style)
        {
            int i = cssStyles.IndexOf(style);
            if (i == -1)
            {
                i = cssStyles.Count;
                cssStyles.Add(style);
            }
            return i;
        }


        private string GetStyle(string style)
        {
            if (InlineStyles)
            {
                return InlineStyle(style);
            }
            else
            {
                int index = UpdateCSSTable(style);
                return GetStyleTag(index);
            }
        }

        private string GetStyle(ReportComponentBase obj)
        {
            if (InlineStyles)
            {
                var style = GetStyleFromObject(obj);
                return InlineStyle(style);
            }
            else
            {
                int index = UpdateCSSTable(obj);
                return GetStyleTag(index);
            }
        }

        private string GetStyle(ReportComponentBase obj, string additionalStyle)
        {
            if (InlineStyles)
            {
                var style = GetStyleFromObject(obj);
                var resultStyle = string.Concat(style, " ", additionalStyle);
                return InlineStyle(resultStyle);
            }
            else
            {
                int index1 = UpdateCSSTable(obj);
                int index2 = UpdateCSSTable(additionalStyle);
                return GetStyleTag(index1, index2);
            }
        }

        private static string InlineStyle(string style)
        {
            return $"style=\"{style}\"";
        }

        private string GetStyleTag(int index)
        {
            return String.Format("class=\"{0}s{1}\"",
                stylePrefix,
                index.ToString()
            );
        }

        private string GetStyleTag(int index1, int index2)
        {
            return String.Format("class=\"{0}s{1} {0}s{2}\"",
                stylePrefix,
                index1.ToString(),
                index2.ToString()
            );
        }

    }
}
