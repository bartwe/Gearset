using System;
using System.Windows;
using System.Windows.Input;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     Interaction logic for Spinner.xaml
    /// </summary>
    public partial class StringItem : VisualItemBase {
        /// <summary>
        ///     Store the valur of IsUpdating when the textbox
        ///     gets focus.
        /// </summary>
        bool _wasUpdating;

        public StringItem() {
            InitializeComponent();
            TextBox1.GotFocus += TextBox1_GotFocus;
            TextBox1.LostFocus += TextBox1_LostFocus;
            TextBox1.KeyDown += TextBox1_KeyDown;
        }

        public void TextBox1_GotFocus(object sender, RoutedEventArgs e) {
            _wasUpdating = TreeNode.Updating;
            TreeNode.Updating = false;
        }

        public override sealed void UpdateUi(Object value) {
            if (value == null)
                return;
            var text = value.ToString();
            TextBox1.Text = text;
            TextBox1.ToolTip = text;
        }

        public override sealed void UpdateVariable() {
            TreeNode.Property = TextBox1.Text;
        }

        public void TextBox1_KeyDown(object sender, KeyEventArgs e) {
            e.Handled = false;
            var textBox = TextBox1;
            if (e.Key == Key.Enter) {
                if (!Keyboard.IsKeyDown(Key.LeftShift))
                    textBox.MoveFocus(TraversalRequest);
                else {
                    var i = textBox.CaretIndex;
                    textBox.Text = textBox.Text.Substring(0, i) + "\n" + textBox.Text.Substring(i, textBox.Text.Length - i);
                    textBox.CaretIndex = i + 1;
                }
            }
            else if (e.Key == Key.Subtract) {
                var box = (System.Windows.Controls.TextBox)sender;
                var caret = box.CaretIndex;
                box.Text = box.Text.Insert(box.CaretIndex, "-");
                box.CaretIndex = caret + 1;
                e.Handled = true;
            }
            textBox.AppendText(String.Empty);
        }

        public void TextBox1_LostFocus(object sender, RoutedEventArgs e) {
            if (TreeNode == null) {
                return;
            }
            UpdateVariable();
            TreeNode.Updating = _wasUpdating;
        }
    }
}
