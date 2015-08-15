using System;
using System.Windows.Media;
using Color = Microsoft.Xna.Framework.Color;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class ColorItem : VisualItemBase {
        readonly SolidColorBrush _brush;

        public ColorItem() {
            InitializeComponent();
            _brush = new SolidColorBrush(Colors.Transparent);
            FrontRect.Fill = _brush;

            UpdateIfExpanded = true;
        }

        public sealed override void UpdateUi(Object value) {
            var color = (Color)value;

            _brush.Color = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
