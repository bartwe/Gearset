﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     Interaction logic for Spinner.xaml
    /// </summary>
    public partial class CollectionMarkerItem : VisualItemBase {
        readonly List<Object> _items;
        bool _updateRequested = true;

        public CollectionMarkerItem() {
            InitializeComponent();
            _items = new List<Object>();

            ListBox1.ItemsSource = _items;
        }

        public override sealed void UpdateUi(Object value) {
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

    ///// <summary>
    ///// Wraps objects in the list.
    ///// </summary>
    //internal sealed class ObjectWrapper
    //{
    //    /// <summary>
    //    /// Name is legacy from InspectorNode.
    //    /// </summary>
    //    internal Object Property { get; private set; }
    //    internal ObjectWrapper(Object o)
    //    {
    //        this.Property = o;
    //    }
    //    internal sealed override string ToString()
    //    {
    //        return Property.ToString();
    //    }
    //}
}
