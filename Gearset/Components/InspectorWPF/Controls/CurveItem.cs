using System;
using System.Windows;
using Microsoft.Xna.Framework;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     Interaction logic for Spinner.xaml
    /// </summary>
    public partial class CurveItem : VisualItemBase {
        public CurveItem() {
            InitializeComponent();
        }

        public override sealed void UpdateUi(Object value) {}
        public override sealed void UpdateVariable() {}

        void editButton_Click(object sender, RoutedEventArgs e) {
            var curve = TreeNode.Property as Curve;
            if (curve == null) {
                curve = new Curve();
            }
            GearsetResources.Console.AddCurve(TreeNode.Name, curve);
            GearsetResources.Console.Bender.Window.Show();
            GearsetResources.Console.Bender.Window.Focus();
        }
    }
}
