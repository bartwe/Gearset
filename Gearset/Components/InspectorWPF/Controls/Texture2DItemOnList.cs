using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Texture2DItemOnList : UserControl {
        /// <summary>
        /// Registers a dependency property
        /// </summary>
        //public static readonly DependencyProperty ObjectWrapperProperty =
        //    DependencyProperty.Register("ObjectWrapper", typeof(Gearset.Components.InspectorWPF.CollectionMarkerItem.ObjectWrapper), typeof(Texture2DItemOnList),
        //    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnObjectWrapperChanged));

        //private static void OnObjectWrapperChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        //{
        //    //((Texture2DItemOnList)d).ObjectWrapper = (Gearset.Components.InspectorWPF.CollectionMarkerItem.ObjectWrapper)args.NewValue;
        //    ((Texture2DItemOnList)d).UpdateUI((Gearset.Components.InspectorWPF.CollectionMarkerItem.ObjectWrapper)args.NewValue);
        //}
        ///// <summary>
        ///// What type of numeric value will this spinner handle
        ///// </summary>
        //public Gearset.Components.InspectorWPF.CollectionMarkerItem.ObjectWrapper ObjectWrapper { get { return (Gearset.Components.InspectorWPF.CollectionMarkerItem.ObjectWrapper)GetValue(ObjectWrapperProperty); } set { SetValue(ObjectWrapperProperty, value); } }
        Texture2D _currentTexture;

        public Texture2DItemOnList() {
            InitializeComponent();
            DataContextChanged += Texture2DItemOnList_DataContextChanged;
        }

        void Texture2DItemOnList_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            UpdateUi(DataContext);
        }

        public void UpdateUi(Object value) {
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

                NameLabel.Text = String.IsNullOrEmpty(_currentTexture.Name) ? "(No Name)" : _currentTexture.Name;
                StringLabel.Text = "(" + _currentTexture.Width + "x" + _currentTexture.Height + ") " + _currentTexture.Format + ". Levels:" + _currentTexture.LevelCount;
            }
        }
    }
}
