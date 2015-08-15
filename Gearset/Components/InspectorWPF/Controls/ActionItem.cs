using System;
using System.Windows;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class ActionItem : VisualItemBase {
        public ActionItem() {
            InitializeComponent();
            Button1.Click += Button1_Click;
        }

        public void Button1_Click(object sender, RoutedEventArgs e) {
            // This will call the Setter that in turn will
            // call the method.
            if (TreeNode == null || TreeNode.Parent == null) {
                GearsetResources.Console.Log("Gearset", "Internal Gearset error, could not call method.");
                return;
            }
            try {
                if (TreeNode.Method.IsStatic) {
                    TreeNode.Method.Invoke(null, new[] { TreeNode.Parent.Property });
                }
                else {
                    var returnValue = TreeNode.Method.Invoke(TreeNode.Parent.Property, null);
                    if (returnValue != null)
                        GearsetResources.Console.Inspect(TreeNode.Parent.Name + '.' + TreeNode.Name + "()", returnValue);
                }
            }
            catch (Exception ex) {
                GearsetResources.Console.Log("Gearset", "Method threw {0}: {1}", ex.GetType(), ex.Message);
            }
        }

        public override sealed void UpdateUi(Object value) {}

        public override sealed void OnTreeNodeChanged() {
            Button1.Content = TreeNode.FriendlyName;
        }
    }
}
