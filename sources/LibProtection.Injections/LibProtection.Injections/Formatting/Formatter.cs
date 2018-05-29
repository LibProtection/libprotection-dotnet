using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LibProtection.Injections.Formatting
{
    internal class Formatter : IFormatProvider
    {
        private readonly ComplementaryFormatter _formatter;
        private readonly List<Fragment> _formattedFragments = new List<Fragment>();

        private Formatter(char complementaryChar)
        {
            _formatter = new ComplementaryFormatter(complementaryChar);
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? _formatter : null;
        }

        public void AddFragment(Fragment fragment)
        {
            _formattedFragments.Add(fragment);
        }

        public static string Format(string format, object[] args, out List<Range> taintedRanges, out List<int> associatedToRangeIndexes)
        {
            taintedRanges = new List<Range>();
            associatedToRangeIndexes = new List<int>();

            var complementaryChar = format.GetComplementaryChar();
            var formatProvider = new Formatter(complementaryChar);
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

                    if (!currentFragment.IsSafe) {
                        var upperBound = currentCharIndex - 1;
                        var currentRange = new Range(lowerBound, upperBound);
                        taintedRanges.Add(currentRange);
                        associatedToRangeIndexes.Add(currentFragment.FragmentArgumentIndex);
                    }
                    currentFragmentIndex++;
                }
                else
                {
                    currentCharIndex++;
                }
            }

            return formattedBuilder.ToString();
        }
    }
}
