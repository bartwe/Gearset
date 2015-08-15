using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

//using Arction.LightningChartBasic;
//using Arction.LightningChartBasic.Axes;
//using Arction.LightningChartBasic.Series;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Inspector : Window {
        /// <summary>
        /// position where the mouse was clicked.
        /// </summary>
        Point _downPosition;

        bool _isDragging;

        /// <summary>
        /// If the node expansion was generated because the currently selected node
        /// dissapeared (because we're adding private fields, for example) then this
        /// would generate a conflict with the expansion.
        /// </summary>
        internal InspectorNode NodeToExpandAfterUpdate;

        public Inspector() {
            InitializeComponent();

            SizeChanged += Inspector_SizeChanged;
            Closing += Inspector_Closing;
        }

        internal bool WasHiddenByGameMinimize { get; set; }

        void Inspector_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (WindowState == WindowState.Minimized)
                Hide();
        }

        public void Inspector_Closing(object sender, CancelEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        public void Item_MouseMove(object sender, RoutedEventArgs e) {
            if (TreeView1.SelectedItem == null)
                return;
            if (Mouse.LeftButton == MouseButtonState.Pressed && !_isDragging) {
                if (e.OriginalSource is System.Windows.Controls.TextBox)
                    return;
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
            var data = new DataObject(typeof(Object), ((InspectorNode)TreeView1.SelectedItem).Property);
            var de = DragDrop.DoDragDrop(TreeView1, data, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.None);
            _isDragging = false;
        }

        public void TreeView1_DragOver(object sender, DragEventArgs e) {
            var targetTreeNode = ((FrameworkElement)e.OriginalSource).DataContext as InspectorNode;
            if (targetTreeNode == null) {
                return;
            }

            var data = e.Data.GetData(typeof(Object));
            if (data == null)
                return;
            var dataType = data.GetType();
            var targetType = targetTreeNode.Type;

            if (targetType.IsAssignableFrom(dataType) ||
                targetType == typeof(float) && dataType == typeof(double) ||
                targetType == typeof(double) && dataType == typeof(float)) {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
            else {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        public void TreeView1_Drop(object sender, DragEventArgs e) {
            var targetTreeNode = ((FrameworkElement)e.OriginalSource).DataContext as InspectorNode;
            if (targetTreeNode == null)
                return;

            var data = e.Data.GetData(typeof(Object));
            if (data == null)
                return;
            var dataType = data.GetType();
            var targetType = targetTreeNode.Type;

            if (targetType.IsAssignableFrom(dataType)) {
                targetTreeNode.Property = data;
                e.Handled = true;
            }
            else if (targetType == typeof(float) && dataType == typeof(double))
                targetTreeNode.Property = (float)((double)data);
            else if (targetType == typeof(double) && dataType == typeof(float))
                targetTreeNode.Property = (double)((float)data);
        }

        protected sealed override void OnGiveFeedback(GiveFeedbackEventArgs e) {
            e.UseDefaultCursors = true;
            base.OnGiveFeedback(e);
        }

        public void Inspect_Click(object sender, RoutedEventArgs e) {
            var item = ((InspectorNode)TreeView1.SelectedItem);
            if (item == null)
                return;

            var o = item.Property;
            if (o != null && !item.Type.IsValueType)
                GearsetResources.Console.Inspect(item.GetPath(), o);
        }

        public void ShowPrivate_Click(object sender, RoutedEventArgs e) {
            var item = ((InspectorNode)TreeView1.SelectedItem);
            if (item == null)
                return;

            item.IsShowingPrivate = true;
        }

        public void Watch_Click(object sender, RoutedEventArgs e) {
            var item = ((InspectorNode)TreeView1.SelectedItem);
            if (item != null)
                GearsetResources.Console.Inspector.Watch(item);
        }

        public void Clear_Click(object sender, RoutedEventArgs e) {
            var item = ((InspectorNode)TreeView1.SelectedItem);
            if (item == null)
                return;

            if (item.CanWrite && !item.Type.IsValueType)
                item.Property = null;
        }

        public void Remove_Click(object sender, RoutedEventArgs e) {
            var item = ((InspectorNode)TreeView1.SelectedItem);
            if (item == null)
                return;

            var o = item.Property;
            if (o != null)
                GearsetResources.Console.RemoveInspect(o);
        }

        //void SavePersistorData_Click(object sender, RoutedEventArgs e)
        //{
        //    GearsetResources.Console.SavePersistorData();
        //}

        public void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (e.NewValue != null) {
                var node = e.NewValue as InspectorNode;
                if (node != null)
                    NodeToExpandAfterUpdate = node;
            }
        }

        public void InvokeButton_Click(object sender, RoutedEventArgs e) {}

        public void TreeView1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            var treeViewItem = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null) {
                // If we're right-clicking on a Collection Marker, don't take the focus.
                var takeFocus = true;
                var node = treeViewItem.DataContext as InspectorNode;
                if (node != null) {
                    if (node.VisualItem != null && node.VisualItem is CollectionMarkerItem)
                        takeFocus = false;
                }
                if (takeFocus) {
                    treeViewItem.Focus();
                    e.Handled = true;
                }
            }
        }

        static DependencyObject VisualUpwardSearch<T>(DependencyObject source) {
            while (source != null && source.GetType() != typeof(T)) source = VisualTreeHelper.GetParent(source);
            return source;
        }

        public void Expander_MouseDown(object sender, MouseButtonEventArgs e) {
            // Prevent selecting the whole expander in the Inspector treeview.
            e.Handled = true;
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

        void Notice_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        void Button_Click(object sender, RoutedEventArgs e) {
            ((InspectorManager)DataContext).Config.SearchText = String.Empty;
        }

        void HidePlotsButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Plotter.HideAll();
        }

        void ShowPlotsButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Plotter.ShowAll();
        }

        void ResetPlotsPositionsButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Plotter.ResetPositions();
        }
    }
}
