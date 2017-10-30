using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace LibProtection.Injections
{
    public abstract class AntlrLanguageProvider : LanguageProvider
    {
        private const int EofAntrlTokenType = -1;

        public override IEnumerable<Token> Tokenize(string text, int offset = 0)
        {
            var lexer = CreateLexer(text);
            var antlrToken = lexer.NextToken();

            while (antlrToken.Type != EofAntrlTokenType)
            {
                yield return CreateToken(ConvertAntlrTokenType(antlrToken.Type), antlrToken.StartIndex + offset,
                    antlrToken.StopIndex + offset, antlrToken.Text);

                antlrToken = lexer.NextToken();
            }
        }

        protected abstract Enum ConvertAntlrTokenType(int antlrTokenType);
        protected abstract Lexer CreateLexer(string text);
    }
}
