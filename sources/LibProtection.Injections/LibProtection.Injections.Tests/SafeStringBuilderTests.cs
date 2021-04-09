using NUnit.Framework;

namespace LibProtection.Injections.Tests
{
    [TestFixture]
    class SafeStringBuilderTests
    {
        [Test]
        public void TestAppend()
        {
            var sb = new SafeStringBuilder<Html>();
            sb.UncheckedAppend("<a href='");
            sb.UncheckedAppend("Default.aspx");
            sb.Append("'onclick ='alert(0);'><!--");
            sb.UncheckedAppend("'>foo</a>");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestDoubleAppend()
        {
            var sb = new SafeStringBuilder<Html>();
            sb.UncheckedAppend("<a href='");
            sb.UncheckedAppend("Default.aspx");
            sb.Append("'onclick ='ale");
            sb.Append("rt(0);'><!--");
            sb.UncheckedAppend("'>foo</a>");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestAppendLine()
        {
            var sb = new SafeStringBuilder<Html>();
            sb.UncheckedAppend("<a href=");
            sb.AppendLine("</br>");
            sb.UncheckedAppend("/>");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestInsert()
        {
            var sb = new SafeStringBuilder<Html>();
            sb.UncheckedAppend("<a href= />");
            sb.Insert(8, "<br>");
            var foo = SafeString<Html>.TryFormat("<a href={0} /> ", out var bar, "<br>");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestDoubleInsert()
        {
            var sb = new SafeStringBuilder<Html>();

            sb.UncheckedAppend("<a href= />");
            sb.Insert(8, "<b");
            sb.UncheckedAppend("<!--fooo>");
            sb.Insert(10, "r>");
            sb.UncheckedInsert(0, "<a href='bar'>");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestRemove()
        {
            var sb = new SafeStringBuilder<Html>();

            sb.UncheckedAppend("<a href=");
            sb.Append("<br>");
            sb.UncheckedAppend(" />");
            sb.Remove(8, 4);
            sb.ToString();
        }

        [Test]
        public void TestRemoveUncomplete()
        {
            var sb = new SafeStringBuilder<Html>();

            sb.UncheckedAppend("<a href=");
            sb.Append("<br>");
            sb.Append("<br>");
            sb.UncheckedAppend(" />");
            sb.Remove(8, 4);
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

    }
}
