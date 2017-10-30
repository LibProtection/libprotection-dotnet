namespace LibProtection.Injections
{
    internal class IslandDto
    {
        public LanguageProvider LanguageProvider { get; }
        public int Offset { get; }
        public string Text { get; }

        public IslandDto(LanguageProvider languageProvider, int offset, string text)
        {
            LanguageProvider = languageProvider;
            Offset = offset;
            Text = text;
        }
    }
}
