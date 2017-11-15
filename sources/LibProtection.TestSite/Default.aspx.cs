using System;
using System.Text;
using System.Web;
using System.Web.UI;
using LibProtection.Injections;
using System.Configuration;
using System.Linq;
using System.Xml;
using System.Data;
using System.Data.SqlClient;

namespace LibProtection.TestSite
{
    public partial class _Default : Page
    {
        protected struct FormatResult
        {
            public bool Successfully { get; }
            public string FormattedValue { get; }

            public FormatResult(bool successfully, string formattedValue)
            {
                Successfully = successfully;
                FormattedValue = formattedValue;
            }
        }

        protected struct Example
        {
            public string Operation { get; set; }
            public Func<string, string[], string> FormatFunc { get; set; }
            public string Prefix { get; set; }
            public string Formatter { get; set; }
            public string Replacer { get; set; }
            public Func<string, string> TagBuilder { get; set; }

            public string FormatId => $"{Prefix}FormatId";
            public string ParameterId => $"{Prefix}ParameterId";
            public string ButtonId => $"{Prefix}ButtonId";
            public string FormatParam => $"{Prefix}Format";
            public string ParameterParam => $"{Prefix}Parameter";
        }

        protected static Example[] Examples =
        {
                    new Example {Operation = "Write to HTTP response", FormatFunc = FormatHelper<Html>, Prefix="Html", Formatter="<a href='{0}' onclick='f(\"{1}\")'>{2}</a>", Replacer="\'onclick=\'alert(0)\r\n\")alert(\"0\r\n<script>alert(0)</script>", TagBuilder = GetFormatTagBuilder("{0}") },
                    new Example {Operation = "Write to HTTP response", FormatFunc = FormatHelper<JavaScript>, Prefix="JavaScript", Formatter="alert('Hello: {0}')", Replacer="');alert('0", TagBuilder = GetFormatTagBuilder("<label onclick=\"{0}\">Click to execute</label>") },
                    new Example {Operation = "Execute SQL query", FormatFunc = FormatHelper<Sql>, Prefix="Sql", Formatter="select * from myTable where myColumn = '{0}'", Replacer="' OR 1 = 1 -- ", TagBuilder = SqlRequestTagBuilder },
                    new Example {Operation = "Write to HTTP response", FormatFunc = FormatHelper<Url>, Prefix="Url", Formatter="./img/{0}", Replacer="../spanch.gif", TagBuilder = GetFormatTagBuilder("<img src=\"{0}\" />") },
                    new Example {Operation = "Read local file", FormatFunc = FormatHelper<FilePath>, Prefix="FilePath", Formatter=@".\files\{0}", Replacer=@"..\textFile.txt", TagBuilder = PathTagBuilder },
                    //new Example {FormatFunc = FormatHelper<Xpath>, Prefix="xpath", Formatter=@"descendant::bk:book[bk:author='{0}']", Replacer="' or ''='", TagBuilder = XPathTagBuilder },
                    //new Example {FormatFunc = FormatHelper<Xml>, Prefix="xml", Formatter="<?xml version=\"1.0\"?>{0}", Replacer="<!DOCTYPE foo [<!ELEMENT foo ANY><!ENTITY xxe SYSTEM \"file:///textfile.txt\">]><foo>&xxe;</foo>", TagBuilder = XMLTagBuilder },
        };

        #region Builders
        protected static string FormatHelper<T>(string format, object[] parameters) where T : LanguageProvider
        {
            return SafeString<T>.Format(format, parameters);
        }

        protected static string XPathTagBuilder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return string.Empty; }

            var doc = new XmlDocument();
            doc.Load(System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, "xmldocument.xml"));
            var root = doc.DocumentElement;

            // Add the namespace.
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("bk", "urn:newbooks-schema");

            var nodes = root.SelectNodes(path, nsmgr);

            var builder = new System.Text.StringBuilder();
            foreach (XmlNode row in nodes)
            {

                builder.AppendFormat("<br>{0} - {1}", row["title"].InnerText, row["author"].InnerText);
            }
            return builder.ToString();
        }

        protected static string XMLTagBuilder(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) { return string.Empty; }

            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Parse;
            var xmlDoc = new XmlDocument();
            xmlDoc.XmlResolver = new XmlUrlResolver();
            var xmlBytes = Encoding.ASCII.GetBytes(xml);

            xmlDoc.Load(XmlReader.Create(new System.IO.MemoryStream(xmlBytes), settings));
            return xmlDoc.LastChild.OuterXml;
        }

        protected static string PathTagBuilder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return string.Empty; }
            var filePath = System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, path);
            if (!System.IO.File.Exists(filePath)) { return string.Empty; }
            return System.IO.File.ReadAllText(filePath);
        }

        protected static string SqlRequestTagBuilder(string request)
        {
            if (string.IsNullOrWhiteSpace(request)) { return string.Empty; }
            var result = string.Empty;

            using (var connection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database.mdf;Integrated Security=True"))
            {
                using (var adapter = new SqlDataAdapter())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = request;
                        adapter.SelectCommand = command;

                        using (var dataSet = new DataSet())
                        {
                            adapter.Fill(dataSet);

                            var builder = new StringBuilder();
                            foreach (DataRow row in dataSet.Tables[0].Rows)
                            {
                                builder.AppendFormat("<br>{0} - {1}", row["Id"], row["myColumn"]);
                            }
                            result = builder.ToString();
                        }
                    }
                    connection.Close();
                }
            }
            return result;
        }

        protected static Func<string, string> GetFormatTagBuilder(string formatter) => (arg) => string.Format(formatter, arg);
        #endregion

        protected bool InputsAreDisabled {
            get {
                bool disabledInputs = false;
                bool.TryParse(ConfigurationManager.AppSettings["DisableArbitraryFormatFeature"], out disabledInputs);
                return disabledInputs;
            }
        }

        protected string SelectedItemTag
        {
            get
            {
                string selectedItemTag = null;
                foreach (var example in Examples)
                {
                    if (Request.Params[example.FormatParam] != null && Request.Params[example.ParameterParam] != null)
                    {
                        selectedItemTag = example.Prefix;
                    }
                }
                return selectedItemTag ?? Examples.First().Prefix;
            }
        }

        protected (string FormatResult, string OperationResult) GetResultsFor(Example example)
        {
            var formatResult = example.FormatFunc(
                Request.Params[example.FormatParam],
                Request.Params[example.ParameterParam].Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            );

            return (formatResult, example.TagBuilder(formatResult));
        }
    }
}