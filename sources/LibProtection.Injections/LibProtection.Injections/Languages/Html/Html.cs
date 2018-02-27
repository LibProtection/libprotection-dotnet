using System;
using System.Collections.Generic;
using System.Web;
using Antlr4.Runtime;
using LibProtection.Injections.Internals;

namespace LibProtection.Injections
{
    public sealed class Html : AntlrLanguageProvider
    {
        private enum HtmlTokenizerState
        {
            Insignificant,
            EventName,
            EventEqualSign,
            EventValue,
            ResourceName,
            ResourceEqualSign,
            ResourceValue
        }

        private readonly HashSet<string> _htmlUrlAttributes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "href", "src", "manifest", "poster", "code", "codebase", "data", "xlink:href", "xml:base", "from", "to",
            "formaction", "action", "dynsrc", "lowsrc"
        };

        private Html() { }

        protected override Enum ConvertAntlrTokenType(int antlrTokenType)
        {
            return (HtmlTokenType)antlrTokenType;
        }

        protected override Lexer CreateLexer(string text)
        {
            return new HTMLLexer(new AntlrInputStream(text));
        }

        public override IEnumerable<Token> Tokenize(string text, int offset = 0)
        {
            var state = HtmlTokenizerState.Insignificant;
            var insideScriptTag = false;

            foreach (var token in base.Tokenize(text, offset))
            {
                if (token.LanguageProvider is Html)
                {
                    var htmlTokenType = (HtmlTokenType)token.Type;
                    switch (state)
                    {
                        case HtmlTokenizerState.EventName:
                            state = htmlTokenType == HtmlTokenType.TagEquals
                            ? HtmlTokenizerState.EventEqualSign
                            : HtmlTokenizerState.Insignificant;
                            break;

                        case HtmlTokenizerState.EventEqualSign:
                            state = htmlTokenType == HtmlTokenType.AttributeValue
                                ? HtmlTokenizerState.EventValue
                                : HtmlTokenizerState.Insignificant;
                            break;

                        case HtmlTokenizerState.ResourceName:
                            state = htmlTokenType == HtmlTokenType.TagEquals
                                ? HtmlTokenizerState.ResourceEqualSign
                                : HtmlTokenizerState.Insignificant;
                            break;

                        case HtmlTokenizerState.ResourceEqualSign:
                            state = htmlTokenType == HtmlTokenType.AttributeValue
                                ? HtmlTokenizerState.ResourceValue
                                : HtmlTokenizerState.Insignificant;
                            break;

                        default:
                            state = HtmlTokenizerState.Insignificant;
                            switch (htmlTokenType)
                            {
                                case HtmlTokenType.AttributeName:
                                    if (token.Text.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                                    {
                                        state = HtmlTokenizerState.EventName;
                                    }
                                    else if (_htmlUrlAttributes.Contains(token.Text))
                                    {
                                        state = HtmlTokenizerState.ResourceName;
                                    }
                                    break;

                                case HtmlTokenType.TagOpen:
                                    if (string.Equals(token.Text, "<script", StringComparison.OrdinalIgnoreCase))
                                    {
                                        insideScriptTag = true;
                                    }
                                    else
                                    if (string.Equals(token.Text, "</script", StringComparison.OrdinalIgnoreCase))
                                    {
                                        insideScriptTag = false;
                                    }

                                    break;
                            }

                            break;
                    }

                    if (IsContextChanged(token, state, insideScriptTag, out var islandData))
                    {
                        var islandTokens = islandData.LanguageProvider.Tokenize(islandData.Text, islandData.Offset);
                        foreach (var islandToken in islandTokens)
                        {
                            yield return islandToken;
                        }
                    }
                    else
                    {
                        yield return token;
                    }
                }
            }
        }

        private static bool IsContextChanged(Token htmlToken, HtmlTokenizerState context, bool insideScriptTag, 
            out IslandDto islandData)
        {
            islandData = null;

            switch (context)
            {
                case HtmlTokenizerState.EventValue:
                    {
                        var islandText = TrimQuotes(htmlToken, out var offset);

                        if (!string.IsNullOrEmpty(islandText))
                        {
                            islandData = new IslandDto(Single<JavaScript>.Instance, offset, islandText);
                        }

                        break;
                    }

                case HtmlTokenizerState.ResourceValue:
                    {
                        var islandText = TrimQuotes(htmlToken, out var offset);

                        if (!string.IsNullOrEmpty(islandText))
                        {
                            islandData = new IslandDto(Single<Url>.Instance, offset, islandText);
                        }

                        break;
                    }

                default:
                    var htmlTokenType = (HtmlTokenType)htmlToken.Type;

                    if (insideScriptTag)
                    {
                        switch (htmlTokenType)
                        {
                            case HtmlTokenType.HtmlText:
                                islandData = new IslandDto(Single<JavaScript>.Instance, htmlToken.Range.LowerBound,
                                    htmlToken.Text);
                                break;

                            case HtmlTokenType.HtmlComment:
                                // TODO: implement
                                break;
                        }
                    }

                    break;
            }

            return islandData != null;
        }

        private static string TrimQuotes(Token token, out int offset)
        {
            var tokenText = token.Text;
            offset = token.Range.LowerBound;

            if (tokenText.Length == 0) { return string.Empty; }

            if (tokenText[0] == '\'' && tokenText[tokenText.Length - 1] == '\'' ||
                tokenText[0] == '"' && tokenText[tokenText.Length - 1] == '"')
            {
                if (tokenText.Length > 2)
                {
                    tokenText = tokenText.Substring(1, tokenText.Length - 2);
                    offset++;
                }
            }

            return tokenText;
        }

        public override bool TrySanitize(string text, Token context, out string sanitized)
        {
            switch (context.LanguageProvider)
            {
                case Html _:
                    sanitized = HtmlEncode(text, (HtmlTokenType)context.Type);
                    return true;

                case Url _:
                    if (Single<Url>.Instance.TrySanitize(text, context, out var urlSanitized))
                    {
                        sanitized = HtmlEncode(urlSanitized, HtmlTokenType.AttributeValue);
                        return true;
                    }
                    break;

                case JavaScript _:
                    if (Single<JavaScript>.Instance.TrySanitize(text, context, out var ecmaScriptSanitized))
                    {
                        sanitized = HtmlEncode(ecmaScriptSanitized, HtmlTokenType.HtmlText);
                        return true;
                    }
                    break;

                default:
                    throw new ArgumentException($"Unsupported HTML island: {context}");
            }

            sanitized = null;
            return false;
        }

        protected override bool IsTrivial(Enum type, string text)
        {
            switch ((HtmlTokenType) type)
            {
                case HtmlTokenType.HtmlComment:
                case HtmlTokenType.HtmlConditionalComment:
                case HtmlTokenType.HtmlText:
                case HtmlTokenType.Cdata:
                case HtmlTokenType.AttributeWhiteSpace:
                case HtmlTokenType.AttributeValue:
                    return true;
            }

            return false;
        }

        private static string HtmlEncode(string text, HtmlTokenType tokenType)
        {
            switch (tokenType)
            {
                case HtmlTokenType.AttributeValue:
                    return HttpUtility.HtmlAttributeEncode(text);

                default:
                    return HttpUtility.HtmlEncode(text);
            }
        }
    }
}