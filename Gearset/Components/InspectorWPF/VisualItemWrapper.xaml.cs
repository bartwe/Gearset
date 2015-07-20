using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Holds items
    /// </summary>
    public partial class VisualItemWrapper : UserControl {
        VisualItemBase _visualItem;

        public VisualItemWrapper() {
            InitializeComponent();
            LabelsPanel.MouseDown += VisualItemWrapper_PreviewMouseDown;
            LabelsPanel.MouseMove += VisualItemWrapper_PreviewMouseMove;

            //transparentRectangle.MouseDown += new MouseButtonEventHandler(VisualItemWrapper_PreviewMouseDown);
            //transparentRectangle.MouseMove += new MouseEventHandler(VisualItemWrapper_PreviewMouseMove);
        }

        public VisualItemBase VisualItem {
            get { return _visualItem; }
            set {
                Grid1.Children.Add(value);
                Grid.SetColumn(value, 2);
                _visualItem = value;
            }
        }

        /// <summary>
        /// The InspectorTreeNode that this control affetcs.
        /// </summary>
        public String Text { get { return (String)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        /// <summary>
        /// Calls the handler that updates the textBlock with the new value.
        /// </summary>
        public static void TextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((VisualItemWrapper)d).UpdateText((string)e.NewValue);
        }

        /// <summary>
        /// Updates the textBlock with the new value.
        /// </summary>
        void UpdateText(String text) {
            TextBlock1.Text = text;
        }

        public void VisualItemWrapper_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            RaiseEvent(new RoutedEventArgs(TextMouseDownEvent, e.OriginalSource));
        }

        public void VisualItemWrapper_PreviewMouseMove(object sender, MouseEventArgs e) {
            RaiseEvent(new RoutedEventArgs(TextMouseMoveEvent, e.OriginalSource));
        }

        #region Attached Events (Mouse Move & Mouse Down)

        public static readonly RoutedEvent TextMouseMoveEvent = EventManager.RegisterRoutedEvent("TextMouseMove", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VisualItemWrapper));
        public static readonly RoutedEvent TextMouseDownEvent = EventManager.RegisterRoutedEvent("TextMouseDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(VisualItemWrapper));

        public static void AddTextMouseMoveHandler(DependencyObject d, RoutedEventHandler handler) {
            var uie = d as UIElement;
            if (uie != null)
                uie.AddHandler(MouseMoveEvent, handler);
        }

        public static void RemoveTextMouseMoveHandler(DependencyObject d, RoutedEventHandler handler) {
            var uie = d as UIElement;
            if (uie != null)
                uie.RemoveHandler(MouseMoveEvent, handler);
        }

        public static void AddTextMouseDownHandler(DependencyObject d, RoutedEventHandler handler) {
            var uie = d as UIElement;
            if (uie != null)
                uie.AddHandler(MouseDownEvent, handler);
        }

        public static void RemoveTextMouseDownHandler(DependencyObject d, RoutedEventHandler handler) {
            var uie = d as UIElement;
            if (uie != null)
                uie.RemoveHandler(MouseDownEvent, handler);
        }

        #endregion

        /// <summary>
        /// Registers a dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(String), typeof(VisualItemWrapper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsParentArrange, TextChangedCallback));
    }
}
