using LibProtection.Injections.Caching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LibProtection.Injections
{
    internal class FormatProvider : IFormatProvider
    {
        private readonly Formatter _formatter;
        private readonly List<Fragment> _formattedFragments = new List<Fragment>();

        private FormatProvider(char complementaryChar)
        {
            _formatter = new Formatter(complementaryChar);
        }

        private static class GenericHolder<T> where T : LanguageProvider
        {
            // ReSharper disable once StaticMemberInGenericType
            public static volatile RandomizedLRUCache<CacheFormatItem, (bool Success, string ResultValue)> Instance
                = new RandomizedLRUCache<CacheFormatItem, (bool Success, string ResultValue)>(1024);

            // ReSharper disable once StaticMemberInGenericType
            public static volatile ICustomCache CustomCache;

            public static Func<CacheFormatItem, (bool Success, string ResultValue)> TryFormatDelegate { get; } = TryFormatInternal<T>;
        }

        public static void SetCustomCache<T>(ICustomCache customCache) where T : LanguageProvider
            => GenericHolder<T>.CustomCache = customCache;

        public static void SetDefaultCacheSize<T>(int cacheTableSize) where T : LanguageProvider
        {
            if (cacheTableSize < 0)
            {
                throw new ArgumentException($"{nameof(cacheTableSize)} argument of {nameof(SetDefaultCacheSize)} method should be greater or equal to zero!");
            }

            GenericHolder<T>.Instance = (cacheTableSize != 0)
                ? GenericHolder<T>.Instance = new RandomizedLRUCache<CacheFormatItem, (bool Success, string ResultValue)>(cacheTableSize)
                : null;
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? _formatter : null;
        }

        public void AddFragment(Fragment fragment)
        {
            _formattedFragments.Add(fragment);
        }

        public static bool TryFormat<T>(string format, out string formatted, object[] args)
            where T : LanguageProvider
        {
            var keyItem = new CacheFormatItem
            {
                Format = format,
                Args = args,
            };

            var customCache = GenericHolder<T>.CustomCache;
            if (customCache != null)
            {
                if (customCache.Get(keyItem, out var formatSuccess, out var formatResult))
                {
                    formatted = formatResult;
                    return formatSuccess;
                }

                (formatSuccess, formatResult) = TryFormatInternal<T>(keyItem);
                customCache.Add(keyItem, formatSuccess, formatResult);
                formatted = formatResult;
                return formatSuccess;
            }

            var lruCache = GenericHolder<T>.Instance;

            var (success, resultValue) = (lruCache != null)
                ? GenericHolder<T>.Instance.Get(keyItem, GenericHolder<T>.TryFormatDelegate)
                : TryFormatInternal<T>(keyItem);

            formatted = resultValue;
            return success;
        }

        private static (bool Success, string ResultValue) TryFormatInternal<T>(CacheFormatItem formatItem)
            where T : LanguageProvider
        {
            var format = formatItem.Format;
            var args = formatItem.Args;

            var complementaryChar = format.GetComplementaryChar();
            var formatProvider = new FormatProvider(complementaryChar);
            var formattedBuilder = new StringBuilder(string.Format(formatProvider, format, args));
            var taintedRanges = new List<Range>();
            var currentCharIndex = 0;
            var currentFragmentIndex = 0;
            
            while (currentCharIndex < formattedBuilder.Length)
            {
                if (formattedBuilder[currentCharIndex] == complementaryChar)
                {
                    var currentFragment = formatProvider._formattedFragments[currentFragmentIndex];
                    var currentValue = currentFragment.FormattedValue;
                    var lowerBound = currentCharIndex;

                    while (currentCharIndex - lowerBound < currentValue.Length)
                    {
                        Debug.Assert(formattedBuilder[currentCharIndex] == complementaryChar);
                        formattedBuilder[currentCharIndex] = currentValue[currentCharIndex - lowerBound];
                        currentCharIndex++;
                    }

                    var upperBound = currentCharIndex - 1;
                    if (!currentFragment.IsSafe) { taintedRanges.Add(new Range(lowerBound, upperBound)); }
                    currentFragmentIndex++;
                }
                else
                {
                    currentCharIndex++;
                }
            }

            var formatted = formattedBuilder.ToString();
            Debug.Assert(!formatted.Contains(complementaryChar.ToString()));

            if (LanguageService<T>.TrySanitize(formatted, taintedRanges, out var sanitized))
            {
                return (true, sanitized);
            }

            return (false, null);
        }
    }
}
