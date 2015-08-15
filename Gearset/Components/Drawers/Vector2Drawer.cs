using System;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    sealed class Vector2Drawer : Gear {
        readonly InternalLineDrawer _lines;

        internal Vector2Drawer()
            : base(GearsetSettings.Instance.LineDrawerConfig) {
            _lines = new InternalLineDrawer();
            _lines.CoordinateSpace = CoordinateSpace.GameSpace;
            Children.Add(_lines);
        }

        internal void ShowVector2(String name, Vector2 location, Vector2 vector) {
            ShowVector2(name, location, vector, Color.White);
        }

        internal void ShowVector2(String name, Vector2 location, Vector2 vector, Color color) {
            var v1 = location;
            var v2 = location + vector;
            // Difference vector
            var diff = v1 - v2;
            var distance = Vector2.Distance(location, v2);
            if (distance == 0)
                return;
            diff *= 1 / distance;
            // Craft a vector that is normal to diff
            var normal1 = new Vector2(diff.Y, -diff.X);

            // TODO: Move this value to a config
            normal1 *= 8;
            diff *= 8;
            _lines.ShowLine(name + "1", v1, v2, color);
            _lines.ShowLine(name + "2", v2 + normal1 + diff, v2, color);
            _lines.ShowLine(name + "3", v2 - normal1 + diff, v2, color);
        }

        internal void ShowVector2Once(Vector2 location, Vector2 vector) {
            ShowVector2Once(location, vector, Color.White);
        }

        internal void ShowVector2Once(Vector2 location, Vector2 vector, Color color) {
            var v1 = location;
            var v2 = vector;
            // Difference vector
            var diff = v1 - v2;
            var distance = Vector2.Distance(location, v2);
            if (distance == 0)
                return;
            diff *= 1 / distance;
            // Craft a vector that is normal to diff
            var normal1 = new Vector2(diff.Y, -diff.X);

            // TODO: Move this value to a config
            normal1 *= 8;
            diff *= 8;
            _lines.ShowLineOnce(v1, v2, color);
            _lines.ShowLineOnce(v2 + normal1 + diff, v2, color);
            _lines.ShowLineOnce(v2 - normal1 + diff, v2, color);
        }
    }
}
