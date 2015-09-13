using System.Collections;
using System.Collections.Generic;
#if WINDOWS
using System.Collections.Specialized;

#endif

namespace Gearset {
    public sealed class FixedLengthQueue<T> :
#if WINDOWS
        INotifyCollectionChanged,
#endif
        IEnumerable<T> {
        readonly Queue<T> _queue;
        int _capacity;

        /// <summary>
        ///     When an item is dequeued it will get queued into <c>DequeueTarget</c>, if any.
        /// </summary>
        public FixedLengthQueue<T> DequeueTarget;

        public FixedLengthQueue(int capacity) {
            _queue = new Queue<T>(capacity);
            _capacity = capacity;
        }

        /// <summary>
        ///     Gets the number of items in the queue.
        /// </summary>
        /// <value>The count</value>
        public int Count { get { return _queue.Count; } }

        /// <summary>
        ///     Setting this is O(abs(Capacity - value))
        /// </summary>
        public int Capacity {
            get { return _capacity; }
            set {
                var diff = Count - value;
                while (diff-- > 0)
                    Dequeue();
                _capacity = value;
            }
        }

        public IEnumerator<T> GetEnumerator() {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _queue.GetEnumerator();
        }

#if WINDOWS
        public event NotifyCollectionChangedEventHandler CollectionChanged;
#endif

        /// <summary>
        ///     Enqueues the specified item. If the queue is full, the oldest item
        ///     will be droppped.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Enqueue(T item) {
            if (_queue.Count + 1 > _capacity && _queue.Count > 0) {
                var removedItem = _queue.Dequeue();
                if (DequeueTarget != null)
                    DequeueTarget.Enqueue(removedItem);
#if WINDOWS
                if (CollectionChanged != null) {
                    var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, 0);
                    CollectionChanged(this, args);
                }
#endif
            }
            _queue.Enqueue(item);

#if WINDOWS
            if (CollectionChanged != null) {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _queue.Count - 1);
                CollectionChanged(this, args);
            }
#endif
        }

        /// <summary>
        ///     Dequeues the oldest item.
        /// </summary>
        public T Dequeue() {
            var t = _queue.Dequeue();
#if WINDOWS
            if (CollectionChanged != null) {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, t);
                CollectionChanged(this, args);
            }
#endif
            return t;
        }
    }
}
