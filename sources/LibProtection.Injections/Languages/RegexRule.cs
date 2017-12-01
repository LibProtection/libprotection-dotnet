using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LibProtection.Injections
{
    public sealed class RegexRule
    {
        public Enum Type { get; }
        public IEnumerable<RegexRule> ModeRules { get; }
        public bool IsPushMode => ModeRules != null;
        public bool IsPopMode { get; }
        public bool IsToken => Type != null;

        private readonly Regex _regex;

        private RegexRule(string regex, bool isPopMode = false)
        {
            _regex = new Regex($"^{regex}", RegexOptions.Compiled);
            IsPopMode = isPopMode;
        }

        private RegexRule(string regex, Enum type, bool isPopMode = false) : this(regex, isPopMode)
        {
            Type = type;
            IsPopMode = isPopMode;
        }

        private RegexRule(string regex, Enum type, IEnumerable<RegexRule> modeRules) : this(regex, type)
        {
            ModeRules = modeRules;
        }

        private RegexRule(string regex, IEnumerable<RegexRule> modeRules) : this(regex)
        {
            ModeRules = modeRules;
        }

        public bool TryMatch(string text, out int length)
        {
            var matchResult = _regex.Match(text);
            length = matchResult.Success ? matchResult.Length : 0;
            return matchResult.Success;
        }

        public static RegexRule Token(string regex, Enum type)
        {
            return new RegexRule(regex, type);
        }

        public static RegexRule TokenPushMode(string regex, Enum type, IEnumerable<RegexRule> modeRules)
        {
            return new RegexRule(regex, type, modeRules);
        }

        public static RegexRule TokenPopMode(string regex, Enum type)
        {
            return new RegexRule(regex, type, isPopMode: true);
        }

        public static RegexRule NoToken(string regex)
        {
            return new RegexRule(regex);
        }

        public static RegexRule NoTokenPushMode(string regex, IEnumerable<RegexRule> modeRules)
        {
            return new RegexRule(regex, modeRules);
        }

        public static RegexRule NoTokenPopMode(string regex)
        {
            return new RegexRule(regex, isPopMode: true);
        }
    }
}
