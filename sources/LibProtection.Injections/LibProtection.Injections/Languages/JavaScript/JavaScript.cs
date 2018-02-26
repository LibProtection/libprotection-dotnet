using System;
using System.Web;
using Antlr4.Runtime;
using LibProtection.Injections.Internals;

namespace LibProtection.Injections
{
    public sealed class JavaScript : AntlrLanguageProvider
    {
        private JavaScript()
        {
        }

        public override bool TrySanitize(string text, Token context, out string sanitized)
        {
            switch (context.LanguageProvider)
            {
                case JavaScript _:
                    if (TryJavaScriptEncode(text, (JavaScriptTokenType) context.Type, out sanitized))
                    {
                        return true;
                    }
                    break;

                default:
                    throw new ArgumentException($"Unsupported JavaScript island: {context}");
            }

            return false;
        }

        protected override Enum ConvertAntlrTokenType(int antlrTokenType)
        {
            return (JavaScriptTokenType)antlrTokenType;
        }

        protected override Lexer CreateLexer(string text)
        {
            var lexer = new JavaScriptCSharpLexer(new AntlrInputStream(text));
            lexer.SetStrictMode(false);
            return lexer;
        }

        protected override bool IsTrivial(Enum type, string text)
        {
            switch ((JavaScriptTokenType) type)
            {
                case JavaScriptTokenType.LineTerminator:
                    
                case JavaScriptTokenType.MultiLineComment:
                case JavaScriptTokenType.SingleLineComment:
                
                case JavaScriptTokenType.RegularExpressionLiteral:
                case JavaScriptTokenType.NullLiteral:
                case JavaScriptTokenType.BooleanLiteral:
                case JavaScriptTokenType.DecimalLiteral:
                case JavaScriptTokenType.HexIntegerLiteral:
                case JavaScriptTokenType.OctalIntegerLiteral:
                case JavaScriptTokenType.StringLiteral:
                    return true;
            }

            return false;
        }

        private static bool TryJavaScriptEncode(string text, JavaScriptTokenType tokenType, out string encoded)
        {
            encoded = null;

            switch (tokenType)
            {
                case JavaScriptTokenType.RegularExpressionLiteral:
                    encoded = HttpUtility.JavaScriptStringEncode(text).Replace("/", @"\/");
                    return true;

                case JavaScriptTokenType.StringLiteral:
                    encoded = HttpUtility.JavaScriptStringEncode(text);
                    return true;
            }

            return false;
        }
    }
}