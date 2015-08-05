using System;

namespace Gearset.Components {
    public class BooleanChangedEventArgs : EventArgs {
        public BooleanChangedEventArgs(bool newValue) {
            NewValue = newValue;
        }

        public bool NewValue { get; set; }
    }
}