using System;

namespace Gearset.Components {
    public sealed class BooleanChangedEventArgs : EventArgs {
        public BooleanChangedEventArgs(bool newValue) {
            NewValue = newValue;
        }

        public bool NewValue { get; set; }
    }
}
