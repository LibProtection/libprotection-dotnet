using System;

namespace LibProtection.Injections
{
    public interface ICache<TKey, TValue>
    {
        TValue Get(TKey key, Func<TKey, TValue> factory);
    }
}
