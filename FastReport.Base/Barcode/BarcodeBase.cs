using FastReport.Utils;
using SkiaSharp;
using System;
using System.ComponentModel;

namespace FastReport.Barcode
{
    /// <summary>
    /// The base class for all barcodes.
    /// </summary>
    [TypeConverter(typeof(FastReport.TypeConverters.BarcodeConverter))]
    public abstract class BarcodeBase
    {
        #region Fields
        internal string text;
        internal int angle;
        internal bool showText;
        internal float zoom;
        internal bool showMarker;
        private SKColor color;
        private SKFont font;
        private string fontFamilyName;
        private float fontSize;

        private static readonly string DefaultFontFamily = "Arial";
        private static readonly float DefaultFontSize = 8;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of barcode.
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get { return Barcodes.GetName(GetType()); }
        }

        /// <summary>
        /// Gets or sets the color of barcode.
        /// </summary>
        public SKColor Color
        {
            get { return color; }
            set { color = value; }
        }

        /// <summary>
        /// Gets or sets the font of barcode.
        /// </summary>
        public SKFont Font
        {
            get { return font; }
            set 
            { 
                font?.Dispose();
                font = value;
                if (value != null)
                {
                    fontFamilyName = value.Typeface?.FamilyName ?? DefaultFontFamily;
                    fontSize = value.Size;
                }
            }
        }

        /// <summary>
        /// Gets the font family name.
        /// </summary>
        public string FontFamilyName
        {
            get { return fontFamilyName ?? DefaultFontFamily; }
        }

        /// <summary>
        /// Gets the font size.
        /// </summary>
        public float FontSize
        {
            get { return fontSize > 0 ? fontSize : DefaultFontSize; }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Creates the exact copy of this barcode.
        /// </summary>
        /// <returns>The copy of this barcode.</returns>
        public BarcodeBase Clone()
        {
            BarcodeBase result = Activator.CreateInstance(GetType()) as BarcodeBase;
            result.Assign(this);
            return result;
        }

        /// <summary>
        /// Assigns properties from other, similar barcode.
        /// </summary>
        /// <param name="source">Barcode object to assign properties from.</param>
        public virtual void Assign(BarcodeBase source)
        {
            Color = source.Color;
            Font = source.Font != null
                ? new SKFont(source.Font.Typeface, source.Font.Size)
                : null;
        }

        internal virtual void Serialize(FRWriter writer, string prefix, BarcodeBase diff)
        {
            if (diff.GetType() != GetType())
                writer.WriteStr("Barcode", Name);
            if (diff.Color != Color)
                writer.WriteValue(prefix + "Color", Color);
            if (diff.Font != Font)
                writer.WriteValue(prefix + "Font", Font);
        }

        internal virtual void Initialize(string text, bool showText, int angle, float zoom)
        {
            this.text = text;
            this.showText = showText;
            this.angle = (angle / 90 * 90) % 360;
            this.zoom = zoom;
        }

        internal virtual void Initialize(string text, bool showText, int angle, float zoom, bool showMarker)
        {
            this.text = text;
            this.showText = showText;
            this.angle = (angle / 90 * 90) % 360;
            this.zoom = zoom;
            this.showMarker = showMarker;
        }

        internal virtual SKSize CalcBounds()
        {
            return SKSize.Empty;
        }

        internal virtual string StripControlCodes(string data)
        {
            return data;
        }

        /// <summary>
        /// Draws a barcode.
        /// </summary>
        /// <param name="g">The graphic surface.</param>
        /// <param name="displayRect">Display rectangle.</param>
        public virtual void DrawBarcode(IGraphics g, SKRect displayRect)
        {
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="BarcodeBase"/> class with default settings.
        /// </summary>
        public BarcodeBase()
        {
            text = "";
            color = SKColors.Black;
            fontFamilyName = DefaultFontFamily;
            fontSize = DefaultFontSize;
            Font = new SKFont(SKTypeface.FromFamilyName(DefaultFontFamily), DefaultFontSize);
        }

        /// <summary>
        /// Get default value of this barcode
        /// </summary>
        /// <returns></returns>
        public virtual string GetDefaultValue()
        {
            return "12345678";
        }
    }
}
