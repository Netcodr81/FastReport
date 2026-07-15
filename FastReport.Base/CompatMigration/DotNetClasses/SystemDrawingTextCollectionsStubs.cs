using System;

namespace System.Drawing.Text
{
    public enum HotkeyPrefix
    {
        None = 0,
        Show = 1,
        Hide = 2
    }

    public enum TextRenderingHint
    {
        SystemDefault = 0,
        SingleBitPerPixelGridFit = 1,
        SingleBitPerPixel = 2,
        AntiAliasGridFit = 3,
        AntiAlias = 4,
        ClearTypeGridFit = 5
    }

    public abstract class FontCollection : IDisposable
    {
        public virtual System.Drawing.FontFamily[] Families => Array.Empty<System.Drawing.FontFamily>();
        public void Dispose() { }
    }

    public sealed class InstalledFontCollection : FontCollection { }

    public sealed class PrivateFontCollection : FontCollection
    {
        public void AddFontFile(string filename) { }
        public void AddMemoryFont(IntPtr memory, int length) { }
    }
}