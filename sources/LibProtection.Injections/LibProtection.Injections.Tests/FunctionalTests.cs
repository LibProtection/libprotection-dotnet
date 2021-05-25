using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;

namespace LibProtection.Injections.Tests
{
    [TestFixture(Category = nameof(FunctionalTests))]
    public class FunctionalTests
    {
        public class DataPoint
        {
            public Type LanguageProviderType { get; private set; }
            public string Result { get; private set; }
            public string Format { get; private set; }
            public object[] Arguments { get; private set; }

            public bool IsAttack()
            {
                return Result == null;
            }

            public static DataPoint GetDataPoint<T>(string result, string format, params object[] arguments) where T : LanguageProvider
            {
                DataPoint point = new DataPoint();
                point.LanguageProviderType = typeof(T);
                point.Result = result;
                point.Format = format;
                point.Arguments = arguments;
                return point;
            }
        }

        private static DataPoint[] testCases = new DataPoint[]{
            //Valid
            DataPoint.GetDataPoint<Html>("<a href='Default.aspx' onclick='alert(\"Hello from embedded JavaScript code!\");return false'>This site&#39;s home page</a>", "<a href='{0}' onclick='alert(\"{1}\");return false'>{2}</a>", "Default.aspx", "Hello from embedded JavaScript code!", "This site's home page"),
            DataPoint.GetDataPoint<JavaScript>("operationResult.innerText = 'operationResult.innerText = \\u0027Hello from internal JavaScript code!\\u0027;';", "operationResult.innerText = '{0}';", "operationResult.innerText = 'Hello from internal JavaScript code!';"),
            DataPoint.GetDataPoint<Sql>("SELECT * FROM myTable WHERE id = 1 AND myColumn = 'value1'", "SELECT * FROM myTable WHERE id = {0} AND myColumn = '{1}'",  1, "value1"),
            DataPoint.GetDataPoint<Url>("Assets/jsFile.js", "{0}/{1}", "Assets", "jsFile.js"),
            DataPoint.GetDataPoint<FilePath>("C:\\inetpub\\playground.libprotection.org\\Assets\\textFile.txt", "C:\\inetpub\\playground.libprotection.org\\Assets\\{0}", "textFile.txt"),
            //Attacks
            DataPoint.GetDataPoint<Html>(null, "<a href={0} />", "<br>"),
            DataPoint.GetDataPoint<JavaScript>(null, "operationResult.innerText = {0};", "' <br>"),
            DataPoint.GetDataPoint<Sql>(null, "SELECT * FROM myTable WHERE id = {0}", "1 OR 1==1 --"),
            DataPoint.GetDataPoint<Url>(null, "{0}/{1}", "../Asserts", "jsFile.js"),
            DataPoint.GetDataPoint<FilePath>(null, "C:\\Assets\\{0}", "..\\jsFile.js"),
            //safe modifier
            DataPoint.GetDataPoint<Html>(":safe", ":safe"),
            DataPoint.GetDataPoint<Html>(":safe&lt;br&gt;", "{0}", ":safe<br>"),
            DataPoint.GetDataPoint<Html>("&lt;br&gt;xxx:safe", "{0}xxx:safe", "<br>"),
            DataPoint.GetDataPoint<Html>("<br>", "{0:safe}", "<br>"),
            DataPoint.GetDataPoint<Html>("<br>", "{0:SaFe}", "<br>"),
            DataPoint.GetDataPoint<Html>("<br>:safe", "{0:safe}:safe", "<br>"),
            DataPoint.GetDataPoint<Html>("<br>&lt;br&gt;", "{0:safe}{1}", "<br>", "<br>"),
            DataPoint.GetDataPoint<Html>("&lt;br&gt;<br>&lt;br&gt;", "{0}{1:safe}{2}", "<br>", "<br>", "<br>"),
        };


        public static IEnumerable TestCases
        {
            get
            {
                int i = 0;
                foreach(var testCase in testCases)
                {
                    yield return new TestCaseData(testCase).SetName($"Functional test #{++i}");
                }
            }
        }

        private delegate bool TryFormatDelegate(string format, out string formatted, params object[] args);
        private delegate string FormatDelegate(RawString format, params object[] args);

        private static void GetFormatters(Type providerType, out TryFormatDelegate tryFormatDelegate, out FormatDelegate formatDelegate)
        {
            var methods = typeof(SafeString<>).MakeGenericType(providerType).GetMethods();

            var tryFormatMethod = methods.First(x => x.Name == "TryFormat" && x.GetParameters()[0].ParameterType == typeof(string));
            tryFormatDelegate = (TryFormatDelegate)Delegate.CreateDelegate(typeof(TryFormatDelegate), null, tryFormatMethod);

            var formatMethod = methods.First(x => x.Name == "Format" && x.GetParameters()[0].ParameterType == typeof(RawString));
            formatDelegate = (FormatDelegate)Delegate.CreateDelegate(typeof(FormatDelegate), null, formatMethod);
        }


        [Test, TestCaseSource(typeof(FunctionalTests), nameof(TestCases))]
        public void FunctionalTest(DataPoint dataPoint)
        {
            GetFormatters(dataPoint.LanguageProviderType, out var tryFormatDelegate, out var formatDelegate);
            var tryFormatResult = tryFormatDelegate(dataPoint.Format, out var tryFormatResultValue, dataPoint.Arguments);

            if (dataPoint.IsAttack())
            {
                Assert.False(tryFormatResult);

                bool failed = false;
                try
                {
                    formatDelegate(dataPoint.Format, dataPoint.Arguments);
                }
                catch(AttackDetectedException)
                {
                    failed = true;
                }
                Assert.IsTrue(failed);
            }
            else{
                Assert.IsTrue(tryFormatResult);
                Assert.AreEqual(tryFormatResultValue, dataPoint.Result);

                var formatResultValue = formatDelegate(dataPoint.Format, dataPoint.Arguments);
                Assert.AreEqual(formatResultValue, dataPoint.Result);
            }
        }
    }
}
