using System;
using System.Windows;
using System.Windows.Input;

namespace Gearset.Components {
    /// <summary>
    /// </summary>
    public partial class FinderWindow : Window {
        bool _isDragging;
        Point _downPosition;

        public FinderWindow() {
            InitializeComponent();
        }

        internal bool WasHiddenByGameMinimize { get; set; }

        void Button_Click(object sender, RoutedEventArgs e) {
            ((Finder)DataContext).Config.SearchText = String.Empty;
        }

        public void Close_Click(object sender, RoutedEventArgs e) {
            Hide();
        }

        public void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        public void Maximize_Click(object sender, RoutedEventArgs e) {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        protected void Results_DoubleClick(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource != null) {
                var o = ((FrameworkElement)e.OriginalSource).DataContext as ObjectDescription;
                if (o != null)
                    GearsetResources.Console.Inspect(o.Name, o.Object);
            }
        }

        public void Item_MouseMove(object sender, RoutedEventArgs e) {
            if (ResultsListBox.SelectedItem == null || !(((FrameworkElement)e.OriginalSource).DataContext is ObjectDescription))
                return;
            if (Mouse.LeftButton == MouseButtonState.Pressed && !_isDragging) {
                var pos = Mouse.GetPosition(this);
                if (Math.Abs(pos.X - _downPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(pos.Y - _downPosition.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    StartDrag();
                }
            }
        }

        public void Item_MouseDown(object sender, RoutedEventArgs e) {
            _downPosition = Mouse.GetPosition(this);
        }

        void StartDrag() {
            _isDragging = true;

            var selected = ResultsListBox.SelectedItem as ObjectDescription;
            if (selected != null) {
                var o = selected.Object;
                if (o != null) {
                    var data = new DataObject(o.GetType(), o);
                    var de = DragDrop.DoDragDrop(ResultsListBox, data, DragDropEffects.Move);
                    _isDragging = false;
                }
            }
        }

        void SearchTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (ResultsListBox.SelectedIndex == -1) {
                if (ResultsListBox.Items.Count > 0)
                    ResultsListBox.SelectedIndex = 0;
                else
                    return;
            }
            if (e.Key == Key.Down) {
                ResultsListBox.SelectedIndex = (ResultsListBox.SelectedIndex + 1) % ResultsListBox.Items.Count;
            }
            else if (e.Key == Key.Up) {
                ResultsListBox.SelectedIndex = (ResultsListBox.SelectedIndex - 1) % ResultsListBox.Items.Count;
            }
            else if (e.Key == Key.Enter || e.Key == Key.Return) {
                var o = ResultsListBox.SelectedItem as ObjectDescription;
                if (o != null)
                    GearsetResources.Console.Inspect(o.Name, o.Object);
            }
        }

        void ResultsListBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter || e.Key == Key.Return) {
                var o = ResultsListBox.SelectedItem as ObjectDescription;
                if (o != null)
                    GearsetResources.Console.Inspect(o.Name, o.Object);
            }
            else
                SearchTextBox.Focus();
            //SearchTextBox.RaiseEvent(new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, e.Key));
        }
    }
}
