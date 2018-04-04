using System;

namespace LibProtection.Injections.Tests
{
    [Serializable]
    internal class TestCase
    {
        // name of test case (should be used for NUnit's `TestCaseData(...).SetName`)
        public string Name;
        // name of language provider
        public string LanguageProvider;
        // format string template
        public string Format;
        // format string arguments
        public string[] Arguments;
        // expected format result (should be null for new test cases)
        public FormatResult Result;
    }
}
