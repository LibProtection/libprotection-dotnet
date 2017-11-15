<%@ Page Title="LibProtection Test Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LibProtection.TestSite._Default"%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <% Response.AddHeader("X-XSS-Protection", "0"); %>
    <div>
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
                    <div style="width:100%">
                        <div style="margin-top: 20px">
                            <h5><asp:Label runat="server" Text="Operation:" /></h5>
                            <%= example.Operation %>
                        </div>
                        <div style="display: table; width: 100%; margin-top: 20px">
                            <div style="padding-right: 10px; display: table-cell">
                                <h5><asp:Label runat="server" Text="Format string: " /></h5>
                                <%
                                    if (InputsAreDisabled)
                                    {%>
                                        <label ID="<%= example.FormatId %>"><%: formatterValue %></label>
                                    <%}else{%>
                                        <input class="form-control" type="text" value="<%: formatterValue %>" ID="<%= example.FormatId %>" />
                                    <%}%>
                            </div>
                            <div style="padding-left: 10px; display: table-cell">
                                <h5><asp:Label runat="server" Text="Arguments (one per line):" /></h5>
                                <textarea class="form-control" rows="5" ID="<%= example.ParameterId %>"><%: parameterValue %></textarea>
                            </div>
                        </div>
                        <div style="text-align: right; margin-top: 20px">
                            <input style="width: 160px; display: inline-block" class="form-control" type="button" id="<%= example.ButtonId %>" value="Try format" />
                        </div>
                    </div>
                    <% try
                       {
                           if (Request.Params[example.ParameterParam] != null || Request.Params[example.FormatParam] != null)
                           { %>
                                <div style="margin-top: 20px">
                                    <% var results = GetResultsFor(example); %>
                                    <h5>Format result:</h5>
                                    <%: results.FormatResult %>
                                </div>
                                <div style="margin-top: 20px">
                                    <h5>Operation result:</h5>
                                    <%= string.IsNullOrEmpty(results.OperationResult) ?
                                            "<span class=\"text-warning\">None</span>" :
                                            $"<span class=\"text-success\">{results.OperationResult}</span>" %>
                                </div>
                    <%     }
                       }
                       catch (LibProtection.Injections.AttackDetectedException e)
                       {
                    %>
                        <div class="alert alert-dismissible alert-danger">
                            <button type="button" class="close" data-dismiss="alert">&times;</button>
                            <strong>Attack detected!</strong>
                        </div>                    
                    <%
                       }
                       catch (Exception e)
                       {
                    %>
                        <div class="alert alert-dismissible alert-warning">
                            <button type="button" class="close" data-dismiss="alert">&times;</button>
                            <strong>Exception occured: <%= e.Message %></strong>
                        </div>
                    <%
                       }
                    %>
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