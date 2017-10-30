<%@ Page Title="LibProtection Test Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" %>
<%@ Import Namespace="LibProtection.Injections" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <% Response.AddHeader("X-XSS-Protection", "0"); %>

    <div>
        <p style="text-align: center; font-size: 20px;">LibProtection Test Site</p>

        <script>
            document.addEventListener("DOMContentLoaded", function () {
                options = $("div[class|=exampleElement]");
                options.hide();
                firstVisible.show();
                grammarSelectorId.value = firstVisible[0].id;
            });
        </script>

        <div id="exampleFormsId">
            <% string selectedItemTag = Examples.First().Prefix;
                bool disabledInputs = false;
                bool.TryParse(ConfigurationManager.AppSettings["DisableArbitraryFormatFeature"], out disabledInputs);
                foreach (var example in Examples) {
                    if(Request.Params[example.FormatParam] != null && Request.Params[example.ParameterParam] != null)
                    {
                        selectedItemTag = example.Prefix;
                    }
                    var parameterValue = Request.Params[example.ParameterParam] ?? example.Replacer;
                    var formatterValue = Request.Params[example.FormatParam] ?? example.Formatter;

               %>

                <div id ="<%=example.Prefix %>" class="exampleElement" style="display: none">
                    <div style="display: table; width:60%;">
                        <div style="display: table-cell; vertical-align: middle;">
                            <asp:Label runat="server" Text="Format: " />
                            <%
                                if (disabledInputs)
                                {%>
                                    <label ID="<%= example.FormatId %>"><%: formatterValue %></label>
                                <%}else{%>
                                    <input type="text" value="<%: formatterValue %>" ID="<%= example.FormatId %>" />
                                <%}%>
                        </div>
                        <div style="display: table-cell; vertical-align: middle;">
                            <asp:Label runat="server" Text="Parameter: " />
                            <input type="text" value="<%: parameterValue %>" ID="<%= example.ParameterId %>" />
                        </div>
                        <div style="display: table-cell; vertical-align: middle;">
                            <input type="button" id="<%= example.ButtonId %>" value="Proceed!" />
                        </div>

                    </div>
                    <br />
                    <%  
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
                    %>
                    <asp:Label runat="server" Text="Format result: " />
                    <%: formatResult %>
                    <br />
                    <asp:Label runat="server" Text="Operation result: " />
                    <%= operationResult %>

                    <script>
                        <%= example.ButtonId %>.onclick = function () {
                            <%if (disabledInputs) {%>
                                var formatValue = <%=example.FormatId%>.textContent;
                            <%}else{%>
                                var formatValue = <%=example.FormatId%>.value;
                            <%}%>
                            window.location = "Default.aspx?<%=example.FormatParam%>=" + encodeURIComponent(formatValue) + "&<%=example.ParameterParam%>=" + encodeURIComponent(<%=example.ParameterId%>.value);
                        }
                        var option = document.createElement("option");
                        option.text = "<%= example.Prefix %>";
                        var option = grammarSelectorId.add(option);
                        
                    </script>
                    <hr />
               </div>
            <% } %>

            <script>
                firstVisible = $("div[id|=<%= selectedItemTag %>]");

                options = $("div[class|=exampleElement]");
                grammarSelectorId.onchange = function () {
                    options.hide();
                    $("div[id|="+grammarSelectorId.value).show();
                }
            </script>

            <script runat="server">

                public struct FormatResult
                {
                    public bool Successfully { get; }
                    public string FormattedValue { get; }

                    public FormatResult(bool successfully, string formattedValue)
                    {
                        Successfully = successfully;
                        FormattedValue = formattedValue;
                    }
                }

                public static string FormatHelper<T>(string format, string[] parameters) where T : LanguageProvider
                {
                    return SafeString<T>.TryFormat(format, out var result, parameters) ? result : null;
                }

                public static string XPathTagBuilder(string path)
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
                    foreach(System.Xml.XmlNode row in nodes)
                    {

                        builder.AppendFormat("<br>{0} - {1}", row["title"].InnerText, row["author"].InnerText);
                    }
                    return builder.ToString();
                }

                public static string XMLTagBuilder(string xml)
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

                public static string PathTagBuilder(string path)
                {
                    if(string.IsNullOrWhiteSpace(path)) { return string.Empty; }
                    var filePath = System.IO.Path.Combine(HttpRuntime.AppDomainAppPath, path);
                    if(!System.IO.File.Exists(filePath)) { return string.Empty; }
                    return System.IO.File.ReadAllText(filePath);
                }

                public static string SqlRequestTagBuilder(string request)
                {
                    if (string.IsNullOrWhiteSpace(request)) { return string.Empty; }
                    var result = string.Empty;

                    using(var connection = new System.Data.SqlClient.SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Database.mdf;Integrated Security=True"))
                    {
                        using(var adapter = new System.Data.SqlClient.SqlDataAdapter())
                        {
                            connection.Open();
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = request;
                                adapter.SelectCommand = command;

                                using(var dataSet = new System.Data.DataSet())
                                {
                                    adapter.Fill(dataSet);

                                    var builder = new System.Text.StringBuilder();
                                    foreach(System.Data.DataRow row in dataSet.Tables[0].Rows)
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

                public static Func<string, string> GetFormatTagBuilder(string formatter) => (arg) => string.Format(formatter, arg);

                public struct Example
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

                static string scriptVector = "<script>alert(0)</script" + ">";

                public static Example[] Examples =
                {
                    new Example {FormatFunc = FormatHelper<Html>, Prefix="html", Formatter="<b>{0}</b>", Replacer=">" + scriptVector, TagBuilder = GetFormatTagBuilder("{0}") },
                    new Example {FormatFunc = FormatHelper<JavaScript>, Prefix="javascript", Formatter="alert({0})", Replacer="\"/>Hello", TagBuilder = GetFormatTagBuilder("<label onclick={0}>ON CLICK</label>") },
                    new Example {FormatFunc = FormatHelper<Sql>, Prefix="sql", Formatter="select * from myTable where myColumn = '{0}'", Replacer="' OR 1 = 1 --", TagBuilder = SqlRequestTagBuilder },
                    new Example {FormatFunc = FormatHelper<Url>, Prefix="url", Formatter="./img/{0}", Replacer="../spanch.gif", TagBuilder = GetFormatTagBuilder("<img src=\"{0}\" />") },
                    new Example {FormatFunc = FormatHelper<FilePath>, Prefix="filepath", Formatter=@".\files\{0}", Replacer=@"..\textFile.txt", TagBuilder = PathTagBuilder },
                    //new Example {FormatFunc = FormatHelper<Xpath>, Prefix="xpath", Formatter=@"descendant::bk:book[bk:author='{0}']", Replacer="' or ''='", TagBuilder = XPathTagBuilder },
                    //new Example {FormatFunc = FormatHelper<Xml>, Prefix="xml", Formatter="<?xml version=\"1.0\"?>{0}", Replacer="<!DOCTYPE foo [<!ELEMENT foo ANY><!ENTITY xxe SYSTEM \"file:///textfile.txt\">]><foo>&xxe;</foo>", TagBuilder = XMLTagBuilder },
                };
            </script>
        </div>
    </div>
</asp:Content>
