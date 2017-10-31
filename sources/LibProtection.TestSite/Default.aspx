<%@ Page Title="LibProtection Test Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LibProtection.TestSite._Default"%>

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
                <% 
                foreach (var example in Examples) {
                    var parameterValue = Request.Params[example.ParameterParam] ?? example.Replacer;
                    var formatterValue = Request.Params[example.FormatParam] ?? example.Formatter;
                %>

                <div id ="<%=example.Prefix %>" class="exampleElement" style="display: none">
                    <div style="display: table; width:60%;">
                        <div style="display: table-cell; vertical-align: middle;">
                            <asp:Label runat="server" Text="Format: " />
                            <%
                                if (InputsAreDisabled)
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

                    <% var results = GetResultsFor(example); %>
                    <asp:Label runat="server" Text="Format result: " />
                    <%: results.FormatResult %>
                    <br />
                    <asp:Label runat="server" Text="Operation result: " />
                    <%= results.OperationResult %>

                    <script>
                        <%= example.ButtonId %>.onclick = function () {
                            <%if (InputsAreDisabled) {%>
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
                firstVisible = $("div[id|=<%= SelectedItemTag %>]");

                options = $("div[class|=exampleElement]");
                grammarSelectorId.onchange = function () {
                    options.hide();
                    $("div[id|="+grammarSelectorId.value).show();
                }
            </script>
        </div>
    </div>
</asp:Content>