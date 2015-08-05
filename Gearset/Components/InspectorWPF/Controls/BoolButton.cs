using System;
using System.Windows;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class BoolButton : VisualItemBase {
        bool _isEventFake;

        public BoolButton() {
            InitializeComponent();

            ToggleButton.Checked += ToggleButton_Checked;
            ToggleButton.Unchecked += ToggleButton_Unchecked;
        }

        public void ToggleButton_Unchecked(object sender, RoutedEventArgs e) {
            if (!_isEventFake)
                UpdateVariable();
        }

        public void ToggleButton_Checked(object sender, RoutedEventArgs e) {
            if (!_isEventFake)
                UpdateVariable();
        }

        public override void UpdateUi(Object value) {
            _isEventFake = true;
            ToggleButton.IsChecked = (bool)value;
            _isEventFake = false;
        }

        public override void UpdateVariable() {
            TreeNode.Property = ToggleButton.IsChecked;
        }
    }
}
