using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Gearset.Components.Logger {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class LoggerWindow : Window {
        public LoggerWindow() {
            InitializeComponent();

            Closing += LoggerWindow_Closing;
        }

        internal bool WasHiddenByGameMinimize { get; set; }
        internal event EventHandler<SoloRequestedEventArgs> SoloRequested;

        public void LoggerWindow_Closing(object sender, CancelEventArgs e) {
            e.Cancel = true;
            Hide();
        }

        public void MenuItem_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.SaveLogToFile();
        }

        public void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        public void Close_Click(object sender, RoutedEventArgs e) {
            Hide();
        }

        public void Maximize_Click(object sender, RoutedEventArgs e) {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        void Solo_Click(object sender, RoutedEventArgs e) {
            //e.OriginalSource.D
            if (SoloRequested != null)
                SoloRequested(this, new SoloRequestedEventArgs((StreamItem)((FrameworkElement)e.OriginalSource).DataContext));
        }

        void DisableAllButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Logger.DisableAllStreams();
        }

        void EnableAllButton_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Logger.EnableAllStreams();
        }
    }
}
