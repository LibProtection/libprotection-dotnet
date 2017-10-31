using System;
using System.Text;
using System.Web;
using System.Web.UI;
using LibProtection.Injections;
using System.Configuration;
using System.Linq;

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

        private const string scriptVector = "<script>alert(0)</script>";

        protected static Example[] Examples =
        {
                    new Example {FormatFunc = FormatHelper<Html>, Prefix="html", Formatter="<b>{0}</b>", Replacer=">" + scriptVector, TagBuilder = GetFormatTagBuilder("{0}") },
                    new Example {FormatFunc = FormatHelper<JavaScript>, Prefix="javascript", Formatter="alert({0})", Replacer="\"/>Hello", TagBuilder = GetFormatTagBuilder("<label onclick={0}>ON CLICK</label>") },
                    new Example {FormatFunc = FormatHelper<Sql>, Prefix="sql", Formatter="select * from myTable where myColumn = '{0}'", Replacer="' OR 1 = 1 --", TagBuilder = SqlRequestTagBuilder },
                    new Example {FormatFunc = FormatHelper<Url>, Prefix="url", Formatter="./img/{0}", Replacer="../spanch.gif", TagBuilder = GetFormatTagBuilder("<img src=\"{0}\" />") },
                    new Example {FormatFunc = FormatHelper<FilePath>, Prefix="filepath", Formatter=@".\files\{0}", Replacer=@"..\textFile.txt", TagBuilder = PathTagBuilder },
                    //new Example {FormatFunc = FormatHelper<Xpath>, Prefix="xpath", Formatter=@"descendant::bk:book[bk:author='{0}']", Replacer="' or ''='", TagBuilder = XPathTagBuilder },
                    //new Example {FormatFunc = FormatHelper<Xml>, Prefix="xml", Formatter="<?xml version=\"1.0\"?>{0}", Replacer="<!DOCTYPE foo [<!ELEMENT foo ANY><!ENTITY xxe SYSTEM \"file:///textfile.txt\">]><foo>&xxe;</foo>", TagBuilder = XMLTagBuilder },
        };

        #region Builders
        protected static string FormatHelper<T>(string format, string[] parameters) where T : LanguageProvider
        {
            return SafeString<T>.TryFormat(format, out var result, parameters) ? result : null;
        }

        protected static string XPathTagBuilder(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) { return string.Empty; }

            var doc = new System.Xml.XmlDocument();
            doc.Load(System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, "xmldocument.xml"));
            var root = doc.DocumentElement;

            // Add the namespace.
            var nsmgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("bk", "urn:newbooks-schema");

            var nodes = root.SelectNodes(path, nsmgr);

            var builder = new System.Text.StringBuilder();
            foreach (System.Xml.XmlNode row in nodes)
            {

                builder.AppendFormat("<br>{0} - {1}", row["title"].InnerText, row["author"].InnerText);
            }
            return builder.ToString();
        }

        protected static string XMLTagBuilder(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) { return string.Empty; }

            var settings = new System.Xml.XmlReaderSettings();
            settings.DtdProcessing = System.Xml.DtdProcessing.Parse;
            var xmlDoc = new System.Xml.XmlDocument();

            var xmlBytes = Encoding.ASCII.GetBytes(xml);

            var doc = new System.Xml.XmlDocument();
            doc.Load(System.Xml.XmlReader.Create(new System.IO.MemoryStream(xmlBytes), settings));
            return doc.LastChild.OuterXml;
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

            using (var connection = new System.Data.SqlClient.SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database.mdf;Integrated Security=True"))
            {
                using (var adapter = new System.Data.SqlClient.SqlDataAdapter())
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = request;
                        adapter.SelectCommand = command;

                        using (var dataSet = new System.Data.DataSet())
                        {
                            adapter.Fill(dataSet);

                            var builder = new System.Text.StringBuilder();
                            foreach (System.Data.DataRow row in dataSet.Tables[0].Rows)
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
                Request.Params[example.FormatParam] ?? string.Empty,
                (Request.Params[example.ParameterParam] ?? string.Empty).Split(',')
            );

            string operationResult;

            if (formatResult != null)
            {
                operationResult = example.TagBuilder(formatResult);
            }
            else
            {
                formatResult = "Attack detected!";
                operationResult = "Not executed.";
            }

            return (formatResult, operationResult);
        }
    }
}