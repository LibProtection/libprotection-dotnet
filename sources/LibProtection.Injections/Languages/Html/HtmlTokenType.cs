namespace LibProtection.Injections
{
    internal enum HtmlTokenType
    {
        HtmlComment = 1,
        HtmlConditionalComment = 2,
        XmlDeclaration = 3,
        Cdata = 4,
        Dtd = 5,
        SpecialTag = 6,
        TagOpen = 7,
        HtmlText = 8,
        ErrorText = 9,
        TagClose = 10,
        TagSlashClose = 11,
        TagSlash = 12,
        TagEquals = 13,
        TagWhiteSpace = 14,
        AttributeName = 15,
        ErrorTag = 16,
        AttributeWhiteSpace = 17,
        AttributeSlash = 18,
        AttributeValue = 19,
        ErrorAttribute = 20
    }
}