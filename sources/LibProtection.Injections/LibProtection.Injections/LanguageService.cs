using System.Collections.Generic;
using System.Text;

namespace LibProtection.Injections
{
    internal static class LanguageService<T> where T : LanguageProvider
    {
        public static bool TrySanitize(string text, List<Range> taintedRanges, out string sanitizedText)
        {
            sanitizedText = null;
            var sanitizedRanges = new List<Range>();
            var languageProvider = Single<T>.Instance;
            var tokens = languageProvider.Tokenize(text);
            var fragments = new Dictionary<Range, string>();

            // Try to sanitize all attacked text's fragments

            foreach (var tokensScope in GetTokensScopes(tokens, taintedRanges))
            {
                var range = tokensScope.Range;
                var fragment = text.Substring(range);

                fragments.Add(range,
                    languageProvider.TrySanitize(fragment, tokensScope.Tokens[0], out var sanitizedFragment)
                        ? sanitizedFragment
                        : fragment);
            }

            // Replace all attacked text's fragments with corresponding sanitized values

            var positionAtText = 0;
            var sanitizedBuilder = new StringBuilder();

            foreach (var range in fragments.Keys)
            {
                var charsToAppend = range.LowerBound - positionAtText;
                sanitizedBuilder.Append(text, positionAtText, charsToAppend);
                var lowerBound = sanitizedBuilder.Length;
                sanitizedBuilder.Append(fragments[range]);
                sanitizedRanges.Add(new Range(lowerBound, sanitizedBuilder.Length - 1));
                positionAtText = range.UpperBound + 1;
            }

            if (positionAtText < text.Length)
            {
                sanitizedBuilder.Append(text, positionAtText, text.Length - positionAtText);
                
            }

            sanitizedText = sanitizedBuilder.ToString();
            return Validate(sanitizedText, sanitizedRanges);
        }

        public static bool Validate(string text, List<Range> ranges)
        {
            var languageProvider = Single<T>.Instance;
            var tokens = languageProvider.Tokenize(text);

            var scopesCount = 0;
            var allTrivial = true;

            foreach (var scope in GetTokensScopes(tokens, ranges))
            {
                scopesCount++;
                allTrivial &= scope.IsTrivial;
                if ((scope.Tokens.Count > 1 ||  scopesCount > 1) && !allTrivial) { return false; }
            }

            return true;
        }

        private static IEnumerable<TokenScope> GetTokensScopes(IEnumerable<Token> tokens, List<Range> ranges)
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