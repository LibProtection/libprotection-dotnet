using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LibProtection.Injections
{
    public sealed class RegexRule
    {
        public Enum Type { get; private set; }
        public IEnumerable<RegexRule> ModeRules { get; private set; }
        public bool IsPushMode => ModeRules != null;
        public bool IsPopMode { get; private set; }
        public bool IsToken => Type != null;

        private readonly Regex _regex;

        private RegexRule(string regex)
        {
            _regex = new Regex($"^{regex}", RegexOptions.Compiled);
        }

        public bool TryMatch(string text, out int length)
        {
            var matchResult = _regex.Match(text);
            length = matchResult.Success ? matchResult.Length : 0;
            return matchResult.Success;
        }

        public static RegexRule Token(string regex, Enum type)
        {
            return new RegexRule(regex) { Type = type };
        }

        public static RegexRule TokenPushMode(string regex, Enum type, IEnumerable<RegexRule> modeRules)
        {
            return new RegexRule(regex) {Type = type, ModeRules = modeRules};
        }

        public static RegexRule TokenPopMode(string regex, Enum type)
        {
            return new RegexRule(regex) { Type = type, IsPopMode = true};
        }

        public static RegexRule NoToken(string regex)
        {
            return new RegexRule(regex);
        }

        public static RegexRule NoTokenPushMode(string regex, IEnumerable<RegexRule> modeRules)
        {
            return new RegexRule(regex) { ModeRules =  modeRules };
        }

        public static RegexRule NoTokenPopMode(string regex)
        {
            return new RegexRule(regex) { IsPopMode = true };
        }
    }
}
