using System;
using System.Windows;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// 
    /// </summary>
    public partial class ClearableGearConfigItem : VisualItemBase {
        bool _isEventFake;

        public ClearableGearConfigItem() {
            InitializeComponent();
        }

        public override void UpdateUi(Object value) {
            var config = value as GearConfig;
            if (config != null) {
                _isEventFake = true;
                ToggleButton.IsChecked = config.Enabled;
                _isEventFake = false;
            }
        }

        public void ToggleButton_Unchecked(object sender, RoutedEventArgs e) {
            if (!_isEventFake)
                UpdateVariable();
        }

        public void ToggleButton_Checked(object sender, RoutedEventArgs e) {
            if (!_isEventFake)
                UpdateVariable();
        }

        public override void UpdateVariable() {
            ((GearConfig)TreeNode.Property).Enabled = ToggleButton.IsChecked.Value;
            ((GearConfig)TreeNode.Property).Visible = ToggleButton.IsChecked.Value;
        }

        public void ClearButton_Click(object sender, RoutedEventArgs e) {
            var value = TreeNode.Property;
            if (value is LineDrawerConfig)
                ((LineDrawerConfig)value).Clear();
            else if (value is LabelerConfig)
                ((LabelerConfig)value).Clear();
            else if (value is TreeViewConfig)
                ((TreeViewConfig)value).Clear();
            else if (value is PlotterConfig)
                ((PlotterConfig)value).Clear();
        }
    }
}
