using System;

namespace Gearset.Components.Logger {
    sealed class SoloRequestedEventArgs : EventArgs {
        public SoloRequestedEventArgs(StreamItem item) {
            StreamItem = item;
        }

        internal StreamItem StreamItem { get; private set; }
    }
}
