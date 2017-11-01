using System.Collections;

namespace LibProtection.Injections
{
    internal static class StringExtensions
    {
        public static char GetComplementaryChar(this string s)
        {
            var min = char.MaxValue;
            var max = char.MinValue;

            var presenceMap = new BitArray(char.MaxValue);

            foreach (var c in s)
            {
                presenceMap[c] = true;
                if (c < min)
                {
                    min = c;
                }
                if (c > max)
                {
                    max = c;
                }
            }

            if (min > char.MinValue)
            {
                return (char) (min - 1);
            }
            if (max < char.MaxValue)
            {
                return (char) (max + 1);
            }

            for (var i = char.MinValue; i < char.MaxValue; i++)
            {
                if (!presenceMap[i])
                {
                    return i;
                }
            }

            return char.MaxValue;
        }

        public static string Substring(this string text, Range range)
        {
            return text.Substring(range.LowerBound, range.Length);
        }
    }
}