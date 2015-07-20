using System;

namespace Gearset.Components {
    /// <summary>
    /// This whole class is ignored by the inspector.
    /// </summary>
    [Serializable]
    public class DataSamplerConfig : GearConfig {
        public DataSamplerConfig() {
            DefaultHistoryLength = 60;
        }

        public int DefaultHistoryLength { get; set; }
    }
}
