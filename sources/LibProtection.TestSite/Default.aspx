<%@ Page Title="LibProtection Test Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="LibProtection.TestSite.Default"%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <% 
        Response.AddHeader("X-XSS-Protection", "0");
        var id = Request.Params["Id"] ?? Examples.Keys.First();
    %>
    <div>
        <script>
            document.addEventListener("DOMContentLoaded", function () {
                window.grammarSelectorId.value = "<%= id %>";
                window.grammarSelectorId.onchange = function () {
                    window.location = "Default.aspx?Id=" + encodeURIComponent(window.grammarSelectorId.value);
                }
            });
        </script>

        <div id="exampleFormsId">
                <%
                    if (!Examples.TryGetValue(id, out var example))
                    {
                    %>
                        <div class="alert alert-dismissible alert-warning">
                            <button type="button" class="close" data-dismiss="alert">&times;</button>
                            <strong>Unknown language provider: <%: id %></strong>
                        </div>
                    <%
                    } else {
                        var parameters = Request.Params["Parameters"] ?? example.Replacer;
                        var format = Request.Params["Format"] ?? example.Formatter;
                    %>

                    <div style="width:100%">
                        <div style="margin-top: 20px">
                            <h5>Vulnerable operation:</h5>
                            <%= example.Operation %>
                        </div>
                        <div style="display: table; width: 100%; margin-top: 20px">
                            <div style="padding-right: 10px; display: table-cell; width: 50%">
                                <h5>Format string:</h5>
                                <%
                                    if (InputsAreDisabled)
                                    {%>
                                        <label ID="format"><%: format %></label>
                                    <%}else{%>
                                        <input class="form-control" type="text" value="<%: format %>" ID="format" />
                                    <%}%>
                            </div>
                            <div style="padding-left: 10px; display: table-cell; width: 50%">
                                <h5>Arguments <small class="text-muted">(one per line):</small></h5>
                                <textarea class="form-control" rows="5" ID="parameters"><%: parameters %></textarea>
                            </div>
                        </div>
                        <div style="text-align: right; margin-top: 20px">
                            <input style="width: 160px; display: inline-block" class="form-control" type="button" id="button" value="Try format" />
                        </div>
                    </div>
                    <% try
                       {
                    %>
                            <div style="margin-top: 20px">
                                <% var results = GetResultsFor(example, format, parameters); %>
                                <h5>Format result:</h5>
                                <%: results.FormatResult %>
                            </div>
                            <div style="margin-top: 20px">
                                <h5>Vulnerable operation result:</h5>
                                <%= string.IsNullOrEmpty(results.OperationResult) ?
                                        "<span class=\"text-warning\">None</span>" :
                                        $"<span class=\"text-success\" id=\"operationResult\">{results.OperationResult}</span>" %>
                            </div>
                    <%
                       }
                       catch (LibProtection.Injections.AttackDetectedException)
                       {
                    %>
                        <div class="alert alert-dismissible alert-danger">
                            <button type="button" class="close" data-dismiss="alert">&times;</button>
                            <strong>Attack detected!</strong> Vulnerable operation was not executed
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
                        button.onclick = function () {
                            <%if (InputsAreDisabled) {%>
                            var formatValue = window.format.textContent;
                            <%}else{%>
                            var formatValue = window.format.value;
                            <%}%>
                            window.location =
                                "Default.aspx?Id=" + encodeURIComponent("<%: id %>") +
                                "&Format=" + encodeURIComponent(formatValue) +
                                "&Parameters=" + encodeURIComponent(window.parameters.value);
                        }
                    </script>
                    <hr />
               </div>
            <% } %>

            <script>
                <% foreach (var item in Examples.Keys)
                   {
                %>
                    var option = document.createElement("option");
                    option.text = "<%= item %>";
                    grammarSelectorId.add(option);
                <% } %>
            </script>
    </div>
</asp:Content>