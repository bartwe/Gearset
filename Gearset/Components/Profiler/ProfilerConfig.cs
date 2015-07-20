using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Gearset.Components.Profiler {
    [Serializable]
    public class ProfilerConfig : GearConfig {
        public ProfilerConfig() {
            // Defaults
            Top = 300;
            Left = 40;
            Width = 700;
            Height = 340;

            HiddenStreams = new List<string>();

            TimeRulerConfig = new TimeRulerUiViewConfig {
                Position = new Vector2(3, 3),
                Size = new Vector2(400, 16),
                Visible = true,
                VisibleLevelsFlags = 1
            };

            PerformanceGraphConfig = new PerformanceGraphUiViewConfig {
                Position = new Vector2(3, 20),
                Size = new Vector2(100, 60),
                Visible = true,
                SkipFrames = 0,
                VisibleLevelsFlags = 1 | 2 | 4
            };

            ProfilerSummaryConfig = new ProfilerSummaryUiViewConfig {
                Position = new Vector2(3, 150),
                Size = new Vector2(100, 60),
                Visible = true,
                VisibleLevelsFlags = 1 | 2 | 4
            };
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

        [InspectorIgnore]
        public TimeRulerUiViewConfig TimeRulerConfig { get; internal set; }

        [InspectorIgnore]
        public PerformanceGraphUiViewConfig PerformanceGraphConfig { get; internal set; }

        [InspectorIgnore]
        public ProfilerSummaryUiViewConfig ProfilerSummaryConfig { get; internal set; }

        [Serializable]
        public class UiViewConfig {
            public UiViewConfig() {
                Visible = true;
                VisibleLevelsFlags = 1;
            }

            [InspectorIgnore]
            public Vector2 Position { get; internal set; }

            [InspectorIgnore]
            public Vector2 Size { get; internal set; }

            [InspectorIgnore]
            public bool Visible { get; internal set; }

            [InspectorIgnore]
            public int VisibleLevelsFlags { get; internal set; }
        }

        [Serializable]
        public class TimeRulerUiViewConfig : UiViewConfig {}

        [Serializable]
        public class PerformanceGraphUiViewConfig : UiViewConfig {
            [InspectorIgnore]
            public uint SkipFrames { get; internal set; }
        }

        [Serializable]
        public class ProfilerSummaryUiViewConfig : UiViewConfig {}
    }
}
