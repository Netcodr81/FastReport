using SkiaSharp;
using System;

using System.Linq;

namespace FastReport.Utils
{
    internal enum MonoRendering
    {
        Undefined,
        Pango,
        Cairo
    }

    /// <summary>
    /// Contains properties and method related to draw operations.
    /// </summary>
    public static partial class DrawUtils
    {
        private static SKFont FDefaultFont;
        private static SKFont FDefaultReportFont;
        private static SKFont FDefaultTextObjectFont;
        private static SKFont FFixedFont;
        private static int FScreenDpi;
        private static float FDpiFX;
        private static MonoRendering FMonoRendering = MonoRendering.Undefined;

        /// <summary>
        /// Gets the primary screen dpi.
        /// </summary>
        public static int ScreenDpi
        {
            get
            {
                if (FScreenDpi == 0)
                    FScreenDpi = GetDpi();
                return FScreenDpi;
            }
        }

        /// <summary>
        /// Gets the primary screen dpi ratio (96 / ScreenDpi).
        /// </summary>
        public static float ScreenDpiFX
        {
            get
            {
                if (FDpiFX == 0f)
                    FDpiFX = 96f / DrawUtils.ScreenDpi;
                return FDpiFX;
            }
        }

        private static int GetDpi()
        {
            // SkiaSharp canvas does not expose monitor DPI directly.
            // Keep the conventional logical DPI used across .NET UI stacks.
            return 96;
        }

        private static float _uiScale = 1;

        /// <summary>
        /// Gets or sets a value that determines additional scale factor applied to FR forms.
        /// Used if you change the <see cref="DefaultFont"/> property.
        /// </summary>
        public static float UIScale
        {
            get => _uiScale;
            set
            {
                _uiScale = Math.Min(Math.Max(1f, value), 1.5f); // valid range is 1..1,5

            }
        }

        /// <summary>
        /// Gets or sets default font used in FR UI.
        /// </summary>
        /// <remarks>
        /// FR UI is optimized for "Tahoma 8.25pt" metrics. If you use larger font, set <see cref="UIScale"/>
        /// property to scale forms.
        /// </remarks>
        public static SKFont DefaultFont
        {
            get
            {
                if (FDefaultFont == null)
                {

                    switch (System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                    {
                        case "ja":
                            FDefaultFont = CreateFont("MS UI Gothic", 9);
                            break;

                        case "zh":
                            FDefaultFont = CreateFont("SimSun", 9);
                            break;

                        default:
                            FDefaultFont = CreateFont("Segoe UI", 8.5f);
                            break;
                    }

                }
                return FDefaultFont;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                FDefaultFont = value;

            }
        }

        /// <summary>
        /// Gets default report font (locale specific).
        /// </summary>
        /// <remarks>
        /// On most locales this is Arial,10. ja,zh locales use different fonts (MS UI Gothic,9 and SimSun,9).
        /// </remarks>
        public static SKFont DefaultReportFont
        {
            get
            {
                if (FDefaultReportFont == null)
                {
                    switch (System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName)
                    {
                        case "ja":
                            FDefaultReportFont = CreateFont("MS UI Gothic", 9);
                            break;

                        case "zh":
                            FDefaultReportFont = CreateFont("SimSun", 9);
                            break;

                        default:
                            FDefaultReportFont = CreateFont("Arial", 10);
                            break;
                    }
                }
                return FDefaultReportFont;
            }
        }

        /// <summary>
        /// Gets the default text object's font (Arial, 10). 
        /// </summary>
        public static SKFont DefaultTextObjectFont
        {
            get
            {
                if (FDefaultTextObjectFont == null)
                    FDefaultTextObjectFont = CreateFont("Arial", 10);
                return FDefaultTextObjectFont;
            }
        }

        /// <summary>
        /// Gets default fixed font.
        /// </summary>
        public static SKFont FixedFont
        {
            get
            {
                if (FFixedFont == null)
#if WPF
                    FFixedFont = CreateFont("Consolas", 9);
#elif AVALONIA
                    if (OperatingSystem.IsWindows())
                    {
                        FFixedFont = CreateFont("Lucida Console", 9);
                    }
                    else if (OperatingSystem.IsMacOS())
                    {
                        FFixedFont = CreateFont("PT Mono", 9);
                    }
                    else if (OperatingSystem.IsLinux())
                    {
                        FFixedFont = CreateFont("Liberation Mono", 9);
                    }
#else
                    FFixedFont = CreateFont("Courier New", 10);
#endif
                return FFixedFont;
            }
        }

        internal static SKFont CreateFont(string familyName, float emSize,
            SKFontStyleWeight weight = SKFontStyleWeight.Normal,
            SKFontStyleWidth width = SKFontStyleWidth.Normal,
            SKFontStyleSlant slant = SKFontStyleSlant.Upright)
        {
            var typeface = SKTypeface.FromFamilyName(familyName, new SKFontStyle((int)weight, (int)width, slant));
            return new SKFont(typeface, emSize);
        }

        internal static System.Drawing.Font ToSystemDrawingFont(SKFont skFont)
        {
            if (skFont == null)
                return new System.Drawing.Font("Arial", 10f);

            string family = skFont.Typeface?.FamilyName;
            if (string.IsNullOrWhiteSpace(family))
                family = "Arial";

            System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular;
            var fontStyle = skFont.Typeface?.FontStyle ?? new SKFontStyle((int)SKFontStyleWeight.Normal, (int)SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            if (fontStyle.Weight >= (int)SKFontStyleWeight.SemiBold)
                style |= System.Drawing.FontStyle.Bold;
            if (fontStyle.Slant == SKFontStyleSlant.Italic || fontStyle.Slant == SKFontStyleSlant.Oblique)
                style |= System.Drawing.FontStyle.Italic;

            return new System.Drawing.Font(family, skFont.Size <= 0 ? 10f : skFont.Size, style);
        }

        internal static SKFont ToSkFont(System.Drawing.Font font)
        {
            if (font == null)
                return CreateFont("Arial", 10f);

            var skStyle = new SKFontStyle(
                font.Bold ? (int)SKFontStyleWeight.Bold : (int)SKFontStyleWeight.Normal,
                (int)SKFontStyleWidth.Normal,
                font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

            var typeface = SKTypeface.FromFamilyName(font.FontFamily?.Name ?? "Arial", skStyle);
            return new SKFont(typeface, font.Size <= 0 ? 10f : font.Size);
        }

        internal static SKFont ResizeFont(SKFont font, float size)
        {
            if (font == null)
                return CreateFont("Arial", Math.Max(1f, size));

            return new SKFont(font.Typeface, Math.Max(1f, size));
        }

        /// <summary>
        /// Measures a text.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <returns>The size of text.</returns>
        public static SKSize MeasureString(string text)
        {
            return MeasureString(text, DefaultFont);
        }

        /// <summary>
        /// Measures a text.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <param name="font">The font.</param>
        /// <returns>The size of text.</returns>
        public static SKSize MeasureString(string text, SKFont font)
        {
            if (string.IsNullOrEmpty(text) || font == null)
                return SKSize.Empty;

            using var paint = new SKPaint();
            float width = font.MeasureText(text, paint);
            var metrics = font.Metrics;
            float height = metrics.Descent - metrics.Ascent + metrics.Leading;
            if (height <= 0)
                height = font.Size;

            return new SKSize(width, height);
        }

        /// <summary>
        /// Measures a text within a rectangle.
        /// </summary>
        public static SKSize MeasureString(string text, SKFont font, SKRect layoutRect)
        {
            var measured = MeasureString(text, font);
            return new SKSize(Math.Min(layoutRect.Width, measured.Width), Math.Min(layoutRect.Height, measured.Height));
        }

        /// <summary>
        /// Compatibility overload for callers that previously passed a graphics context.
        /// </summary>
        public static SKSize MeasureString(IGraphics _, string text, SKFont font)
        {
            return MeasureString(text, font);
        }

        /// <summary>
        /// Compatibility overload for callers that previously passed a graphics context and rectangle.
        /// </summary>
        public static SKSize MeasureString(IGraphics _, string text, SKFont font, SKRect layoutRect)
        {
            return MeasureString(text, font, layoutRect);
        }

        /// <summary>
        /// Compatibility overload retained for migration; format is ignored in SkiaSharp path.
        /// </summary>
        public static SKSize MeasureString(IGraphics _, string text, SKFont font, SKRect layoutRect, object format)
        {
            return MeasureString(text, font, layoutRect);
        }
#if !TRANSPORT
        internal static MonoRendering GetMonoRendering(IGraphics printerGraphics)
        {
            if (FMonoRendering == MonoRendering.Undefined)
            {
                const string s = "test string test string test string test string";
                float f1 = MeasureString(s, DefaultReportFont).Width;
                FMonoRendering = f1 > 200 ? MonoRendering.Pango : MonoRendering.Cairo;
            }
            return FMonoRendering;
        }

        /// <summary>
        /// The method adjusts the dotted line style for the <see cref="Pen"/> in a graphical context.
        /// </summary>
        /// <param name="dashPattern">Collection of values for custom dash pattern.</param>
        /// <param name="pen">Pen for lines.</param>
        /// <param name="border">Border around the report object.</param>
        /// <remarks>
        /// If a <c>DashPattern</c> pattern is specified and contains elements, the method checks each element.
        /// If the element is less than or equal to 0, it is replaced by 1.<br/>
        /// Then the resulting array of patterns is converted to the <see cref="float"/> type and set as a dotted line pattern for the <see cref="Pen"/>.<br/>
        /// If the pattern is empty or not specified,
        /// the method sets the style of the dotted line of the <see cref="Pen"/> equal to the style of the dotted line of the <see cref="Border"/> object.
        ///</remarks>
        internal static void SetPenDashPatternOrStyle(FloatCollection dashPattern, object pen, Border border)
        {
            // Migration placeholder: dash mapping will be moved to SKPaint.PathEffect at call sites.
            // Keeping method to preserve call compatibility while removing System.Drawing type dependency.
            if (dashPattern?.Count > 0)
            {
                for (int i = 0; i < dashPattern.Count; i++)
                {
                    if (dashPattern[i] <= 0)
                        dashPattern[i] = 1;
                }
            }
        }
#endif    
    }
}