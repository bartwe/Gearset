using System;
using System.ComponentModel;
using Gearset.Components.CurveEditorControl;
using Microsoft.Xna.Framework;
using Color = System.Windows.Media.Color;

namespace Gearset.Components {
    sealed class CurveTreeViewModel : INotifyPropertyChanged {
        /// <summary>
        ///     A random used to generate colors, used with a predefined seed so multiple
        ///     runs of the same code path will yield the same colors.
        /// </summary>
        readonly Random _random = new Random(32154);

        ///// <summary>
        ///// The currently selected curve. This value must be set by the owner
        ///// window of the control.
        ///// </summary>
        //public CurveWrapper SelectedCurve
        //{
        //    get
        //    {
        //        return selectedCurve;
        //    }
        //    set
        //    {
        //        selectedCurve = value;
        //        OnPropertyChanged("SelectedCurve");
        //    }
        //}

        CurveTreeNode _root;

        public CurveTreeViewModel(CurveEditorControl2 control) {
            Control = control;
            Root = new CurveTreeNode(String.Empty, null);
        }

        public CurveEditorControl2 Control { get; private set; }

        public CurveTreeNode Root {
            get { return _root; }
            private set {
                _root = value;
                OnPropertyChanged("Root");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Removes the provided Curve from Bender.
        /// </summary>
        internal void RemoveCurve(Curve curve) {
            var leaf = FindContainerLeaf(Root, curve);
            if (leaf != null) {
                RemoveNodeAndCurves(leaf);
                leaf.Parent.Children.Remove(leaf);
            }
        }

        static CurveTreeLeaf FindContainerLeaf(CurveTreeNode node, Curve curve) {
            for (var i = node.Children.Count - 1; i >= 0; i--) {
                var leaf = node.Children[i] as CurveTreeLeaf;
                if (leaf != null && ReferenceEquals(curve, leaf.Curve.Curve)) {
                    return leaf;
                }
                // Call recursively on this child;
                leaf = FindContainerLeaf(node.Children[i], curve);
                if (leaf != null)
                    return leaf;
            }
            return null;
        }

        internal void RemoveCurveOrGroup(String name) {
            var path = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            CurveTreeNode child;
            var node = Root;
            for (var i = 0; i < path.Length - 1; i++) {
                if ((child = GetChildNode(node, path[i])) == null) {
                    GearsetResources.Console.Log("Gearset", "Bender error. Curve or Group '{0}' doesn't exists.", name);
                    return;
                }
                node = child;
            }

            // Check if the last item actually exists, and finnally remove it.
            var curveName = path[path.Length - 1];
            if ((child = GetChildNode(node, curveName)) == null) {
                GearsetResources.Console.Log("Gearset", "Bender error. Curve or Group '{0}' doesn't exists.", name);
                return;
            }
            RemoveNodeAndCurves(child);
            node.Children.Remove(child);
        }

        /// <summary>
        ///     Will remove the whole subtree of nodes below the provided node (from the control).
        ///     The provided node must be removed from the tree by the calling code.
        /// </summary>
        void RemoveNodeAndCurves(CurveTreeNode node) {
            for (var i = node.Children.Count - 1; i >= 0; i--) {
                RemoveNodeAndCurves(node.Children[i]);
            }
            var leaf = node as CurveTreeLeaf;
            if (leaf != null)
                Control.Curves.Remove(leaf.Curve);
        }

        /// <summary>
        ///     Adds the curve to the TreeView and also the CurveEditorControl.
        /// </summary>
        internal void AddCurve(string name, Curve curve) {
            // Check if the curve is already added.
            var container = FindContainerLeaf(Root, curve);
            if (container != null) {
                container.IsSelected = true;
                container.IsVisible = true;
                return;
            }

            // Add to tree
            var path = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var node = Root;
            for (var i = 0; i < path.Length - 1; i++) {
                CurveTreeNode child;
                if ((child = GetChildNode(node, path[i])) == null) {
                    child = new CurveTreeNode(path[i], node);
                    node.Children.Add(child);
                }
                node = child;
            }

            // Add to control
            var curveName = path[path.Length - 1];
            var wrapper = new CurveWrapper(curve, curveName, Control, GetColor(curveName));
            Control.Curves.Add(wrapper);
            // Add to tree
            node.Children.Add(new CurveTreeLeaf(node, wrapper));

            //for (int i = 0; i < 20; i++)
            //{
            //    wrapper.AddKey(new CurveKey((float)random.NextDouble(), (float)random.NextDouble()));
            //}
        }

        static CurveTreeNode GetChildNode(CurveTreeNode node, string name) {
            foreach (var item in node.Children) {
                if (item.Name == name)
                    return item;
            }
            return null;
        }

        Color GetColor(string name) {
            name = name.ToUpper();
            if (name == "X" || name == "R" || name == "RED")
                return Color.FromRgb(200, 80, 80);
            if (name == "Y" || name == "G" || name == "GREEN")
                return Color.FromRgb(80, 200, 80);
            if (name == "Z" || name == "B" || name == "BLUE")
                return Color.FromRgb(80, 80, 200);
            if (name == "W" || name == "A" || name == "ALPHA")
                return Color.FromRgb(220, 220, 220);
            return Color.FromRgb((byte)(_random.Next(155) + 100), (byte)(_random.Next(155) + 100), (byte)(_random.Next(155) + 100));
        }
    }
}
