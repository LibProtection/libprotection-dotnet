using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibProtection.Injections
{
    internal struct SanitizeResult
    {
        public bool Success { get; set; }
        public Token[] Tokens { get; set; }
        public string SanitizedText { get; set; }
        public Token AttackToken { get; set; }
    }

    internal static class LanguageService
    {
        /// <summary>
        /// Try to sanitize or 
        /// </summary>
        /// <param name="text">Text to sanitize</param>
        /// <param name="taintedRanges">ranges of taint</param>
        /// <param name="tokens">Artifact tokens</param>
        /// <param name="sanitizedText">Sanitized text if success</param>
        /// <param name="attackToken">Attack token if failed</param>
        /// <returns>success of sanitize</returns>
        public static SanitizeResult TrySanitize(LanguageProvider languageProvider, string text, List<Range> taintedRanges)
        {
            var sanitizedRanges = new List<Range>();

            if (languageProvider.GetType() == typeof(Html))
            {
                text = Html.HtmlUnescape(text, ref taintedRanges);
            }

            var tokens = languageProvider.Tokenize(text).ToArray();
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
                sanitizedRanges.Add(new Range(lowerBound, sanitizedBuilder.Length));
                positionAtText = range.UpperBound;
            }

            if (positionAtText < text.Length)
            {
                sanitizedBuilder.Append(text, positionAtText, text.Length - positionAtText);
                
            }

            var sanitizedText = sanitizedBuilder.ToString();
            var success = Validate(languageProvider, sanitizedText, sanitizedRanges, out var attackToken);

            return new SanitizeResult
            {
                Success = success,
                AttackToken = success ? null : attackToken,
                SanitizedText = success ? sanitizedText : null,
                Tokens = tokens,
            };
        }

        public static bool Validate(LanguageProvider languageProvider, string text, List<Range> ranges, out Token attackToken)
        {
            var tokens = languageProvider.Tokenize(text);

            var scopesCount = 0;
            var allTrivial = true;

            foreach (var scope in GetTokensScopes(tokens, ranges))
            {
                scopesCount++;
                allTrivial &= scope.IsTrivial;
                if ((scope.Tokens.Count > 1 ||  scopesCount > 1) && !allTrivial) {
                    attackToken = scope.Tokens.Find(token => !token.IsTrivial);
                    return false;
                }
            }

            attackToken = null;
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