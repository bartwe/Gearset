using System;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    sealed class SphereDrawer : Gear {
        readonly InternalLineDrawer _lines;
        const int CircleSteps = 20;
        const int Sides = 12;
        static readonly float[] SinCircleSteps;
        static readonly float[] CosCircleSteps;
        static readonly float[] SinSideSteps;
        static readonly float[] CosSideSteps;

        static SphereDrawer() {
            SinCircleSteps = new float[CircleSteps + 1];
            CosCircleSteps = new float[CircleSteps + 1];
            for (var i = 0; i <= CircleSteps; i++) {
                var angle = i / (float)CircleSteps * MathHelper.TwoPi;
                SinCircleSteps[i] = (float)Math.Sin(angle);
                CosCircleSteps[i] = (float)Math.Cos(angle);
            }

            SinSideSteps = new float[Sides + 1];
            CosSideSteps = new float[Sides + 1];
            for (var i = 0; i <= Sides; i++) {
                var angle = i / (float)Sides * MathHelper.TwoPi;
                SinSideSteps[i] = (float)Math.Sin(angle);
                CosSideSteps[i] = (float)Math.Cos(angle);
            }
        }

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
                    _lines.ShowLine(name + "x" + j + i, center + y * SinCircleSteps[i] + x * CosCircleSteps[i], center + y * SinCircleSteps[i + 1] + x * CosCircleSteps[i + 1], color);
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
                var x = new Vector3(CosSideSteps[j], 0, SinSideSteps[j]) * radius;
                var y = Vector3.UnitY * radius;

                // Draw the vertical circles.
                for (var i = 0; i < CircleSteps; i++) {
                    _lines.ShowLineOnce(center + y * SinCircleSteps[i] + x * CosCircleSteps[i], center + y * SinCircleSteps[i + 1] + x * CosCircleSteps[i + 1], color);
                }
            }

            var x2 = Vector3.UnitX * radius;
            var z2 = Vector3.UnitZ * radius;
            // Draw the equator.
            for (var i = 0; i < CircleSteps; i++) {
                _lines.ShowLineOnce(center + x2 * SinCircleSteps[i] + z2 * CosCircleSteps[i], center + x2 * SinCircleSteps[i + 1] + z2 * CosCircleSteps[i + 1], color);
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
                var sin1 = SinCircleSteps[i];
                var cos1 = CosCircleSteps[i];
                var sin2 = SinCircleSteps[i + 1];
                var cos2 = CosCircleSteps[i + 1];

                _lines.ShowLineOnce(center + x2 * sin1 + z2 * cos1 - yp, center + x2 * sin1 + z2 * cos1 + yp, color);
                _lines.ShowLineOnce(center + x2 * sin1 + z2 * cos1 - yp, center + x2 * sin2 + z2 * cos2 - yp, color);
                _lines.ShowLineOnce(center + x2 * sin1 + z2 * cos1 + yp, center + x2 * sin2 + z2 * cos2 + yp, color);
            }
        }
    }
}
