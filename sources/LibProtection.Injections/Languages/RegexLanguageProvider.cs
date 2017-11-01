using System;
using System.Collections.Generic;

namespace LibProtection.Injections
{
    public abstract class RegexLanguageProvider : LanguageProvider
    {
        protected abstract Enum ErrorTokenType { get; }
        protected abstract IEnumerable<RegexTokenDefinition> TokenDefinitions { get; }

        public override IEnumerable<Token> Tokenize(string text, int offset = 0)
        {
            var currentPosition = 0;

            while (!string.IsNullOrEmpty(text))
            {
                var isMatched = false;

                foreach (var tokenDefinition in TokenDefinitions)
                {
                    if (tokenDefinition.TryMatch(text, out var matchedLength) && matchedLength != 0)
                    {
                        isMatched = true;
                        var tokenText = text.Substring(0, matchedLength);

                        var token = CreateToken(tokenDefinition.Type, currentPosition + offset,
                            currentPosition + offset + tokenText.Length - 1, tokenText);

                        text = text.Substring(matchedLength);
                        currentPosition += matchedLength;
                        yield return token;
                        break;
                    }
                }

                // Simply error-tolerance strategy: consider current char as error-token and move to next
                if (!isMatched)
                {
                    var token = CreateToken(ErrorTokenType, currentPosition + offset, currentPosition + offset,
                        text.Substring(0, 1));

                    text = text.Substring(1);
                    currentPosition++;
                    yield return token;
                }
            }
        }
    }
}
