using System;

namespace Gearset.Components {
    sealed class ActionItem {
        internal Action Action;
        internal String Name;

        public override string ToString() {
            return Name;
        }
    }
}
