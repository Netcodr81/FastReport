using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using SkiaSharp;

namespace FastReport
{
    public sealed class FontFamily
    {
        public string Name { get; }

        public static FontFamily GenericSansSerif { get; } = new FontFamily(SKTypeface.Default?.FamilyName ?? "Sans Serif");

        public FontFamily(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? GenericSansSerif.Name : name;
        }
    }

    public abstract class FontCollection : IDisposable
    {
        private readonly List<FontFamily> families = new List<FontFamily>();

        public FontFamily[] Families => families.ToArray();

        protected void AddFamily(FontFamily family)
        {
            if (family == null)
                return;
            if (families.Any(f => string.Equals(f.Name, family.Name, StringComparison.OrdinalIgnoreCase)))
                return;
            families.Add(family);
        }

        protected void AddFamily(string familyName)
        {
            AddFamily(new FontFamily(familyName));
        }

        public virtual void Dispose()
        {
        }
    }

    public sealed class PrivateFontCollection : FontCollection
    {
        public void AddFontFile(string filename)
        {
            try
            {
                using SKTypeface typeface = SKTypeface.FromFile(filename);
                AddFamily(typeface?.FamilyName ?? Path.GetFileNameWithoutExtension(filename));
            }
            catch
            {
            }
        }

        public void AddMemoryFont(IntPtr memory, int length)
        {
            if (memory == IntPtr.Zero || length <= 0)
                return;

            try
            {
                byte[] bytes = new byte[length];
                Marshal.Copy(memory, bytes, 0, length);
                using SKData data = SKData.CreateCopy(bytes);
                using SKTypeface typeface = SKTypeface.FromData(data);
                AddFamily(typeface?.FamilyName ?? FontFamily.GenericSansSerif.Name);
            }
            catch
            {
            }
        }
    }

    public sealed class InstalledFontCollection : FontCollection
    {
        public InstalledFontCollection()
        {
            AddFamily(SKTypeface.Default?.FamilyName ?? "Sans Serif");
        }
    }

    /// <summary>
    /// Contains font management methods and properties.
    /// </summary>
    public static partial class FontManager
    {
        // NOT THREAD SAFE!
        private static PrivateFontCollection PrivateFontCollection { get; } = new PrivateFontCollection();

        // NOT THREAD SAFE!
        // Do not update PrivateFontCollection at realtime, you must update property value then dispose previous.
        private static PrivateFontCollection TemporaryFontCollection { get; set; } = null;

        private static InstalledFontCollection InstalledFontCollection { get; } = new InstalledFontCollection();

        private static List<FontSubstitute> SubstituteFonts { get; } = new List<FontSubstitute>();

        /// <summary>
        /// Gets all installed font families.
        /// </summary>
        /// <remarks>
        /// This method enumerates all font collections (PrivateFontCollection, TemporaryFontCollection, InstalledFontCollection)
        /// and sorts the result.
        /// </remarks>
        public static FontFamily[] AllFamilies
        {
            get
            {
                var families = new List<FontFamily>();

                families.AddRange(InstalledFontCollection.Families);
                families.AddRange(PrivateFontCollection.Families);
                if (TemporaryFontCollection != null)
                {
                    families.AddRange(TemporaryFontCollection.Families);
                }

                families.Sort((x, y) => x.Name.CompareTo(y.Name));
                return families.ToArray();
            }
        }

        /// <summary>
        /// Adds a new substitute font item.
        /// </summary>
        /// <param name="originalFontName">The original font name, e.g. "Arial"</param>
        /// <param name="substituteFonts">The alternatives list, e.g. "Ubuntu Sans", "Liberation Sans", "Helvetica"</param>
        /// <remarks>
        /// Substitute font replaces the original font if it is not present on a machine.
        /// For example, you may define "Helvetica Neue" substitute for "Arial".
        /// </remarks>
        public static void AddSubstituteFont(string originalFontName, params string[] substituteFonts)
        {
            SubstituteFonts.Add(new FontSubstitute(originalFontName, substituteFonts));
        }

        /// <summary>
        /// Removes substitute fonts for the given font.
        /// </summary>
        /// <param name="originalFontName">The original font name, e.g. "Arial"</param>
        public static void RemoveSubstituteFont(string originalFontName)
        {
            for (int i = 0; i < SubstituteFonts.Count; i++)
            {
                if (SubstituteFonts[i].Name == originalFontName)
                {
                    SubstituteFonts.RemoveAt(i);
                    i--;
                }
            }
        }

        /// <summary>
        /// Clears all substitute fonts.
        /// </summary>
        public static void ClearSubstituteFonts()
        {
            SubstituteFonts.Clear();
        }

        /// <summary>
        /// Finds a FontFamily by its name in specified font collections.
        /// </summary>
        /// <param name="name">The family name, e.g. "Arial".</param>
        /// <param name="searchScope">Search scope.</param>
        /// <returns>The FontFamily instance if found; otherwise null.</returns>
        private static FontFamily FindFontFamily(string name, SearchScope searchScope = SearchScope.All)
        {
            FontFamily family = null;
            
            if ((searchScope & SearchScope.Temporary) != 0)
            {
                family = Find(TemporaryFontCollection);
            }
            if ((searchScope & SearchScope.Private) != 0)
            {
                family ??= Find(PrivateFontCollection);
            }
            if ((searchScope & SearchScope.Installed) != 0)
            {
                family ??= Find(InstalledFontCollection);
            }
            return family;

            FontFamily Find(FontCollection collection) =>
                collection?.Families.Where(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }

        /// <summary>
        /// Adds a font from the specified file to the private font collection.
        /// </summary>
        /// <param name="filename">The path to the font file.</param>
        /// <returns><c>true</c> if the font was added; <c>false</c> if the file was not found.</returns>
        public static bool AddFont(string filename)
        {
            if (!File.Exists(filename))
                return false;
            PrivateFontCollection.AddFontFile(filename);
            return true;
        }

        /// <summary>
        /// Adds a font contained in system memory to the private font collection.
        /// </summary>
        /// <param name="memory">The memory address of the font to add.</param>
        /// <param name="length">The memory length of the font to add.</param>
        public static void AddFont(IntPtr memory, int length)
        {
            PrivateFontCollection.AddMemoryFont(memory, length);
        }

        /// <summary>
        /// Checks whether the font from the specified file is installed in the system.
        /// </summary>
        /// <param name="filename">The path to the font file.</param>
        /// <returns><c>true</c> if the font is installed on the system; otherwise <c>false</c>.</returns>
        public static bool CheckFontIsInstalled(string filename)
        {
            try
            {
                using SKTypeface typeface = SKTypeface.FromFile(filename);
                if (typeface == null) return false;
                return FindFontFamily(typeface.FamilyName, SearchScope.Installed) != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Finds a FontFamily by its name.
        /// </summary>
        /// <param name="name">The family name, e.g. "Arial".</param>
        /// <returns>The FontFamily instance if found; otherwise default FontFamily.GenericSansSerif.</returns>
        internal static FontFamily GetFontFamilyOrDefault(string name)
        {
            var fontFamily = FindFontFamily(name);

            if (fontFamily == null)
            {
                // try to substitute
                foreach (var item in SubstituteFonts)
                {
                    if (item.Name == name)
                    {
                        // may be null!
                        fontFamily = item.SubstituteFamily; 
                        break;
                    }
                }
            }

            // return default if not found
            return fontFamily ?? FontFamily.GenericSansSerif;
        }
    }
}
