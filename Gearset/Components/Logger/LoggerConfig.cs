using System;
using System.Collections.Generic;

namespace Gearset.Components {
    [Serializable]
    public class LoggerConfig : GearConfig {
        public LoggerConfig() {
            // Defaults
            Top = 300;
            Left = 40;
            Width = 700;
            Height = 340;

            HiddenStreams = new List<string>();
        }

        [InspectorIgnore]
        public double Top { get; internal set; }

        [InspectorIgnore]
        public double Left { get; internal set; }

        [InspectorIgnore]
        public double Width { get; internal set; }

        [InspectorIgnore]
        public double Height { get; internal set; }

        [InspectorIgnore]
        public List<String> HiddenStreams { get; internal set; }
    }
}
