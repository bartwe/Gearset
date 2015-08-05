using System;

namespace Gearset.Components.Logger {
    class SoloRequestedEventArgs : EventArgs {
        public SoloRequestedEventArgs(StreamItem item) {
            StreamItem = item;
        }

        internal StreamItem StreamItem { get; private set; }
    }
}