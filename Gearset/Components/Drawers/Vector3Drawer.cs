﻿using System;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    sealed class Vector3Drawer : Gear {
        readonly InternalLineDrawer _lines;

        internal Vector3Drawer()
            : base(GearsetSettings.Instance.LineDrawerConfig) {
            _lines = new InternalLineDrawer();
            Children.Add(_lines);
        }

        internal void ShowVector3(String name, Vector3 location, Vector3 vector) {
            ShowVector3(name, location, vector, Color.White);
        }

        internal void ShowVector3(String name, Vector3 location, Vector3 vector, Color color) {
            var v1 = location;
            var v2 = location + vector;
            // Difference vector
            var diff = v1 - v2;
            var distance = Vector3.Distance(v1, v2);
            if (distance == 0)
                return;
            diff *= 1 / distance;
            // Craft a vector that is normal to diff
            var normal1 = Vector3.Cross(diff, new Vector3(diff.Y, diff.X, diff.Z));
            if (normal1 == Vector3.Zero)
                normal1 = Vector3.Cross(diff, diff + Vector3.UnitY);
            // Craft a vector that is normal to both diff and normal1
            var normal2 = Vector3.Cross(diff, normal1);

            normal1 *= distance * 0.02f;
            normal2 *= distance * 0.02f;
            diff *= distance * 0.04f;
            _lines.ShowLine(name + "1", v1, v2, color);
            _lines.ShowLine(name + "2", v2 + normal1 + diff, v2, color);
            _lines.ShowLine(name + "3", v2 - normal1 + diff, v2, color);
            _lines.ShowLine(name + "4", v2 + normal2 + diff, v2, color);
            _lines.ShowLine(name + "5", v2 - normal2 + diff, v2, color);
        }

        internal void ShowVector3Once(Vector3 location, Vector3 vector) {
            ShowVector3Once(location, vector, Color.White);
        }

        internal void ShowVector3Once(Vector3 location, Vector3 vector, Color color) {
            var v1 = location;
            var v2 = location + vector;
            // Difference vector
            var diff = v1 - v2;
            var distance = Vector3.Distance(v1, v2);
            if (distance == 0)
                return;
            diff *= 1 / distance;
            // Craft a vector that is normal to diff
            var normal1 = Vector3.Cross(diff, new Vector3(diff.Y, diff.X, diff.Z));
            if (normal1 == Vector3.Zero)
                normal1 = Vector3.Cross(diff, diff + Vector3.UnitY);
            // Craft a vector that is normal to both diff and normal1
            var normal2 = Vector3.Cross(diff, normal1);

            normal1 *= distance * 0.02f;
            normal2 *= distance * 0.02f;
            diff *= distance * 0.04f;
            _lines.ShowLineOnce(v1, v2, color);
            _lines.ShowLineOnce(v2 + normal1 + diff, v2, color);
            _lines.ShowLineOnce(v2 - normal1 + diff, v2, color);
            _lines.ShowLineOnce(v2 + normal2 + diff, v2, color);
            _lines.ShowLineOnce(v2 - normal2 + diff, v2, color);
        }
    }
}
