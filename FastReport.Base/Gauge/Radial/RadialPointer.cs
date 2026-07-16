using FastReport.Utils;
using SkiaSharp;
using System.ComponentModel;

namespace FastReport.Gauge.Radial
{
    /// <summary>
    /// Represents a linear pointer.
    /// </summary>
#if !DEBUG
    [DesignTimeVisible(false)]
#endif
    public class RadialPointer : GaugePointer
    {
        #region Fields
        private RadialScale scale;
        private bool gradAutoRotate;
        #endregion // Fields

        /// <summary>
        /// Gets or sets the value, indicating that gradient should be rotated automatically
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool GradientAutoRotate
        {
            get { return gradAutoRotate; }
            set { gradAutoRotate = value; }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RadialPointer"/>
        /// </summary>
        /// <param name="parent">The parent gauge object.</param>
        /// <param name="scale">The scale object.</param>
        public RadialPointer(GaugeObject parent, RadialScale scale) : base(parent)
        {
            this.scale = scale;
            gradAutoRotate = true;
        }

        #endregion // Constructors

        #region Private Methods

        private static SKColor ToSKColor(System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        private static SKPoint[] RotateVector(SKPoint[] vector, double angle, SKPoint center)
        {
            SKPoint[] rotatedVector = new SKPoint[2];
            rotatedVector[0] = new SKPoint(
                (float)(center.X + (vector[0].X - center.X) * System.Math.Cos(angle) + (center.Y - vector[0].Y) * System.Math.Sin(angle)),
                (float)(center.Y + (vector[0].X - center.X) * System.Math.Sin(angle) + (vector[0].Y - center.Y) * System.Math.Cos(angle)));
            rotatedVector[1] = new SKPoint(
                (float)(center.X + (vector[1].X - center.X) * System.Math.Cos(angle) + (center.Y - vector[1].Y) * System.Math.Sin(angle)),
                (float)(center.Y + (vector[1].X - center.X) * System.Math.Sin(angle) + (vector[1].Y - center.Y) * System.Math.Cos(angle)));
            return rotatedVector;
        }

        private void DrawHorz(FRPaintEventArgs e)
        {
            IGraphics g = e.Graphics;
            using SKPaint pen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ToSKColor(BorderColor),
                StrokeWidth = BorderWidth * e.ScaleX,
                IsAntialias = true
            };

            SKPoint center = (Parent as RadialGauge).Center;
            float circleWidth = Parent.Width / 16f;
            float circleHeight = Parent.Height / 16f;
            SKRect pointerCircle = new SKRect(
                center.X - circleWidth / 2 * e.ScaleX,
                center.Y - circleHeight / 2 * e.ScaleY,
                center.X + circleWidth / 2 * e.ScaleX,
                center.Y + circleHeight / 2 * e.ScaleY);

            double startAngle = -135 * RadialGauge.Radians;
            double angle = (Parent.Value - Parent.Minimum) / scale.StepValue * scale.MajorStep * RadialGauge.Radians;
            if ((Parent as RadialGauge).Type == RadialGaugeType.Semicircle)
            {
                if ((Parent as RadialGauge).Position == RadialGaugePosition.Bottom || (Parent as RadialGauge).Position == RadialGaugePosition.Top)
                {
                    startAngle = -90 * RadialGauge.Radians;
                    if ((Parent as RadialGauge).Position == RadialGaugePosition.Bottom)
                        angle *= -1;
                }
                else if ((Parent as RadialGauge).Position == RadialGaugePosition.Left)
                    startAngle = -180 * RadialGauge.Radians;
                else if ((Parent as RadialGauge).Position == RadialGaugePosition.Right)
                {
                    startAngle = -180 * RadialGauge.Radians;
                    angle *= -1;
                }
            }
            else if (RadialUtils.IsQuadrant(Parent))
            {
                if (RadialUtils.IsLeft(Parent) && RadialUtils.IsTop(Parent))
                    startAngle = -90 * RadialGauge.Radians;
                else if (RadialUtils.IsLeft(Parent) && RadialUtils.IsBottom(Parent))
                    startAngle = -180 * RadialGauge.Radians;
                else if (RadialUtils.IsRight(Parent) && RadialUtils.IsTop(Parent))
                    startAngle = 90 * RadialGauge.Radians;
                else if (RadialUtils.IsRight(Parent) && RadialUtils.IsBottom(Parent))
                {
                    startAngle = 180 * RadialGauge.Radians;
                    angle *= -1;
                }
            }

            float ptrLineY = center.Y - pointerCircle.Width / 2 - pointerCircle.Width / 5;
            float ptrLineY1 = scale.AvrTick.Y + scale.MinorTicks.Length * 1.7f;
            float ptrLineWidth = circleWidth / 3 * e.ScaleX;
            SKPoint[] pointerPerpStrt = new SKPoint[2];
            pointerPerpStrt[0] = new SKPoint(center.X - ptrLineWidth, ptrLineY);
            pointerPerpStrt[1] = new SKPoint(center.X + ptrLineWidth, ptrLineY);

            SKPoint[] pointerPerpEnd = new SKPoint[2];
            pointerPerpEnd[0] = new SKPoint(center.X - ptrLineWidth / 3, ptrLineY1);
            pointerPerpEnd[1] = new SKPoint(center.X + ptrLineWidth / 3, ptrLineY1);

            pointerPerpStrt = RotateVector(pointerPerpStrt, startAngle, center);
            pointerPerpEnd = RotateVector(pointerPerpEnd, startAngle, center);

            SKPoint[] rotatedPointerPerpStrt = RotateVector(pointerPerpStrt, angle, center);
            SKPoint[] rotatedPointerPerpEnd = RotateVector(pointerPerpEnd, angle, center);

            if (gradAutoRotate && Fill is LinearGradientFill)
            {
                (Fill as LinearGradientFill).Angle = (int)(startAngle / RadialGauge.Radians + angle / RadialGauge.Radians) + 90;
            }

            using SKPaint brush = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = Export.ExportUtils.GetColorFromFill(Fill),
                IsAntialias = true
            };

            SKPoint[] p = new SKPoint[]
            {
                rotatedPointerPerpStrt[0],
                rotatedPointerPerpStrt[1],
                rotatedPointerPerpEnd[1],
                rotatedPointerPerpEnd[0],
            };

            using SKPath path = new SKPath();
            path.AddPoly(p, true);

            g.FillAndDrawEllipse(pen, brush, pointerCircle);
            g.FillAndDrawPath(pen, brush, path);
        }

        #endregion // Private Methods

        #region Public Methods

        /// <inheritdoc/>
        public override void Draw(FRPaintEventArgs e)
        {
            base.Draw(e);
            DrawHorz(e);
        }

        #endregion // Public Methods
    }
}
