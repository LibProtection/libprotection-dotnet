using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace LibProtection.Injections.Tests
{
    [TestFixture]
    class HtmlUnescapeTests
    {
        [Test]
        public void TestHTMLUnescapeWithOneCharRef()
        {
            string s = "Tom&quot;StartEnd";
            List<Range> taintedRanges;
            string decoded;

            // Case 1: HR_1 < T_1 and HR_2 < T_2

            /* Case 1.1: HR_2 < T_1
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *            ###
             *            |T|
             */
            taintedRanges = new List<Range>() { 
                new Range(11, 14), 
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "art");

            /* Case 1.2: HR_2 == T_1
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *         ###
             *         |T|
             */
            taintedRanges = new List<Range>() {
                new Range(8, 11),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "\"St");

            /* Case 1.3: HR_2 > T_1
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *       #####
             *       | T |
             */
            taintedRanges = new List<Range>() {
                new Range(6, 11),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "\"St");

            /* Case 2: HR_1 < T_1 and HR_2 == T_2
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *       ###
             *       |T|
             */
            taintedRanges = new List<Range>() {
                new Range(6, 9),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "\"");


            /* Case 3: HR_1 < T_1 and HR_2 > T_2
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *      ##
             *      |T|
             */
            taintedRanges = new List<Range>() {
                new Range(5, 7),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "\"");

            /* Case 4: HR_1 == T_1 and HR_2 < T_2
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *    ########
             *    |   T  |
             */
            taintedRanges = new List<Range>() {
                new Range(3, 11),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "\"St");

            /* Case 5: HR_1 == T_1 and HR_2 == T_2
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *    ######
             *    |  T |
             */
            taintedRanges = new List<Range>() {
                new Range(3, 9),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "\"");

            /* Case 6: HR_1 == T_1 and HR_2 > T_2
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *    ##
             *    |T|
             */
            taintedRanges = new List<Range>() {
                new Range(3, 5),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "\"");

            /* Case 7: HR_1 > T_1 and HR_2 < T_2
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *  #########
             *  |   T   | 
             */
            taintedRanges = new List<Range>() {
                new Range(1, 10),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "om\"S");

            /* Case 8: HR_1 > T_1 and HR_2 == T_2
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *  ########
             *  |   T  |
             */
            taintedRanges = new List<Range>() {
                new Range(1, 9),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "om\"");

            // Case 9: HR_1 > T_1 and HR_2 > T_2

            /* Case 9.1: HR_1 < T_2
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *  ######
             *  |  T |
             */
            taintedRanges = new List<Range>() {
                new Range(1, 7),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "om\"");

            /* Case 9.2: HR_1 == T_2
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *  ###
             *  |T|
             */
            taintedRanges = new List<Range>() {
                new Range(1, 4),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "om\"");

            /* Case 9.3: HR_1 > T_2
             *    | HR |
             *    ######
             * Tom&quot;StartEnd
             *  ##
             * |T|
             */
            taintedRanges = new List<Range>() {
                new Range(1, 3),
            };
            decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "om");
        }

        [Test]
        public void TestHTMLUnescapeWithTwoCharRefs()
        {
            var s = "HTML: <a href=\"#\" onclick='alert(&quot;False Positive&quot;);'>test</a>";
            var taintedRanges = new List<Range>() {
                new Range(39, 53),
            };
            var decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "False Positive");
            Assert.AreEqual(decoded, "HTML: <a href=\"#\" onclick='alert(\"False Positive\");'>test</a>");
        }

        [Test]
        public void TestHtmlUnescapeWithNoSingleDecodedChar()
        {
            var s = "Tom&Bfr;StartEnd";
            var taintedRanges = new List<Range>() {
                new Range(5, 8),
            };
            var decoded = Html.HtmlUnescape(s, ref taintedRanges);
            Assert.AreEqual(decoded.Substring(taintedRanges[0]), "\U0001d505");  // "\U0001d505".Length = 2
        }
    }
}
