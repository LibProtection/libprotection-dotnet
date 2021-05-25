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
            Assert.AreEqual(SafeString<Html>.Format($"<p>{input}</p>"), "<p>&lt;script&gt;alert(`XSS`)&lt;/script&gt;</p>");
        }
    }
}
