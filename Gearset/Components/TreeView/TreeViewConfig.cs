using System;

namespace Gearset.Components {
    [Serializable]
    public sealed class TreeViewConfig : GearConfig {
        string _filter;

        [Inspector(FriendlyName = "Filter (Items that match will be shown)")]
        public string Filter { get { return _filter; } set { _filter = value.ToLower(); } }

        /// <summary>
        /// Raised when the user request lines to be cleared.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler Cleared;

        /// <summary>
        /// Clears all lines
        /// </summary>
        public void Clear() {
            if (Cleared != null)
                Cleared(this, EventArgs.Empty);
        }
    }
}
