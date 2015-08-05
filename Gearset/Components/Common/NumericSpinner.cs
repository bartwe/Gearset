using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gearset.Components {
    /// <summary>
    /// Interaction logic for the NumericSpinner control
    /// </summary>
    public partial class NumericSpinner : UserControl {
        Point _downPosition;
        bool _isDragging;
        Object _mouseDownValue = 0f;

        /// <summary>
        /// Defines the state of the spinner, if IsEditing 
        /// is because the user is currently editing the value
        /// so the Updating value of the TreeNode will be set 
        /// to false until the control loses focus.
        /// </summary>
        public bool IsEditing;

        public NumericSpinner() {
            InitializeComponent();

            TextBox1.PreviewMouseDown += MouseDownHandler;

            Button1.PreviewMouseDown += Button1_MouseDownHandler;
            Button1.PreviewMouseUp += Button1_MouseUpHandler;
            Button1.PreviewMouseMove += Button1_MouseMoveHandler;

            TextBox1.LostFocus += TextBox1_LostFocus;
            TextBox1.KeyDown += TextBox1_KeyDown;

            Value = 0.0f;
        }

        public NumericSpinnerMode Mode { get { return (NumericSpinnerMode)GetValue(ModeProperty); } set { SetValue(ModeProperty, value); } }
        public object Value { get { return (float)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }
        public bool ShowNaN { get { return (bool)GetValue(ShowNaNProperty); } set { SetValue(ShowNaNProperty, value); } }

        static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            ((NumericSpinner)d).Mode = (NumericSpinnerMode)args.NewValue;
        }

        static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var str = e.NewValue.ToString();
            if (str == "NaN" && !((NumericSpinner)d).ShowNaN)
                ((NumericSpinner)d).TextBox1.Text = String.Empty;
            else
                ((NumericSpinner)d).TextBox1.Text = e.NewValue.ToString();
        }

        public void TextBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                //TextBox1.MoveFocus(traversalRequest);
                TextBox1_LostFocus(this, null);
                TextBox1.SelectAll();
            }
            if (e.Key == Key.Subtract) {
                var box = (TextBox)sender;
                var text = box.Text;
                var caret = box.CaretIndex;
                if (box.SelectionLength > 0)
                    text = text.Substring(0, box.SelectionStart) + text.Substring(box.SelectionStart + box.SelectionLength);
                box.Text = text.Insert(box.CaretIndex, "-");
                box.CaretIndex = caret + 1;
                e.Handled = true;
            }
        }

        public void TextBox1_LostFocus(object sender, RoutedEventArgs e) {
            Object boxedValue;
            try {
                switch (Mode) {
                    case NumericSpinnerMode.Byte:
                        boxedValue = byte.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.Char:
                        boxedValue = char.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.Decimal:
                        boxedValue = decimal.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.Double:
                        boxedValue = double.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.Float:
                        boxedValue = float.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.Int:
                        boxedValue = int.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.Long:
                        boxedValue = long.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.SByte:
                        boxedValue = sbyte.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.Short:
                        boxedValue = short.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.UInt:
                        boxedValue = uint.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.ULong:
                        boxedValue = ulong.Parse(TextBox1.Text);
                        break;
                    case NumericSpinnerMode.UShort:
                        boxedValue = ushort.Parse(TextBox1.Text);
                        break;
                    default:
                        boxedValue = null;
                        break;
                }
                Value = boxedValue;

                // If we're dragging is because the focus has 
                // changed to the UpDown Button so we should 
                // change the mouseDownValue to reflect any change
                // we've made while editing.
                // If the focus has changed to the spinner
                // do not return the value of Updating yet because
                // the Button1_MouseUp will do it.
                if (_isDragging)
                    _mouseDownValue = boxedValue;
            }
            catch {
                GearsetResources.Console.Log("Error while parsing the entered value.");
            }
            finally {
                // Whatever happes, we won't be editing anymore.
                IsEditing = false;
            }
        }

        /// <summary>
        /// What type of numeric value will this spinner handle
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(NumericSpinnerMode), typeof(NumericSpinner),
                new FrameworkPropertyMetadata(NumericSpinnerMode.Float, FrameworkPropertyMetadataOptions.AffectsRender, OnModeChanged));

        /// <summary>
        /// The numeric value held by this spinner. It must be unboxed
        /// to the type defined by the spinner mode.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Object), typeof(NumericSpinner), new PropertyMetadata(0f, ValueChangedCallback));

        /// <summary>
        /// Determines whether the spinner should be blank instead of 
        /// showing a "NaN" value.
        /// </summary>
        public static readonly DependencyProperty ShowNaNProperty =
            DependencyProperty.Register("ShowNaN", typeof(bool), typeof(NumericSpinner), new PropertyMetadata(true, ValueChangedCallback));

        /// <summary>
        /// Defines a way to move the focus out of the
        /// textbox when enter is pressed.
        /// </summary>
        protected static readonly TraversalRequest TraversalRequest = new TraversalRequest(FocusNavigationDirection.Next);

        #region Mouse movement events

        public void Button1_MouseUpHandler(object sender, MouseButtonEventArgs e) {
            Mouse.Capture(null, CaptureMode.None);
            _isDragging = false;
        }

        static int Clamp(int value, int min, int max) {
            return Math.Max(Math.Min(value, max), min);
        }

        public void Button1_MouseMoveHandler(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && _isDragging) {
                var pos = Mouse.GetPosition(Button1);

                Object newValue = null;
                try {
                    switch (Mode) {
                        case NumericSpinnerMode.Byte: {
                            var delta = Math.Max((byte)((byte)_mouseDownValue / 100), (byte)1);
                            var movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (byte)Clamp((byte)_mouseDownValue - movement * delta, byte.MinValue, byte.MaxValue);
                            break;
                        }
                        case NumericSpinnerMode.Char:
                            break;
                        case NumericSpinnerMode.Decimal: {
                            var delta = Math.Max((decimal)_mouseDownValue / 100, 1);
                            var movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (decimal)_mouseDownValue - movement * delta;
                            break;
                        }
                        case NumericSpinnerMode.Int: {
                            var delta = Math.Max((int)_mouseDownValue / 100, 1);
                            var movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (int)_mouseDownValue - movement * delta;
                            break;
                        }
                        case NumericSpinnerMode.Long: {
                            var delta = Math.Max((long)_mouseDownValue / 100, 1);
                            var movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (long)_mouseDownValue - movement * delta;
                            break;
                        }
                        case NumericSpinnerMode.SByte: {
                            var delta = Math.Max((sbyte)((sbyte)_mouseDownValue / 100), (sbyte)1);
                            var movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (sbyte)((sbyte)_mouseDownValue - movement * delta);
                            break;
                        }
                        case NumericSpinnerMode.Short: {
                            var delta = Math.Max((short)((short)_mouseDownValue / 100), (short)1);
                            var movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (short)((short)_mouseDownValue - movement * delta);
                            break;
                        }
                        case NumericSpinnerMode.UInt: {
                            var delta = Math.Max((uint)_mouseDownValue / 100, 1);
                            var movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (uint)_mouseDownValue + (uint)Math.Max(-movement * (int)delta, -(int)(uint)_mouseDownValue);
                            break;
                        }
                        case NumericSpinnerMode.ULong: {
                            var delta = Math.Max((ulong)_mouseDownValue / 100, 1);
                            var movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (ulong)_mouseDownValue + (ulong)Math.Max(-movement * (int)delta, -(int)(ulong)_mouseDownValue);
                            break;
                        }
                        case NumericSpinnerMode.UShort: {
                            var delta = Math.Max((ushort)((ushort)_mouseDownValue / 100), (ushort)1);
                            var movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (ushort)((ushort)_mouseDownValue + (ushort)Math.Max(-movement * delta, -(ushort)_mouseDownValue));
                            break;
                        }
                        case NumericSpinnerMode.Double: {
                            var delta = Math.Max((double)_mouseDownValue / 100.0, 0.01);
                            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                                delta *= 10;
                            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                                delta *= 0.1;
                            double movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (double)_mouseDownValue - movement * delta;
                            break;
                        }
                        case NumericSpinnerMode.Float: {
                            var delta = Math.Max((float)_mouseDownValue / 100.0f, 0.01f);
                            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                                delta *= 10f;
                            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                                delta *= 0.1f;
                            float movement = (int)(pos.Y - _downPosition.Y);
                            newValue = (float)_mouseDownValue - movement * delta;
                            break;
                        }
                    }
                }
                catch {
                    // Silently ignore if value is not good for the specified type.
                }

                if (newValue != Value && newValue != null) {
                    Value = newValue;
                }
            }
        }

        public void Button1_MouseDownHandler(object sender, MouseButtonEventArgs e) {
            Mouse.Capture(Button1);
            _mouseDownValue = Value;
            _downPosition = e.GetPosition(Button1);
            _isDragging = true;
        }

        void MouseDownHandler(object sender, MouseButtonEventArgs e) {
            // If we're already editing the TextBox, don't do anything
            if (!IsEditing) {
                // Flag that we're current
                IsEditing = true;
            }
        }

        #endregion
    }
}
