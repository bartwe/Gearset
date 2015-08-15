using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gearset.Components {
    /// <summary>
    /// Textblock that can be eddited.
    /// </summary>
    [TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    public partial class EditableTextBlock : TextBox {
        TextBlock _block;
        TextBox _box;
        int _clickCount;

        public EditableTextBlock() {
            InitializeComponent();
        }

        public bool IsEditing { get { return (bool)GetValue(IsEditingProperty); } set { SetValue(IsEditingProperty, value); } }

        /// <summary>
        /// Same as setting IsEditing
        /// </summary>
        public void StartEdit() {
            IsEditing = true;
        }

        /// <summary>
        /// Same as unsetting IsEditing
        /// </summary>
        public void StopEdit() {
            IsEditing = false;
        }

        protected static void OnIsEditingChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            var o = ((EditableTextBlock)d);
            if ((bool)args.NewValue) {
                o._box.Visibility = Visibility.Visible;
                o._block.Visibility = Visibility.Hidden;
                o._box.Focus();
                o._box.SelectAll();
            }
            else {
                o._box.Visibility = Visibility.Hidden;
                o._block.Visibility = Visibility.Visible;
                o._clickCount = 0;
            }
        }

        public sealed override void OnApplyTemplate() {
            _block = GetTemplateChild("PART_TextBlock") as TextBlock;
            _box = GetTemplateChild("PART_TextBox") as TextBox;

            if (_box == null || _block == null)
                throw new NullReferenceException("Parts of the EditableTextBlock are not available in the provided Template.");

            _box.PreviewKeyDown += box_PreviewKeyDown;
            _box.LostKeyboardFocus += box_LostKeyboardFocus;
            base.OnApplyTemplate();
        }

        void box_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
            IsEditing = false;
        }

        protected sealed override void OnMouseDown(MouseButtonEventArgs e) {
            if (e.ChangedButton != MouseButton.Left)
                return;
            if (!(e.Source is TextBlock))
                _clickCount++;
            if (_clickCount == 2) {
                StartEdit();
                e.Handled = true;
            }
            else {
                // Don't handle it so the TreeViewItem can become selected.
                e.Handled = false;
            }
        }

        void box_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                StopEdit();
            }
        }

        /// <summary>
        /// Gets or sets whether this TextBlock is editing or not.
        /// </summary>
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register("IsEditing", typeof(bool), typeof(EditableTextBlock),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, OnIsEditingChanged));
    }
}
