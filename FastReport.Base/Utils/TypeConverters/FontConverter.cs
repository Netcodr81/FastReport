// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Note: the original file was modified by AlexTZ:
// - code changed to support .Net 2.0
// - PrivateFontCollection property added
// - Instance property added
// - Up priority of PrivateFontCollection

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using SkiaSharp;

namespace FastReport.TypeConverters
{
    [Flags]
    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8
    }

    public enum GraphicsUnit
    {
        World,
        Display,
        Pixel,
        Point,
        Inch,
        Document,
        Millimeter
    }

    public partial class FontConverter : TypeConverter
    {
        private const string StylePrefix = "style=";


        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(string)) || (destinationType == typeof(InstanceDescriptor));
        }

        /// <inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is SKFont)
            {
                SKFont font = value as SKFont;
                if (destinationType == typeof(string))
                {
                    if (culture == null)
                    {
                        culture = CultureInfo.CurrentCulture;
                    }

                    StringBuilder sb = new StringBuilder();
                    string fontName = font.Typeface?.FamilyName ?? SKTypeface.Default.FamilyName;
                    FontStyle style = GetFontStyle(font);

                    sb.Append(fontName);
                    sb.Append(culture.TextInfo.ListSeparator[0]);
                    sb.Append(' ');
                    sb.Append(font.Size.ToString(culture.NumberFormat));
                    sb.Append("pt");

                    if (style != FontStyle.Regular)
                    {
                        sb.Append(culture.TextInfo.ListSeparator[0]);
                        sb.Append(" style=");
                        sb.Append(style.ToString());
                    }

                    return sb.ToString();
                }

                if (destinationType == typeof(InstanceDescriptor))
                {
                    ConstructorInfo met = typeof(SKFont).GetConstructor(new Type[] { typeof(SKTypeface), typeof(float) });
                    object[] args = new object[2];
                    args[0] = font.Typeface ?? SKTypeface.Default;
                    args[1] = font.Size;

                    return new InstanceDescriptor(met, args);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }

            string font = value as string;
            font = font.Trim();

            // Expected string format: "name[, size[, units[, style=style1[, style2[...]]]]]"
            // Example using 'vi-VN' culture: "Microsoft Sans Serif, 8,25pt, style=Italic, Bold"
            if (font.Length == 0)
            {
                return null;
            }

            if (culture == null)
            {
                culture = CultureInfo.CurrentCulture;
            }

            char separator = culture.TextInfo.ListSeparator[0]; // For vi-VN: ','
            string fontName = font; // start with the assumption that only the font name was provided.
            string style = null;
            string sizeStr = null;
            float fontSize = 8.25f;
            FontStyle fontStyle = FontStyle.Regular;
            GraphicsUnit units = GraphicsUnit.Point;

            // Get the index of the first separator (would indicate the end of the name in the string).
            int nameIndex = font.IndexOf(separator);

            if (nameIndex < 0)
            {
                SKFontStyle skStyle = GetSKFontStyle(fontStyle);
                SKTypeface defaultTypeface = SKTypeface.FromFamilyName(fontName, skStyle) ?? SKTypeface.Default;
                return new SKFont(defaultTypeface, fontSize);
            }

            // Some parameters are provided in addition to name.
            fontName = font.Substring(0, nameIndex);

            if (nameIndex < font.Length - 1)
            {
                // Get the style index (if any). The size is a bit problematic because it can be formatted differently
                // depending on the culture, we'll parse it last.
                int styleIndex = culture.CompareInfo.IndexOf(font, StylePrefix, CompareOptions.IgnoreCase);

                if (styleIndex != -1)
                {
                    // style found.
                    style = font.Substring(styleIndex, font.Length - styleIndex);

                    // Get the mid-substring containing the size information.
                    sizeStr = font.Substring(nameIndex + 1, styleIndex - nameIndex - 1);
                }
                else
                {
                    // no style.
                    sizeStr = font.Substring(nameIndex + 1);
                }

                // Parse size.
                string unitTokensSize = null;
                string unitTokensUnit = null;
                ParseSizeTokens(sizeStr, separator, ref unitTokensSize, ref unitTokensUnit);

                if (unitTokensSize != null)
                {
                    try
                    {
                        fontSize = (float)TypeDescriptor.GetConverter(typeof(float)).ConvertFromString(context, culture, unitTokensSize);
                    }
                    catch
                    {
                        // Exception from converter is too generic.
                        throw new ArgumentException("Invalid font string: " + font);
                    }
                }

                if (unitTokensUnit != null)
                {
                    // ParseGraphicsUnits throws an ArgumentException if format is invalid.
                    units = ParseGraphicsUnits(unitTokensUnit);
                }

                if (style != null)
                {
                    // Parse FontStyle
                    style = style.Substring(6); // style string always starts with style=
                    string[] styleTokens = style.Split(separator);

                    for (int tokenCount = 0; tokenCount < styleTokens.Length; tokenCount++)
                    {
                        string styleText = styleTokens[tokenCount];
                        styleText = styleText.Trim();

                        fontStyle |= (FontStyle)Enum.Parse(typeof(FontStyle), styleText, true);

                        // Enum.IsDefined doesn't do what we want on flags enums...
                        FontStyle validBits = FontStyle.Regular | FontStyle.Bold | FontStyle.Italic | FontStyle.Underline | FontStyle.Strikeout;
                        if ((fontStyle | validBits) != validBits)
                        {
                            throw new InvalidEnumArgumentException("FontStyle", (int)fontStyle, typeof(FontStyle));
                        }
                    }
                }
            }

            SKFontStyle skFontStyle = GetSKFontStyle(fontStyle);
            SKTypeface typeface = SKTypeface.FromFamilyName(fontName, skFontStyle) ?? SKTypeface.Default;
            return new SKFont(typeface, fontSize);
        }

        private void ParseSizeTokens(string text, char separator, ref string size, ref string units)
        {
            text = text.Trim();

            int length = text.Length;
            int splitPoint;

            if (length > 0)
            {
                // text is expected to have a format like " 8,25pt, ". Leading and trailing spaces (trimmed above),
                // last comma, unit and decimal value may not appear.  We need to make it ####.##CC
                for (splitPoint = 0; splitPoint < length; splitPoint++)
                {
                    if (char.IsLetter(text[splitPoint]))
                    {
                        break;
                    }
                }

                char[] trimChars = new char[] { separator, ' ' };

                if (splitPoint > 0)
                {
                    size = text.Substring(0, splitPoint);
                    // Trimming spaces between size and units.
                    size = size.Trim(trimChars);
                }

                if (splitPoint < length)
                {
                    units = text.Substring(splitPoint);
                    units = units.TrimEnd(trimChars);
                }
            }
        }

        private GraphicsUnit ParseGraphicsUnits(string units)
        {
            if (units == "display") return GraphicsUnit.Display;
            else if (units == "doc") return GraphicsUnit.Document;
            else if (units == "pt") return GraphicsUnit.Point;
            else if (units == "in") return GraphicsUnit.Inch;
            else if (units == "mm") return GraphicsUnit.Millimeter;
            else if (units == "px") return GraphicsUnit.Pixel;
            else if (units == "world") return GraphicsUnit.World;
            else throw new ArgumentException("Invalid font units: " + units);
        }

        private static FontStyle GetFontStyle(SKFont font)
        {
            FontStyle style = FontStyle.Regular;
            SKTypeface typeface = font.Typeface;
            if (typeface != null)
            {
                if (typeface.FontWeight >= (int)SKFontStyleWeight.SemiBold)
                    style |= FontStyle.Bold;
                if (typeface.FontSlant != SKFontStyleSlant.Upright)
                    style |= FontStyle.Italic;
            }
            return style;
        }

        private static SKFontStyle GetSKFontStyle(FontStyle style)
        {
            SKFontStyleWeight weight = (style & FontStyle.Bold) != 0
                ? SKFontStyleWeight.Bold
                : SKFontStyleWeight.Normal;
            SKFontStyleSlant slant = (style & FontStyle.Italic) != 0
                ? SKFontStyleSlant.Italic
                : SKFontStyleSlant.Upright;
            return new SKFontStyle(weight, SKFontStyleWidth.Normal, slant);
        }

        /// <inheritdoc/>
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            object value;
            float size = 8;
            string name = null;
            FontStyle style = FontStyle.Regular;
            GraphicsUnit unit = GraphicsUnit.Point;

            if ((value = propertyValues["Size"]) != null)
                size = (float)value;

            if ((value = propertyValues["Unit"]) != null)
                unit = (GraphicsUnit)value;

            if ((value = propertyValues["Name"]) != null)
                name = (string)value;

            if ((value = propertyValues["Bold"]) != null)
            {
                if ((bool)value == true)
                    style |= FontStyle.Bold;
            }

            if ((value = propertyValues["Italic"]) != null)
            {
                if ((bool)value == true)
                    style |= FontStyle.Italic;
            }

            if ((value = propertyValues["Strikeout"]) != null)
            {
                if ((bool)value == true)
                    style |= FontStyle.Strikeout;
            }

            if ((value = propertyValues["Underline"]) != null)
            {
                if ((bool)value == true)
                    style |= FontStyle.Underline;
            }

            string familyName = string.IsNullOrEmpty(name) ? "Tahoma" : name;
            SKFontStyle skStyle = GetSKFontStyle(style);
            SKTypeface typeface = SKTypeface.FromFamilyName(familyName, skStyle) ?? SKTypeface.Default;
            return new SKFont(typeface, size);
        }

        /// <inheritdoc/>
        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <inheritdoc/>
        public override PropertyDescriptorCollection GetProperties(
            ITypeDescriptorContext context,
            object value,
            Attribute[] attributes)
        {
            return value is SKFont ? TypeDescriptor.GetProperties(value, attributes) : base.GetProperties(context, value, attributes);
        }

        /// <inheritdoc/>
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public sealed class FontNameConverter : TypeConverter, IDisposable
        {
            private readonly string[] _fonts;

            public FontNameConverter()
            {
                _fonts = SKFontManager.Default.FontFamilies.ToArray();
            }

            void IDisposable.Dispose()
            {
            }

            /// <inheritdoc/>
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);
            }

            /// <inheritdoc/>
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                return value is string ? MatchFontName((value as string), context) : base.ConvertFrom(context, culture, value);
            }

            /// <inheritdoc/>
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                string[] values = (string[])_fonts.Clone();
                Array.Sort(values, Comparer.Default);

                return new TypeConverter.StandardValuesCollection(values);
            }

            // We allow other values other than those in the font list.
            /// <inheritdoc/>
            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            // Yes, we support picking an element from the list.
            /// <inheritdoc/>
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            private string MatchFontName(string name, ITypeDescriptorContext context)
            {
                // Try a partial match
                string bestMatch = null;

                foreach (string fontName in GetStandardValues(context))
                {
                    if (fontName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // For an exact match, return immediately
                        return fontName;
                    }
                    if (fontName.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (bestMatch == null || fontName.Length <= bestMatch.Length)
                        {
                            bestMatch = fontName;
                        }
                    }
                }

                // No match... fall back on whatever was provided
                return bestMatch != null ? bestMatch : name;
            }
        }

        public class FontUnitConverter : EnumConverter
        {
            public FontUnitConverter() : base(typeof(GraphicsUnit)) { }

            /// <inheritdoc/>
            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                // display graphic unit is not supported.
                if (Values == null)
                {
                    base.GetStandardValues(context); // sets "values"
                    ArrayList filteredValues = new ArrayList(Values);
                    filteredValues.Remove(GraphicsUnit.Display);
                    Values = new StandardValuesCollection(filteredValues);
                }
                return Values;
            }
        }
    }
}
