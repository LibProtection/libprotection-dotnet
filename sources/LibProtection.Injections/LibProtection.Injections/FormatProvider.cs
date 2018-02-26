using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LibProtection.Injections
{
    internal class FormatProvider<T> : IFormatProvider where T: LanguageProvider
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly RandomizedLRUCache<FormatCacheItem, Option<string>> Cache
            = new RandomizedLRUCache<FormatCacheItem, Option<string>>(1024);

        private readonly Formatter<T> _formatter;
        private readonly List<Fragment> _formattedFragments = new List<Fragment>();

        private FormatProvider(char complementaryChar)
        {
            _formatter = new Formatter<T>(complementaryChar);
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? _formatter : null;
        }

        public void AddFragment(Fragment fragment)
        {
            _formattedFragments.Add(fragment);
        }

        public static bool TryFormat(string format, out string formatted, object[] args)
        {
            var keyItem = new FormatCacheItem(format, args);
            var cacheOption = Cache.Get(keyItem, TryFormatInternal);
            formatted = cacheOption.Value;
            return cacheOption.HasValue;
        }

        private static Option<string> TryFormatInternal(FormatCacheItem formatItem)
        {
            var format = formatItem.Format;
            var args = formatItem.Args;

            var complementaryChar = format.GetComplementaryChar();
            var formatProvider = new FormatProvider<T>(complementaryChar);
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

            return LanguageService<T>.TrySanitize(formatted, taintedRanges, out var sanitized)
                ? new Option<string>(sanitized)
                : Option<string>.None;
        }
    }
}
