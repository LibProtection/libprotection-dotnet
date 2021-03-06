﻿using NUnit.Framework;

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
        public void TestAppendFormat()
        {
            var sb = new SafeStringBuilder<Html>();
            sb.AppendFormat("<a href={0} />", "</br>");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestLength()
        {
            var sb = new SafeStringBuilder<Html>();
            sb.UncheckedAppend("<a href=");
            sb.AppendLine("</br>");
            sb.UncheckedAppend("/>");
            sb.Length = 8;
            sb.ToString();
        }

        [Test]
        public void TestUncheckedReplace()
        {
            var sb = new SafeStringBuilder<Html>();
            sb.UncheckedAppend("<a href=");
            sb.Append("<br>");
            sb.UncheckedAppend(" />");
            sb.UncheckedReplace("<br> ", "   ");
            sb.ToString();
        }

        [Test]
        public void TestUncheckedReplace2()
        {
            var sb = new SafeStringBuilder<Html>();
            sb.UncheckedAppend("<a href=");
            sb.Append("<br>foo");
            sb.UncheckedAppend(" />");
            sb.UncheckedReplace("foo", "bar");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestUncheckedReplaceSplit()
        {
            var sb = new SafeStringBuilder<Html>();
            sb.UncheckedAppend("<a href=");
            sb.Append("<brfoooo>");
            sb.UncheckedAppend(" />");
            sb.UncheckedReplace("foooo", "");
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

        [Test]
        public void TestReplace()
        {
            var sb = new SafeStringBuilder<Html>();

            sb.UncheckedAppend("<a href={0} />");
            sb.Replace("{0}", "<br>");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestDoubleReplace()
        {
            var sb = new SafeStringBuilder<Html>();

            sb.UncheckedAppend("<{0} href={1} />");
            sb.Replace("{0}", "a");
            sb.Replace("{1}", "<br>");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestDoubleDoubleReplace()
        {
            var sb = new SafeStringBuilder<Html>();

            sb.UncheckedAppend("<{0} href={1} /> </{0}> {1}");
            sb.Replace("{0}", "a");
            sb.Replace("{1}", "<br>");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestReplaceThenSafeReplce()
        {
            var sb = new SafeStringBuilder<Html>();

            sb.UncheckedAppend("<{0} href='{1}' >Click me!</{0}>");
            sb.Replace("{1}", "default.html");
            sb.UncheckedReplace("{0}", "a");
            sb.ToString();
        }

        [Test]
        public void TestReplaceEmpty()
        {
            var sb = new SafeStringBuilder<Html>();

            sb.UncheckedAppend("<a href=");
            sb.Append("<b");
            sb.UncheckedAppend("{0}");
            sb.Append("r>");
            sb.Replace("{0}", "");
            Assert.Throws<AttackDetectedException>(() => sb.ToString());
        }

        [Test]
        public void TestReplaceTaintedRange()
        {
            var sb = new SafeStringBuilder<Html>();

            sb.UncheckedAppend("<a href=");
            sb.Append("<br>");
            sb.UncheckedAppend(" />");
            sb.Replace("<br>", "");
            sb.ToString();
        }

        [Test][Ignore("Find a example without an attack injection.")]
        public void TestReplaceTwoTaintedRanges()
        {
            var sb = new SafeStringBuilder<Html>();

            sb.UncheckedAppend("<a href=");
            sb.Append("<br>");
            sb.UncheckedAppend(" />");
            sb.Append("<script>alert(1)");
            sb.UncheckedAppend("</script>");
            sb.Replace("<br> /><script>", "/>");
            sb.ToString();
        }
    }
}
