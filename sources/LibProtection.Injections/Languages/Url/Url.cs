using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace LibProtection.Injections
{
    public sealed class Url : RegexLanguageProvider
    {
        protected override Enum ErrorTokenType { get; } = UrlTokenType.Error;

        protected override IEnumerable<RegexTokenDefinition> TokenDefinitions { get; } = new[]
        {
            new RegexTokenDefinition(@"[^:/?#]+:", UrlTokenType.SchemeCtx),
            new RegexTokenDefinition(@"//[^/?#]*", UrlTokenType.AuthorityCtx),
            new RegexTokenDefinition(@"[^?#]*", UrlTokenType.PathCtx),
            new RegexTokenDefinition(@"\?[^#]*", UrlTokenType.QueryCtx),
            new RegexTokenDefinition(@"#.*", UrlTokenType.FragmentCtx)
        };

        private Url() { }

        public override IEnumerable<Token> Tokenize(string text, int offset = 0)
        {
            foreach (var token in base.Tokenize(text, offset))
            {
                var tokenText = token.Text;
                var lowerBound = token.Range.LowerBound;

                switch ((UrlTokenType) token.Type)
                {
                    case UrlTokenType.SchemeCtx:
                        foreach (var subToken in SplitToken(tokenText, lowerBound, ":", UrlTokenType.Scheme))
                        {
                            yield return subToken;
                        }
                        break;
                    
                    case UrlTokenType.AuthorityCtx:
                        foreach (var subToken in SplitToken(tokenText, lowerBound, "\\/:@", UrlTokenType.AuthorityEntry))
                        {
                            yield return subToken;
                        }
                        break;

                    case UrlTokenType.PathCtx:
                        foreach (var subToken in SplitToken(tokenText, lowerBound, "\\/", UrlTokenType.PathEntry))
                        {
                            yield return subToken;
                        }
                        break;

                    case UrlTokenType.QueryCtx:
                        foreach (var subToken in SplitToken(tokenText, lowerBound, "?&=", UrlTokenType.QueryEntry))
                        {
                            yield return subToken;
                        }
                        break;
                    
                    case UrlTokenType.FragmentCtx:
                        foreach (var subToken in SplitToken(tokenText, lowerBound, "#", UrlTokenType.Fragment))
                        {
                            yield return subToken;
                        }
                        break;

                    default:
                        yield return token;
                        break;
                }
            }
        }

        public override bool TrySanitize(string text, Token context, out string sanitized)
        {
            switch (context.LanguageProvider)
            {
                case Url _:
                    if (TryUrlEncode(text, (UrlTokenType) context.Type, out sanitized))
                    {
                        return true;
                    }
                    break;

                default:
                    throw new ArgumentException($"Unsupported URL island: {context}");
            }

            sanitized = null;
            return false;
        }

        protected override bool IsTrivial(Enum type, string text)
        {
            switch ((UrlTokenType) type)
            {
                case UrlTokenType.QueryEntry:
                case UrlTokenType.Fragment:
                    return true;

                case UrlTokenType.PathEntry:
                    return !text.Contains("..");
            }

            return false;
        }

        private IEnumerable<Token> SplitToken(string text, int lowerBound, string splitChars, UrlTokenType tokenType)
        {
            if (string.IsNullOrEmpty(text)) { yield break; }
            var tokenTextBuilder = new StringBuilder();

            foreach (var currentChar in text)
            {
                if (splitChars.Contains(currentChar.ToString()))
                {
                    if (tokenTextBuilder.Length != 0)
                    {
                        var tokenText = tokenTextBuilder.ToString();
                        tokenTextBuilder.Clear();
                        var upperBound = lowerBound + tokenText.Length - 1;
                        yield return CreateToken(tokenType, lowerBound, upperBound, tokenText);
                        lowerBound = upperBound + 1;
                    }

                    yield return CreateToken(UrlTokenType.Separator, lowerBound, lowerBound, currentChar.ToString());
                    lowerBound++;
                }
                else
                {
                    tokenTextBuilder.Append(currentChar);
                }
            }

            if (tokenTextBuilder.Length != 0)
            {
                var lastTokenText = tokenTextBuilder.ToString();
                yield return CreateToken(tokenType, lowerBound, lowerBound + lastTokenText.Length - 1, lastTokenText);
            }
        }

        private static bool TryUrlEncode(string text, UrlTokenType tokenType, out string encoded)
        {
            encoded = null;

            switch (tokenType)
            {
                case UrlTokenType.PathEntry:
                    var fragments = text.Split('/');

                    for (var i = 0; i < fragments.Length; i++)
                    {
                        if (fragments[i] != string.Empty)
                        {
                            fragments[i] = HttpUtility.UrlEncode(fragments[i]);
                        }
                    }

                    encoded = string.Join("/", fragments);
                    return true;

                case UrlTokenType.QueryEntry:
                case UrlTokenType.Fragment:
                    encoded = HttpUtility.UrlEncode(text);
                    return true;

            }

            return false;
        }
    }
}
