using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Gearset.Components;
using Gearset.Components.Profiler;
#if WINDOWS
using System.Runtime.Serialization.Formatters.Binary;

#endif

namespace Gearset {
    /// <summary>
    ///     This sealed class holds the settings of Gearset.
    /// </summary>
    [Serializable]
    public sealed class GearsetSettings : INotifyPropertyChanged {
        bool _enabled;
        bool _showOverlays;
        float _saveFrequency;

        public GearsetSettings() {
            Enabled = true;
            ShowOverlays = true;
#if WINDOWS
            InspectorConfig = new InspectorConfig();
            LoggerConfig = new LoggerConfig();
#endif
            ProfilerConfig = new ProfilerConfig();
            DataSamplerConfig = new DataSamplerConfig();
            PlotterConfig = new PlotterConfig();
            TreeViewConfig = new TreeViewConfig();
            LabelerConfig = new LabelerConfig();
            LineDrawerConfig = new LineDrawerConfig();
            AlerterConfig = new AlerterConfig();
#if WINDOWS
            FinderConfig = new FinderConfig();
#endif

            // IMPORTANT:
            // NEW CONFIG INSTANCES SHOULD BE ADDED IN THE LOAD METHOD BELOW.
            DepthBufferEnabled = true;
            _saveFrequency = 5;
        }

        [Inspector(FriendlyName = "Master Switch")]
        public bool Enabled {
            get { return _enabled; }
            set {
                _enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        [Inspector(FriendlyName = "Show Overlays (Ctrl + Space)", HideCantWriteIcon = true)]
        public bool ShowOverlays {
            get { return _showOverlays; }
            set {
                _showOverlays = value;
                OnPropertyChanged("ShowOverlays");
            }
        }

        [Inspector(FriendlyName = "Enable Depth Buffer for 3D overlays")]
        public bool DepthBufferEnabled { get; set; }

        [Inspector(FriendlyName = "Save Frequency (secs)")]
        public float SaveFrequency { get { return _saveFrequency; } set { _saveFrequency = Math.Max(value, 2); } }

        [Inspector(FriendlyName = "Profiler", HideCantWriteIcon = true)]
        public ProfilerConfig ProfilerConfig { get; internal set; }

        [Inspector(FriendlyName = "Overlaid Plots", HideCantWriteIcon = true)]
        public PlotterConfig PlotterConfig { get; internal set; }

        [Inspector(FriendlyName = "Overlaid Tree View", HideCantWriteIcon = true)]
        public TreeViewConfig TreeViewConfig { get; internal set; }

        [Inspector(FriendlyName = "Overlaid Labels", HideCantWriteIcon = true)]
        public LabelerConfig LabelerConfig { get; internal set; }

        [Inspector(FriendlyName = "Overlaid Geometry", HideCantWriteIcon = true)]
        public LineDrawerConfig LineDrawerConfig { get; internal set; }

        [Inspector(FriendlyName = "Overlaid Alerts", HideCantWriteIcon = true)]
        public AlerterConfig AlerterConfig { get; private set; }

        [InspectorIgnore]
        public DataSamplerConfig DataSamplerConfig { get; internal set; }

        /// <summary>
        ///     Actual settings.
        /// </summary>
        public static GearsetSettings Instance { get; private set; }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string p) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        /// <summary>
        ///     Saves the current state of the configuration.
        /// </summary>
        internal static void Save() {
#if WINDOWS
            try {
                using (var memFile = new MemoryStream()) {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(memFile, Instance);

                    // If the file serialized correctly to the memfile, dump it to an actual file.
                    using (var file = new FileStream("gearset.conf", FileMode.Create)) {
                        file.Write(memFile.GetBuffer(), 0, (int)memFile.Length);
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine("Gearset settings could not be saved: " + e.Message);
            }
#endif
        }

        /// <summary>
        ///     Loads a saved configuration
        /// </summary>
        internal static void Load() {
#if WINDOWS
            try {
                if (File.Exists("gearset.conf")) {
                    using (var file = new FileStream("gearset.conf", FileMode.Open)) {
                        var formatter = new BinaryFormatter();
                        Instance = formatter.Deserialize(file) as GearsetSettings;
                    }
                }
                else {
                    Console.WriteLine("No gearset.conf file found, using a fresh config.");
                    Instance = new GearsetSettings();
                }
            }
            catch {
                Console.WriteLine("Problem while reading gearset.conf, using a fresh config.");
                Instance = new GearsetSettings();
            }
#else
            Instance = new GearsetSettings();
#endif
            InitializeSettingsIntroducedAfterV1();
        }

        static void InitializeSettingsIntroducedAfterV1() {
#if WINDOWS
            // Here we should check Configs added after 1st release to permit backward
            // compatibility with old gearset.conf files.
            if (Instance.FinderConfig == null)
                Instance.FinderConfig = new FinderConfig();

            if (Instance.LoggerConfig.HiddenStreams == null)
                Instance.LoggerConfig.HiddenStreams = new List<string>();

            if (Instance.BenderConfig == null)
                Instance.BenderConfig = new BenderConfig();
#endif

            if (Instance.PlotterConfig.HiddenPlots == null)
                Instance.PlotterConfig.HiddenPlots = new List<string>();

            if (Instance.ProfilerConfig == null)
                Instance.ProfilerConfig = new ProfilerConfig();

            if (Instance.ProfilerConfig.TimeRulerConfig == null)
                Instance.ProfilerConfig.TimeRulerConfig = new ProfilerConfig.TimeRulerUiViewConfig();

            if (Instance.ProfilerConfig.PerformanceGraphConfig == null)
                Instance.ProfilerConfig.PerformanceGraphConfig = new ProfilerConfig.PerformanceGraphUiViewConfig();

            if (Instance.ProfilerConfig.ProfilerSummaryConfig == null)
                Instance.ProfilerConfig.ProfilerSummaryConfig = new ProfilerConfig.ProfilerSummaryUiViewConfig();
        }

#if WINDOWS
        [Inspector(FriendlyName = "Inspector", HideCantWriteIcon = true)]
        public InspectorConfig InspectorConfig { get; internal set; }

        [Inspector(FriendlyName = "Logger", HideCantWriteIcon = true)]
        public LoggerConfig LoggerConfig { get; internal set; }

        [Inspector(FriendlyName = "Finder", HideCantWriteIcon = true)]
        public FinderConfig FinderConfig { get; set; }

        [Inspector(FriendlyName = "Bender", HideCantWriteIcon = true)]
        public BenderConfig BenderConfig { get; set; }
#endif
    }
}
