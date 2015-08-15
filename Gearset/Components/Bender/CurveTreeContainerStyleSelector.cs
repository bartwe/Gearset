using System.Windows;
using System.Windows.Controls;

namespace Gearset.Components {
    public sealed class CurveTreeContainerStyleSelector : StyleSelector {
        public Style LeafStyle { get; set; }
        public Style NodeStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container) {
            if (item is CurveTreeLeaf)
                return LeafStyle;
            return NodeStyle;
        }
    }
}
