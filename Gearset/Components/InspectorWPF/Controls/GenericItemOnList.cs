using System.Windows;
using System.Windows.Controls;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class GenericItemOnList : UserControl {
        public GenericItemOnList() {
            InitializeComponent();
            DataContextChanged += GenericItemOnList_DataContextChanged;
        }

        void GenericItemOnList_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            TextBlock1.Text = (DataContext != null) ? DataContext.ToString() : "(null)";
        }
    }
}
