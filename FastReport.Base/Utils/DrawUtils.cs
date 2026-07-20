using System;
using System.Linq;

using SkiaSharp;

namespace FastReport.Utils;

internal enum MonoRendering
{
    Undefined,
    Pango,
    Cairo
}

[Flags]
public enum FontStyle
{
    Regular = 0,
    Bold = 1,
    Italic = 2,
    Underline = 4,
    Strikeout = 8
}

internal enum GraphicsUnit
{
    World,
    Display,
    Pixel,
    Point,
    Inch,
    Document,
    Millimeter
}

public sealed class StringFormat : IDisposable
{
    public static StringFormat GenericTypographic { get; } = new StringFormat();
    public StringAlignment Alignment { get; set; }
    public StringAlignment LineAlignment { get; set; }
    public StringTrimming Trimming { get; set; }
    public StringFormatFlags FormatFlags { get; set; }
    public object HotkeyPrefix { get; set; }
    private float firstTabOffset;
    private float[] tabStops = Array.Empty<float>();

    public float[] GetTabStops(out float firstTabStop)
    {
        firstTabStop = firstTabOffset;
        return (float[])tabStops.Clone();
    }

    public void SetTabStops(float firstTabOffset, float[] tabStops)
    {
        this.firstTabOffset = firstTabOffset;
        this.tabStops = tabStops == null ? Array.Empty<float>() : (float[])tabStops.Clone();
    }

    public StringFormat Clone()
    {
        StringFormat format = new StringFormat();
        format.Alignment = Alignment;
        format.LineAlignment = LineAlignment;
        format.Trimming = Trimming;
        format.FormatFlags = FormatFlags;
        format.HotkeyPrefix = HotkeyPrefix;
        format.firstTabOffset = firstTabOffset;
        format.tabStops = (float[])tabStops.Clone();
        return format;
    }

    public void Dispose()
    {
    }
}

internal sealed class Pen
{
    public float[] DashPattern { get; set; }
    public DashStyle DashStyle { get; set; }
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
            _uiScale = Math.Min(Math.Max(1f, value), 1.5f);
        }
    }

    /// <summary>
    /// Gets or sets default font used in FR UI.
    /// </summary>
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
                        FDefaultFont = CreateFont("Tahoma", 8.25f);
                        break;
                }
            }
            return FDefaultFont;
        }
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            FDefaultFont = value;
        }
    }

    /// <summary>
    /// Gets default report font (locale specific).
    /// </summary>
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
                FFixedFont = CreateFont("Courier New", 10);
            return FFixedFont;
        }
    }

    internal static SKFont CreateFont(string familyName, float emSize,
        FontStyle style = FontStyle.Regular,
        GraphicsUnit unit = GraphicsUnit.Point,
        byte gdiCharSet = 1,
        bool gdiVerticalFont = false)
    {
        SKFontStyleWeight weight = (style & FontStyle.Bold) != 0 ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
        SKFontStyleSlant slant = (style & FontStyle.Italic) != 0 ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;
        SKTypeface typeface = SKTypeface.FromFamilyName(familyName, new SKFontStyle(weight, SKFontStyleWidth.Normal, slant)) ?? SKTypeface.Default;
        return new SKFont(typeface, emSize);
    }

    /// <summary>
    /// Measures a text.
    /// </summary>
    public static SKSize MeasureString(string text)
    {
        return MeasureString(text, DefaultFont);
    }

    /// <summary>
    /// Measures a text.
    /// </summary>
    public static SKSize MeasureString(string text, SKFont font)
    {
        using SKPaint paint = new SKPaint { IsAntialias = true };
        return MeasureString(null, text, font, new SKRect(0, 0, 10000, 10000), null, paint);
    }

    /// <summary>
    /// Measures a text.
    /// </summary>
    public static SKSize MeasureString(object g, string text, SKFont font, StringFormat format)
    {
        using SKPaint paint = new SKPaint { IsAntialias = true };
        return MeasureString(g, text, font, new SKRect(0, 0, 10000, 10000), format, paint);
    }

    /// <summary>
    /// Measures a text.
    /// </summary>
    public static SKSize MeasureString(object g, string text, SKFont font, SKRect layoutRect, StringFormat format)
    {
        using SKPaint paint = new SKPaint { IsAntialias = true };
        return MeasureString(g, text, font, layoutRect, format, paint);
    }

    private static SKSize MeasureString(object g, string text, SKFont font, SKRect layoutRect, StringFormat format, SKPaint paint)
    {
        if (string.IsNullOrEmpty(text) || font == null)
            return SKSize.Empty;

        using SKPaint paintForMeasure = new SKPaint();
        float width = font.MeasureText(text, paintForMeasure);
        SKFontMetrics metrics = font.Metrics;
        float height = metrics.Descent - metrics.Ascent + metrics.Leading;
        if (layoutRect.Width > 0)
            width = Math.Min(width, layoutRect.Width);
        return new SKSize(width, height);
    }
    internal static MonoRendering GetMonoRendering(IGraphics printerGraphics)
    {
        if (FMonoRendering == MonoRendering.Undefined)
        {
            const string s = "test string test string test string test string";
            float f1 = printerGraphics.MeasureString(s, DefaultReportFont).Width;
            FMonoRendering = f1 > 200 ? MonoRendering.Pango : MonoRendering.Cairo;
        }
        return FMonoRendering;
    }

    /// <summary>
    /// The method adjusts the dotted line style for the <see cref="Pen"/> in a graphical context.
    /// </summary>
    internal static void SetPenDashPatternOrStyle(FloatCollection dashPattern, Pen pen, Border border)
    {
        if (dashPattern?.Count > 0)
        {
            for (int i = 0; i < dashPattern.Count; i++)
            {
                if (dashPattern[i] <= 0)
                    dashPattern[i] = 1;
            }
            pen.DashPattern = dashPattern.Cast<float>().ToArray();
        }
        else
        {
            pen.DashStyle = border.DashStyle;
        }
    }
}