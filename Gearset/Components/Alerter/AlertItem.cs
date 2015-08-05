using System;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    class AlertItem {
        internal String Text;
        internal Vector2 Position;
        internal int RemainingTime = -1;

        internal AlertItem(String text, Vector2 position) {
            Text = text;
            Position = position;
        }

        internal AlertItem(String text, Vector2 position, int time) {
            Text = text;
            Position = position;
            RemainingTime = time;
        }
    }
}
