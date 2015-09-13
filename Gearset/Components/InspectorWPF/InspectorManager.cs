using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Microsoft.Xna.Framework;
using Control = System.Windows.Forms.Control;

namespace Gearset.Components.InspectorWPF {
    public sealed class InspectorManager : Gear, INotifyPropertyChanged {
        /// <summary>
        ///     Objects being inspected.
        /// </summary>
        readonly ObservableCollection<InspectorNode> _inspectedObjects;

        /// <summary>
        ///     Nodes shown in the Watch window.
        /// </summary>
        readonly ObservableCollection<InspectorNode> _watchedNodes;

        /// <summary>
        ///     Methods Callers
        /// </summary>
        readonly ObservableCollection<MethodCaller> _methodCallers;

        readonly BackgroundWorker _filterWorker = new BackgroundWorker();
        Control _gameWindow;
        //private ObservableDataSource<Vector2> testSource;
        //private FixedLengthPointSource testSource2;
        //private PointLineSeries series;
        int _updateCount;
        bool _locationJustChanged;
        string[] _searchTerms;

        /// <summary>
        ///     It is set to the seconds to wait between the user stop typing and the filtering
        ///     is actually performed. It is reset to some value with every keystroke.
        /// </summary>
        float _updateSearchFilteringDelay;

        /// <summary>
        ///     List of notices to show.
        /// </summary>
        ObservableCollection<NoticeViewModel> _notices;

        /// <summary>
        ///     Constructor, creates the inspector logger.
        /// </summary>
        public InspectorManager()
            : base(GearsetSettings.Instance.InspectorConfig) {
            Config = GearsetSettings.Instance.InspectorConfig;
            Notices = new ObservableCollection<NoticeViewModel>();

            // Start the inspector form.
            Window = new Inspector();
            Window.DataContext = this;
            Config.ModifiedOnlyChanged += Config_ModifiedOnlyChanged;
            Config.SearchTextChanged += Config_SearchTextChanged;
            // This is needed in order to correctly handle keyboard input.
            ElementHost.EnableModelessKeyboardInterop(Window);

            _updateSearchFilteringDelay = 0.25f;

            Window.Top = Config.Top;
            Window.Left = Config.Left;
            Window.Width = Config.Width;
            Window.Height = Config.Height;

            WindowHelper.EnsureOnScreen(Window);

            if (Config.Visible)
                Window.Show();

            InspectorNode.ExtensionMethodTypes.Add(typeof(InspectorExtensionsMethods));

            // Create both item sources.
            _inspectedObjects = new ObservableCollection<InspectorNode>();
            _watchedNodes = new ObservableCollection<InspectorNode>();
            _methodCallers = new ObservableCollection<MethodCaller>();

            var source = new CollectionViewSource();
            source.Source = _inspectedObjects;
            source.View.Filter = FilterPredicate;
            Window.TreeView1.ItemsSource = source.View;
            //Inspector.WatchTreeView.ItemsSource = WatchedNodes;
            //Inspector.TreeView2.ItemsSource = MethodCallers;

            Window.TreeView1.ItemContainerGenerator.StatusChanged += ItemContainerGenerator1_StatusChanged;
            //Inspector.TreeView2.ItemContainerGenerator.StatusChanged += new EventHandler(ItemContainerGenerator2_StatusChanged);

            Window.TreeView1.SelectedItemChanged += TreeView1_SelectedItemChanged;

            Window.Plots.DataContext = GearsetResources.Console.Plotter.Plots;

            Window.LocationChanged += InspectorWindow_LocationChanged;
            Window.SizeChanged += InspectorWindow_SizeChanged;
            Window.IsVisibleChanged += InspectorWindow_IsVisibleChanged;


            // The game logger, listen for events for attachement.
            _gameWindow = Control.FromHandle(GearsetResources.Game.Window.Handle);
            //gameWindow.Move += new EventHandler(gameWindow_Move);
            //gameWindow.Resize += new EventHandler(gameWindow_Resize);

            // If we're attached update the position of the logger,
            // if the screen is wide enough to allow both windows to
            // be side by side, move them.
            //UpdatePosition();
            //if (Inspector.Left < 0 && gameWindow.Right - Inspector.Left < Screen.AllScreens[0].Bounds.Right)
            //{
            //    gameWindow.Left += (int)-Inspector.Left;
            //    UpdatePosition();
            //}
        }

        internal Inspector Window { get; private set; }

        /// <summary>
        ///     Gets the Inspector's config.
        /// </summary>
        public InspectorConfig Config { get; private set; }

        public ObservableCollection<NoticeViewModel> Notices {
            get { return _notices; }
            set {
                _notices = value;
                OnPropertyChanged("Notices");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void Config_SearchTextChanged(object sender, EventArgs e) {
            // Wait until the filterworker is done in case it's doing something
            _updateSearchFilteringDelay = 0.25f;
        }

        void filterWorker_DoWork(object sender, DoWorkEventArgs e) {
            _searchTerms = Config.SearchText.ToUpper().Split(' ');
            var dispatcher = Window.TreeView1.Dispatcher;
            dispatcher.Invoke((Action)delegate { ((CollectionView)Window.TreeView1.ItemsSource).Refresh(); });
        }

        void source_Filter(object sender, FilterEventArgs e) {
            e.Accepted = FilterPredicate(e.Item);
        }

        void Config_ModifiedOnlyChanged(object sender, EventArgs e) {
            foreach (var o in _inspectedObjects) {
                UpdateFilterRecursively(o);
            }
            ((CollectionView)Window.TreeView1.ItemsSource).Refresh();
        }

        void UpdateFilterRecursively(InspectorNode node) {
            if (node.ChildrenView != null) {
                foreach (var child in node.Children) {
                    UpdateFilterRecursively(child);
                }
                node.ChildrenView.View.Refresh();
                if (node.UserModified && node.UiContainer != null)
                    node.UiContainer.IsExpanded = true;
            }
        }

        public bool FilterPredicate(Object o) {
            // If there's nothing filtering, accept everything.
            if (!Config.ModifiedOnly && String.IsNullOrWhiteSpace(Config.SearchText))
                return true;

            var node = o as InspectorNode;
            if (node != null) {
                var acceptedByModifiedOnly = (!Config.ModifiedOnly || node.UserModified);
                // HACK: The parent == null condition is to check if it's a root node,
                // this is a hack because a cleaner solution would be to use a different filter
                // predicate for child nodes.
                if (node.Parent == null && _searchTerms != null) {
                    for (var i = 0; i < _searchTerms.Length; i++) {
                        if (!(node.Name.ToUpper().Contains(_searchTerms[i]) ||
                              node.Type.Name.ToUpper().Contains(_searchTerms[i]))) {
                            // Rejected by search
                            return false;
                        }
                    }
                }
                if (acceptedByModifiedOnly)
                    return true;
            }
            // Rejected by modifiedOnly
            return false;
        }

        void InspectorWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            Config.Visible = Window.IsVisible;
        }

        void InspectorWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
            _locationJustChanged = true;
        }

        void InspectorWindow_LocationChanged(object sender, EventArgs e) {
            _locationJustChanged = true;
        }

        void TreeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var node = e.NewValue as InspectorNode;
            if (node != null) {
                Window.Methods.DataContext = node.Methods;
                Window.NodeToExpandAfterUpdate = node;
            }
        }

        void gameWindow_Resize(object sender, EventArgs e) {}
        void gameWindow_Move(object sender, EventArgs e) {}

        /// <summary>
        ///     Updates the position of the inspector if its attached
        ///     to the game window.
        /// </summary>
        void UpdatePosition() {
            Window.Left = GearsetResources.Game.Window.ClientBounds.Left - Window.Width;
            Window.Top = GearsetResources.Game.Window.ClientBounds.Top;
        }

        protected override void OnVisibleChanged() {
            if (Window != null) {
                Window.Visibility = Visible ? Visibility.Visible : Visibility.Hidden;
                Window.WasHiddenByGameMinimize = false;
            }
        }

        /// <summary>
        ///     Updates each component on the inspector TreeView.
        /// </summary>
        public override void Update(GameTime gameTime) {
            foreach (var obj in Window.TreeView1.Items) {
                var o = (InspectorNode)obj;
                o.Update();
            }

            foreach (var o in _methodCallers) {
                o.Update();
            }

            if (_locationJustChanged) {
                _locationJustChanged = false;
                Config.Top = Window.Top;
                Config.Left = Window.Left;
                Config.Width = Window.Width;
                Config.Height = Window.Height;
            }

            if (_updateSearchFilteringDelay > 0) {
                var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                _updateSearchFilteringDelay -= dt;
                if (_updateSearchFilteringDelay <= 0) {
                    _updateSearchFilteringDelay = 0;

                    // There's a chance that the worker is busy. Wait.
                    while (_filterWorker.IsBusy) {}
                    _filterWorker.DoWork -= filterWorker_DoWork;
                    _filterWorker.DoWork += filterWorker_DoWork;
                    _filterWorker.RunWorkerAsync();
                }
            }

            // If the node expansion was generated because the currently selected node
            // dissapeared (because we're adding private fields, for example) then this
            // would generate a conflict with the expansion.
            if (Window.NodeToExpandAfterUpdate != null) {
                Window.NodeToExpandAfterUpdate.Expand();
                Window.NodeToExpandAfterUpdate = null;
            }

            //series.AddPoints(new SeriesPoint[] { new SeriesPoint() { X = updateCount, Y = (float)Math.Sin(updateCount / 20f) } }, false);
            //float offset = updateCount / 20f;
            //for (int i = 0; i < 60; i++)
            //{
            //    series.Points[i] = new SeriesPoint() { X = i, Y = Math.Sin(offset + i / 10f) * 10f };
            //}
            //series.InvalidateData();
            //testSource.AppendAsync(System.Windows.Threading.Dispatcher.CurrentDispatcher, new Vector2((float)gameTime.TotalGameTime.TotalSeconds, (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds)));
            _updateCount++;
            //if (updateCount % 5 == 0 || true)
            //    XdtkResources.Console.DataSamplerManager.AddSample("testSampler1", (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 10), 30);
            //if (updateCount % 5 == 1 || true)
            //    XdtkResources.Console.DataSamplerManager.AddSample("testSampler2", (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 11), 30);
            //if (updateCount % 5 == 2 || true)
            //    XdtkResources.Console.DataSamplerManager.AddSample("testSampler3", (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 12), 30);
            //if (updateCount % 5 == 3 || true)
            //    XdtkResources.Console.DataSamplerManager.AddSample("testSampler4", (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 14), 30);
            //if (updateCount % 5 == 4 || true)
            //    XdtkResources.Console.DataSamplerManager.AddSample("testSampler5", (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 20), 30);
        }

        /// <summary>
        ///     Adds the object to the inspector logger so the user
        ///     can inspect its fields, properties and methods. The node
        ///     will be autoExpanded.
        /// </summary>
        /// <param name="name">A friendly name to use in the Inspector Tree.</param>
        /// <param name="o">The object to inspect.</param>
        public void Inspect(String name, Object o) {
            Inspect(name, o, false);
        }

        /// <summary>
        ///     Adds the object to the inspector logger so the user
        ///     can inspect its fields, properties and methods.
        /// </summary>
        /// <param name="name">A friendly name to use in the Inspector Tree.</param>
        /// <param name="o">The object to inspect.</param>
        /// <param name="autoExpand">Determines whether the node should automatically expand when added to the Inspector Tree.</param>
        public void Inspect(String name, Object o, bool autoExpand) {
            if (String.IsNullOrEmpty(name))
                name = "(unnamed object)";
            if (o == null)
                return;
            var t = o.GetType();
            if (t.IsValueType) {
                //GearsetResources.Console.Log("Gearset", "ValueTypes cannot be directly inspected. Ignoring {0} ({1}).", name, t.Name);
                //return;

                t = Type.GetType("Gearset.Components.InspectorWPF.ValueTypeWrapper`1").MakeGenericType(t);
                var wrapper = Activator.CreateInstance(t);
                var wrapperType = wrapper.GetType();
                wrapperType.GetProperty("Value").SetValue(wrapper, o, null);
                o = wrapper;
            }
            if (t == typeof(String)) {
                GearsetResources.Console.Log("Gearset", "Strings cannot be directly inspected. Ignoring {0} ({1}).", name, t.Name);
                return;
            }
            if (o == null) {
                GearsetResources.Console.Log("Gearset", "Object to inspect cannot be null. Ignoring {0} ({1}).", name, t.Name);
                return;
            }
            if (t.IsNestedPrivate) {
                GearsetResources.Console.Log("Gearset", "Cannot inspect inner (nested) types that are private. Ignoring {0} ({1}).", name, t.Name);
                return;
            }

            var insertPosition = Math.Min(2, _inspectedObjects.Count);
            foreach (var currentObject in _inspectedObjects) {
                var currentNode = currentObject;
                if (currentNode.Name == name) {
                    if (currentNode.Type != t) {
                        _inspectedObjects.Remove(currentNode);
                        currentNode = new InspectorNode(o, name, autoExpand);
                        _inspectedObjects.Add(currentNode);
                    }
                    else {
                        currentNode.RootTarget = o;
                    }
                    // This might be null if the window has not oppened yet.
                    if (currentNode.UiContainer != null) {
                        currentNode.UiContainer.IsSelected = true;
                        currentNode.UiContainer.BringIntoView();
                    }
                    return;
                }
            }
            var root = new InspectorNode(o, name, autoExpand);
            _inspectedObjects.Insert(insertPosition, root);
        }

        /// <summary>
        ///     Adds the object to the inspector logger so the user
        ///     can inspect its fields, properties and methods.
        /// </summary>
        /// <param name="name">A friendly name to use in the Inspector Tree.</param>
        /// <param name="o">The object to inspect.</param>
        /// <param name="autoExpand">Determines whether the node should automatically expand when added to the Inspector Tree.</param>
        internal void Watch(InspectorNode node) {
            if (node == null)
                return;
            _watchedNodes.Add(node);
        }

        /// <summary>
        ///     Remove the object from the inspector, if exist.
        /// </summary>
        /// <param name="o">The object to remove.</param>
        public void RemoveInspect(Object o) {
            InspectorNode container = null;
            foreach (var node in _inspectedObjects) {
                if (node.Target == o) {
                    container = node;
                    break;
                    ;
                }
            }
            if (container != null)
                _inspectedObjects.Remove(container);
        }

        /// <summary>
        ///     Get the TreeViewItems (containers) and let the InspectorTreeNodes
        ///     know where they are.
        /// </summary>
        void ItemContainerGenerator1_StatusChanged(object sender, EventArgs e) {
            if (Window.TreeView1.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) {
                foreach (var obj in Window.TreeView1.Items) {
                    var o = (InspectorNode)obj;
                    if (o.UiContainer == null || (o.UiContainer != null && o.UiContainer.Header != null && o.UiContainer.Header.ToString().Equals("{DisconnectedItem}"))) {
                        var item = Window.TreeView1.ItemContainerGenerator.ContainerFromItem(o) as TreeViewItem;
                        o.UiContainer = item;

                        // If we're in the Modified only view, expand everything.
                        if (Config.ModifiedOnly && o.UiContainer != null)
                            o.UiContainer.IsExpanded = true;


                        // If this item didn't have a UIContainer is because it
                        // is new, expand it. TODO: This line used to expand every 
                        // root node, but it makes the inspector really slow to appear.
                        // This should be configurable.
                        //o.Expand();
                        // item could be null if we're filtering the collection (there's an item but is not being show).
                        if (_inspectedObjects.Count > 2 && item != null) {
                            if (o.AutoExpand)
                                o.UiContainer.IsExpanded = true;
                            item.IsSelected = true;
                            item.BringIntoView();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Get the TreeViewItems (containers) and let the InspectorTreeNodes
        ///     know where they are.
        /// </summary>
        void ItemContainerGenerator2_StatusChanged(object sender, EventArgs e) {
            //if (Inspector.TreeView2.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            //{
            //    foreach (var o in MethodCallers)
            //    {
            //        if (o.UIContainer == null)
            //        {
            //            o.UIContainer = (ItemsControl)Inspector.TreeView2.ItemContainerGenerator.ContainerFromItem(o);
            //        }
            //    }
            //}
        }

        public void CraftMethodCall(MethodInfo info) {
            MethodCaller caller = new GenericMethodCaller(info, null);
            _methodCallers.Add(caller);
        }

        public void ClearInspectedObjects() {
            for (var i = _inspectedObjects.Count - 1; i >= 2; i--) {
                _inspectedObjects.RemoveAt(i);
            }
        }

        public void ClearMethods() {
            _methodCallers.Clear();
        }

        void OnPropertyChanged(String propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void AddNotice(string message, string url, string linkText) {
            var notice = new NoticeViewModel();
            notice.NoticeText = message;
            notice.NoticeHyperlinkUrl = url;
            notice.NoticeHyperlinkText = linkText;
            Window.Dispatcher.BeginInvoke((MethodInvoker)delegate { Notices.Add(notice); });
        }
    }
}
