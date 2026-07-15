using System;
using System.Drawing;

namespace FastReport.Utils
{
    internal static class TypeConverterBridge
    {
        private sealed class FontFamilyMatcherAdapter : FastReport.TypeConverters.FontConverter.IFontFamilyMatcher
        {
            private readonly Func<string, FontFamily> matcher;

            public FontFamilyMatcherAdapter(Func<string, FontFamily> matcher)
            {
                this.matcher = matcher;
            }

            public FontFamily GetFontFamilyOrDefault(string name)
            {
                return matcher(name);
            }
        }

        internal static string FontToInvariantString(Font value)
        {
            return new FastReport.TypeConverters.FontConverter().ConvertToInvariantString(value);
        }

        internal static Font FontFromInvariantString(string value)
        {
            return new FastReport.TypeConverters.FontConverter().ConvertFromInvariantString(value) as Font;
        }

        internal static Color ColorFromInvariantString(string value)
        {
            return (Color)new ColorConverter().ConvertFromInvariantString(value);
        }

        internal static void SetFontFamilyMatcher(Func<string, FontFamily> matcher)
        {
            FastReport.TypeConverters.FontConverter.FontFamilyMatcher = new FontFamilyMatcherAdapter(matcher);
        }
    }
}
