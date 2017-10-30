using System;
using System.Diagnostics;

namespace LibProtection.Injections
{
    public class Token
    {
        public LanguageProvider LanguageProvider { get; }
        public Enum Type { get; }        
        public string Text { get; }
        public Range Range { get; }
        public bool IsSafe { get; }

        public Token(LanguageProvider languageProvider, Enum type, int lowerBound, int upperBound, string text, bool isSafe)
        {
            Debug.Assert(!string.IsNullOrEmpty(text));

            LanguageProvider = languageProvider;
            Type = type;
            Range = new Range(lowerBound, upperBound);
            Text = text;
            IsSafe = isSafe;
        }

        public override string ToString()
        {
            return $"{Type}:\"{Text}\":{Range}";
        }
    }
}