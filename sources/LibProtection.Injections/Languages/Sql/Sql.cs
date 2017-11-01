using System;
using Antlr4.Runtime;
using LibProtection.Injections.Internals;

namespace LibProtection.Injections
{
    public sealed class Sql : AntlrLanguageProvider
    {
        private Sql() { }

        protected override Enum ConvertAntlrTokenType(int antlrTokenType)
        {
            return (SqlTokenType)antlrTokenType;
        }

        protected override Lexer CreateLexer(string text)
        {
            return new SQLLexer(new AntlrInputStream(text));
        }

        public override bool TrySanitize(string text, Token context, out string sanitized)
        {
            switch (context.LanguageProvider)
            {
                case Sql _:
                    if (TrySqlEncode(text, (SqlTokenType) context.Type, out sanitized))
                    {
                        return true;
                    }
                    break;

                default:
                    throw new ArgumentException($"Unsupported JavaScript island: {context}");
            }

            return base.TrySanitize(text, context, out sanitized);
        }

        protected override bool IsSafeToken(Enum type, string text)
        {
            switch ((SqlTokenType) type)
            {
                case SqlTokenType.NullLiteral:
                case SqlTokenType.FilesizeLiteral:
                case SqlTokenType.StartNationalStringLiteral:
                case SqlTokenType.StringLiteral:
                case SqlTokenType.DecimalLiteral:
                case SqlTokenType.HexadecimalLiteral:
                case SqlTokenType.RealLiteral:
                case SqlTokenType.NullSpecLiteral:
                    return true;
            }

            return false;
        }

        private static bool TrySqlEncode(string text, SqlTokenType tokenType, out string encoded)
        {
            encoded = null;

            switch (tokenType)
            {
                case SqlTokenType.StringLiteral:
                    encoded = text.Replace("''", "'").Replace("'", "''");
                    return true;
            }

            return false;
        }
    }
}