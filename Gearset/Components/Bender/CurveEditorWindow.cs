using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Xml;
using Microsoft.Win32;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Point = System.Windows.Point;

namespace Gearset.Components {
    /// <summary>
    /// </summary>
    public partial class CurveEditorWindow : Window {
        bool isDragging;
        Point downPosition;

        public CurveEditorWindow() {
            InitializeComponent();

            ElementHost.EnableModelessKeyboardInterop(this);

            KeyDown += CurveEditorWindow_KeyDown;
            Closing += CurveEditorWindow_Closing;
        }

        internal bool WasHiddenByGameMinimize { get; set; }

        void CurveEditorWindow_KeyDown(object sender, KeyEventArgs e) {
            // Is Ctrl down?
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control) {
                // Undo!
                if (e.Key == Key.Z) {
                    curveEditorControl.Undo();
                }

                // Undo!
                if (e.Key == Key.Y) {
                    curveEditorControl.Redo();
                }
            }
        }

        public void CurveEditorWindow_Closing(object sender, CancelEventArgs e) {
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

        void DisableAllButton_Click(object sender, RoutedEventArgs e) {}
        void EnableAllButton_Click(object sender, RoutedEventArgs e) {}

        void SaveCurve_Click(object sender, RoutedEventArgs e) {
            var leaf = ((FrameworkElement)sender).DataContext as CurveTreeLeaf;
            if (leaf == null)
                return;

            //  Get the actual curve to save.
            var curve = leaf.Curve.Curve;
            if (curve == null)
                return;

            // Configure save file dialog box
            var dlg = new SaveFileDialog();
            dlg.FileName = leaf.Curve.Name; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "Xml files (.xml)|*.xml"; // Filter files by extension

            // Show save file dialog box
            var result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result != true) {
                return;
            }

            // Write!
            using (var streamOut = new FileStream(dlg.FileName, FileMode.Create)) {
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "  ";
                var writer = XmlWriter.Create(streamOut, settings);
                IntermediateSerializer.Serialize(writer, curve, dlg.FileName);
                writer.Close();
            }
        }

        void curveTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            //var leaf = curveTree.SelectedItem as CurveTreeLeaf;
            //if (leaf != null)
            //    ((CurveTreeViewModel)curveTree.DataContext).SelectedCurve = leaf.Curve;
            //else
            //    ((CurveTreeViewModel)curveTree.DataContext).SelectedCurve = null;
        }

        void RemoveCurve_Click(object sender, RoutedEventArgs e) {
            //HACK:
            var leaf = ((FrameworkElement)e.Source).DataContext as CurveTreeLeaf;
            if (leaf != null)
                GearsetResources.Console.RemoveCurve(leaf.Curve.Curve);
        }

        void NewCurve_Click(object sender, RoutedEventArgs e) {
            GearsetResources.Console.Bender.AddCurve("New Curve", new Curve());
        }

        void curveTree_MouseDown(object sender, MouseButtonEventArgs e) {
            downPosition = Mouse.GetPosition(this);
        }

        void curveTree_MouseMove(object sender, MouseEventArgs e) {
            if (curveTree.SelectedItem == null)
                return;
            if (Mouse.LeftButton == MouseButtonState.Pressed && !isDragging) {
                if (e.OriginalSource is TextBox)
                    return;
                var pos = Mouse.GetPosition(this);
                if (Math.Abs(pos.X - downPosition.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(pos.Y - downPosition.Y) > SystemParameters.MinimumVerticalDragDistance) {
                    StartDrag();
                }
            }
        }

        void StartDrag() {
            isDragging = true;
            var data = new DataObject(typeof(Object), ((CurveTreeLeaf)curveTree.SelectedItem).Curve.Curve);
            var de = DragDrop.DoDragDrop(curveTree, data, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.None);
            isDragging = false;
        }
    }
}
