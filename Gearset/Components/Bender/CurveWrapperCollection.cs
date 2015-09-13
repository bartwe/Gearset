using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    ///     A collection of Curve Wrappers. It can be accessed as a dictionary
    ///     using the [] semantic either by name or index.
    /// </summary>
    public sealed class CurveWrapperCollection : ICollection<CurveWrapper> {
        readonly Dictionary<long, CurveWrapper> _curves;

        public CurveWrapperCollection() {
            _curves = new Dictionary<long, CurveWrapper>();
        }

        public CurveWrapper this[long index] { get { return _curves[index]; } set { _curves[index] = value; } }

        public void Add(CurveWrapper item) {
            if (_curves.ContainsKey(item.Id)) {
                throw new InvalidOperationException("The same Curve was added twice.");
            }
            _curves.Add(item.Id, item);
            if (ItemAdded != null)
                ItemAdded(this, new ItemAddedEventArgs<CurveWrapper>(item));
        }

        public void Clear() {
            _curves.Clear();
        }

        public bool Contains(CurveWrapper item) {
            var result = _curves.ContainsKey(item.Id);
            // Check for inconsistencies.
            Debug.Assert(_curves[item.Id] == item, "Inconsistency found in CurveCollection");
            return result;
        }

        public void CopyTo(CurveWrapper[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public int Count { get { return _curves.Count; } }
        public bool IsReadOnly { get { return false; } }

        public bool Remove(CurveWrapper item) {
            if (ItemRemoved != null)
                ItemRemoved(this, new ItemRemovedEventArgs<CurveWrapper>(item));
            return _curves.Remove(item.Id);
        }

        public IEnumerator<CurveWrapper> GetEnumerator() {
            foreach (var item in _curves) {
                yield return item.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _curves.GetEnumerator();
        }

        public event EventHandler<ItemAddedEventArgs<CurveWrapper>> ItemAdded;
        public event EventHandler<ItemRemovedEventArgs<CurveWrapper>> ItemRemoved;
    }
}
