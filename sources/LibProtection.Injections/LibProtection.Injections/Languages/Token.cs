using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LibProtection.Injections
{
    public class Token
    {
        public LanguageProvider LanguageProvider { get; }
        public Enum Type { get; }        
        public string Text { get; }
        public Range Range { get; }
        public bool IsTrivial { get; }

        public Token(LanguageProvider languageProvider, Enum type, int lowerBound, int upperBound, string text,
            bool isTrivial)
        {
            Debug.Assert(!string.IsNullOrEmpty(text));

            LanguageProvider = languageProvider;
            Type = type;
            Range = new Range(lowerBound, upperBound);
            Text = text;
            IsTrivial = isTrivial;
        }

        public override string ToString()
        {
            return $"{Type}:\"{Text}\":{Range}";
        }

        public override bool Equals(object obj)
        {
            var token = obj as Token;
            return token != null &&
                   EqualityComparer<LanguageProvider>.Default.Equals(LanguageProvider, token.LanguageProvider) &&
                   EqualityComparer<Enum>.Default.Equals(Type, token.Type) &&
                   Text == token.Text &&
                   Range.Equals(token.Range) &&
                   IsTrivial == token.IsTrivial;
        }

        public override int GetHashCode()
        {
            var hashCode = -737555491;
            unchecked
            {
                hashCode = hashCode * -1521134295 + EqualityComparer<LanguageProvider>.Default.GetHashCode(LanguageProvider);
                hashCode = hashCode * -1521134295 + EqualityComparer<Enum>.Default.GetHashCode(Type);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
                hashCode = hashCode * -1521134295 + EqualityComparer<Range>.Default.GetHashCode(Range);
                hashCode = hashCode * -1521134295 + IsTrivial.GetHashCode();
            }
            return hashCode;
        }
    }
}