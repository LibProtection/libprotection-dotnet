using System;
using System.Collections.Generic;

namespace LibProtection.Injections
{
    public sealed class FilePath : RegexLanguageProvider
    {
        private const string DisallowedSymbols = @"<>:""/\\\|\?\*\x00-\x1f";

        private FilePath() { }

        protected override Enum ErrorTokenType => FilePathTokenType.Error;

        protected override IEnumerable<RegexRule> MainModeRules { get; } = new[]
        {
            RegexRule.Token(@"[\\/]+",                            FilePathTokenType.Separator),
            RegexRule.Token(@"[a-zA-Z]+[\$:](?=[\\/])",           FilePathTokenType.DeviceID),
            RegexRule.Token(@"[^" + DisallowedSymbols + "]+",     FilePathTokenType.FSEntryName),
            RegexRule.Token(@":+\$[^" + DisallowedSymbols + "]+", FilePathTokenType.NTFSAttribute),
            RegexRule.Token($"[{DisallowedSymbols}]",             FilePathTokenType.DisallowedSymbol),
        };
        public override bool TrySanitize(string text, Token context, out string sanitized)
        {
            sanitized = null;
            return false;
        }

        protected override bool IsTrivial(Enum type, string text)
        {
            return (FilePathTokenType) type == FilePathTokenType.FSEntryName && !text.Contains("..");
        }
    }
}