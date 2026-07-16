using FastReport.Utils;
using SkiaSharp;
using System.ComponentModel;

namespace FastReport.Gauge.Simple
{
    /// <summary>
    /// Represents a simple gauge.
    /// </summary>
    public partial class SimpleGauge : GaugeObject
    {
        /// <summary>
        /// Gets or sets gauge label.
        /// </summary>
        [Browsable(false)]
        public override GaugeLabel Label
        {
            get { return base.Label; }
            set { base.Label = value; }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleGauge"/> class.
        /// </summary>
        public SimpleGauge() : base()
        {
            Value = 75;
            Scale = new SimpleScale(this);
            Pointer = new SimplePointer(this);
            Height = 2.0f * Units.Centimeters;
            Width = 8.0f * Units.Centimeters;
        }

        #endregion // Constructors

        #region Public Methods

        /// <inheritdoc/>
        public override void Draw(FRPaintEventArgs e)
        {
            base.Draw(e);
            IGraphics g = e.Graphics;

            if (Report != null && Report.SmoothGraphics)
            {
                g.SamplingOptions = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
            }

            Scale.Draw(e);
            Pointer.Draw(e);
            Border.Draw(e, new SKRect(AbsLeft, AbsTop, AbsLeft + Width, AbsTop + Height));
        }

        #endregion // Public Methods
    }
}
