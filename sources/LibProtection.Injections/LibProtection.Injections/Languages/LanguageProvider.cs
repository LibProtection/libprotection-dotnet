using System;
using System.Collections.Generic;

namespace LibProtection.Injections
{
    public abstract class LanguageProvider
    {
        public abstract IEnumerable<Token> Tokenize(string text, int offset = 0);

        public abstract bool TrySanitize(string text, Token context, out string sanitized);

        protected Token CreateToken(Enum type, int lowerBound, int upperBound, string text)
        {
            return new Token(this, type, lowerBound, upperBound, text, IsTrivial(type, text));
        }

        protected abstract bool IsTrivial(Enum type, string text);
    }
}