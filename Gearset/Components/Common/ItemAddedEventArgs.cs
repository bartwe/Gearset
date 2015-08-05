using System;

namespace Gearset.Components {
    public class ItemAddedEventArgs<T> : EventArgs {
        public ItemAddedEventArgs(T addedItem) {
            AddedItem = addedItem;
        }

        public T AddedItem { get; private set; }
    }
}