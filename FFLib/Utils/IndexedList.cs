using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFLib
{
    /// <summary>
    /// A list of items indexed by a key. Items with duplicate keys are stored in a list by that key.
    /// Key Capacity is the number of keys the list is preallocated to hold, it will expand as needed.
    /// Item Capacity is the number of items per Key (i.e. duplicate keys) the per Key Lists are preallocated to hold, they expand as needed.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class IndexedList<TKey,TValue> : Dictionary<TKey,List<TValue>>
    {
        int _itemCapacity = 3;
        /// <summary>
        /// Create an instance of an IndexedList with default keyCapacity and ItemCapacity.
        /// Default KeyCapacity is 10 and default ItemCapacity is 3.
        /// </summary>
        public IndexedList() : base() { }
        /// <summary>
        /// Create an instance of IndexedList with specified KeyCapacity and Default Item Capacity.
        /// Default ItemCapacity is 3.
        /// </summary>
        /// <param name="keyCapacity"></param>
        public IndexedList(int keyCapacity) : base(keyCapacity) { }
        /// <summary>
        /// Create an instance of an IndexedList with specified KeyCapacity and specified ItemCapacity.
        /// </summary>
        /// <param name="keyCapacity"></param>
        /// <param name="itemCapacity"></param>
        public IndexedList(int keyCapacity, int itemCapacity) : base(keyCapacity)
        {
            _itemCapacity = itemCapacity;
        }
        /// <summary>
        /// Add a value to the indexed list, if the item is a duplicate key then it is added to the list of items for that key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Add(TKey key, TValue value)
        {
            if (this.ContainsKey(key)) this[key].Add(value);
            else
            {
                this.Add(key, new List<TValue>(_itemCapacity));
                this[key].Add(value);
            }
        }

        /// <summary>
        /// Get the first Item for a given Key. If the Key is not found a default value is returned.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public TValue GetFirstItem(TKey key)
        {
            if (key == null) return default(TValue);
            if (!this.ContainsKey(key)) return default(TValue);
            if (this[key] == null) return default(TValue);
            return this[key][0];
        }

        /// <summary>
        /// Get the Last Item for a given Key. If the Key is not found a default value is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue GetLastItem(TKey key)
        {
            if (key == null) return default(TValue);
            if (!this.ContainsKey(key)) return default(TValue);
            if (this[key] == null) return default(TValue);
            return this[key][this[key].Count-1];
        }
    }
}
