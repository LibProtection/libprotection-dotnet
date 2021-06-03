using NUnit.Framework;

namespace LibProtection.Injections.Tests
{
    [TestFixture]
    class SafeStringTests
    {
        [Test]
        public void TestFormatWithInterpolatedString()
        {
            var input = "<script>alert(`XSS`)</script>";
            var output = SafeString<Html>.Format($"<p>{input}</p>");
            Assert.AreEqual(output, "<p>&lt;script&gt;alert(`XSS`)&lt;/script&gt;</p>");
        }

        [Test]
        public void TestNormalFormattingWithHtmlReferences()
        {
            var input = "False Positive";
            var output = SafeString<Html>.Format($"HTML: <a href=\"#\" onclick='alert(&quot;{input}&quot;);'>test</a>");
            Assert.AreEqual(output, "HTML: <a href=\"#\" onclick='alert(\"False Positive\");'>test</a>");
        }

        [Test]
        public void TestXSSDetectionWithHtmlReferences()
        {
            var input = "+alert(2)+";
            Assert.Throws<AttackDetectedException>(() => SafeString<Html>.Format($"<a href='#' onclick='alert(&quot;\"{input}\"&quot;);'>XSS</a>"));
        }

        [Test]
        public void TestSanitizingWithOverlappingHtmlReferences()
        {
            var input = "quot;);alert(2);//";
            var output = SafeString<Html>.Format($"<a href='#' onclick='alert(\"Tom&{input}\");'>XSS?</a>");
            Assert.AreEqual(output, "<a href='#' onclick='alert(\"Tom\\\");alert(2);//\");'>XSS?</a>");
        }

        [Test]
        public void TestInsideScriptTags()
        {
            var input = "</script><script>alert(0)//";
            var output = SafeString<Html>.Format($"<html><body><script>var a = \"{input}\";</script></body></html>");
            Assert.AreEqual(output, "<html><body><script>var a = \"&lt;/script&gt;&lt;script&gt;alert(0)//\";</script></body></html>");
        }
    }
}
