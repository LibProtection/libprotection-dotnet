using System;
using System.Collections.Generic;
using System.Web;

namespace LibProtection.Injections
{
    public sealed class Url : RegexLanguageProvider
    {
        protected override Enum ErrorTokenType { get; } = UrlTokenType.Error;

        protected override IEnumerable<RegexRule> MainModeRules { get; } = new[]
        {
            RegexRule.NoTokenPushMode(@"[^:/?#]+:", SchemeModeRules),
            RegexRule.NoTokenPushMode(@"//[^/?#]*", AuthorityModeRules),
            RegexRule.NoTokenPushMode(@"[^?#]*",    PathModeRules),
            RegexRule.NoTokenPushMode(@"\?[^#]*",   QueryModeRules),
            RegexRule.NoTokenPushMode(@"#.*",       FragmentModeRules)
        };

        private static readonly IEnumerable<RegexRule> SchemeModeRules = new[]
        {
            RegexRule.Token("[^:]+", UrlTokenType.Scheme),
            RegexRule.TokenPopMode(":", UrlTokenType.Separator)
        };

        private static readonly IEnumerable<RegexRule> AuthorityModeRules = new[]
        {
            RegexRule.Token("//", UrlTokenType.Separator),
            RegexRule.Token("[^/@:]+", UrlTokenType.AuthorityEntry),
            RegexRule.Token("[:@]", UrlTokenType.Separator),
            RegexRule.NoTokenPopMode("/")
        };

        private static readonly IEnumerable<RegexRule> PathModeRules = new[]
        {
            RegexRule.Token("/", UrlTokenType.Separator),
            RegexRule.Token("[^/?#]+", UrlTokenType.PathEntry),
            RegexRule.NoTokenPopMode("[?#]")
        };

        private static readonly IEnumerable<RegexRule> QueryModeRules = new[]
        {
            RegexRule.Token("\\?", UrlTokenType.Separator),
            RegexRule.Token("[^?/=&#]+", UrlTokenType.QueryEntry),
            RegexRule.Token("[=&]", UrlTokenType.Separator),
            RegexRule.NoTokenPopMode("#"), 
        };

        private static readonly IEnumerable<RegexRule> FragmentModeRules = new[]
        {
            RegexRule.Token("#", UrlTokenType.Separator),
            RegexRule.TokenPopMode("[^#]*", UrlTokenType.Fragment), 
        };

        private Url() { }

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
