using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;
using LibProtection.Injections;
using System.Text;

namespace LibProtection.Playground.Pages
{

    public class IndexModel : PageModel
    {
        private IConfiguration configuration;

        public IndexModel(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            this.configuration = configuration;
            Examples["FilePath"].Format = System.IO.Path.Combine(hostingEnvironment.WebRootPath, "Assets", "{0}");
            Examples["FilePath"].TagBuilder = s => PathTagBuilder(s, hostingEnvironment);
            Examples["Sql"].TagBuilder = s => SqlRequestTagBuilder(s, hostingEnvironment);
        }

        public class Example
        {
            public string Operation { get; set; }
            public Func<string, string[], string> FormatFunc { get; set; }
            public string Format { get; set; }
            public string Parameters { get; set; }
            public Func<string, string> TagBuilder { get; set; }
        }

        public Dictionary<string, Example> Examples = new Dictionary<string, Example>
        {
            {
                "Html",
                new Example
                {
                    Operation = "Renders the given HTML markup on the client side.",
                    FormatFunc = FormatHelper<Html>,
                    Format = "<a href='{0}' onclick='alert(\"{1}\");return false'>{2}</a>",
                    Parameters = "Index\r\nHello from embedded JavaScript code!\r\nThis site's home page",
                    TagBuilder = s => s
                }
            },
            {
                "JavaScript",
                new Example
                {
                    Operation = "Executes the given JavaScript code on the client side.",
                    FormatFunc = FormatHelper<JavaScript>,
                    Format = "operationResult.innerText = '{0}';",
                    Parameters = "Hello from internal JavaScript code!",
                    TagBuilder = s => $"<script>\r\n{s}\r\n</script>"
                }
            },
            {
                "Sql",
                new Example
                {
                    Operation = "Executes the given SQL query on the sever side and outputs its results.",
                    FormatFunc = FormatHelper<Sql>,
                    Format = "SELECT * FROM myTable WHERE id = {0} AND myColumn = '{1}'",
                    Parameters = "1\r\nvalue1",
                    TagBuilder = null
                }
            },
            {
                "Url",
                new Example
                {
                    Operation = "Uses the given URL on the client side to load and execute an external JavaScript code.",
                    FormatFunc = FormatHelper<Url>,
                    Format = "{0}/{1}",
                    Parameters = "Assets\r\njsFile.js",
                    TagBuilder = s => $"<script src=\"{s}\"></script>"
                }
            },
            {
                "FilePath",
                new Example
                {
                    Operation = "Reads content of the given local file on the server side and outputs it.",
                    FormatFunc = FormatHelper<FilePath>,
                    Format = null,
                    Parameters = "textFile.txt",
                    TagBuilder = null
                }
            },
        };

        protected static string FormatHelper<T>(string format, object[] parameters) where T : LanguageProvider
        {
            return SafeString<T>.Format(format, parameters);
        }

        protected static string SqlRequestTagBuilder(string request, IHostingEnvironment hostingEnvironment)
        {
            if (string.IsNullOrWhiteSpace(request)) { return string.Empty; }
            var connectionStringBuilder = new SqliteConnectionStringBuilder();
            connectionStringBuilder.DataSource = System.IO.Path.Combine(hostingEnvironment.WebRootPath, "Assets", "Database.sqlite");
            connectionStringBuilder.Mode = SqliteOpenMode.ReadOnly;

            string result;
            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = request;
                    using (var reader = command.ExecuteReader())
                    {
                        var builder = new StringBuilder();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                builder.AppendFormat("Id: {0}, myColumn: '{1}'<br>", reader["Id"], reader["myColumn"]);
                            }
                        }
                        result = builder.ToString();
                    }
                }
                connection.Close();
            }
            return result;
        }

        protected static string PathTagBuilder(string path, IHostingEnvironment hostingEnvironment)
        {
            var filePath = System.IO.Path.Combine(hostingEnvironment.ContentRootPath, path);
            return System.IO.File.ReadAllText(filePath);
        }

        public bool InputsAreDisabled
        {
            get
            {
                bool.TryParse(configuration.GetSection("AppConfiguration")["DisableArbitraryFormatFeature"], out var disabledInputs);
                return disabledInputs;
            }
        }

        public (string FormatResult, string OperationResult) GetResultsFor(Example example, string format,
            string parameters)
        {
            var formatResult = example.FormatFunc(
                format,
                parameters.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
            );

            return (formatResult, example.TagBuilder(formatResult));
        }
    }
}
