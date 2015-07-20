using System;

namespace Gearset.Components {
    [Serializable]
    public class BenderConfig : GearConfig {
        public BenderConfig() {
            // Defaults
            Top = 300;
            Left = 40;
            Width = 700;
            Height = 340;
        }

        [InspectorIgnore]
        public double Top { get; internal set; }

        [InspectorIgnore]
        public double Left { get; internal set; }

        [InspectorIgnore]
        public double Width { get; internal set; }

        [InspectorIgnore]
        public double Height { get; internal set; }
    }
}
