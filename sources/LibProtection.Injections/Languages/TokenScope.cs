using System.Collections.Generic;

namespace LibProtection.Injections
{
    internal class TokenScope
    {
        public Range Range { get; }
        public List<Token> Tokens { get; }

        public TokenScope(Range range)
        {
            Range = range;
            Tokens = new List<Token>();
        }
    }
}
