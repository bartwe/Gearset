using System;

namespace Gearset.Components {
    /// <summary>
    ///     This whole sealed class is ignored by the inspector.
    /// </summary>
    [Serializable]
    public sealed class DataSamplerConfig : GearConfig {
        public DataSamplerConfig() {
            DefaultHistoryLength = 60;
        }

        public int DefaultHistoryLength { get; set; }
    }
}
