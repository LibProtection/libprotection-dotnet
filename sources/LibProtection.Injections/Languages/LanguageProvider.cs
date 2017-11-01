using System;
using System.Collections.Generic;

namespace LibProtection.Injections
{
    public abstract class LanguageProvider
    {
        public abstract IEnumerable<Token> Tokenize(string text, int offset = 0);

        public virtual bool TrySanitize(string text, Token context, out string sanitized)
        {
            sanitized = null;

            if (IsSafeToken(context.Type, context.Text))
            {
                sanitized = context.Text;
                return true;
            }

            return false;
        }

        protected Token CreateToken(Enum type, int lowerBound, int upperBound, string text)
        {
            return new Token(this, type, lowerBound, upperBound, text, IsSafeToken(type, text));
        }

        protected abstract bool IsSafeToken(Enum type, string text);
    }
}