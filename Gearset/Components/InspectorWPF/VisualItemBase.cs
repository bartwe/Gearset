using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     Interaction logic for Spinner.xaml
    /// </summary>
    public class VisualItemBase : UserControl {
        /// <summary>
        ///     The InspectorTreeNode that this float spinner affetcs.
        /// </summary>
        public InspectorNode TreeNode { get { return (InspectorNode)GetValue(TreeNodeProperty); } set { SetValue(TreeNodeProperty, value); } }

        /// <summary>
        ///     This will make the TreeNode stop calling UpdateUI on this
        ///     node if we're expanded. Ussually used for the genericItem
        ///     because the ToString() representation of a variable is
        ///     the same as watching it's children updating.
        /// </summary>
        public bool UpdateIfExpanded { get; set; }

        public static void TreeNodeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var item = d as VisualItemBase;
            if (item != null && item.TreeNode != null) {
                item.TreeNode.VisualItem = item;
                item.OnTreeNodeChanged();
            }
        }

        public virtual void OnTreeNodeChanged() {}

        /// <summary>
        ///     sealed override this method and add logic to update
        ///     the control to reflect the new value.
        /// </summary>
        /// <param name="value"></param>
        public virtual void UpdateUi(Object value) {
            // Do nothing
        }

        /// <summary>
        ///     sealed override this method and add logic to update
        ///     the value of the variable of the treenode.
        /// </summary>
        public virtual void UpdateVariable() {
            // Do nothing
        }

        /// <summary>
        ///     Defines a way to move the focus out of the
        ///     textbox when enter is pressed.
        /// </summary>
        protected static readonly TraversalRequest TraversalRequest = new TraversalRequest(FocusNavigationDirection.Next);

        /// <summary>
        ///     Registers a dependency property as backing store for the FloatValue property
        /// </summary>
        public static readonly DependencyProperty TreeNodeProperty =
            DependencyProperty.Register("TreeNode", typeof(InspectorNode), typeof(VisualItemBase),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, TreeNodeChangedCallback));
    }
}
