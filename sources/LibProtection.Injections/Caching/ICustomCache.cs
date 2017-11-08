namespace LibProtection.Injections.Caching
{
    public interface ICustomCache
    {
        bool Add(CacheFormatItem formatItem, string result);
        bool Get(CacheFormatItem formatItem, out string result);
    }
}
