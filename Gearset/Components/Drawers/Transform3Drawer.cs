using System;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    sealed class Transform3Drawer : Gear {
        readonly Vector3Drawer _vectors;
        internal float AxisSize = 1f;

        internal Transform3Drawer()
            : base(GearsetSettings.Instance.LineDrawerConfig) {
            _vectors = new Vector3Drawer();
            Children.Add(_vectors);
        }

        internal void ShowTransform(String name, Matrix transform) {
            ShowTransform(name, transform, 1);
        }

        internal void ShowTransform(String name, Matrix transform, float axisScale) {
            var t = transform.Translation;
            var scale = AxisSize * axisScale;
            _vectors.ShowVector3(name + "x", t, transform.Right * scale, Color.Red);
            _vectors.ShowVector3(name + "y", t, transform.Up * scale, Color.Green);
            _vectors.ShowVector3(name + "z", t, transform.Forward * scale, Color.Blue);
        }

        internal void ShowTransformOnce(Matrix transform) {
            ShowTransformOnce(transform, 1);
        }

        internal void ShowTransformOnce(Matrix transform, float axisScale) {
            var t = transform.Translation;
            var scale = AxisSize * axisScale;
            _vectors.ShowVector3Once(t, transform.Right * scale, Color.Red);
            _vectors.ShowVector3Once(t, transform.Up * scale, Color.Green);
            _vectors.ShowVector3Once(t, transform.Forward * scale, Color.Blue);
        }
    }
}
