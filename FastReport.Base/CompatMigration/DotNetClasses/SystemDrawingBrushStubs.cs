using System;

namespace System.Drawing
{
    public abstract class Brush : IDisposable
    {
        public virtual void Dispose() { }
    }

    public sealed class SolidBrush : Brush
    {
        public Color Color { get; }
        public SolidBrush(Color color) => Color = color;
    }

    public static class Brushes
    {
        public static SolidBrush Black { get; } = new SolidBrush(Color.Black);
        public static SolidBrush White { get; } = new SolidBrush(Color.White);
        public static SolidBrush Red { get; } = new SolidBrush(Color.Red);
        public static SolidBrush Transparent { get; } = new SolidBrush(Color.Transparent);
    }
}