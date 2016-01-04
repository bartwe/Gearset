using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace Gearset.Components {
    /// <summary>
    ///     Widget that is shown above the Game's titlebar.
    /// </summary>
    public partial class WidgetWindow : Window {
        public WidgetWindow() {
            WindowStyle = WindowStyle.ToolWindow;
            InitializeComponent();
            //System.Windows.Interop.WindowInteropHelper wih = new System.Windows.Interop.WindowInteropHelper(this);
            //wih.Owner = GearsetResources.GameWindow.Handle;

            Loaded += XdtkWidget_Loaded;
        }

        public void Button_Click(object sender, RoutedEventArgs e) {
            // Execute the action.
            var button = (Button)e.Source;
            var action = ((ActionItem)button.DataContext).Action;
            if (action != null) {
                try {
                    action();
                }
                catch (Exception ex) {
                    button.Background = new SolidColorBrush(Color.FromRgb(200, 20, 20));
                    button.ToolTip = ex.Message;
                }
            }
        }

        public void ContextMenu_Click(object sender, RoutedEventArgs e) {
            var item = e.Source as MenuItem;
            if (((String)item.Header) == "About") {
                GearsetResources.AboutWindow.Show();
                e.Handled = true;
            }
            else if (((String)item.Header) == "Support/Feature Request") {
                Process.Start(new ProcessStartInfo("http://www.thecomplot.com/lib/support"));
                e.Handled = true;
            }
        }

        #region Don't show in alt-tab

        public void XdtkWidget_Loaded(object sender, RoutedEventArgs e) {
            var wndHelper = new WindowInteropHelper(this);
            var exStyle = (int)NativeMethods.GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GwlExstyle);
            exStyle |= (int)ExtendedWindowStyles.WsExToolwindow;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GwlExstyle, (IntPtr)exStyle);
        }

        [Flags]
        public enum ExtendedWindowStyles {
            // ...
            WsExToolwindow = 0x00000080
            // ...    
        }

        public enum GetWindowLongFields {
            // ...
            GwlExstyle = (-20)
            // ...    
        }

        static class NativeMethods {
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowLong(IntPtr hWnd, Int32 nIndex);
        }

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong) {
            var error = 0;
            var result = IntPtr.Zero; // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);
            if (IntPtr.Size == 4) {
                var tempResult = SetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else {
                result = SetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }
            if ((result == IntPtr.Zero) && (error != 0)) {
                throw new Win32Exception(error);
            }
            return result;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowLongPtr(IntPtr hWnd, Int32 nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern Int32 SetWindowLong(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);

        static int IntPtrToInt32(IntPtr intPtr) {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(Int32 dwErrorCode);

        #endregion
    }
}
