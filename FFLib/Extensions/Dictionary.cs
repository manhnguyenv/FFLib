using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFLib.Extensions
{
    public static class DictionaryExtension
    {

        public static T Item<K, T>(this Dictionary<K, T> self, K key)
        {
            if (!self.ContainsKey(key)) return default(T) ;
            return self[key];
        }

        public static void AddOrReplace<K,T>(this Dictionary<K, T> self, K key, T value)
        {
            if (self.ContainsKey(key)) self[key] = value;
            else self.Add(key, value);
        }
    }
}
