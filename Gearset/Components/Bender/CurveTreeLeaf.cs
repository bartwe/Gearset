using System.Windows.Media;
using Gearset.Components.CurveEditorControl;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    public sealed class CurveTreeLeaf : CurveTreeNode {
        public CurveTreeLeaf(CurveTreeNode parent, CurveWrapper curve)
            : base(parent) {
            Curve = curve;
        }

        public CurveWrapper Curve { get; private set; }
        public Brush ColorBrush { get { return Curve.ColorBrush; } }
        public sealed override string Name { get { return Curve.Name; } set { Curve.Name = value; } }

        public sealed override bool AreParentsVisible {
            get { return base.AreParentsVisible; }
            set {
                var prevValue = AreParentsVisible;
                areParentsVisible = value;
                OnParentVisibilityChanged(prevValue);
            }
        }

        public sealed override bool IsVisible {
            get { return base.IsVisible; }
            set {
                var becomingVisible = value && !IsVisible && AreParentsVisible;
                var becomingInvisible = !value && IsVisible && AreParentsVisible;
                if (becomingVisible) {
                    IsActuallyVisible = true;
                }
                else if (becomingInvisible) {
                    IsActuallyVisible = false;
                }
                isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }

        public bool IsActuallyVisible { get { return Curve.Visible; } private set { Curve.Visible = value; } }

        public CurveLoopType PreLoop {
            get { return Curve != null ? Curve.PreLoop : default(CurveLoopType); }
            set {
                Curve.PreLoop = value;
                OnPropertyChanged("PreLoop");
            }
        }

        public CurveLoopType PostLoop {
            get { return Curve != null ? Curve.PostLoop : default(CurveLoopType); }
            set {
                Curve.PostLoop = value;
                OnPropertyChanged("PostLoop");
            }
        }

        void OnParentVisibilityChanged(bool previousValue) {
            // We are visible but some ancestor is not. Hide it.
            if (previousValue == false && AreParentsVisible && IsVisible) {
                IsActuallyVisible = true;
            }

            if (previousValue && !AreParentsVisible && IsVisible) {
                IsActuallyVisible = false;
            }

            OnPropertyChanged("AreParentsVisible");
        }
    }
}
