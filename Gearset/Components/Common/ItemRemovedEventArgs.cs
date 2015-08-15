using System;

namespace Gearset.Components {
    public sealed class ItemRemovedEventArgs<T> : EventArgs {
        public ItemRemovedEventArgs(T removedItem) {
            RemovedItem = removedItem;
        }

        public T RemovedItem { get; private set; }
    }
}
