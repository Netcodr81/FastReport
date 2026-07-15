using System;

namespace System.Drawing
{
    public enum StringAlignment { Near = 0, Center = 1, Far = 2 }

    [Flags]
    public enum StringFormatFlags
    {
        DirectionRightToLeft = 1,
        DirectionVertical = 2,
        FitBlackBox = 4,
        DisplayFormatControl = 32,
        NoFontFallback = 1024,
        MeasureTrailingSpaces = 2048,
        NoWrap = 4096,
        LineLimit = 8192,
        NoClip = 16384
    }

    public enum StringTrimming
    {
        None = 0,
        Character = 1,
        Word = 2,
        EllipsisCharacter = 3,
        EllipsisWord = 4,
        EllipsisPath = 5
    }

    public readonly struct CharacterRange
    {
        public int First { get; }
        public int Length { get; }

        public CharacterRange(int first, int length)
        {
            First = first;
            Length = length;
        }
    }

    public class StringFormat : IDisposable, ICloneable
    {
        private float firstTab;
        private float[] tabStops = Array.Empty<float>();

        public static StringFormat GenericDefault { get; } = new StringFormat();
        public static StringFormat GenericTypographic { get; } = new StringFormat();

        public StringAlignment Alignment { get; set; }
        public StringAlignment LineAlignment { get; set; }
        public StringTrimming Trimming { get; set; }
        public StringFormatFlags FormatFlags { get; set; }
        public System.Drawing.Text.HotkeyPrefix HotkeyPrefix { get; set; }

        public void SetTabStops(float firstTabOffset, float[] tabs)
        {
            firstTab = firstTabOffset;
            tabStops = tabs ?? Array.Empty<float>();
        }

        public float[] GetTabStops(out float firstTabOffset)
        {
            firstTabOffset = firstTab;
            return tabStops;
        }

        public void SetMeasurableCharacterRanges(CharacterRange[] ranges)
        {
        }

        public object Clone()
        {
            return new StringFormat
            {
                Alignment = Alignment,
                LineAlignment = LineAlignment,
                Trimming = Trimming,
                FormatFlags = FormatFlags,
                HotkeyPrefix = HotkeyPrefix
            };
        }

        public void Dispose() { }
    }
}