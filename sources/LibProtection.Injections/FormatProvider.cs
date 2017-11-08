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

        private static class GenericCacheHolder<T>
        {
            public static RandomizedLRUCache<CacheFormatItem, (bool Success, string ResultValue)> Instance
                = new RandomizedLRUCache<CacheFormatItem, (bool Success, string ResultValue)>(1024);

            public static volatile ICustomCache customCache = null;
        }

        public static void SetCustomCache<T>(ICustomCache customCache)
            => GenericCacheHolder<T>.customCache = customCache;

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

            var customCache = GenericCacheHolder<T>.customCache;
            if (customCache != null)
            {
                if(customCache.Get(keyItem, out var formatSuccess, out var formatResult))
                {
                    formatted = formatResult;
                    return formatSuccess;
                }

                (formatSuccess, formatResult) = TryFormatInternal<T>(keyItem);
                customCache.Add(keyItem, formatSuccess, formatResult);
                formatted = formatResult;
                return formatSuccess;
            }

            var (cachedSuccess, cachedResultValue) = GenericCacheHolder<T>.Instance.Get(keyItem, TryFormatInternal<T>);

            formatted = cachedResultValue;
            return cachedSuccess;
        }

        private static (bool Success, string ResultValue) TryFormatInternal<T>(CacheFormatItem formatItem)
            where T : LanguageProvider
        {
            var format = formatItem.Format;
            var args = formatItem.Args;

            var complementaryChar = string.Concat(format, string.Concat(args)).GetComplementaryChar();
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

            var rawFormatted = formattedBuilder.ToString();
            Debug.Assert(!rawFormatted.Contains(complementaryChar.ToString()));

            if (LanguageService<T>.TrySanitize(rawFormatted, taintedRanges, out var formatted, out var sanitizedRanges))
            {
                if (LanguageService<T>.Validate(formatted, sanitizedRanges))
                {
                    return (true, formatted);
                }
            }

            return (false, null);
        }
    }
}
