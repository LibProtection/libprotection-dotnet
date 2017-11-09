namespace LibProtection.Injections.Caching
{
    public interface ICustomCache
    {
        bool Add(CacheFormatItem formatItem, bool formatSuccess, string formatResult);
        bool Get(CacheFormatItem formatItem, out bool formatSuccess, out string resultValue);
    }
}
