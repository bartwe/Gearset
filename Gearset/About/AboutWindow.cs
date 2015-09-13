using System.Diagnostics;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Navigation;

namespace Gearset {
    /// <summary>
    ///     Interaction logic for LicenseWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window {
        public AboutWindow() {
            InitializeComponent();
            ElementHost.EnableModelessKeyboardInterop(this);
        }

        void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
