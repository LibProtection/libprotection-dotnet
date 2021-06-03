using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace LibProtection.Injections.Tests
{
    [TestFixture]
    class HtmlCharRefDecoderTests
    {
        [Test]
        public static void testCharRefDecodingWithoutTrailingSemicolon()
        {
            Assert.AreEqual(HtmlCharRefDecoder.decodeCharRef("&#x34"), "4");
        }

        [Test]
        public static void testHexNumericCharRefDecoding()
        {
            Assert.AreEqual(HtmlCharRefDecoder.decodeCharRef("&#x34;"), "4");
            Assert.AreEqual(HtmlCharRefDecoder.decodeCharRef("&#X34;"), "4");
        }

        [Test]
        public static void testDecimalNumericCharRefDecoding()
        {
            Assert.AreEqual(HtmlCharRefDecoder.decodeCharRef("&#35;"), "#");
        }

        [Test]
        public static void testInvalidCharRefDecoding()
        {
            Assert.AreEqual(HtmlCharRefDecoder.decodeCharRef("&#x89;"), "\u2030");
        }

        [Test]
        public static void testNamedCharRefDecoding()
        {
            Assert.AreEqual(HtmlCharRefDecoder.decodeCharRef("&not;"), "¬");
        }

        [Test]
        public static void test1()
        {
            var a = HtmlCharRefDecoder.decodeCharRef("&Bfr;");

        }
    }
}
