using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Antlr4.Runtime;
using LibProtection.Injections.Internals;

namespace LibProtection.Injections
{
    public sealed class Html : AntlrLanguageProvider
    {
        private enum HtmlTokenizerContext
        {
            Insignificant,
            EventName,
            EventEqualSign,
            EventValue,
            ResourceName,
            ResourceEqualSign,
            ResourceValue
        }

        private const string LanguageNotSupportedTemplate = "{0} language is not currently supported";

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
            var context = HtmlTokenizerContext.Insignificant;

            foreach (var token in base.Tokenize(text, offset))
            {
                if (token.LanguageProvider is Html)
                {
                    var htmlTokenType = (HtmlTokenType)token.Type;
                    switch (context)
                    {
                        case HtmlTokenizerContext.EventName:
                            context = htmlTokenType == HtmlTokenType.TagEquals
                            ? HtmlTokenizerContext.EventEqualSign
                            : HtmlTokenizerContext.Insignificant;
                            break;

                        case HtmlTokenizerContext.EventEqualSign:
                            context = htmlTokenType == HtmlTokenType.AttvalueValue
                                ? HtmlTokenizerContext.EventValue
                                : HtmlTokenizerContext.Insignificant;
                            break;

                        case HtmlTokenizerContext.ResourceName:
                            context = htmlTokenType == HtmlTokenType.TagEquals
                                ? HtmlTokenizerContext.ResourceEqualSign
                                : HtmlTokenizerContext.Insignificant;
                            break;

                        case HtmlTokenizerContext.ResourceEqualSign:
                            context = htmlTokenType == HtmlTokenType.AttvalueValue
                                ? HtmlTokenizerContext.ResourceValue
                                : HtmlTokenizerContext.Insignificant;
                            break;

                        default:
                            context = HtmlTokenizerContext.Insignificant;
                            if (htmlTokenType == HtmlTokenType.TagName)
                            {
                                var tokenLoweredText = token.Text.ToLowerInvariant();

                                if (tokenLoweredText == "style")
                                {
                                    throw new LanguageNotSupportedException(string.Format(LanguageNotSupportedTemplate,
                                        "CSS"));
                                }

                                if (tokenLoweredText.StartsWith("on"))
                                {
                                    context = HtmlTokenizerContext.EventName;
                                }
                                else if (new[] { "href", "src", "manifest", "poster", "code", "codebase", "data" }
                                    .Contains(tokenLoweredText))
                                {
                                    context = HtmlTokenizerContext.ResourceName;
                                }
                            }

                            break;
                    }

                    if (IsContextChanged(token, context, out var islandData))
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

        private static bool IsContextChanged(Token htmlToken, HtmlTokenizerContext context, out IslandDto islandData)
        {
            islandData = null;

            switch (context)
            {
                case HtmlTokenizerContext.EventValue:
                    {
                        var islandText = TrimQuotes(htmlToken, out var offset);

                        if (!string.IsNullOrEmpty(islandText))
                        {
                            islandData = new IslandDto(Single<JavaScript>.Instance, offset, islandText);
                        }

                        break;
                    }

                case HtmlTokenizerContext.ResourceValue:
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

                    if (htmlTokenType == HtmlTokenType.StyleBody || htmlTokenType == HtmlTokenType.StyleShortBody)
                    {
                        throw new LanguageNotSupportedException(string.Format(LanguageNotSupportedTemplate, "CSS"));
                    }

                    if (htmlTokenType == HtmlTokenType.ScriptBody || htmlTokenType == HtmlTokenType.ScriptShortBody)
                    {
                        // `</script>` or `</>` case
                        // Remove trailing tag
                        var closingTagIndex = htmlToken.Text.Length - 2;

                        while (htmlToken.Text.Substring(closingTagIndex, 2) != "</" && closingTagIndex >= 0)
                        {
                            closingTagIndex--;
                        }

                        var islandText = closingTagIndex >= 0
                            ? htmlToken.Text.Substring(0, closingTagIndex)
                            : htmlToken.Text;

                        islandData = new IslandDto(Single<JavaScript>.Instance,
                            htmlToken.Range.LowerBound, islandText);
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
                        sanitized = HtmlEncode(urlSanitized, HtmlTokenType.Attribute);
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
                case HtmlTokenType.SeaWs:
                
                case HtmlTokenType.HtmlComment:
                case HtmlTokenType.HtmlConditionalComment:
                    
                case HtmlTokenType.HtmlText:
                case HtmlTokenType.TagName:
                case HtmlTokenType.Attribute:
                    return true;
            }

            return false;
        }

        private static string HtmlEncode(string text, HtmlTokenType tokenType)
        {
            switch (tokenType)
            {
                case HtmlTokenType.AttvalueValue:
                case HtmlTokenType.Attribute:
                case HtmlTokenType.ErrorAttvalue:
                    return HttpUtility.HtmlAttributeEncode(text);

                default:
                    return HttpUtility.HtmlEncode(text);
            }
        }
    }
}