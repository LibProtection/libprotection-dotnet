using System;
using System.Collections.Generic;

namespace LibProtection.Injections
{
    public abstract class RegexLanguageProvider : LanguageProvider
    {
        protected abstract Enum ErrorTokenType { get; }
        protected abstract IEnumerable<RegexRule> MainModeRules { get; }

        public override IEnumerable<Token> Tokenize(string text, int offset = 0)
        {
            var currentPosition = 0;
            var modeRulesStack = new Stack<IEnumerable<RegexRule>>();
            modeRulesStack.Push(MainModeRules);

            while (!string.IsNullOrEmpty(text))
            {
                var isMatched = false;

                foreach (var rule in modeRulesStack.Peek())
                {
                    if (rule.TryMatch(text, out var matchedLength) && matchedLength != 0)
                    {
                        isMatched = true;
                        if (rule.IsToken)
                        {
                            var tokenText = text.Substring(0, matchedLength);

                            var token = CreateToken(rule.Type, currentPosition + offset,
                                currentPosition + offset + tokenText.Length - 1, tokenText);

                            text = text.Substring(matchedLength);
                            currentPosition += matchedLength;
                            yield return token;
                        }

                        if (rule.IsPopMode) { modeRulesStack.Pop(); }
                        if (rule.IsPushMode) { modeRulesStack.Push(rule.ModeRules); }

                        break;
                    }
                }

                // Simply error-tolerance strategy: consider current char as error-token and move to next
                if (!isMatched)
                {
                    var token = CreateToken(ErrorTokenType, currentPosition + offset, currentPosition + offset,
                        text[0].ToString());

                    text = text.Substring(1);
                    currentPosition++;
                    yield return token;
                }
            }
        }
    }
}
