using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Gearset.Components.CurveEditorControl {
    public sealed class GridRuler : FrameworkElement {
        public double halfDpiX;
        public double halfDpiY;
        bool guidelinesFixed;
        Pen gridLinePen;
        Pen gridBoldLinePen;
        Pen gridZeroLinePen;
        Brush backgroundBrush;
        SolidColorBrush textBrush;
        Glyphs glyphs;
        GeometryDrawing SeekNeedle;
        bool mouseLeftDown;
        bool dragging;
        Point mouseDownPos;

        public GridRuler() {
            ClipToBounds = true;

            MouseWheel += CurveEditorControl_MouseWheel;
            MouseMove += CurveEditorControl_MouseMove;
            MouseDown += CurveEditorControl_MouseDown;
            MouseUp += CurveEditorControl_MouseUp;

            InitializeResources();
        }

        public float Min { get { return (float)GetValue(MinProperty); } set { SetValue(MinProperty, value); } }
        public float Max { get { return (float)GetValue(MaxProperty); } set { SetValue(MaxProperty, value); } }
        public RulerOrientation Orientation { get { return (RulerOrientation)GetValue(OrientationProperty); } set { SetValue(OrientationProperty, value); } }
        float MinX { get { return Orientation == RulerOrientation.Horizontal ? Min : 0; } }
        float MaxX { get { return Orientation == RulerOrientation.Horizontal ? Max : 0.1f; } }
        float MinY { get { return Orientation == RulerOrientation.Vertical ? Min : 0; } }
        float MaxY { get { return Orientation == RulerOrientation.Vertical ? Max : 0.1f; } }
        public double NeedlePosition { get; set; }

        public static void LimitsChangedCalllBack(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var control = d as GridRuler;
            if (control != null)
                control.InvalidateVisual();
        }

        /// <summary>
        ///     Initializes all general pens and brushes.
        /// </summary>
        void InitializeResources() {
            textBrush = new SolidColorBrush(Color.FromRgb(120, 120, 120));
            textBrush.Freeze();

            Brush gridLineBrush = new SolidColorBrush(Color.FromRgb(160, 160, 160));
            ;
            gridLineBrush.Freeze();
            gridLinePen = new Pen(gridLineBrush, 1);
            gridLinePen.Freeze();

            Brush gridBoldLineBrush = new SolidColorBrush(Color.FromRgb(20, 20, 20));
            gridBoldLineBrush.Freeze();
            gridBoldLinePen = new Pen(gridBoldLineBrush, 1);
            gridZeroLinePen = new Pen(gridBoldLineBrush, 2);
            gridBoldLinePen.Freeze();
            gridZeroLinePen.Freeze();


            backgroundBrush = new SolidColorBrush(Color.FromArgb(0, 48, 48, 48));
            backgroundBrush.Freeze();

            glyphs = new Glyphs();
            glyphs.FontUri = new Uri(Environment.GetFolderPath(Environment.SpecialFolder.Fonts) + "\\Arial.TTF");
            glyphs.FontRenderingEmSize = 11;
            glyphs.StyleSimulations = StyleSimulations.None;

            // Seek Needle
            Brush seekNeedleBrush = Brushes.Red;
            var seekNeedlePen = new Pen(Brushes.Black, 1);
            seekNeedlePen.Freeze();
            SeekNeedle = new GeometryDrawing();
            var path = new PathGeometry();
            path.Figures.Add(new PathFigure(
                new Point(-4, 0), new[] {
                    new PolyLineSegment(new[] {
                        new Point(4, 0),
                        new Point(0, -7),
                        new Point(0, -20),
                        new Point(0, -7)
                    }, true)
                }, true));
            SeekNeedle.Geometry = path;
            SeekNeedle.Brush = seekNeedleBrush;
            SeekNeedle.Pen = seekNeedlePen;
        }

        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(float), typeof(GridRuler), new PropertyMetadata(-0.5f, LimitsChangedCalllBack));

        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(float), typeof(GridRuler), new PropertyMetadata(2f, LimitsChangedCalllBack));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(RulerOrientation), typeof(GridRuler), new PropertyMetadata(RulerOrientation.Vertical));

        #region Coords transforms

        public Point CurveCoordsToScreen(ref Point point) {
            return new Point(((point.X - MinX) / (MaxX - MinX)) * ActualWidth, -((point.Y - MinY) / (MaxY - MinY)) * ActualHeight + ActualHeight);
        }

        public Point ScreenCoordsToCurve(ref Point point) {
            return new Point(((point.X) / (ActualWidth)) * (MaxX - MinX) + MinX, ((-point.Y + ActualHeight) / (ActualHeight)) * (MaxY - MinY) + MinY);
        }

        public Point ScreenCoordsToCurveNormal(ref Point point) {
            return new Point(((point.X) / (ActualWidth)) * (MaxX - MinX), ((-point.Y) / (ActualHeight)) * (MaxY - MinY));
        }

        #endregion

        #region Mouse handling

        void CurveEditorControl_MouseDown(object sender, MouseButtonEventArgs e) {
            mouseLeftDown = true;
            mouseDownPos = e.GetPosition(this);
        }

        void CurveEditorControl_MouseMove(object sender, MouseEventArgs e) {
            var mousePos = e.GetPosition(this);

            // Detect a drag.
            if ((mouseLeftDown) && !dragging) {
                if (Math.Abs(mousePos.X - mouseDownPos.X) > 1 || // SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(mousePos.Y - mouseDownPos.Y) > 1) //SystemParameters.MinimumVerticalDragDistance)
                {
                    dragging = true;
                    Mouse.Capture(this);
                }
            }

            if (dragging) {
                mousePos = ScreenCoordsToCurve(ref mousePos);
                NeedlePosition = mousePos.X;
                Dispatcher.BeginInvoke(new Action(InvalidateVisual), DispatcherPriority.SystemIdle);
            }
        }

        bool invalidateRequested;

        public new void InvalidateVisual() {
            invalidateRequested = true;
        }

        public void UpdateRender() {
            if (invalidateRequested) {
                base.InvalidateVisual();
                invalidateRequested = false;
            }
        }

        void CurveEditorControl_MouseUp(object sender, MouseButtonEventArgs e) {
            mouseLeftDown = false;
            dragging = false;
            Mouse.Capture(null);
        }

        static void CurveEditorControl_MouseWheel(object sender, MouseWheelEventArgs e) {}

        #endregion

        #region Rendering

        protected override void OnRender(DrawingContext dc) {
            if (!guidelinesFixed) {
                var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
                halfDpiX = m.M11 * 0.5;
                halfDpiY = m.M22 * 0.5;
                guidelinesFixed = true;
            }

            // Create a guidelines set
            var guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(halfDpiX);
            guidelines.GuidelinesY.Add(halfDpiY);
            dc.PushGuidelineSet(guidelines);

            dc.DrawRectangle(backgroundBrush, null, new Rect(0, 0, ActualWidth, ActualHeight));

            double pos = 0;
            const double stepSize = 0.1;

            switch (Orientation) {
                case RulerOrientation.Vertical:
                    //// Thin horizontal lines
                    //range = MaxY - MinY;
                    //orderOfMag = Math.Pow(10, Math.Truncate(Math.Log10(range)));
                    //stepSize = 0.1 * orderOfMag;
                    pos = (Math.Truncate(MinY / stepSize) - 0) * stepSize;
                    //DrawGridHorizonalLines(dc, pos, stepSize, gridLinePen);
                    break;
                case RulerOrientation.Horizontal:
                    // Thin vertical lines.
                    //range = MaxX - MinX;
                    //orderOfMag = Math.Pow(10, Math.Truncate(Math.Log10(range)));
                    //stepSize = 0.1 * orderOfMag;
                    pos = Math.Truncate(MinX / stepSize) * stepSize;
                    //DrawGridVerticalLines(dc, pos, stepSize, gridLinePen);

                    // Draw the seek needle
                    var needlePos = new Point(NeedlePosition, 0);
                    needlePos = CurveCoordsToScreen(ref needlePos);
                    dc.PushTransform(new TranslateTransform(needlePos.X, needlePos.Y));
                    dc.DrawDrawing(SeekNeedle);
                    dc.Pop();
                    break;
            }

            dc.Pop();

            switch (Orientation) {
                case RulerOrientation.Vertical:
                    DrawHorizontalLinesText(dc, pos, stepSize);
                    break;
                case RulerOrientation.Horizontal:
                    DrawVerticalLinesText(dc, pos, stepSize);
                    break;
            }
            base.OnRender(dc);
        }

        void DrawHorizontalLinesText(DrawingContext dc, double initialPos, double stepSize) {
            var previousTextPos = initialPos;
            while (initialPos < MaxY) {
                initialPos += stepSize;

                var p0 = new Point(MinX, initialPos);
                p0 = CurveCoordsToScreen(ref p0);
                p0.Y = Math.Truncate(p0.Y);

                if (Math.Abs(p0.Y - previousTextPos) > 20) {
                    p0.Y = Math.Truncate(p0.Y) + 0.5;
                    // Draw the tick
                    var p1 = new Point(0, p0.Y);
                    p0 = new Point(3, p0.Y);
                    dc.DrawLine(gridLinePen, p0, p1);

                    p0.Y = Math.Truncate(p0.Y);
                    glyphs.UnicodeString = String.Format("{0:0.###}", initialPos);
                    glyphs.OriginX = 4;
                    glyphs.OriginY = p0.Y + 5;

                    dc.DrawGlyphRun(textBrush, glyphs.ToGlyphRun());
                    previousTextPos = p0.Y;
                }
            }
        }

        void DrawGridHorizonalLines(DrawingContext dc, double initialPos, double stepSize, Pen pen) {
            var previousTextPos = initialPos;
            while (initialPos < MaxY) {
                initialPos += stepSize;

                var p1 = new Point(MaxX, initialPos);
                p1 = CurveCoordsToScreen(ref p1);
                p1.Y = Math.Truncate(p1.Y);
                var p0 = new Point(p1.X - 5, p1.Y);
                dc.DrawLine(pen, p0, p1);
            }
        }

        void DrawVerticalLinesText(DrawingContext dc, double initialPos, double stepSize) {
            var previousTextPos = initialPos;
            while (initialPos < MaxX) {
                initialPos += stepSize;

                var p0 = new Point(initialPos, MinY);
                p0 = CurveCoordsToScreen(ref p0);
                p0.X = Math.Truncate(p0.X);

                if (Math.Abs(p0.X - previousTextPos) > 40) {
                    p0.X = Math.Truncate(p0.X) + 0.5;
                    // Draw the tick
                    var p1 = new Point(p0.X, ActualHeight);
                    p0 = new Point(p0.X, ActualHeight - 3);
                    dc.DrawLine(gridLinePen, p0, p1);

                    p0.X = Math.Truncate(p0.X - 3);
                    glyphs.UnicodeString = String.Format("{0:0.###}", initialPos);
                    glyphs.OriginX = p0.X;
                    glyphs.OriginY = 12;

                    dc.DrawGlyphRun(textBrush, glyphs.ToGlyphRun());
                    previousTextPos = p0.X;
                }
            }
        }

        void DrawGridVerticalLines(DrawingContext dc, double initialPos, double stepSize, Pen pen) {
            while (initialPos < MaxX) {
                initialPos += stepSize;

                var p0 = new Point(initialPos, MinY);
                var p1 = new Point(initialPos, MaxY);
                p0 = CurveCoordsToScreen(ref p0);
                p1 = CurveCoordsToScreen(ref p1);
                p0.X = p1.X = Math.Truncate(p0.X);
                dc.DrawLine(pen, p0, p1);
            }
        }

        #endregion
    }
}
