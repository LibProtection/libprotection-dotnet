using System;
using System.Text.RegularExpressions;

namespace LibProtection.Injections
{
    public class RegexTokenDefinition
    {
        public Enum Type { get; }

        private readonly Regex _regex;

        public RegexTokenDefinition(string regex, Enum type)
        {
            Type = type;
            _regex = new Regex($"^{regex}", RegexOptions.Compiled);
        }

        public bool TryMatch(string text, out int length)
        {
            var matchResult = _regex.Match(text);
            length = matchResult.Success ? matchResult.Length : 0;
            return matchResult.Success;
        }
    }
}
