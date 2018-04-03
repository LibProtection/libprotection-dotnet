using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LibProtection.Injections
{
    internal class FormatProvider<T> : IFormatProvider where T: LanguageProvider
    {
        private class RangeForArgumentIndex
        {
            public int Index { get; set; }
            public Range Range { get; set; }
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly RandomizedLRUCache<FormatCacheItem, FormatResult> Cache
            = new RandomizedLRUCache<FormatCacheItem, FormatResult>(1024);

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

        public static FormatResult TryFormat(string format, object[] args)
        {
            var keyItem = new FormatCacheItem(format, args);
            var cacheOption = Cache.Get(keyItem, TryFormatInternal);
            return cacheOption;
        }

        private static FormatResult TryFormatInternal(FormatCacheItem formatItem)
        {
            var format = formatItem.Format;
            var args = formatItem.Args;

            var complementaryChar = format.GetComplementaryChar();
            var formatProvider = new FormatProvider<T>(complementaryChar);
            //wrap arguments to memorize argument index
            var wrappedArguments = new ArgumentWrapper[args.Length];
            for(int i = 0; i < args.Length; i++)
            {
                wrappedArguments[i] = new ArgumentWrapper
                {
                    Argument = args[i],
                    Index = i,
                };
            }
            
            var formattedBuilder = new StringBuilder(string.Format(formatProvider, format, wrappedArguments));
            var taintedRanges = new List<Range>();
            var argumentRanges = new List<RangeForArgumentIndex>();
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
                    if (!currentFragment.IsSafe) {
                        var currentRange = new Range(lowerBound, upperBound);
                        taintedRanges.Add(currentRange);
                        argumentRanges.Add(new RangeForArgumentIndex
                        {
                            Index = currentFragment.FragmentArgumentIndex,
                            Range = currentRange,
                        });
                    }
                    currentFragmentIndex++;
                }
                else
                {
                    currentCharIndex++;
                }
            }

            var formatted = formattedBuilder.ToString();

            var sanitizeResult = LanguageService<T>.TrySanitize(formatted, taintedRanges);
            if (sanitizeResult.Success)
            {
                return FormatResult.Success(sanitizeResult.Tokens, sanitizeResult.SanitizedText);
            }
            else
            {
                var attackArgument = argumentRanges.Find(argumentRange => argumentRange.Range.Overlaps(sanitizeResult.AttackToken.Range));
                Debug.Assert(attackArgument != null, "Cannot find attack argument for attack token.");
                return FormatResult.Fail(sanitizeResult.Tokens, attackArgument.Index);
            }
        }
    }
}
