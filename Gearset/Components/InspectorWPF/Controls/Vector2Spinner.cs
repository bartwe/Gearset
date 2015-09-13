using System;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Point = System.Windows.Point;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Vector2Spinner : VisualItemBase {
        Vector2 _downPosition;

        /// <summary>
        ///     True if the TreeNode was Updating before the user
        ///     started to edit it.
        /// </summary>
        bool _wasUpdating;

        public Vector2 RealValue;
        ModeEnum _currentMode;
        Vector2 _mouseDownDirection;
        Vector2 _mouseDownModeValue;
        bool _isDragging;

        public Vector2Spinner() {
            InitializeComponent();

            Button1.PreviewMouseDown += Button1_MouseDownHandler;
            Button1.PreviewMouseUp += Button1_MouseUpHandler;
            Button1.PreviewMouseMove += Button1_MouseMoveHandler;

            Button2.PreviewMouseDown += Button2_MouseDownHandler;
            Button2.PreviewMouseUp += Button1_MouseUpHandler;
            Button2.PreviewMouseMove += Button1_MouseMoveHandler;
        }

        public Vector2 Value {
            get { return RealValue; }
            set {
                RealValue = value;
                var x = RealValue.X;
                var y = RealValue.Y;
                var l = RealValue.Length();
                String xstr;
                String ystr;
                String lstr;
                if (Math.Abs(x) > 1e-4)
                    xstr = String.Format("{0:0.0000}", x);
                else
                    xstr = String.Format("{0:0.0#e+00}", x);
                if (Math.Abs(y) > 1e-4)
                    ystr = String.Format("{0:0.0000}", y);
                else
                    ystr = String.Format("{0:0.0#e+00}", y);
                if (Math.Abs(l) > 1e-4)
                    lstr = String.Format("{0:0.0000}", l);
                else
                    lstr = String.Format("{0:0.0#e+00}", l);
                TextBlock1.Text = String.Format("({0}, {1}) {2} ", xstr, ystr, lstr);
            }
        }

        /// <summary>
        ///     Updates the UI.
        /// </summary>
        public override sealed void UpdateUi(Object value) {
            Value = (Vector2)value;
        }

        /// <summary>
        ///     Updates the variable fromt he UI.
        /// </summary>
        public override sealed void UpdateVariable() {
            TreeNode.Property = Value;
        }

        enum ModeEnum {
            Xy,
            L
        }

        #region Mouse movement events

        public void Button1_MouseUpHandler(object sender, MouseButtonEventArgs e) {
            Mouse.Capture(null, CaptureMode.None);
            _isDragging = false;
            TreeNode.Updating = _wasUpdating;
        }

        public void Button1_MouseMoveHandler(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && _isDragging) {
                var p = new Point();

                switch (_currentMode) {
                    case ModeEnum.Xy:
                        p = Mouse.GetPosition(Button1);
                        break;
                    case ModeEnum.L:
                        p = Mouse.GetPosition(Button2);
                        p.Y = -p.Y;
                        break;
                }
                var currentPos = new Vector2((float)p.X, (float)p.Y);

                var deltaVector = new Vector2 {
                    X = Math.Max(Math.Abs(_mouseDownModeValue.X) / 100.0f, 0.01f),
                    Y = Math.Max(Math.Abs(_mouseDownModeValue.Y) / 100.0f, 0.01f)
                };

                var delta = deltaVector.Length();
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    delta *= 10f;
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    delta *= 0.1f;

                var movement = currentPos - _downPosition;
                movement *= -1;
                var newValue = _mouseDownModeValue - movement * delta;

                var currentModeValue = GetCurrentModeValue();
                var currentPropertyValue = (Vector2)TreeNode.Property;

                if (newValue != currentModeValue && newValue != null) {
                    var v = new Vector2();
                    switch (_currentMode) {
                        case ModeEnum.Xy:
                            v = new Vector2(newValue.X, newValue.Y);
                            break;
                        case ModeEnum.L:
                            v = _mouseDownDirection * newValue.Y;
                            break;
                    }
                    RealValue = v;
                    UpdateVariable();
                    UpdateUi(TreeNode.Property);
                }
            }
        }

        Vector2 GetCurrentModeValue() {
            var result = new Vector2();
            var currentPropertyValue = (Vector2)TreeNode.Property;
            switch (_currentMode) {
                case ModeEnum.Xy:
                    result = new Vector2(currentPropertyValue.X, currentPropertyValue.Y);
                    break;
                case ModeEnum.L:
                    result = new Vector2(0, currentPropertyValue.Length());
                    break;
            }
            return result;
        }

        public void Button1_MouseDownHandler(object sender, MouseButtonEventArgs e) {
            _currentMode = ModeEnum.Xy;
            _wasUpdating = TreeNode.Updating;
            _mouseDownModeValue = GetCurrentModeValue();
            TreeNode.Updating = false;
            Mouse.Capture(Button1);
            var p = e.GetPosition(Button1);
            _downPosition = new Vector2((float)p.X, (float)p.Y);
            _isDragging = true;
        }

        public void Button2_MouseDownHandler(object sender, MouseButtonEventArgs e) {
            _currentMode = ModeEnum.L;
            _wasUpdating = TreeNode.Updating;
            _mouseDownModeValue = GetCurrentModeValue();

            // Get the current vector direction so we can scale it
            _mouseDownDirection = (Vector2)TreeNode.Property;
            if (_mouseDownDirection != Vector2.Zero)
                _mouseDownDirection.Normalize();
            else
                _mouseDownDirection = Vector2.Normalize(Vector2.One);

            TreeNode.Updating = false;
            Mouse.Capture(Button2);
            var p = e.GetPosition(Button2);
            _downPosition = new Vector2((float)p.X, (float)p.Y);
            _isDragging = true;
        }

        #endregion
    }
}
