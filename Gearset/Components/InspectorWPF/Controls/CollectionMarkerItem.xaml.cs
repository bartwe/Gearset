using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class CollectionMarkerItem : VisualItemBase {
        readonly List<Object> _items;
        bool _updateRequested = true;

        public CollectionMarkerItem() {
            InitializeComponent();
            _items = new List<Object>();

            ListBox1.ItemsSource = _items;
        }

        public override void UpdateUi(Object value) {
            if (_updateRequested) {
                _items.Clear();
                var enumerable = value as IEnumerable;
                foreach (var item in enumerable) {
                    _items.Add(item);
                }

                // Update the listbox and count.
                ListBox1.ItemsSource = null;
                ListBox1.ItemsSource = _items;
                CountTextBlock.Text = _items.Count.ToString();


                _updateRequested = false;
            }
        }

        protected void CollectionListView_DoubleClick(object sender, MouseButtonEventArgs e) {
            // Inspect collection items.
            var element = e.OriginalSource as FrameworkElement;
            if (element != null && element.DataContext != null)
                GearsetResources.Console.Inspect("Item (" + element.DataContext + ")", element.DataContext);
            e.Handled = true;
        }

        public void ListView_LostFocus(object sender, RoutedEventArgs e) {
            var list = sender as ListView;
            if (list != null)
                list.UnselectAll();
        }

        public void ListView_GotFocus(object sender, RoutedEventArgs e) {
            var node = GearsetResources.Console.Inspector.Window.TreeView1.SelectedItem as InspectorNode;
            if (node != null) {
                var item = node.UiContainer;
                if (item != null)
                    item.IsSelected = false;
            }
        }

        public void ListView_MouseDown(object sender, MouseButtonEventArgs e) {
            e.Handled = true;
        }

        public void ListView_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {}

        void Button_Click_1(object sender, RoutedEventArgs e) {
            _updateRequested = true;
        }
    }

    public class CollectionMarkerListTemplateSelector : DataTemplateSelector {
        static readonly CachedTemplate Texture2DTemplateCache = new CachedTemplate("textureTemplate");
        static readonly CachedTemplate GenericTemplateCache = new CachedTemplate("genericTemplate");

        /// <summary>
        /// Static constructor
        /// </summary>
        static CollectionMarkerListTemplateSelector() {}

        public override DataTemplate SelectTemplate(Object item, DependencyObject container) {
            var element = container as FrameworkElement;

            if (element != null && item != null) {
                element.DataContext = item;

                if (item is Texture2D) {
                    if (Texture2DTemplateCache.DataTemplate == null)
                        Texture2DTemplateCache.DataTemplate = element.FindResource(Texture2DTemplateCache.Name) as DataTemplate;
                    return Texture2DTemplateCache.DataTemplate;
                }
                if (GenericTemplateCache.DataTemplate == null)
                    GenericTemplateCache.DataTemplate = element.FindResource(GenericTemplateCache.Name) as DataTemplate;
                return GenericTemplateCache.DataTemplate;
            }

            return null;
        }
    }

    ///// <summary>
    ///// Wraps objects in the list.
    ///// </summary>
    //internal class ObjectWrapper
    //{
    //    /// <summary>
    //    /// Name is legacy from InspectorNode.
    //    /// </summary>
    //    internal Object Property { get; private set; }
    //    internal ObjectWrapper(Object o)
    //    {
    //        this.Property = o;
    //    }
    //    internal override string ToString()
    //    {
    //        return Property.ToString();
    //    }
    //}
}
