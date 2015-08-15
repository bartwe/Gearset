using System;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    sealed class SphereDrawer : Gear {
        readonly InternalLineDrawer _lines;
        internal int CircleSteps = 20;
        internal int Sides = 12;

        internal SphereDrawer()
            : base(GearsetSettings.Instance.LineDrawerConfig) {
            _lines = new InternalLineDrawer();
            Children.Add(_lines);
        }

        internal void ShowSphere(String name, BoundingSphere sphere) {
            ShowSphere(name, sphere, Color.White);
        }

        internal void ShowSphere(String name, BoundingSphere sphere, Color color) {
            ShowSphere(name, sphere.Center, sphere.Radius, color);
        }

        internal void ShowSphere(String name, Vector3 center, float radius) {
            ShowSphere(name, center, radius, Color.White);
        }

        internal void ShowSphere(String name, Vector3 center, float radius, Color color) {
            for (var j = 0; j < Sides; j++) {
                var angle = j / (float)Sides * MathHelper.TwoPi;
                var x = new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle)) * radius;
                var y = Vector3.UnitY * radius;

                // Draw the vertical circles.
                for (var i = 0; i < CircleSteps; i++) {
                    var angle1 = i / (float)CircleSteps * MathHelper.TwoPi;
                    var angle2 = (i + 1) / (float)CircleSteps * MathHelper.TwoPi;
                    var sin1 = (float)Math.Sin(angle1);
                    var cos1 = (float)Math.Cos(angle1);
                    var sin2 = (float)Math.Sin(angle2);
                    var cos2 = (float)Math.Cos(angle2);
                    _lines.ShowLine(name + "x" + j + i, center + y * sin1 + x * cos1, center + y * sin2 + x * cos2, color);
                }
            }

            var x2 = Vector3.UnitX * radius;
            var z2 = Vector3.UnitZ * radius;
            // Draw the equator.
            for (var i = 0; i < CircleSteps; i++) {
                var sin1 = (float)Math.Sin(i / (float)CircleSteps * MathHelper.TwoPi);
                var cos1 = (float)Math.Cos(i / (float)CircleSteps * MathHelper.TwoPi);
                var sin2 = (float)Math.Sin((i + 1) / (float)CircleSteps * MathHelper.TwoPi);
                var cos2 = (float)Math.Cos((i + 1) / (float)CircleSteps * MathHelper.TwoPi);
                _lines.ShowLine(name + "y" + i, center + x2 * sin1 + z2 * cos1, center + x2 * sin2 + z2 * cos2, color);
            }
        }

        internal void ShowSphereOnce(BoundingSphere sphere) {
            ShowSphereOnce(sphere, Color.White);
        }

        internal void ShowSphereOnce(BoundingSphere sphere, Color color) {
            ShowSphereOnce(sphere.Center, sphere.Radius, color);
        }

        internal void ShowSphereOnce(Vector3 center, float radius) {
            ShowSphereOnce(center, radius, Color.White);
        }

        internal void ShowSphereOnce(Vector3 center, float radius, Color color) {
            for (var j = 0; j < Sides; j++) {
                var angle = j / (float)Sides * MathHelper.TwoPi;
                var x = new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle)) * radius;
                var y = Vector3.UnitY * radius;

                // Draw the vertical circles.
                for (var i = 0; i < CircleSteps; i++) {
                    var angle1 = i / (float)CircleSteps * MathHelper.TwoPi;
                    var angle2 = (i + 1) / (float)CircleSteps * MathHelper.TwoPi;
                    var sin1 = (float)Math.Sin(angle1);
                    var cos1 = (float)Math.Cos(angle1);
                    var sin2 = (float)Math.Sin(angle2);
                    var cos2 = (float)Math.Cos(angle2);
                    _lines.ShowLineOnce(center + y * sin1 + x * cos1, center + y * sin2 + x * cos2, color);
                }
            }

            var x2 = Vector3.UnitX * radius;
            var z2 = Vector3.UnitZ * radius;
            // Draw the equator.
            for (var i = 0; i < CircleSteps; i++) {
                var sin1 = (float)Math.Sin(i / (float)CircleSteps * MathHelper.TwoPi);
                var cos1 = (float)Math.Cos(i / (float)CircleSteps * MathHelper.TwoPi);
                var sin2 = (float)Math.Sin((i + 1) / (float)CircleSteps * MathHelper.TwoPi);
                var cos2 = (float)Math.Cos((i + 1) / (float)CircleSteps * MathHelper.TwoPi);
                _lines.ShowLineOnce(center + x2 * sin1 + z2 * cos1, center + x2 * sin2 + z2 * cos2, color);
            }
        }

        internal void ShowCylinderOnce(Vector3 center, Vector3 radius) {
            ShowCylinderOnce(center, radius, Color.White);
        }

        internal void ShowCylinderOnce(Vector3 center, Vector3 radius, Color color) {
            var x2 = Vector3.UnitX * radius.X;
            var z2 = Vector3.UnitZ * radius.Z;
            var yp = Vector3.UnitY * radius.Y;
            // Draw the equator.
            for (var i = 0; i < CircleSteps; i++) {
                var sin1 = (float)Math.Sin(i / (float)CircleSteps * MathHelper.TwoPi);
                var cos1 = (float)Math.Cos(i / (float)CircleSteps * MathHelper.TwoPi);
                var sin2 = (float)Math.Sin((i + 1) / (float)CircleSteps * MathHelper.TwoPi);
                var cos2 = (float)Math.Cos((i + 1) / (float)CircleSteps * MathHelper.TwoPi);

                _lines.ShowLineOnce(center + x2 * sin1 + z2 * cos1 - yp, center + x2 * sin1 + z2 * cos1 + yp, color);
                _lines.ShowLineOnce(center + x2 * sin1 + z2 * cos1 - yp, center + x2 * sin2 + z2 * cos2 - yp, color);
                _lines.ShowLineOnce(center + x2 * sin1 + z2 * cos1 + yp, center + x2 * sin2 + z2 * cos2 + yp, color);
            }
        }
    }
}
