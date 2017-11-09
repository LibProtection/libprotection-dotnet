using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

internal class RandomizedLRUCache<TKey, TValue>
{
    private class CacheRecord
    {
        public TKey Key;
        public TValue Value;
        public int Counter;
    }

    private static readonly Random seedSource = new Random(DateTime.Now.Millisecond);

    [ThreadStatic]
    private static Random _random;

    private readonly ConcurrentDictionary<TKey, CacheRecord> _cache;
    private readonly CacheRecord[] _records;
    private readonly Func<TKey, TValue> _valueFactory;
    private readonly Func<TKey, CacheRecord> _recordFactory;

    public RandomizedLRUCache(Func<TKey, TValue> factory, int capacity, IEqualityComparer<TKey> comparer = null)
    {
        _cache = new ConcurrentDictionary<TKey, CacheRecord>(comparer ?? EqualityComparer<TKey>.Default);
        _records = new CacheRecord[capacity];
        _valueFactory = factory;
        _recordFactory = RecordFactory;
    }

    public TValue Get(TKey key)
    {
        var record = _cache.GetOrAdd(key, _recordFactory);
        var index = GetRandomIndex();
        var oldRecord = Interlocked.Exchange(ref _records[index], record);

        if (oldRecord == null)
        {
            Interlocked.Increment(ref record.Counter);
        }
        else if (oldRecord != record)
        {
            Interlocked.Increment(ref record.Counter);
            if (Interlocked.Decrement(ref oldRecord.Counter) == 0)
            {
                _cache.TryRemove(oldRecord.Key, out oldRecord);
            }
        }

        return record.Value;
    }

    private CacheRecord RecordFactory(TKey key) =>
        new CacheRecord { Key = key, Value = _valueFactory(key) };

    private int GetRandomIndex()
    {
        if (_random == null)
        {
            lock (seedSource)
            {
                _random = new Random(seedSource.Next());
            }
        }

        return _random.Next(_records.Length);
    }
}
