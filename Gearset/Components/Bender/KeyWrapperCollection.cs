using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Gearset.Components.CurveEditorControl {
    /// <summary>
    /// A collection of Key Wrappers. It can be accessed as a dictionary
    /// using the [] semantic.
    /// </summary>
    public sealed class KeyWrapperCollection : ICollection<KeyWrapper> {
        readonly Dictionary<long, KeyWrapper> _keys;

        public KeyWrapperCollection() {
            _keys = new Dictionary<long, KeyWrapper>();
        }

        public KeyWrapper this[long index] { get { return _keys[index]; } set { _keys[index] = value; } }

        public void Add(KeyWrapper item) {
            if (_keys.ContainsKey(item.Id)) {
                throw new InvalidOperationException("The same Curve Key was added twice.");
            }
            _keys.Add(item.Id, item);
        }

        public void Clear() {
            _keys.Clear();
        }

        public bool Contains(KeyWrapper item) {
            var result = _keys.ContainsKey(item.Id);
            // Check for inconsistencies.
            Debug.Assert(_keys[item.Id] == item, "Inconsistency found in KeyCollection");
            return result;
        }

        public void CopyTo(KeyWrapper[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public int Count { get { return _keys.Count; } }
        public bool IsReadOnly { get { return false; } }

        public bool Remove(KeyWrapper item) {
            return _keys.Remove(item.Id);
        }

        public IEnumerator<KeyWrapper> GetEnumerator() {
            foreach (var item in _keys) {
                yield return item.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _keys.GetEnumerator();
        }

        /// <summary>
        /// Returns the KeyWrapper of the provided key. This is a relatively
        /// expensive O(n) method because it iterates over the values of a dict.
        /// </summary>
        public KeyWrapper GetWrapper(CurveKey key) {
            foreach (var wrapper in _keys.Values) {
                if (ReferenceEquals(wrapper.Key, key))
                    return wrapper;
            }

            return null;
        }
    }
}
