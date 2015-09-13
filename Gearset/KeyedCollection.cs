using System;
using System.Collections.Generic;

namespace Gearset {
    public sealed class KeyedCollection<T> {
        /// <summary>
        ///     Dictionary that maps a key (string) to
        ///     an index in the IList.
        /// </summary>
        readonly Dictionary<String, int> _keyIndexTable;

        /// <summary>
        ///     The list that holds the values.
        /// </summary>
        readonly IList<T> _list;

        /// <summary>
        ///     Constructs a KeyCollection and uses the specified
        ///     list to hold the values. The list must be empty and
        ///     if it doesn't resize dynamically then it must have
        ///     enough space to hold everything inserted.
        /// </summary>
        public KeyedCollection(IList<T> list) {
            if (list.Count > 0)
                throw new ArgumentException("Parameter is not valid, the list must be empty", "list");
            _list = list;
            _keyIndexTable = new Dictionary<string, int>();
        }

        /// <summary>
        ///     Constructs a KeyedCollection with a List of T
        ///     to holds the values.
        /// </summary>
        public KeyedCollection() {
            _list = new List<T>();
            _keyIndexTable = new Dictionary<string, int>();
        }

        /// <summary>
        ///     Returns a reference to the back-end list.
        /// </summary>
        public IList<T> List { get { return _list; } }

        public int Count { get { return List.Count; } }
        public T this[int index] { get { return _list[index]; } }

        public T this[String index] {
            get {
                if (_keyIndexTable.ContainsKey(index))
                    return _list[_keyIndexTable[index]];
                throw new KeyNotFoundException();
            }
        }

        public bool ContainsKey(String key) {
            return _keyIndexTable.ContainsKey(key);
        }

        public int IndexOf(T value) {
            return List.IndexOf(value);
        }

        public void Insert(int index, T value) {
            throw new NotSupportedException("Use Set() instead");
        }

        public void RemoveAt(int index) {
            List.RemoveAt(index);
        }

        public void RemoveAt(String key) {
            if (ContainsKey(key))
                List.RemoveAt(_keyIndexTable[key]);
        }

        /// <summary>
        ///     Adds a new value to the keyed collection. If the key
        ///     already exist, the value will be changed.
        /// </summary>
        public int Set(String key, T value) {
            int index;
            if (ContainsKey(key)) {
                index = _keyIndexTable[key];
                _list[index] = value;
            }
            else {
                index = _list.Count;
                _list.Add(value);
                _keyIndexTable.Add(key, index);
            }
            return index;
        }

        /// <summary>
        ///     Adds a new value to the keyed collection
        /// </summary>
        public void Set(int index, T value) {
            if (index >= _list.Count) {
                throw new IndexOutOfRangeException("The index must be withing the list range.");
            }
            _list[index] = value;
        }

        /// <summary>
        ///     Adds a new value and returns its index.
        /// </summary>
        /// <param name="value"></param>
        public int Add(T value) {
            _list.Add(value);
            return _list.Count - 1;
        }
    }
}
