using System;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Point = System.Windows.Point;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Vector3Spinner : VisualItemBase {
        public enum ModeEnum {
            Xy,
            Xz,
            Yz,
            L
        }

        Vector2 _downPosition;
        Vector2 _mouseDownModeValue;
        bool _isDragging;

        /// <summary>
        ///     True if the TreeNode was Updating before the user
        ///     started to edit it.
        /// </summary>
        bool _wasUpdating;

        public Vector3 RealValue;
        ModeEnum _currentMode;
        Vector3 _mouseDownDirection;

        public Vector3Spinner() {
            InitializeComponent();

            Button1.PreviewMouseDown += Button1_MouseDownHandler;
            Button1.PreviewMouseUp += Button1_MouseUpHandler;
            Button1.PreviewMouseMove += Button1_MouseMoveHandler;

            Button2.PreviewMouseDown += Button2_MouseDownHandler;
            Button2.PreviewMouseUp += Button1_MouseUpHandler;
            Button2.PreviewMouseMove += Button1_MouseMoveHandler;

            Button3.PreviewMouseDown += Button3_MouseDownHandler;
            Button3.PreviewMouseUp += Button1_MouseUpHandler;
            Button3.PreviewMouseMove += Button1_MouseMoveHandler;

            Button4.PreviewMouseDown += Button4_MouseDownHandler;
            Button4.PreviewMouseUp += Button1_MouseUpHandler;
            Button4.PreviewMouseMove += Button1_MouseMoveHandler;
        }

        public Vector3 Value {
            get { return RealValue; }
            set {
                RealValue = value;
                var x = RealValue.X;
                var y = RealValue.Y;
                var z = RealValue.Z;
                var l = RealValue.Length();
                String xstr;
                String ystr;
                String zstr;
                String lstr;
                if (x * x > 1e-7 || x == 0)
                    xstr = String.Format("{0:0.0000}", x);
                else
                    xstr = String.Format("{0:0.0#e+00}", x);
                if (y * y > 1e-7 || y == 0)
                    ystr = String.Format("{0:0.0000}", y);
                else
                    ystr = String.Format("{0:0.0#e+00}", y);
                if (z * z > 1e-7 || z == 0)
                    zstr = String.Format("{0:0.0000}", z);
                else
                    zstr = String.Format("{0:0.0#e+00}", z);
                if (l * l > 1e-7 || l == 0)
                    lstr = String.Format("{0:0.0000}", l);
                else
                    lstr = String.Format("{0:0.0#e+00}", l);
                TextBlock1.Text = String.Format("({0}, {1}, {2}) {3}", xstr, ystr, zstr, lstr);
            }
        }

        /// <summary>
        ///     Updates the UI.
        /// </summary>
        public override sealed void UpdateUi(Object value) {
            Value = (Vector3)value;
        }

        /// <summary>
        ///     Updates the variable fromt he UI.
        /// </summary>
        public override sealed void UpdateVariable() {
            TreeNode.Property = Value;
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
                    case ModeEnum.Xz:
                        p = Mouse.GetPosition(Button2);
                        break;
                    case ModeEnum.Yz:
                        p = Mouse.GetPosition(Button3);
                        break;
                    case ModeEnum.L:
                        p = Mouse.GetPosition(Button4);
                        break;
                }
                var currentPos = new Vector2((float)p.X, (float)p.Y);

                var deltaVector = new Vector2 {
                    X = Math.Max(_mouseDownModeValue.X / 100.0f, 0.01f),
                    Y = Math.Max(_mouseDownModeValue.Y / 100.0f, 0.01f)
                };

                var delta = deltaVector.Length();
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    delta *= 10f;
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    delta *= 0.1f;

                var movement = currentPos - _downPosition;
                movement.X = -movement.X;
                var newValue = _mouseDownModeValue - movement * delta;

                var currentModeValue = GetCurrentModeValue();
                var currentPropertyValue = (Vector3)TreeNode.Property;

                if (newValue != currentModeValue && newValue != null) {
                    var v = new Vector3();
                    switch (_currentMode) {
                        case ModeEnum.Xy:
                            v = new Vector3(newValue.X, newValue.Y, currentPropertyValue.Z);
                            break;
                        case ModeEnum.Xz:
                            v = new Vector3(newValue.X, currentPropertyValue.Y, newValue.Y);
                            break;
                        case ModeEnum.Yz:
                            v = new Vector3(currentPropertyValue.X, newValue.X, newValue.Y);
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
            var currentPropertyValue = (Vector3)TreeNode.Property;
            switch (_currentMode) {
                case ModeEnum.Xy:
                    result = new Vector2(currentPropertyValue.X, currentPropertyValue.Y);
                    break;
                case ModeEnum.Xz:
                    result = new Vector2(currentPropertyValue.X, currentPropertyValue.Z);
                    break;
                case ModeEnum.Yz:
                    result = new Vector2(currentPropertyValue.Y, currentPropertyValue.Z);
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
            _currentMode = ModeEnum.Xz;
            _wasUpdating = TreeNode.Updating;
            _mouseDownModeValue = GetCurrentModeValue();
            TreeNode.Updating = false;
            Mouse.Capture(Button2);
            var p = e.GetPosition(Button2);
            _downPosition = new Vector2((float)p.X, (float)p.Y);
            _isDragging = true;
        }

        public void Button3_MouseDownHandler(object sender, MouseButtonEventArgs e) {
            _currentMode = ModeEnum.Yz;
            _wasUpdating = TreeNode.Updating;
            _mouseDownModeValue = GetCurrentModeValue();
            TreeNode.Updating = false;
            Mouse.Capture(Button3);
            var p = e.GetPosition(Button3);
            _downPosition = new Vector2((float)p.X, (float)p.Y);
            _isDragging = true;
        }

        public void Button4_MouseDownHandler(object sender, MouseButtonEventArgs e) {
            _currentMode = ModeEnum.L;
            _wasUpdating = TreeNode.Updating;
            _mouseDownModeValue = GetCurrentModeValue();

            // Get the current vector direction so we can scale it
            _mouseDownDirection = (Vector3)TreeNode.Property;
            if (_mouseDownDirection != Vector3.Zero)
                _mouseDownDirection.Normalize();
            else
                _mouseDownDirection = Vector3.Normalize(Vector3.One);

            TreeNode.Updating = false;
            Mouse.Capture(Button4);
            var p = e.GetPosition(Button4);
            _downPosition = new Vector2((float)p.X, (float)p.Y);
            _isDragging = true;
        }

        #endregion
    }
}
