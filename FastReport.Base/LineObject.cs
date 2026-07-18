using FastReport.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using SkiaSharp;
using RectangleF = SkiaSharp.SKRect;
using GraphicsPath = SkiaSharp.SKPath;

namespace FastReport
{
    /// <summary>
    /// Represents a line object.
    /// </summary>
    /// <remarks>
    /// Use the <b>Border.Width</b>, <b>Border.Style</b> and <b>Border.Color</b> properties to set 
    /// the line width, style and color. Set the <see cref="Diagonal"/> property to <b>true</b>
    /// if you want to show a diagonal line.
    /// </remarks>
    public partial class LineObject : ReportComponentBase
    {
        #region Fields
        private bool diagonal;
        private CapSettings startCap;
        private CapSettings endCap;
        private FloatCollection dashPattern;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating that the line is diagonal.
        /// </summary>
        /// <remarks>
        /// If this property is <b>false</b>, the line can be only horizontal or vertical.
        /// </remarks>
        [DefaultValue(false)]
        [Category("Appearance")]
        public bool Diagonal
        {
            get { return diagonal; }
            set { diagonal = value; }
        }

        /// <summary>
        /// Gets or sets the start cap settings.
        /// </summary>
        [Category("Appearance")]
        public CapSettings StartCap
        {
            get { return startCap; }
            set { startCap = value; }
        }

        /// <summary>
        /// Gets or sets the end cap settings.
        /// </summary>
        [Category("Appearance")]
        public CapSettings EndCap
        {
            get { return endCap; }
            set { endCap = value; }
        }

        /// <summary>
        /// Gets or sets collection of values for custom dash pattern.
        /// </summary>
        /// <remarks>
        /// Each element should be a non-zero positive number. 
        /// If the number is negative or zero, that number is replaced by one.
        /// </remarks>
        [Category("Appearance")]
        public FloatCollection DashPattern
        {
            get { return dashPattern; }
            set { dashPattern = value; }
        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public override void Assign(Base source)
        {
            base.Assign(source);

            LineObject src = source as LineObject;
            Diagonal = src.Diagonal;
            StartCap.Assign(src.StartCap);
            EndCap.Assign(src.EndCap);
            DashPattern.Assign(src.DashPattern);
        }

        /// <inheritdoc/>
        public override void Draw(FRPaintEventArgs e)
        {
            IGraphics g = e.Graphics;
            // draw crosshair when inserting a line
            if (Width == 0 && Height == 0)
            {
                DrawCrossHair(e, AbsLeft, AbsTop);
                return;
            }

            using SKPaint paint = new SKPaint
            {
                Color = Border.Color,
                StrokeWidth = Border.Width * e.ScaleX,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Butt,
                StrokeJoin = SKStrokeJoin.Miter
            };

            ApplyDashStyle(paint, DashPattern, Border.DashStyle);

            float width = Width;
            float height = Height;

            if (!Diagonal)
            {
                if (Math.Abs(Width) > Math.Abs(Height))
                    height = 0;
                else
                    width = 0;
            }

            float x1 = AbsLeft * e.ScaleX;
            float y1 = AbsTop * e.ScaleY;
            float x2 = (AbsLeft + width) * e.ScaleX;
            float y2 = (AbsTop + height) * e.ScaleY;

            if (StartCap.Style == CapStyle.None && EndCap.Style == CapStyle.None)
            {
                g.DrawLine(paint, x1, y1, x2, y2);
            }
            else
            {
                // draw line caps manually. It is necessary for correct svg rendering
                float angle = (float)(Math.Atan2(x2 - x1, y2 - y1) / Math.PI * 180);
                float len = (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
                float scale = Border.Width * e.ScaleX;

                IGraphicsState state = g.Save();
                g.TranslateTransform(x1, y1);
                g.RotateTransform(-angle);
                float y = 0;
                GraphicsPath startCapPath = null;
                GraphicsPath endCapPath = null;
                float inset = 0;
                if (StartCap.Style != CapStyle.None)
                {
                    StartCap.GetCustomCapPath(out startCapPath, out inset);
                    y += inset * scale;
                }
                if (EndCap.Style != CapStyle.None)
                {
                    EndCap.GetCustomCapPath(out endCapPath, out inset);
                    len -= inset * scale;
                }
                g.DrawLine(paint, 0, y, 0, len);
                g.Restore(state);

                using SKPaint capPaint = new SKPaint
                {
                    Color = Border.Color,
                    StrokeWidth = 1,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                };

                if (StartCap.Style != CapStyle.None)
                {
                    state = g.Save();
                    g.TranslateTransform(x1, y1);
                    g.RotateTransform(180 - angle);
                    g.ScaleTransform(scale, scale);
                    g.DrawPath(capPaint, startCapPath);
                    g.Restore(state);
                    startCapPath.Dispose();
                }
                if (EndCap.Style != CapStyle.None)
                {
                    state = g.Save();
                    g.TranslateTransform(x2, y2);
                    g.RotateTransform(-angle);
                    g.ScaleTransform(scale, scale);
                    g.DrawPath(capPaint, endCapPath);
                    g.Restore(state);
                    endCapPath.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public override bool IsHaveToConvert(object sender)
        {
            return StartCap.Style != CapStyle.None || EndCap.Style != CapStyle.None;
        }


        internal override RectangleF GetExtendedSize()
        {
            using (GraphicsPath path = CreatePath())
            {
                var bounds = path.Bounds;
                if (Parent is ComponentBase parent)
                {
                    bounds.Left -= parent.AbsLeft;
                    bounds.Top -= parent.AbsTop;
                    bounds.Right -= parent.AbsLeft;
                    bounds.Bottom -= parent.AbsTop;
                }
                return bounds;
            }
        }

        internal GraphicsPath CreatePath()
        {
            float scale = Border.Width;
            float angle = (float)Math.Abs(Math.Atan2(AbsRight - AbsLeft, AbsBottom - AbsTop) / Math.PI * 180);
            GraphicsPath fullPath = new GraphicsPath();
            if (StartCap.Style != CapStyle.None)
            {
                StartCap.GetCustomCapPath(out GraphicsPath path, out float _);
                path.Transform(SKMatrix.CreateScale(scale, scale));
                path.Transform(SKMatrix.CreateRotationDegrees(180 - angle));
                path.Transform(SKMatrix.CreateTranslation(AbsLeft, AbsTop));
                fullPath.AddPath(path);
                path.Dispose();
            }

            fullPath.MoveTo(AbsLeft, AbsTop);
            fullPath.LineTo(AbsRight, AbsBottom);

            if (EndCap.Style != CapStyle.None)
            {
                EndCap.GetCustomCapPath(out GraphicsPath path, out float _);
                path.Transform(SKMatrix.CreateScale(scale, scale));
                path.Transform(SKMatrix.CreateRotationDegrees(-angle));
                path.Transform(SKMatrix.CreateTranslation(AbsRight, AbsBottom));
                fullPath.AddPath(path);
                path.Dispose();
            }

            return fullPath;
        }

        private static void ApplyDashStyle(SKPaint paint, FloatCollection dashPattern, DashStyle dashStyle)
        {
            if (dashPattern?.Count > 0)
            {
                float[] intervals = new float[dashPattern.Count];
                for (int i = 0; i < dashPattern.Count; i++)
                {
                    float value = dashPattern[i] <= 0 ? 1 : dashPattern[i];
                    intervals[i] = value * paint.StrokeWidth;
                }
                paint.PathEffect = SKPathEffect.CreateDash(intervals, 0);
                return;
            }

            float w = paint.StrokeWidth;
            switch (dashStyle)
            {
                case DashStyle.Dash:
                    paint.PathEffect = SKPathEffect.CreateDash(new[] { 3 * w, 1 * w }, 0);
                    break;
                case DashStyle.Dot:
                    paint.PathEffect = SKPathEffect.CreateDash(new[] { 1 * w, 1 * w }, 0);
                    break;
                case DashStyle.DashDot:
                    paint.PathEffect = SKPathEffect.CreateDash(new[] { 3 * w, 1 * w, 1 * w, 1 * w }, 0);
                    break;
                case DashStyle.DashDotDot:
                    paint.PathEffect = SKPathEffect.CreateDash(new[] { 3 * w, 1 * w, 1 * w, 1 * w, 1 * w, 1 * w }, 0);
                    break;
                default:
                    paint.PathEffect = null;
                    break;
            }
        }

        /// <inheritdoc/>
        public override List<ValidationError> Validate()
        {
            List<ValidationError> listError = new List<ValidationError>();

            if (Height == 0 && Width == 0)
                listError.Add(new ValidationError(Name, ValidationError.ErrorLevel.Error, Res.Get("Messages,Validator,IncorrectSize"), this));

            if (Name == "")
                listError.Add(new ValidationError(Name, ValidationError.ErrorLevel.Error, Res.Get("Messages,Validator,UnnamedObject"), this));

            if (Parent is ReportComponentBase && !Validator.RectContainInOtherRect((Parent as ReportComponentBase).AbsBounds, this.AbsBounds))
                listError.Add(new ValidationError(Name, ValidationError.ErrorLevel.Error, Res.Get("Messages,Validator,OutOfBounds"), this));

            return listError;
        }

        /// <inheritdoc/>
        public override void Serialize(FRWriter writer)
        {
            Border.SimpleBorder = true;
            base.Serialize(writer);
            LineObject c = writer.DiffObject as LineObject;

            if (Diagonal != c.Diagonal)
                writer.WriteBool("Diagonal", Diagonal);
            StartCap.Serialize("StartCap", writer, c.StartCap);
            EndCap.Serialize("EndCap", writer, c.EndCap);
            if (DashPattern?.Count > 0)
                writer.WriteValue("DashPattern", DashPattern);
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="LineObject"/> class with default settings.
        /// </summary>
        public LineObject()
        {
            startCap = new CapSettings();
            endCap = new CapSettings();
            FlagSimpleBorder = true;
            FlagUseFill = false;
            dashPattern = new FloatCollection();
        }
    }
}
