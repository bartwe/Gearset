using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     Interaction logic for Spinner.xaml
    /// </summary>
    public partial class TextBox : UserControl {
        bool _isEmpty;
        TextDecorationCollection _savedDecorations;
        Brush _subtleBrush;
        Brush _normalBrush;
        String _placeholderText = String.Empty;
        bool _fakeTextChanged;

        public TextBox() {
            _placeholderText = "(empty string)";
            InitializeComponent();

            Loaded += TextBox_Loaded;

            TextBox1.TextChanged += TextBox1_TextChanged;
            //PlaceholderTextBlock.MouseDown += new MouseButtonEventHandler(PlaceholderTextBlock_MouseDown);
            TextBox1.LostFocus += TextBox1_LostFocus;
            TextBox1.GotFocus += TextBox1_GotFocus;
        }

        /// <summary>
        ///     What text to show as placeholder
        /// </summary>
        public String PlaceholderText { get { return (String)GetValue(PlaceholderTextProperty); } set { SetValue(PlaceholderTextProperty, value); } }

        /// <summary>
        ///     Text alignment
        /// </summary>
        public TextAlignment TextAlignment { get { return TextBox1.TextAlignment; } set { TextBox1.TextAlignment = value; } }

        /// <summary>
        ///     Text to show, if this value is empty, the palceholder will be shown instead.
        /// </summary>
        public String Text { get { return (String)GetValue(System.Windows.Controls.TextBox.TextProperty); } set { SetValue(System.Windows.Controls.TextBox.TextProperty, value); } }

        public System.Windows.Controls.TextBox ActualTextBox { get { return TextBox1; } }

        static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            ((TextBox)d)._placeholderText = (String)args.NewValue;
        }

        static void OnTextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            ((TextBox)d).TextBox1.TextAlignment = (TextAlignment)args.NewValue;
        }

        void TextBox_Loaded(object sender, RoutedEventArgs e) {
            _subtleBrush = (Brush)FindResource("subtle1");
            _normalBrush = (Brush)FindResource("normalText1");

            _savedDecorations = new TextDecorationCollection();
            foreach (var decoration in TextBox1.TextDecorations)
                _savedDecorations.Add(decoration);

            CheckIfEmpty();
        }

        void TextBox1_GotFocus(object sender, RoutedEventArgs e) {
            if (_isEmpty)
                TextBox1.Text = String.Empty;
        }

        void TextBox1_LostFocus(object sender, RoutedEventArgs e) {
            CheckIfEmpty();
        }

        //void PlaceholderTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    // Show the TextBox
        //    TextBox1.Visibility = System.Windows.Visibility.Visible;
        //    PlaceholderTextBlock.Visibility = System.Windows.Visibility.Hidden;
        //    //TextBox1.Focus();
        //}

        void TextBox1_TextChanged(object sender, TextChangedEventArgs e) {
            if (_fakeTextChanged)
                return;
            if (TextBox1.Text != String.Empty) {
                UseNormal();
            }
            else {
                if (!TextBox1.IsFocused) {
                    UsePlaceholder();
                }
            }
        }

        void UseNormal() {
            if (TextBox1.TextDecorations.Count == 0 && _savedDecorations.Count > 0) {
                foreach (var decoration in _savedDecorations)
                    TextBox1.TextDecorations.Add(decoration.Clone()); // It won't work without the Clone call.
            }
            TextBox1.Foreground = _normalBrush;
            _isEmpty = false;
        }

        void CheckIfEmpty() {
            if (TextBox1.Text == String.Empty) {
                UsePlaceholder();
            }
            else {
                UseNormal();
            }
            //if (TextBox1.Text == String.Empty)
            //{
            //    TextBox1.Visibility = System.Windows.Visibility.Hidden;
            //    PlaceholderTextBlock.Visibility = System.Windows.Visibility.Visible;
            //}
            //else
            //{
            //    TextBox1.Visibility = System.Windows.Visibility.Visible;
            //    PlaceholderTextBlock.Visibility = System.Windows.Visibility.Hidden;
            //}
        }

        void UsePlaceholder() {
            TextBox1.TextDecorations.Clear();

            _fakeTextChanged = true;
            TextBox1.Text = _placeholderText;
            _fakeTextChanged = false;
            TextBox1.Foreground = _subtleBrush;
            _isEmpty = true;
        }

        /// <summary>
        ///     Registers a dependency property
        /// </summary>
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register("PlaceholderText", typeof(String), typeof(TextBox), new PropertyMetadata(OnPlaceholderTextChanged));

        /// <summary>
        ///     Registers a dependency property
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(TextBox), new PropertyMetadata(OnTextAlignmentChanged));

        /// <summary>
        ///     Registers a dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String), typeof(TextBox));
    }
}
