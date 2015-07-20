using System;

namespace Gearset.Components {
    public class ItemRemovedEventArgs<T> : EventArgs {
        public ItemRemovedEventArgs(T removedItem) {
            RemovedItem = removedItem;
        }

        public T RemovedItem { get; private set; }
    }

    public class ItemAddedEventArgs<T> : EventArgs {
        public ItemAddedEventArgs(T addedItem) {
            AddedItem = addedItem;
        }

        public T AddedItem { get; private set; }
    }
}
