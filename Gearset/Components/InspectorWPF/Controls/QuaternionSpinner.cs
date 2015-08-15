using System;
using System.Windows.Input;
using Microsoft.Xna.Framework;
using Point = System.Windows.Point;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class QuaternionSpinner : VisualItemBase {
        public enum ModeEnum {
            X,
            Y,
            Z
        }

        Quaternion _mouseDownValue;
        bool _isDragging;

        /// <summary>
        /// True if the TreeNode was Updating before the user
        /// started to edit it.
        /// </summary>
        bool _wasUpdating;

        public Quaternion RealValue;
        ModeEnum _currentMode;
        Vector2 _mouseDownPosition;

        public QuaternionSpinner() {
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
        }

        public Quaternion Value {
            get { return RealValue; }
            set {
                RealValue = value;
                TextBlock1.Text = RealValue.ToString();
            }
        }

        /// <summary>
        /// Updates the UI.
        /// </summary>
        public override sealed void UpdateUi(Object value) {
            Value = (Quaternion)value;
        }

        /// <summary>
        /// Updates the variable fromt he UI.
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
                    case ModeEnum.X:
                        p = Mouse.GetPosition(Button1);
                        break;
                    case ModeEnum.Y:
                        p = Mouse.GetPosition(Button2);
                        break;
                    case ModeEnum.Z:
                        p = Mouse.GetPosition(Button3);
                        break;
                }
                var currentPos = (float)p.Y;

                // Angle to rotate per pixel moved
                var delta = 0.004f;

                var movement = currentPos - _mouseDownPosition.Y;
                var angle = movement * delta;


                var angles = new Vector3();
                switch (_currentMode) {
                    case ModeEnum.X:
                        angles = new Vector3(angle, 0, 0);
                        break;
                    case ModeEnum.Y:
                        angles = new Vector3(0, angle, 0);
                        break;
                    case ModeEnum.Z:
                        angles = new Vector3(0, 0, angle);
                        break;
                }

                // Yaw and Roll are switched.
                var newValue = _mouseDownValue * Quaternion.CreateFromYawPitchRoll(angles.Y, angles.X, angles.Z);
                RealValue = newValue;
                UpdateVariable();
                UpdateUi(TreeNode.Property);
            }
        }

        public void Button1_MouseDownHandler(object sender, MouseButtonEventArgs e) {
            _currentMode = ModeEnum.X;
            _wasUpdating = TreeNode.Updating;
            _mouseDownValue = (Quaternion)TreeNode.Property;
            TreeNode.Updating = false;
            Mouse.Capture(Button1);
            var p = e.GetPosition(Button1);
            _mouseDownPosition = new Vector2((float)p.X, (float)p.Y);
            _isDragging = true;
        }

        public void Button2_MouseDownHandler(object sender, MouseButtonEventArgs e) {
            _currentMode = ModeEnum.Y;
            _wasUpdating = TreeNode.Updating;
            _mouseDownValue = (Quaternion)TreeNode.Property;
            TreeNode.Updating = false;
            Mouse.Capture(Button2);
            var p = e.GetPosition(Button2);
            _mouseDownPosition = new Vector2((float)p.X, (float)p.Y);
            _isDragging = true;
        }

        public void Button3_MouseDownHandler(object sender, MouseButtonEventArgs e) {
            _currentMode = ModeEnum.Z;
            _wasUpdating = TreeNode.Updating;
            _mouseDownValue = (Quaternion)TreeNode.Property;
            TreeNode.Updating = false;
            Mouse.Capture(Button3);
            var p = e.GetPosition(Button3);
            _mouseDownPosition = new Vector2((float)p.X, (float)p.Y);
            _isDragging = true;
        }

        #endregion
    }
}
