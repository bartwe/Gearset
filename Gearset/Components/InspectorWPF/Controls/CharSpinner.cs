using System;
using System.Windows;
using System.Windows.Input;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class CharSpinner : VisualItemBase {
        /// <summary>
        /// The real char value.
        /// </summary>
        char _charValue;

        public CharSpinner() {
            InitializeComponent();
            TextBox1.LostFocus += TextBox1_LostFocus;
            TextBox1.KeyDown += TextBox1_KeyDown;
        }

        public char CharValue {
            get { return _charValue; }
            set {
                _charValue = value;
                TextBox1.Text = _charValue.ToString();
            }
        }

        public void TextBox1_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                TextBox1.MoveFocus(TraversalRequest);
            }
            else if (e.Key == Key.Subtract) {
                var box = (System.Windows.Controls.TextBox)sender;
                var caret = box.CaretIndex;
                box.Text = box.Text.Insert(box.CaretIndex, "-");
                box.CaretIndex = caret + 1;
                e.Handled = true;
            }
        }

        public override sealed void UpdateUi(Object value) {
            CharValue = (char)value;
        }

        public override sealed void UpdateVariable() {
            TreeNode.Property = _charValue;
        }

        public void TextBox1_LostFocus(object sender, RoutedEventArgs e) {
            if (TreeNode == null) {
                return;
            }
            char newCharValue;
            if (!char.TryParse(TextBox1.Text, out newCharValue)) {
                // TODO: Change the style here to something red
            }
            else {
                _charValue = newCharValue;
                UpdateVariable();
            }
        }
    }
}
