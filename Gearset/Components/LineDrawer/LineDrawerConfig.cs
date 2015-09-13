using System;

namespace Gearset.Components {
    [Serializable]
    public sealed class LineDrawerConfig : GearConfig {
        /// <summary>
        ///     Raised when the user request lines to be cleared.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler Cleared;

        /// <summary>
        ///     Clears all lines
        /// </summary>
        public void Clear() {
            if (Cleared != null)
                Cleared(this, EventArgs.Empty);
        }
    }
}
