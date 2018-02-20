using System;
using System.Collections.Generic;
using System.Threading;

namespace LibProtection.Injections
{
    internal sealed class HashTableCache<TKey, TValue> : ICache<TKey, TValue>
    {
        private class CacheRecord
        {
            public TKey Key;
            public TValue Value;
        }

        private readonly CacheRecord[] _cache;
        private readonly IEqualityComparer<TKey> _comparer;

        public HashTableCache(int capacity, IEqualityComparer<TKey> comparer = null)
        {
            _cache = new CacheRecord[capacity];
            _comparer = comparer ?? EqualityComparer<TKey>.Default;
        }

        public TValue Get(TKey key, Func<TKey, TValue> factory)
        {
            var hashCode = unchecked((uint)_comparer.GetHashCode(key));
            var index = hashCode % _cache.Length;
            var record = _cache[index];
            Thread.MemoryBarrier();

            if (record == null || !_comparer.Equals(record.Key, key))
            {
                record = new CacheRecord
                {
                    Key = key,
                    Value = factory(key),
                };

                Thread.MemoryBarrier();
                _cache[index] = record;
            }

            return record.Value;
        }
    }

}
