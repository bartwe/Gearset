using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Wpf.Behaviours {
    /// <summary>
    ///     Static sealed class used to attach to wpf control
    /// </summary>
    public static class GridViewColumnResize {
        public static string GetWidth(DependencyObject obj) {
            return (string)obj.GetValue(WidthProperty);
        }

        public static void SetWidth(DependencyObject obj, string value) {
            try {
                obj.SetValue(WidthProperty, value);
            }
            catch {}
        }

        public static bool GetEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value) {
            try {
                obj.SetValue(EnabledProperty, value);
            }
            catch {}
        }

        #region Nested type: GridViewColumnResizeBehavior

        /// <summary>
        ///     GridViewColumn sealed class that gets attached to the GridViewColumn control
        /// </summary>
        public sealed class GridViewColumnResizeBehavior {
            readonly GridViewColumn _element;

            public GridViewColumnResizeBehavior(GridViewColumn element) {
                _element = element;
            }

            public string Width { get; set; }
            public bool IsStatic { get { return StaticWidth >= 0; } }

            public double StaticWidth {
                get {
                    double result;
                    return double.TryParse(Width, out result) ? result : -1;
                }
            }

            public double Percentage {
                get {
                    if (!IsStatic) {
                        return Mulitplier * 100;
                    }
                    return 0;
                }
            }

            public double Mulitplier {
                get {
                    if (Width == "*" || Width == "1*")
                        return 1;
                    if (Width.EndsWith("*")) {
                        double perc;
                        if (double.TryParse(Width.Substring(0, Width.Length - 1), out perc)) {
                            return perc;
                        }
                    }
                    return 1;
                }
            }

            public void SetWidth(double allowedSpace, double totalPercentage) {
                if (IsStatic) {
                    _element.Width = StaticWidth;
                }
                else {
                    var width = allowedSpace * (Percentage / totalPercentage);
                    _element.Width = width;
                }
            }
        }

        #endregion

        #region Nested type: ListViewResizeBehavior

        /// <summary>
        ///     ListViewResizeBehavior sealed class that gets attached to the ListView control
        /// </summary>
        public sealed class ListViewResizeBehavior {
            const int Margin = 25;
            const long RefreshTime = Timeout.Infinite;
            const long Delay = 50;
            readonly ListView _element;
            readonly Timer _timer;

            public ListViewResizeBehavior(ListView element) {
                if (element == null)
                    throw new ArgumentNullException("element");
                _element = element;
                element.Loaded += OnLoaded;

                // Action for resizing and re-enable the size lookup
                // This stops the columns from constantly resizing to improve performance
                Action resizeAndEnableSize = () => {
                    Resize();
                    _element.SizeChanged += OnSizeChanged;
                };
                _timer = new Timer(x => element.Dispatcher.BeginInvoke(resizeAndEnableSize), null, Delay,
                    RefreshTime);
            }

            public bool Enabled { get; set; }

            void OnLoaded(object sender, RoutedEventArgs e) {
                _element.SizeChanged += OnSizeChanged;
            }

            void OnSizeChanged(object sender, SizeChangedEventArgs e) {
                if (e.WidthChanged) {
                    _element.SizeChanged -= OnSizeChanged;
                    _timer.Change(Delay, RefreshTime);
                }
            }

            void Resize() {
                if (Enabled) {
                    var totalWidth = _element.ActualWidth;
                    var gv = _element.View as GridView;
                    if (gv != null) {
                        var allowedSpace = totalWidth - GetAllocatedSpace(gv);
                        allowedSpace = allowedSpace - Margin;
                        if (allowedSpace < 0)
                            return;
                        var totalPercentage = GridViewColumnResizeBehaviors(gv).Sum(x => x.Percentage);
                        foreach (var behavior in GridViewColumnResizeBehaviors(gv)) {
                            behavior.SetWidth(allowedSpace, totalPercentage);
                        }
                    }
                }
            }

            static IEnumerable<GridViewColumnResizeBehavior> GridViewColumnResizeBehaviors(GridView gv) {
                foreach (var t in gv.Columns) {
                    var gridViewColumnResizeBehavior =
                        t.GetValue(GridViewColumnResizeBehaviorProperty) as GridViewColumnResizeBehavior;
                    if (gridViewColumnResizeBehavior != null) {
                        yield return gridViewColumnResizeBehavior;
                    }
                }
            }

            static double GetAllocatedSpace(GridView gv) {
                double totalWidth = 0;
                foreach (var t in gv.Columns) {
                    var gridViewColumnResizeBehavior =
                        t.GetValue(GridViewColumnResizeBehaviorProperty) as GridViewColumnResizeBehavior;
                    if (gridViewColumnResizeBehavior != null) {
                        if (gridViewColumnResizeBehavior.IsStatic) {
                            totalWidth += gridViewColumnResizeBehavior.StaticWidth;
                        }
                    }
                    else {
                        totalWidth += t.ActualWidth;
                    }
                }
                return totalWidth;
            }
        }

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.RegisterAttached("Width", typeof(string), typeof(GridViewColumnResize),
                new PropertyMetadata(OnSetWidthCallback));

        public static readonly DependencyProperty GridViewColumnResizeBehaviorProperty =
            DependencyProperty.RegisterAttached("GridViewColumnResizeBehavior",
                typeof(GridViewColumnResizeBehavior), typeof(GridViewColumnResize),
                null);

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(GridViewColumnResize),
                new PropertyMetadata(OnSetEnabledCallback));

        public static readonly DependencyProperty ListViewResizeBehaviorProperty =
            DependencyProperty.RegisterAttached("ListViewResizeBehaviorProperty",
                typeof(ListViewResizeBehavior), typeof(GridViewColumnResize), null);

        #endregion

        #region CallBack

        static void OnSetWidthCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
            var element = dependencyObject as GridViewColumn;
            if (element != null) {
                var behavior = GetOrCreateBehavior(element);
                behavior.Width = e.NewValue as string;
            }
            else {
                Console.Error.WriteLine("Error: Expected type GridViewColumn but found " +
                                        dependencyObject.GetType().Name);
            }
        }

        static void OnSetEnabledCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
            var element = dependencyObject as ListView;
            if (element != null) {
                var behavior = GetOrCreateBehavior(element);
                behavior.Enabled = (bool)e.NewValue;
            }
            else {
                Console.Error.WriteLine("Error: Expected type ListView but found " + dependencyObject.GetType().Name);
            }
        }


        static ListViewResizeBehavior GetOrCreateBehavior(ListView element) {
            var behavior = element.GetValue(GridViewColumnResizeBehaviorProperty) as ListViewResizeBehavior;
            if (behavior == null) {
                behavior = new ListViewResizeBehavior(element);
                element.SetValue(ListViewResizeBehaviorProperty, behavior);
            }

            return behavior;
        }

        static GridViewColumnResizeBehavior GetOrCreateBehavior(GridViewColumn element) {
            var behavior = element.GetValue(GridViewColumnResizeBehaviorProperty) as GridViewColumnResizeBehavior;
            if (behavior == null) {
                behavior = new GridViewColumnResizeBehavior(element);
                element.SetValue(GridViewColumnResizeBehaviorProperty, behavior);
            }

            return behavior;
        }

        #endregion
    }
}
