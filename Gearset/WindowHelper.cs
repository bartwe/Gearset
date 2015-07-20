using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Gearset {
    public class WindowHelper {
        public static void EnsureOnScreen(Window window) {
            Screen[] sc;
            sc = Screen.AllScreens;

            //Rect boundsWpf = window.Bounds;
            var bounds = new Rectangle((int)window.Left, (int)window.Top, (int)window.Width, (int)window.Height);
            var isVisible = false;
            for (var i = 0; i < sc.Length; i++) {
                if (sc[i].WorkingArea.IntersectsWith(bounds)) {
                    isVisible = true;
                    break;
                }
            }

            if (!isVisible) {
                window.Left = 0;
                window.Top = 0;
            }
        }
    }
}
