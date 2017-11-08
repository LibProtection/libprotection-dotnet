using System;
using System.Collections.Generic;
using System.Threading;

namespace LibProtection.Injections.Caching
{
    internal class RandomizedLRUCache<TKey, TValue>
    {
        private class CacheRecord
        {
            public TKey Key;
            public TValue Value;
            public int Counter;
        }

        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private readonly Dictionary<TKey, CacheRecord> _cache;
        private readonly CacheRecord[] _records;
        private readonly Random _random;

        public RandomizedLRUCache(int capacity, IEqualityComparer<TKey> comparer = null)
        {
            _cache = new Dictionary<TKey, CacheRecord>(capacity + 1, comparer ?? EqualityComparer<TKey>.Default);
            _records = new CacheRecord[capacity];
            _random = new Random(DateTime.Now.Millisecond);
        }

        public TValue Get(TKey key, Func<TKey, TValue> factory)
        {
            try
            {
                _locker.EnterUpgradeableReadLock();

                CacheRecord record;
                if (!_cache.TryGetValue(key, out record))
                {
                    var value = factory(key);

                    _locker.EnterWriteLock();

                    if (!_cache.TryGetValue(key, out record))
                    {
                        record = new CacheRecord
                        {
                            Key = key,
                            Value = value,
                        };
                        _cache.Add(key, record);
                    }
                }
                else
                {
                    _locker.EnterWriteLock();
                }

                var index = _random.Next(_records.Length);
                var oldRecord = _records[index];
                _records[index] = record;

                if (oldRecord == null)
                {
                    ++record.Counter;
                }
                else if (oldRecord != record)
                {
                    ++record.Counter;

                    if (--oldRecord.Counter == 0)
                    {
                        _cache.Remove(oldRecord.Key);
                    }
                }

                return record.Value;
            }
            finally
            {
                if (_locker.IsWriteLockHeld) { _locker.ExitWriteLock(); }
                if (_locker.IsUpgradeableReadLockHeld) { _locker.ExitUpgradeableReadLock(); }
            }
        }
    }
}
