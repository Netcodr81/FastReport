using FastReport.Utils;
using SkiaSharp;
using System;
using System.ComponentModel;

namespace FastReport.Gauge.Radial
{
    /// <summary>
    /// Represents a linear scale.
    /// </summary>
#if !DEBUG
    [DesignTimeVisible(false)]
#endif
    public class RadialScale : GaugeScale
    {
        #region Fields

        private float left;
        private float top;
        private float height;
        private float width;
        private float majorTicksOffset;
        private float minorTicksOffset;
        private SKPoint avrTick;
        private double stepValue;
        private SKPoint center;
        private double avrValue;
        private float majorStep;
        private float minorStep;
        private int sideTicksCount;
        private bool drawRight, drawLeft;

        #endregion // Fields

        #region Enums
        private enum HorAlign
        {
            Middle,
            Left,
            Right
        }
        private enum VertAlign
        {
            Bottom,
            Middle,
            Top
        }

        #endregion //Enums

        #region Properties

        [Browsable(false)]
        internal SKPoint AvrTick
        {
            get { return avrTick; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal double StepValue
        {
            get { return stepValue; }
            set { stepValue = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal double AverageValue
        {
            get { return avrValue; }
            set { avrValue = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal float MajorStep
        {
            get { return majorStep; }
            set { majorStep = value; }
        }
        #endregion // Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RadialScale"/> class.
        /// </summary>
        /// <param name="parent">The parent gauge object.</param>
        public RadialScale(RadialGauge parent) : base(parent)
        {
            MajorTicks = new ScaleTicks(5, 2, System.Drawing.Color.Black, 11);
            MinorTicks = new ScaleTicks(2, 1, System.Drawing.Color.Black, 4);
            majorStep = 27; //degree, 135/5
            minorStep = 5.4f; // degree, 27/5
            drawRight = true;
            drawLeft = true;
        }

        #endregion // Constructors

        #region Private Methods

        private bool isNegative(int i, bool isRightPart)
        {
            if (RadialUtils.IsSemicircle(Parent) || RadialUtils.IsQuadrant(Parent))
            {
                if (i <= sideTicksCount / 2)
                {
                    if (isRightPart)
                    {
                        if (RadialUtils.IsBottom(Parent) && RadialUtils.IsLeft(Parent))
                        {
                            return false;
                        }
                        else if (RadialUtils.IsBottom(Parent))
                            return false;
                        else if (RadialUtils.IsLeft(Parent))
                            return true;
                        else if (RadialUtils.IsRight(Parent))
                            return true; //!!!!!!!!!!!!!!!!!!
                        else return true; //Check!!!!
                    }
                    else
                    {
                        if (RadialUtils.IsTop(Parent) && RadialUtils.IsLeft(Parent))
                        {
                            return true;
                        }
                        else if (RadialUtils.IsBottom(Parent))
                            return false;
                        else if (RadialUtils.IsLeft(Parent))
                            return false;
                        else if (RadialUtils.IsRight(Parent))
                            return false; //!!!!!!!!!!!!!!!!!!
                        else return true; //Check!!!!
                    }
                }
                return false; //shouldn't be reached
            }
            else if (i > sideTicksCount / 2)
                return false;
            else return true;
        }
        private static SKColor ToSKColor(System.Drawing.Color color)
        {
            return new SKColor(color.R, color.G, color.B, color.A);
        }

        private SKFont CreateFont(FRPaintEventArgs e)
        {
            float size = Parent.IsPrinting ? Font.Size : Font.Size * e.ScaleX * 96f / DrawUtils.ScreenDpi;
            SKFontStyleWeight weight = (Font.Style & System.Drawing.FontStyle.Bold) != 0
                ? SKFontStyleWeight.Bold
                : SKFontStyleWeight.Normal;
            SKFontStyleSlant slant = (Font.Style & System.Drawing.FontStyle.Italic) != 0
                ? SKFontStyleSlant.Italic
                : SKFontStyleSlant.Upright;
            SKTypeface typeface = SKTypeface.FromFamilyName(Font.FontFamily.Name, new SKFontStyle(weight, SKFontStyleWidth.Normal, slant));
            return new SKFont(typeface, size);
        }

        private static SKPoint[] RotateVector(SKPoint[] vector, double angle, SKPoint center)
        {
            SKPoint[] rotatedVector = new SKPoint[2];
            rotatedVector[0] = new SKPoint(
                (float)(center.X + (vector[0].X - center.X) * Math.Cos(angle) + (center.Y - vector[0].Y) * Math.Sin(angle)),
                (float)(center.Y + (vector[0].X - center.X) * Math.Sin(angle) + (vector[0].Y - center.Y) * Math.Cos(angle)));
            rotatedVector[1] = new SKPoint(
                (float)(center.X + (vector[1].X - center.X) * Math.Cos(angle) + (center.Y - vector[1].Y) * Math.Sin(angle)),
                (float)(center.Y + (vector[1].X - center.X) * Math.Sin(angle) + (vector[1].Y - center.Y) * Math.Cos(angle)));
            return rotatedVector;
        }

        private void DrawText(FRPaintEventArgs e, string text, SKPaint brush, float x, float y, HorAlign hAlign, VertAlign vAlign)
        {
            IGraphics g = e.Graphics;
            using SKFont font = CreateFont(e);
            SKSize strSize = g.MeasureString(text, font);
            float dx = 0;
            float dy = 0;
            if (hAlign == HorAlign.Middle)
                dx = -strSize.Width / 2;
            else if (hAlign == HorAlign.Left)
                dx = 0;
            else if (hAlign == HorAlign.Right)
                dx = -strSize.Width;

            if (vAlign == VertAlign.Bottom)
                dy = -strSize.Height;
            else if (vAlign == VertAlign.Middle)
                dy = -strSize.Height / 2;
            else if (vAlign == VertAlign.Top)
                dy = 0;
            g.DrawString(text, font, brush, x + dx, y + dy);
        }

        private SKPoint GetTextPoint(SKPoint[] tick, float txtOffset, bool negativ, bool isRight)
        {
            float dx = Math.Abs(tick[1].X - tick[0].X);
            float dy = Math.Abs(tick[1].Y - tick[0].Y);
            float absA = (float)Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            float sinAlpha = dy / absA;
            float cosAlpha = dx / absA;
            float absA1 = absA + txtOffset;
            float dx1 = Math.Abs(absA1 * cosAlpha);
            float dy1 = Math.Abs(absA1 * sinAlpha);
            float pointX;
            float pointY;
            if (negativ)
                pointY = tick[1].Y - dy1;
            else
                pointY = tick[1].Y + dy1;
            if (isRight)
                pointX = tick[1].X + dx1;
            else
                pointX = tick[1].X - dx1;
            return new SKPoint(pointX, pointY);
        }

        private void DrawMajorTicks(FRPaintEventArgs e)
        {
            center = (Parent as RadialGauge).Center;
            stepValue = (Parent.Maximum - Parent.Minimum) / (MajorTicks.Count - 1);
            if (RadialUtils.IsQuadrant(Parent))
                stepValue *= 2;

            avrValue = Parent.Minimum + (Parent.Maximum - Parent.Minimum) / 2;

            bool isRightPart = true;
            bool isLeftPart = false;
            SKPoint txtPoint;

            IGraphics g = e.Graphics;
            using SKPaint pen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ToSKColor(MajorTicks.Color),
                StrokeWidth = MajorTicks.Width * e.ScaleX,
                IsAntialias = true
            };
            using SKPaint brush = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = Export.ExportUtils.GetColorFromFill(TextFill),
                IsAntialias = true
            };
            sideTicksCount = (MajorTicks.Count - 1) / 2;
            MajorTicks.Length = width / 12;

            using SKFont measureFont = CreateFont(e);
            SKSize maxTxt = g.MeasureString(Parent.Maximum.ToString(), measureFont);
            SKSize minTxt = g.MeasureString(Parent.Minimum.ToString(), measureFont);
            float maxTxtOffset = maxTxt.Height > maxTxt.Width ? maxTxt.Height : maxTxt.Width;
            float minTxtOffset = minTxt.Height > minTxt.Width ? minTxt.Height : minTxt.Width;
            majorTicksOffset = maxTxtOffset > minTxtOffset ? maxTxtOffset : minTxtOffset;

            SKPoint[] tick0 = new SKPoint[2];
            avrTick = new SKPoint(left + width / 2, top + majorTicksOffset);
            //first tick
            tick0[0] = avrTick;
            tick0[1] = new SKPoint(tick0[0].X, tick0[0].Y + MajorTicks.Length);

            double angle = 0;
            HorAlign horAlign = HorAlign.Middle;
            VertAlign vertAlign = VertAlign.Bottom;
            double startValue = avrValue;
            if (RadialUtils.IsSemicircle(Parent))
            {
                drawRight = true;
                drawLeft = true;
                if (RadialUtils.IsBottom(Parent))
                {
                    angle = 180 * RadialGauge.Radians;
                    horAlign = HorAlign.Middle;
                    vertAlign = VertAlign.Top;
                    majorStep *= -1;
                    isRightPart = true;
                    isLeftPart = false;
                }
                else if (RadialUtils.IsLeft(Parent))
                {
                    angle = -90 * RadialGauge.Radians;
                    horAlign = HorAlign.Right;
                    vertAlign = VertAlign.Middle;
                    isRightPart = false;
                    isLeftPart = false;
                }
                else if (RadialUtils.IsRight(Parent))
                {
                    angle = 90 * RadialGauge.Radians;
                    horAlign = HorAlign.Left;
                    vertAlign = VertAlign.Middle;
                    majorStep *= -1;
                    isRightPart = true; //false
                    isLeftPart = true; // false
                }
            }


            else if (RadialUtils.IsQuadrant(Parent))
            {
                if (RadialUtils.IsTop(Parent) && RadialUtils.IsLeft(Parent))
                {
                    startValue = Parent.Maximum;
                    //angle = 180 * RadialGauge.Radians;
                    horAlign = HorAlign.Middle;
                    vertAlign = VertAlign.Bottom;
                    //majorStep *= -1;
                    //isRightPart = true;
                    //isLeftPart = false;
                    drawRight = false;
                    drawLeft = true;

                    isRightPart = false;
                    isLeftPart = false;
                }
                else if (RadialUtils.IsBottom(Parent) && RadialUtils.IsLeft(Parent))
                {
                    startValue = Parent.Minimum;
                    angle = 180 * RadialGauge.Radians;
                    horAlign = HorAlign.Middle;
                    vertAlign = VertAlign.Top;
                    drawRight = true;
                    drawLeft = false;

                    isRightPart = false;
                    isLeftPart = false;
                }
                else if (RadialUtils.IsTop(Parent) && RadialUtils.IsRight(Parent))
                {
                    stepValue *= -1;
                    startValue = Parent.Maximum;
                    angle = 0;
                    horAlign = HorAlign.Middle;
                    vertAlign = VertAlign.Bottom;
                    drawRight = true;
                    drawLeft = false;

                    isRightPart = true;
                    isLeftPart = true;
                }
                else if (RadialUtils.IsBottom(Parent) && RadialUtils.IsRight(Parent))
                {
                    stepValue *= -1;
                    startValue = Parent.Minimum;
                    angle = 180 * RadialGauge.Radians;
                    horAlign = HorAlign.Middle;
                    vertAlign = VertAlign.Top;
                    drawRight = false;
                    drawLeft = true;

                    isRightPart = true;
                    isLeftPart = true;
                }
            }
            else
            {
                drawRight = true;
                drawLeft = true;
            }

            tick0 = RotateVector(tick0, angle, center);

            g.DrawLine(pen, tick0[0].X, tick0[0].Y, tick0[1].X, tick0[1].Y);
            string text = startValue.ToString();
            DrawText(e, text, brush, tick0[0].X, tick0[0].Y, horAlign, vertAlign);

            //rest of ticks
            SKPoint[] tick = new SKPoint[2];
            angle = majorStep * RadialGauge.Radians;

            for (int i = 0; i < sideTicksCount; i++)
            {
                //right side
                if (drawRight)
                {
                    tick = RotateVector(tick0, angle, center);
                    g.DrawLine(pen, tick[0].X, tick[0].Y, tick[1].X, tick[1].Y);
                    text = Convert.ToString(Math.Round(startValue + stepValue * (i + 1)));

                    if (i == sideTicksCount / 2)
                    {
                        if (RadialUtils.IsSemicircle(Parent) || RadialUtils.IsQuadrant(Parent))
                        {
                            if (RadialUtils.IsLeft(Parent) && RadialUtils.IsTop(Parent))
                            {
                                horAlign = HorAlign.Right;
                                vertAlign = VertAlign.Middle;
                            }
                            else if (RadialUtils.IsLeft(Parent) && RadialUtils.IsBottom(Parent))
                            {
                                horAlign = HorAlign.Right;
                                vertAlign = VertAlign.Middle;
                            }
                            else if (RadialUtils.IsRight(Parent) && RadialUtils.IsTop(Parent))
                            {
                                horAlign = HorAlign.Left;
                                vertAlign = VertAlign.Middle;
                            }
                            else if (RadialUtils.IsLeft(Parent))
                            {
                                horAlign = HorAlign.Middle;
                                vertAlign = VertAlign.Bottom;
                            }
                            else if (RadialUtils.IsRight(Parent))
                            {
                                horAlign = HorAlign.Middle;
                                vertAlign = VertAlign.Bottom;
                            }
                            else
                            {
                                horAlign = HorAlign.Left;
                                vertAlign = VertAlign.Middle;
                            }
                        }
                        else
                        {
                            horAlign = HorAlign.Left;
                            vertAlign = VertAlign.Middle;
                        }
                    }
                    else if (i < sideTicksCount / 2)
                    {
                        horAlign = HorAlign.Left;
                        if (RadialUtils.IsSemicircle(Parent) || RadialUtils.IsQuadrant(Parent))
                        {
                            if (RadialUtils.IsLeft(Parent) && RadialUtils.IsTop(Parent))
                            {
                                horAlign = HorAlign.Right;
                                vertAlign = VertAlign.Middle;
                            }
                            if (RadialUtils.IsLeft(Parent) && RadialUtils.IsBottom(Parent))
                            {
                                vertAlign = VertAlign.Top;
                                horAlign = HorAlign.Right;
                            }
                            else if (RadialUtils.IsBottom(Parent))
                                vertAlign = VertAlign.Top;
                            else if (RadialUtils.IsLeft(Parent))
                            {
                                horAlign = HorAlign.Right;
                                vertAlign = VertAlign.Bottom;
                            }
                            else if (RadialUtils.IsRight(Parent))
                            {
                                horAlign = HorAlign.Left;
                                vertAlign = VertAlign.Bottom;
                            }
                        }
                        else
                            vertAlign = VertAlign.Bottom;
                    }
                    else
                    {
                        horAlign = HorAlign.Left;
                        vertAlign = VertAlign.Top;
                    }
                    txtPoint = GetTextPoint(tick, -1 * e.ScaleX, isNegative(i, true), isRightPart);
                    DrawText(e, text, brush, txtPoint.X, txtPoint.Y, horAlign, vertAlign);
                }

                if (drawLeft)
                {
                    //left side
                    angle *= -1;
                    tick = RotateVector(tick0, angle, center);
                    g.DrawLine(pen, tick[0].X, tick[0].Y, tick[1].X, tick[1].Y);
                    text = Convert.ToString(Math.Round(startValue - stepValue * (i + 1)));

                    if (i == sideTicksCount / 2)
                    {
                        if (RadialUtils.IsSemicircle(Parent) || RadialUtils.IsQuadrant(Parent))
                        {
                            if ((RadialUtils.IsTop(Parent) || RadialUtils.IsBottom(Parent)) && RadialUtils.IsSemicircle(Parent))
                            {
                                horAlign = HorAlign.Right;
                                vertAlign = VertAlign.Middle;
                            }
                            else if (RadialUtils.IsLeft(Parent) && RadialUtils.IsTop(Parent))
                            {
                                horAlign = HorAlign.Right;
                                vertAlign = VertAlign.Middle;
                            }
                            else if (RadialUtils.IsRight(Parent) && RadialUtils.IsBottom(Parent))
                            {
                                horAlign = HorAlign.Left;
                                vertAlign = VertAlign.Middle;
                            }
                            else if (RadialUtils.IsLeft(Parent))
                            {
                                horAlign = HorAlign.Middle;
                                vertAlign = VertAlign.Top;
                            }
                            else if (RadialUtils.IsRight(Parent))
                            {
                                horAlign = HorAlign.Middle;
                                vertAlign = VertAlign.Top;
                            }
                        }
                        else
                        {
                            horAlign = HorAlign.Right;
                            vertAlign = VertAlign.Middle;
                        }
                    }
                    else if (i < sideTicksCount / 2)
                    {
                        horAlign = HorAlign.Right;

                        if (RadialUtils.IsSemicircle(Parent) || RadialUtils.IsQuadrant(Parent))
                        {
                            if (RadialUtils.IsRight(Parent) && RadialUtils.IsBottom(Parent))
                            {
                                vertAlign = VertAlign.Top;
                                horAlign = HorAlign.Left;
                            }
                            else if (RadialUtils.IsTop(Parent) && RadialUtils.IsLeft(Parent))
                            {
                                vertAlign = VertAlign.Bottom;
                                horAlign = HorAlign.Right;
                            }
                            else if (RadialUtils.IsBottom(Parent))
                                vertAlign = VertAlign.Top;
                            else if (RadialUtils.IsLeft(Parent))
                            {
                                horAlign = HorAlign.Right;
                                vertAlign = VertAlign.Top;
                            }
                            else if (RadialUtils.IsRight(Parent))
                            {
                                horAlign = HorAlign.Left;
                                vertAlign = VertAlign.Top;
                            }
                        }
                        else
                            vertAlign = VertAlign.Bottom;
                    }
                    else
                    {
                        horAlign = HorAlign.Right;
                        vertAlign = VertAlign.Top;
                    }
                    txtPoint = GetTextPoint(tick, -1 * e.ScaleX, isNegative(i, false), isLeftPart);
                    DrawText(e, text, brush, txtPoint.X, txtPoint.Y, horAlign, vertAlign);
                    angle *= -1;
                }

                angle += majorStep * RadialGauge.Radians;
            }
        }

        private void DrawMinorTicks(FRPaintEventArgs e)
        {
            IGraphics g = e.Graphics;
            using SKPaint pen = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = ToSKColor(MinorTicks.Color),
                StrokeWidth = MinorTicks.Width * e.ScaleX,
                IsAntialias = true
            };

            MinorTicks.Length = width / 24;
            minorTicksOffset = majorTicksOffset + MajorTicks.Length / 2 - MinorTicks.Length / 2;
            SKPoint center = new SKPoint(left + width / 2, top + height / 2);

            SKPoint[] tick0 = new SKPoint[2];
            //first tick
            tick0[0] = new SKPoint(left + width / 2, top + minorTicksOffset);
            tick0[1] = new SKPoint(tick0[0].X, tick0[0].Y + MinorTicks.Length);

            double angle = 0;
            if (RadialUtils.IsSemicircle(Parent) || RadialUtils.IsQuadrant(Parent))
            {
                if (RadialUtils.IsBottom(Parent) && RadialUtils.IsLeft(Parent))
                {
                    angle = -180 * RadialGauge.Radians;
                }
                else if (RadialUtils.IsTop(Parent) && RadialUtils.IsLeft(Parent))
                {
                    angle = 0;
                }
                else if (RadialUtils.IsTop(Parent) && RadialUtils.IsRight(Parent))
                {
                    angle = 0;
                }
                else if (RadialUtils.IsBottom(Parent))
                {
                    angle = 180 * RadialGauge.Radians;
                    majorStep *= -1;
                }
                else if (RadialUtils.IsLeft(Parent))
                {
                    angle = -90 * RadialGauge.Radians;
                }
                else if (RadialUtils.IsRight(Parent))
                {
                    angle = 90 * RadialGauge.Radians;
                    majorStep *= -1;
                }
            }
            tick0 = RotateVector(tick0, angle, center);

            //rest of ticks
            SKPoint[] tick = new SKPoint[2];
            angle = minorStep * RadialGauge.Radians;
            for (int i = 0; i < MajorTicks.Count / 2 * (MinorTicks.Count + 1); i++)
            {
                if ((i + 1) % (MinorTicks.Count + 1) != 0)
                {
                    if (drawRight)
                    {
                        tick = RotateVector(tick0, angle, center);
                        g.DrawLine(pen, tick[0].X, tick[0].Y, tick[1].X, tick[1].Y);
                    }
                    if (drawLeft)
                    {
                        angle *= -1;
                        tick = RotateVector(tick0, angle, center);
                        g.DrawLine(pen, tick[0].X, tick[0].Y, tick[1].X, tick[1].Y);
                        angle *= -1;
                    }
                }
                angle += minorStep * RadialGauge.Radians;
            }
        }

        #endregion // Private Methods

        #region Public Methods

        /// <inheritdoc/>
        public override void Assign(GaugeScale src)
        {
            base.Assign(src);

            RadialScale s = src as RadialScale;
            MajorTicks.Assign(s.MajorTicks);
            MinorTicks.Assign(s.MinorTicks);
        }

        /// <inheritdoc/>
        public override void Draw(FRPaintEventArgs e)
        {
            base.Draw(e);
            if ((Parent as RadialGauge).Type == RadialGaugeType.Circle)
            {
                MajorTicks.Count = 11;
                MinorTicks.Count = 4;
                majorStep = 135f / 5;
                minorStep = 135f / 5f / 5f;
            }
            else if ((Parent as RadialGauge).Type == RadialGaugeType.Semicircle)
            {
                MajorTicks.Count = 5;
                MinorTicks.Count = 3;
                majorStep = 90f / 2;
                minorStep = 90f / 2 / 4;
            }
            else if ((Parent as RadialGauge).Type == RadialGaugeType.Quadrant)
            {
                MajorTicks.Count = 5;
                MinorTicks.Count = 3;
                majorStep = 90f / 2;
                minorStep = 90f / 2 / 4;
            }
            left = Parent.AbsLeft * e.ScaleX;
            top = Parent.AbsTop * e.ScaleY;
            height = Parent.Height * e.ScaleY;
            width = Parent.Width * e.ScaleX;
            DrawMajorTicks(e);
            DrawMinorTicks(e);
        }

        /// <inheritdoc/>
        public override void Serialize(FRWriter writer, string prefix, GaugeScale diff)
        {
            base.Serialize(writer, prefix, diff);

            RadialScale dc = diff as RadialScale;
            MajorTicks.Serialize(writer, prefix + ".MajorTicks", dc.MajorTicks);
            MinorTicks.Serialize(writer, prefix + ".MinorTicks", dc.MinorTicks);
        }

        #endregion // Public Methods
    }
}
