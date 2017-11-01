namespace LibProtection.Injections
{
    internal enum HtmlTokenType
    {
        HtmlComment = 1,
        HtmlConditionalComment = 2,
        XmlDeclaration = 3,
        Cdata = 4,
        Dtd = 5,
        Scriptlet = 6,
        SeaWs = 7,
        ScriptOpen = 8,
        StyleOpen = 9,
        TagOpen = 10,
        HtmlText = 11,
        ErrorText = 12,
        TagClose = 13,
        TagSlashClose = 14,
        TagSlash = 15,
        TagEquals = 16,
        TagName = 17,
        TagWhitespace = 18,
        ErrorTag = 19,
        ScriptBody = 20,
        ScriptShortBody = 21,
        ErrorScript = 22,
        StyleBody = 23,
        StyleShortBody = 24,
        ErrorStyle = 25,
        AttvalueValue = 26,
        Attribute = 27,
        ErrorAttvalue = 28
    }
}