﻿using System;
using System.Collections.Generic;

namespace LibProtection.Injections
{
    public sealed class FilePath : RegexLanguageProvider
    {
        private const string DisallowedSymbols = @"<>:""/\\\|\?\*\x00-\x1f";

        private FilePath() { }

        protected override Enum ErrorTokenType => FilePathTokenType.Error;

        protected override IEnumerable<RegexTokenDefinition> TokenDefinitions { get; } = new[]
        {
            new RegexTokenDefinition(@"[\\/]+", FilePathTokenType.Separator),
            new RegexTokenDefinition(@"[a-zA-Z]+[\$:](?=[\\/])", FilePathTokenType.DeviceID),
            new RegexTokenDefinition(@"[^" + DisallowedSymbols + "]+", FilePathTokenType.FSEntryName),
            new RegexTokenDefinition(@":+\$[^" + DisallowedSymbols + "]+", FilePathTokenType.NTFSAttribute),
            new RegexTokenDefinition($"[{DisallowedSymbols}]", FilePathTokenType.DisallowedSymbol),
        };

        protected override bool IsSafeToken(Enum type, string text)
        {
            return (FilePathTokenType) type == FilePathTokenType.FSEntryName && !text.Contains("..");
        }
    }
}