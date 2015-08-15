using System;
using System.Windows.Controls;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class EnumItem : VisualItemBase {
        bool _wasUpdating;

        /// <summary>
        /// When the variable changes, the UpdateUI method
        /// will trigger a SelectionChanged event which would
        /// update the variable back (not wanted)
        /// </summary>
        bool _isEventFake;

        public EnumItem() {
            InitializeComponent();

            ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
            //ComboBox1.DropDownOpened += new EventHandler(ComboBox1_DropDownOpened);
            //ComboBox1.DropDownClosed += new EventHandler(ComboBox1_DropDownClosed);
        }

        public Enum EnumValue { get { return (Enum)Enum.Parse(TreeNode.Type, ((ComboBoxItem)ComboBox1.SelectedValue).Content.ToString()); } }

        public void ComboBox1_DropDownClosed(object sender, EventArgs e) {
            TreeNode.Updating = _wasUpdating;
        }

        public void ComboBox1_DropDownOpened(object sender, EventArgs e) {
            _wasUpdating = TreeNode.Updating;
            TreeNode.Updating = false;

            //if (!a)
            //GearsetResources.Console.Inspect("DropDown", this);
            //a = true;
        }

        public void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!_isEventFake) {
                UpdateVariable();
            }
        }

        public sealed override void UpdateUi(Object value) {
            var e = value as Enum;
            if (TreeNode.Type == typeof(Enum))
                return;
            if (Enum.IsDefined(TreeNode.Type, value)) {
                _isEventFake = true;
                ComboBox1.Text = value.ToString();
                _isEventFake = false;
            }
        }

        public sealed override void UpdateVariable() {
            TreeNode.Property = EnumValue;
        }

        /// <summary>
        /// When the treeNode is set, we populate the comboBox.
        /// </summary>
        public sealed override void OnTreeNodeChanged() {
            base.OnTreeNodeChanged();
            if (TreeNode.Type == typeof(Enum))
                return;
            foreach (var value in Enum.GetNames(TreeNode.Type)) {
                var item = new ComboBoxItem();
                item.Content = value;
                ComboBox1.Items.Add(item);
            }
        }
    }
}
