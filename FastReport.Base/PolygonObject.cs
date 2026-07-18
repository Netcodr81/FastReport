using FastReport.Utils;
using SkiaSharp;
using GraphicsPath = SkiaSharp.SKPath;

namespace FastReport
{
    /// <summary>
    /// Represents a polygon object.
    /// </summary>
    /// <remarks>
    /// Use the <b>Border.Width</b>, <b>Border.Style</b> and <b>Border.Color</b> properties to set 
    /// the line width, style and color.
    /// 
    /// </remarks>
    public partial class PolygonObject : PolyLineObject
    {
        #region Protected Methods

        /// <summary>
        /// Calculate GraphicsPath for draw to page
        /// </summary>
        /// <param name="pen">Pen for lines</param>
        /// <param name="scaleX">scale by width</param>
        /// <param name="scaleY">scale by height</param>
        /// <returns>Always returns a non-empty path</returns>
        protected GraphicsPath getPolygonPath(SKPaint pen, float scaleX, float scaleY)
        {
            GraphicsPath gp = base.GetPath(pen, AbsLeft, AbsTop, AbsRight, AbsBottom, scaleX, scaleY);
            gp.Close();
            return gp;
        }

        /// <summary>
        /// Draw polyline path to graphics
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void drawPoly(FRPaintEventArgs e)
        {
            float x = (AbsLeft + Border.Width / 2) * e.ScaleX;
            float y = (AbsTop + Border.Width / 2) * e.ScaleY;
            float dx = (Width - Border.Width) * e.ScaleX - 1;
            float dy = (Height - Border.Width) * e.ScaleY - 1;

            float strokeWidth = polygonSelectionMode == PolygonSelectionMode.MoveAndScale
                ? Border.Width * e.ScaleX
                : 1f;

            using SKPaint pen = new SKPaint
            {
                Color = Border.Color,
                StrokeWidth = strokeWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            if (DashPattern?.Count > 0)
            {
                float[] intervals = new float[DashPattern.Count];
                for (int i = 0; i < DashPattern.Count; i++)
                    intervals[i] = (DashPattern[i] <= 0 ? 1 : DashPattern[i]) * strokeWidth;
                pen.PathEffect = SKPathEffect.CreateDash(intervals, 0);
            }
            else
            {
                float w = strokeWidth;
                switch (Border.DashStyle)
                {
                    case DashStyle.Dash:
                        pen.PathEffect = SKPathEffect.CreateDash(new[] { 3 * w, 1 * w }, 0);
                        break;
                    case DashStyle.Dot:
                        pen.PathEffect = SKPathEffect.CreateDash(new[] { 1 * w, 1 * w }, 0);
                        break;
                    case DashStyle.DashDot:
                        pen.PathEffect = SKPathEffect.CreateDash(new[] { 3 * w, 1 * w, 1 * w, 1 * w }, 0);
                        break;
                    case DashStyle.DashDotDot:
                        pen.PathEffect = SKPathEffect.CreateDash(new[] { 3 * w, 1 * w, 1 * w, 1 * w, 1 * w, 1 * w }, 0);
                        break;
                }
            }

            SKColor fillColor;
            if (Fill is SolidFill solidFill)
                fillColor = solidFill.Color;
            else
            {
                FastReport.Brush fb = Fill.CreateBrush(new SKRect(x, y, x + dx, y + dy), e.ScaleX, e.ScaleY);
                fillColor = (fb as SolidBrush)?.Color ?? SKColors.Transparent;
                fb.Dispose();
            }

            using SKPaint brush = new SKPaint
            {
                Color = fillColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using (GraphicsPath path = getPolygonPath(pen, e.ScaleX, e.ScaleY))
            {
                if (polygonSelectionMode == PolygonSelectionMode.MoveAndScale)
                    e.Graphics.FillAndDrawPath(pen, brush, path);
                else
                    e.Graphics.DrawPath(pen, path);
            }
        }

        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Serialize(FRWriter writer)
        {
            Border.SimpleBorder = true;
            base.Serialize(writer);
            PolygonObject c = writer.DiffObject as PolygonObject;
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LineObject"/> class with default settings.
        /// </summary>
        public PolygonObject() : base()
        {
            FlagSimpleBorder = true;
            FlagUseFill = true;
        }
    }
}