using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibProtection.Injections
{
    internal static class LanguageService<T> where T : LanguageProvider
    {
        public static bool TrySanitize(string text, List<Range> taintedRanges, out string sanitizedText,
            out List<Range> sanitizedRanges)
        {
            sanitizedText = null;
            sanitizedRanges = new List<Range>();
            var languageProvider = Single<T>.Instance;
            var tokens = languageProvider.Tokenize(text);
            var sanitizedFragments = new Dictionary<Range, string>();

            // Try to sanitize all attacked text's fragments

            foreach (var tokensScope in GetTokensScopes(tokens, taintedRanges))
            {
                if (tokensScope.Tokens.Count < 2)
                {
                    continue;
                }

                var range = tokensScope.Range;
                var fragment = text.Substring(range);

                if (languageProvider.TrySanitize(fragment, tokensScope.Tokens[0], out var sanitizedFragment))
                {
                    sanitizedFragments.Add(range, sanitizedFragment);
                }
                else
                {
                    return false;
                }
            }

            // Replace all attacked text's fragments with corresponding sanitized values

            var positionAtText = 0;
            var sanitizedBuilder = new StringBuilder();

            foreach (var range in sanitizedFragments.Keys)
            {
                var charsToAppend = range.LowerBound - positionAtText;
                sanitizedBuilder.Append(text.Substring(positionAtText, charsToAppend));
                var lowerBound = sanitizedBuilder.Length;
                sanitizedBuilder.Append(sanitizedFragments[range]);
                sanitizedRanges.Add(new Range(lowerBound, sanitizedBuilder.Length - 1));
                positionAtText = range.UpperBound + 1;
            }

            if (positionAtText < text.Length)
            {
                sanitizedBuilder.Append(text.Substring(positionAtText, text.Length - positionAtText));
                
            }

            sanitizedText = sanitizedBuilder.ToString();
            return true;
        }

        public static bool Validate(string text, List<Range> ranges)
        {
            var languageProvider = Single<T>.Instance;
            var tokens = languageProvider.Tokenize(text);

            var scopesCount = 0;

            foreach (var scope in GetTokensScopes(tokens, ranges))
            {
                // Vanile injection
                if (scope.Tokens.Count != 1) { return false; }

                // Fragmented injection
                scopesCount++;
                if (scopesCount > 1 && !scope.Tokens.All(token => token.IsSafe)) { return false; }
            }

            return true;
        }

        private static IEnumerable<TokenScope> GetTokensScopes(IEnumerable<Token> tokens, IReadOnlyCollection<Range> ranges)
        {
            var scopesMap = new Dictionary<Range, TokenScope>();

            foreach (var token in tokens)
            {
                foreach (var range in ranges)
                {
                    if (range.Overlaps(token.Range))
                    {
                        if (!scopesMap.TryGetValue(range, out var tokensScope))
                        {
                            tokensScope = new TokenScope(range);
                            scopesMap.Add(range, tokensScope);
                        }

                        tokensScope.Tokens.Add(token);
                    }
                }
            }

            return scopesMap.Values;
        }
    }
}