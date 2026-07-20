using System.ComponentModel;

using SkiaSharp;

namespace FastReport.TypeConverters;

public partial class FontConverter : TypeConverter
{
    public static IFontFamilyMatcher FontFamilyMatcher { get; set; } = new DefaultFontFamilyMatcher();

    public interface IFontFamilyMatcher
    {
        SKTypeface GetFontFamilyOrDefault(string name);
    }

    private class DefaultFontFamilyMatcher : IFontFamilyMatcher
    {
        public SKTypeface GetFontFamilyOrDefault(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                SKTypeface matched = SKTypeface.FromFamilyName(name);
                if (matched != null)
                    return matched;
            }

            return SKTypeface.Default;
        }
    }
}