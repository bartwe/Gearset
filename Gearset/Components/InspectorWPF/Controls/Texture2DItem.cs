using System;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Texture2DItem : VisualItemBase {
        Texture2D _currentTexture;

        public Texture2DItem() {
            InitializeComponent();
        }

        public sealed override void UpdateUi(Object value) {
            if (value != _currentTexture) {
                _currentTexture = value as Texture2D;
                if (_currentTexture == null)
                    return;

                using (var pngImage = new MemoryStream()) {
                    _currentTexture.SaveAsPng(pngImage, 20, 20);

                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.DecodePixelWidth = 20;
                    bi.StreamSource = pngImage;
                    bi.EndInit();
                    pngImage.Close();
                    Image.Source = bi;
                }
            }
        }
    }
}
