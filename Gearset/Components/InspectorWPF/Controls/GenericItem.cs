using System;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     Interaction logic for Spinner.xaml
    /// </summary>
    public partial class GenericItem : VisualItemBase {
        public GenericItem() {
            InitializeComponent();
        }

        public override sealed void UpdateUi(Object value) {
            if (value != null) {
                var text = value.ToString();
                TextBlock1.Text = text;
                TextBlock1.ToolTip = text;
            }
            else {
                TextBlock1.Text = "(null)";
            }
        }
    }
}
