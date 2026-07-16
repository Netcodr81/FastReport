using FastReport.Utils;
using SkiaSharp;
using System.ComponentModel;

namespace FastReport.Gauge.Linear
{
    /// <summary>
    /// Represents a linear pointer.
    /// </summary>
#if !DEBUG
    [DesignTimeVisible(false)]
#endif
    public class LinearPointer : GaugePointer
    {
        #region Fields

        private float left;
        private float top;
        private float height;
        private float width;

        #endregion // Fields

        #region Properties

        /// <summary>
        /// Gets o sets the height of gauge pointer.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        /// <summary>
        /// Gets or sets the width of a pointer.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        #endregion // Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearPointer"/>
        /// </summary>
        /// <param name="parent">The parent gauge object.</param>
        public LinearPointer(GaugeObject parent) : base(parent)
        {
            height = 4.0f;
            width = 8.0f;
        }

        #endregion // Constructors

        #region Private Methods

        private static SKColor ToSKColor(System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        private SKColor GetFillColor()
        {
            if (Fill is SolidFill solidFill)
                return solidFill.Color;

            return SKColors.Orange;
        }

        private void DrawHorz(FRPaintEventArgs e)
        {
            IGraphics g = e.Graphics;

            left = (float)(Parent.AbsLeft + 0.5f * Units.Centimeters + (Parent.Width - 1.0f * Units.Centimeters) * (Parent.Value - Parent.Minimum) / (Parent.Maximum - Parent.Minimum)) * e.ScaleX;
            top = (Parent.AbsTop + Parent.Height / 2) * e.ScaleY;
            height = Parent.Height * 0.4f * e.ScaleY;
            width = Parent.Width * 0.036f * e.ScaleX;

            float dx = width / 2;
            float dy = height * 0.3f;
            SKPoint[] p = new SKPoint[]
            {
                new SKPoint(left, top),
                new SKPoint(left + dx, top + dy),
                new SKPoint(left + dx, top + height),
                new SKPoint(left - dx, top + height),
                new SKPoint(left - dx, top + dy)
            };

            if ((Parent as LinearGauge).Inverted)
            {
                p[1].Y = top - dy;
                p[2].Y = top - height;
                p[3].Y = top - height;
                p[4].Y = top - dy;
            }

            using SKPath path = new SKPath();
            path.AddPoly(p, true);
            using SKPaint pen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ToSKColor(BorderColor),
                StrokeWidth = BorderWidth * e.ScaleX,
                IsAntialias = true
            };
            using SKPaint brush = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = GetFillColor(),
                IsAntialias = true
            };

            g.FillAndDrawPath(pen, brush, path);
        }

        private void DrawVert(FRPaintEventArgs e)
        {
            IGraphics g = e.Graphics;

            left = (Parent.AbsLeft + Parent.Width / 2) * e.ScaleX;
            top = (float)(Parent.AbsTop + Parent.Height - 0.5f * Units.Centimeters - (Parent.Height - 1.0f * Units.Centimeters) * (Parent.Value - Parent.Minimum) / (Parent.Maximum - Parent.Minimum)) * e.ScaleY;
            height = Parent.Height * 0.036f * e.ScaleY;
            width = Parent.Width * 0.4f * e.ScaleX;

            float dx = width * 0.3f;
            float dy = height / 2;
            SKPoint[] p = new SKPoint[]
            {
                new SKPoint(left, top),
                new SKPoint(left + dx, top - dy),
                new SKPoint(left + width, top - dy),
                new SKPoint(left + width, top + dy),
                new SKPoint(left + dx, top + dy)
            };

            if ((Parent as LinearGauge).Inverted)
            {
                p[1].X = left - dx;
                p[2].X = left - width;
                p[3].X = left - width;
                p[4].X = left - dx;
            }

            using SKPath path = new SKPath();
            path.AddPoly(p, true);
            using SKPaint pen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ToSKColor(BorderColor),
                StrokeWidth = BorderWidth * e.ScaleX,
                IsAntialias = true
            };
            using SKPaint brush = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = GetFillColor(),
                IsAntialias = true
            };

            g.FillAndDrawPath(pen, brush, path);
        }

        #endregion // Private Methods

        #region Public Methods

        /// <inheritdoc/>
        public override void Assign(GaugePointer src)
        {
            base.Assign(src);

            LinearPointer s = src as LinearPointer;
            Height = s.Height;
            Width = s.Width;
        }

        /// <inheritdoc/>
        public override void Draw(FRPaintEventArgs e)
        {
            base.Draw(e);

            if (Parent.Vertical)
            {
                DrawVert(e);
            }
            else
            {
                DrawHorz(e);
            }
        }

        /// <inheritdoc/>
        public override void Serialize(FRWriter writer, string prefix, GaugePointer diff)
        {
            base.Serialize(writer, prefix, diff);

            LinearPointer dc = diff as LinearPointer;
            if (Height != dc.Height)
            {
                writer.WriteFloat(prefix + ".Height", Height);
            }
            if (Width != dc.Width)
            {
                writer.WriteFloat(prefix + ".Width", Width);
            }
        }

        #endregion // Public Methods
    }
}
