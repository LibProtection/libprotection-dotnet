using System;
using System.Collections;
using System.IO;
using LibProtection.Injections.Formatting;
using Newtonsoft.Json;
using NUnit.Framework;

namespace LibProtection.Injections.Tests
{
    [TestFixture]
    internal class TokenizationTests
    {
        [Test, TestCaseSource(typeof(TokenizationTests), nameof(TestCases))]
        public FormatResult FormatAndTokenizationTest(string format, string[] arguments, string languageProvider)
        {
            var providerType = typeof(LanguageProvider).Assembly.GetType($"{typeof(LanguageProvider).Namespace}.{languageProvider}", throwOnError: true);

            var formatExMethod = typeof(SafeString<>).MakeGenericType(providerType).GetMethod("FormatEx", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            return (FormatResult)formatExMethod.Invoke(null, new object[] { format, arguments });
        }

        public static IEnumerable TestCases
        {
            get
            {
                var casesDirectory = Path.Combine(
                    Path.GetDirectoryName(new Uri(typeof(TokenizationTests).Assembly.CodeBase).LocalPath),
                    $"TestCases{Path.DirectorySeparatorChar}formatCases");

                foreach(var file in Directory.GetFiles(casesDirectory))
                {
                    var jsonContent = File.ReadAllText(file);
                    var testCase = JsonConvert.DeserializeObject<TestCase>(jsonContent, new TokenConverter());

                    yield return new TestCaseData(testCase.Format, testCase.Arguments, testCase.LanguageProvider)
                        .SetName(testCase.Name)
                        .Returns(testCase.Result);
                }
            }
        }
    }
}
