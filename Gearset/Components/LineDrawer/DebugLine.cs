using Microsoft.Xna.Framework;

namespace Gearset.Components {
    public class DebugLine {
        public Color Color;
        public Vector3 V1;
        public Vector3 V2;
        public Vector3 Visible { get; set; }

        #region Constructors

        public DebugLine(Vector3 v1, Vector3 v2) {
            Initialize(v1, v2, Color.White);
        }

        public DebugLine(Vector3 v1, Vector3 v2, Color color) {
            Initialize(v1, v2, color);
        }

        void Initialize(Vector3 v1, Vector3 v2, Color color) {
            Color = color;
            V1 = v1;
            V2 = v2;
        }

        #endregion
    }
}
