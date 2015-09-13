using System;
using System.Windows;
using System.Windows.Input;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     Interaction logic for Spinner.xaml
    /// </summary>
    public partial class NumericItem : VisualItemBase {
        Point _downPosition;
        bool _isDragging;
        Object _mouseDownValue = 0f;
        bool _plotValues;

        /// <summary>
        ///     Defines the state of the spinner, if IsEditing
        ///     is because the user is currently editing the value
        ///     so the Updating value of the TreeNode will be set
        ///     to false until the control loses focus.
        /// </summary>
        public bool IsEditing;

        /// <summary>
        ///     True if the TreeNode was Updating before the user
        ///     started to edit it.
        /// </summary>
        bool _wasUpdating;

        public Object RealValue;

        public NumericItem() {
            InitializeComponent();

            TextBox1.PreviewMouseDown += MouseDownHandler;

            Button1.PreviewMouseDown += Button1_MouseDownHandler;
            Button1.PreviewMouseUp += Button1_MouseUpHandler;
            Button1.PreviewMouseMove += Button1_MouseMoveHandler;

            TextBox1.LostFocus += TextBox1_LostFocus;
            TextBox1.KeyDown += TextBox1_KeyDown;
        }

        /// <summary>
        ///     What type of numeric value will this spinner handle
        /// </summary>
        public NumericSpinnerMode Mode { get { return (NumericSpinnerMode)GetValue(ModeProperty); } set { SetValue(ModeProperty, value); } }

        public Object Value {
            get { return RealValue; }
            set {
                RealValue = value;
                TextBox1.Text = RealValue.ToString();
            }
        }

        static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            ((NumericItem)d).Mode = (NumericSpinnerMode)args.NewValue;
        }

        public void TextBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                TextBox1.MoveFocus(TraversalRequest);
            }
            if (e.Key == Key.Subtract) {
                var box = (System.Windows.Controls.TextBox)sender;
                var text = box.Text;
                var caret = box.CaretIndex;
                if (box.SelectionLength > 0)
                    text = text.Substring(0, box.SelectionStart) + text.Substring(box.SelectionStart + box.SelectionLength);
                box.Text = text.Insert(box.CaretIndex, "-");
                box.CaretIndex = caret + 1;
                e.Handled = true;
            }
        }

        /// <summary>
        ///     Updates the UI.
        /// </summary>
        public override sealed void UpdateUi(Object value) {
            try {
                switch (Mode) {
                    case NumericSpinnerMode.Byte:
                        Value = (byte)value;
                        break;
                    case NumericSpinnerMode.Char:
                        Value = (char)value;
                        break;
                    case NumericSpinnerMode.Decimal:
                        Value = (decimal)value;
                        break;
                    case NumericSpinnerMode.Double:
                        Value = (double)value;
                        break;
                    case NumericSpinnerMode.Float:
                        Value = (float)value;
                        break;
                    case NumericSpinnerMode.Int:
                        Value = (int)value;
                        break;
                    case NumericSpinnerMode.Long:
                        Value = (long)value;
                        break;
                    case NumericSpinnerMode.SByte:
                        Value = (sbyte)value;
                        break;
                    case NumericSpinnerMode.Short:
                        Value = (short)value;
                        break;
                    case NumericSpinnerMode.UInt:
                        Value = (uint)value;
                        break;
                    case NumericSpinnerMode.ULong:
                        Value = (ulong)value;
                        break;
                    case NumericSpinnerMode.UShort:
                        Value = (ushort)value;
                        break;
                }

                if (_plotValues) {
                    switch (Mode) {
                        case NumericSpinnerMode.Byte:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (byte)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.Char:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (char)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.Decimal:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (float)(decimal)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.Double:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (float)(double)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.Float:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (float)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.Int:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (int)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.Long:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (long)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.SByte:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (sbyte)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.Short:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (short)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.UInt:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (uint)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.ULong:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (ulong)TreeNode.Property);
                            break;
                        case NumericSpinnerMode.UShort:
                            GearsetResources.Console.Plot(TreeNode.FriendlyName, (ushort)TreeNode.Property);
                            break;
                    }
                }
            }
            catch {
                GearsetResources.Console.Log("Error while updating NumericItem");
                throw;
            }
        }

        /// <summary>
        ///     Updates the variable fromt he UI.
        /// </summary>
        public override sealed void UpdateVariable() {
            TreeNode.Property = Value;
        }

        public void TextBox1_LostFocus(object sender, RoutedEventArgs e) {
            // If we don't have an assigned TreeNode
            if (TreeNode == null) {
                return;
            }
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
                RealValue = boxedValue;
                UpdateVariable();

                // If we're dragging is because the focus has 
                // changed to the UpDown Button so we should 
                // change the mouseDownValue to reflect any change
                // we've made while editing.
                // If the focus has changed to the spinner
                // do not return the value of Updating yet because
                // the Button1_MouseUp will do it.
                if (_isDragging)
                    _mouseDownValue = boxedValue;
                else
                    TreeNode.Updating = _wasUpdating;
            }
            catch {
                // If something happened while parsing the new value
                // update the UI so it has the previous value.
                UpdateUi(TreeNode.Property);
                GearsetResources.Console.Log("Error while parsing the entered value.");
            }
            finally {
                // Whatever happes, we won't be editing anymore.
                IsEditing = false;
            }
        }

        void PlotToggleButton_Checked(object sender, RoutedEventArgs e) {
            _plotValues = PlotToggleButton.IsChecked ?? false;

            if (!_plotValues)
                GearsetResources.Console.Plotter.RemovePlot(TreeNode.Name);
        }

        /// <summary>
        ///     The mode of the Numeric Item
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(NumericSpinnerMode), typeof(NumericItem),
                new FrameworkPropertyMetadata(NumericSpinnerMode.Float, FrameworkPropertyMetadataOptions.AffectsRender, OnModeChanged));

        #region Mouse movement events

        public void Button1_MouseUpHandler(object sender, MouseButtonEventArgs e) {
            Mouse.Capture(null, CaptureMode.None);
            _isDragging = false;
            TreeNode.Updating = _wasUpdating;
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
                    RealValue = newValue;
                    UpdateVariable();
                    UpdateUi(TreeNode.Property);
                }
            }
        }

        public void Button1_MouseDownHandler(object sender, MouseButtonEventArgs e) {
            if (!IsEditing) {
                _wasUpdating = TreeNode.Updating;
            }
            TreeNode.Updating = false;
            Mouse.Capture(Button1);
            _mouseDownValue = TreeNode.Property;
            _downPosition = e.GetPosition(Button1);
            _isDragging = true;
        }

        void MouseDownHandler(object sender, MouseButtonEventArgs e) {
            // If we're already editing the TextBox, don't do anything
            if (!IsEditing) {
                // Save the value of Updating so we can restore it
                // after the edit.
                _wasUpdating = TreeNode.Updating;
                TreeNode.Updating = false;

                // Flag that we're current
                IsEditing = true;
            }
        }

        #endregion
    }
}
