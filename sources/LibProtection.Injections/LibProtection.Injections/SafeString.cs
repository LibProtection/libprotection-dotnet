using LibProtection.Injections.Formatting;
using System.Diagnostics;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo(assemblyName: "LibProtection.Injections.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100a94dafa0c51a7dded3c5bc2666186c18f1a98b514fff0549778b7cce9bff7fd7e0cb63eae03e27001132bf6c561038c54c0cbad6455017302847b4ee56b839fd7923f5d3792d85c0e51fd6268f7b2a1d61df8dcdac522c082a1bb1be2afe5a8bedc9a407cb2d7a7162ed64e38a6b8e06bae07077c43634bb343f0be7697b70b4")]

namespace LibProtection.Injections
{
    public static partial class SafeString<T> where T : LanguageProvider
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly RandomizedLRUCache<FormatCacheItem, FormatResult> cache
            = new RandomizedLRUCache<FormatCacheItem, FormatResult>(1024);

        public static string Format(string format, params object[] args) 
        {
            if (TryFormat(format, out var formatted, args))
            {
                return formatted;
            }
            
            throw new AttackDetectedException();
        }

        public static bool TryFormat(string format, out string formatted, params object[] args) 
        {
            var formatResult = FormatEx(format, args);
            formatted = formatResult.IsAttackDetected ? null : formatResult.FormattedString;
            return !formatResult.IsAttackDetected;
        }

        internal static FormatResult FormatEx(string format, params object[] args)
            => cache.Get(new FormatCacheItem(format, args), FormatExImpl);

        private static FormatResult FormatExImpl(FormatCacheItem formatItem)
        {
            var formattedString = Formatter.Format(formatItem.Format, formatItem.Args, out var taintedRanges, out var associatedToRangeIndexes);
            var sanitizeResult = LanguageService.TrySanitize(Single<T>.Instance, formattedString, taintedRanges);

            if (sanitizeResult.Success)
            {
                return FormatResult.Success(sanitizeResult.Tokens, sanitizeResult.SanitizedText);
            }
            else
            {
                var attackArgumentIndex = taintedRanges.FindIndex(range => range.Overlaps(sanitizeResult.AttackToken.Range));
                Debug.Assert(attackArgumentIndex != -1, "Cannot find attack argument for attack token.");
                return FormatResult.Fail(sanitizeResult.Tokens, associatedToRangeIndexes[attackArgumentIndex]);
            }
        }
    }
}