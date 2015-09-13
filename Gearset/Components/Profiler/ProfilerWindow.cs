using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Gearset.Components.Profiler {
    /// <summary>
    ///     Interaction logic for Window1.xaml
    /// </summary>
    public partial class ProfilerWindow : Window {
        public ProfilerWindow() {
            InitializeComponent();

            Closing += ProfilerWindowClosing;
        }

        internal bool WasHiddenByGameMinimize { get; set; }

        public void ProfilerWindowClosing(object sender, CancelEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        public void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        public void CloseClick(object sender, RoutedEventArgs e) {
            Hide();
        }

        public void MaximizeClick(object sender, RoutedEventArgs e) {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        void Solo_Click(object sender, RoutedEventArgs e) {}

        void trEnableAllButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Profiler.TimeRuler.EnableAllLevels();
        }

        void trDisableAllButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Profiler.TimeRuler.DisableAllLevels();
        }

        void pgEnableAllButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Profiler.PerformanceGraph.EnableAllLevels();
        }

        void pgDisableAllButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Profiler.PerformanceGraph.DisableAllLevels();
        }

        void psEnableAllButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Profiler.ProfilerSummary.EnableAllLevels();
        }

        void psDisableAllButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Profiler.ProfilerSummary.DisableAllLevels();
        }
    }
}
