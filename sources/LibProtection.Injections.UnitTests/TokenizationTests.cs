using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace LibProtection.Injections.UnitTests
{
    [TestFixture]
    public class TokenizationTests
    {
        [Test, TestCaseSource(typeof(TokenizationTestCases), nameof(TokenizationTestCases.HtmlTestCases))]
        public bool Html(string languageName, string fileName)
        {
            return TestRunner(languageName, fileName);
        }

        [Test, TestCaseSource(typeof(TokenizationTestCases), nameof(TokenizationTestCases.JavaScriptTestCases))]
        public bool JavaScript(string languageName, string fileName)
        {
            return TestRunner(languageName, fileName);
        }

        [Test, TestCaseSource(typeof(TokenizationTestCases), nameof(TokenizationTestCases.FilePathTestCases))]
        public bool FilePath(string languageName, string fileName)
        {
            return TestRunner(languageName, fileName);
        }

        [Test, TestCaseSource(typeof(TokenizationTestCases), nameof(TokenizationTestCases.SqlTestCases))]
        public bool Sql(string languageName, string fileName)
        {
            return TestRunner(languageName, fileName);
        }

        [Test, TestCaseSource(typeof(TokenizationTestCases), nameof(TokenizationTestCases.UrlTestCases))]
        public bool Url(string languageName, string fileName)
        {
            return TestRunner(languageName, fileName);
        }

        private static bool TestRunner(string languageName, string caseFileName)
        {
            var caseText = File.ReadAllText(caseFileName);

            LanguageProvider languageProvider;

            switch (languageName)
            {
                case "html":
                    languageProvider = Single<Html>.Instance;
                    break;

                case "javascript":
                    languageProvider = Single<JavaScript>.Instance;
                    break;

                case "filepath":
                    languageProvider = Single<FilePath>.Instance;
                    break;

                case "sql":
                    languageProvider = Single<Sql>.Instance;
                    break;

                case "url":
                    languageProvider = Single<Url>.Instance;
                    break;

                default:
                    throw new ArgumentException("Language not supported", languageName);
            }

            string[] obtainedTokens = languageProvider
                .Tokenize(caseText)
                .Select(token =>
                {
                    var obtainedText = caseText.Substring(token.Range.LowerBound,
                        token.Range.UpperBound - token.Range.LowerBound + 1);

                    if (token.Text != obtainedText)
                    {
                        throw new Exception($"Expected at {token.Range}: {token.Text}, obtained: {obtainedText}");
                    }
                    return EscapeNewLine($"{token.LanguageProvider.GetType().Name}:{token}");
                })
                .ToArray();

            // ReSharper disable once AssignNullToNotNullAttribute
            var tokensFileName =
                Path.Combine(Path.GetDirectoryName(caseFileName), $"{caseFileName}.tokens");

            if (File.Exists(tokensFileName))
            {
                string[] expectedTokens = File.ReadAllLines(tokensFileName);

                if (expectedTokens.Length != obtainedTokens.Length)
                {
                    throw new Exception("Lengths of expected and obtained token-arrays are different");
                }

                for (var tokenIndex = 0; tokenIndex < expectedTokens.Length; tokenIndex++)
                {
                    var expectedToken = expectedTokens[tokenIndex];
                    var obtainedToken = obtainedTokens[tokenIndex];

                    if (expectedToken != obtainedToken)
                    {
                        throw new Exception($"Expected at {tokenIndex}: `{expectedToken}` but obtained: `{obtainedToken}`");
                    }
                }

                return true;
            }
            else
            {
                Debug.Assert(false, $"{tokensFileName} file is missed. Press skip to generate it.");
                // ReSharper disable once HeuristicUnreachableCode
                File.WriteAllLines(tokensFileName, obtainedTokens);
                return true;
            }


            return false;
        }

        private static string EscapeNewLine(string s)
        {
            return s.Replace("\r", $"\\r").Replace("\n", "\\n");
        }
    }

    internal static class TokenizationTestCases
    {
        public static IEnumerable HtmlTestCases => TestCases("html");
        public static IEnumerable JavaScriptTestCases => TestCases("javascript");
        public static IEnumerable FilePathTestCases => TestCases("filepath");
        public static IEnumerable SqlTestCases => TestCases("sql");
        public static IEnumerable UrlTestCases => TestCases("url");

        private static IEnumerable TestCases(string languageName)
        {
            var casesDirectory =
                Path.Combine(TestContext.CurrentContext.TestDirectory, "TokenizationTestCases");

            foreach (var fileName in Directory.GetFiles(casesDirectory, $"*.{languageName}"))
            {
                yield return new TestCaseData(languageName, fileName)
                    .Returns(true)
                    .SetName(Path.GetFileName(fileName));
            }
        }
    }
}
