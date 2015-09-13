using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Texture2DMarkerItem : VisualItemBase {
        Texture2D _currentTexture;

        public Texture2DMarkerItem() {
            InitializeComponent();
        }

        public override sealed void UpdateUi(Object value) {
            if (value != _currentTexture) {
                _currentTexture = value as Texture2D;
                if (_currentTexture == null)
                    return;

                UpdateTexture();
            }
        }

        void UpdateTexture() {
            // Copy the XNA texture to a MemoryStream as png and then to the BitmapImage.
            using (var pngImage = new MemoryStream()) {
                var height = (int)Image.Height;
                var scale = height / (float)_currentTexture.Height;
                _currentTexture.SaveAsPng(pngImage, (int)(_currentTexture.Width * scale), height);

                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.DecodePixelHeight = height;
                bi.StreamSource = pngImage;
                bi.EndInit();
                pngImage.Close();
                Image.Source = bi;
            }
        }

        public void Button_Click(object sender, RoutedEventArgs e) {
            UpdateTexture();
        }
    }
}
