namespace LibProtection.Injections
{
    internal struct FormatResult
    {
        private FormatResult(Token[] tokens, bool isAttackDetected, int injectionPointIndex, string formattedString)
        {
            Tokens = tokens;
            IsAttackDetected = isAttackDetected;
            InjectionPointIndex = injectionPointIndex;
            FormattedString = formattedString;
        }

        public static FormatResult Success(Token[] tokens, string formattedString)
            => new FormatResult(tokens, false, -1, formattedString);

        public static FormatResult Fail(Token[] tokens, int injectionPointIndex)
            => new FormatResult(tokens, true, injectionPointIndex, null);

        /// <summary>
        /// array of tokenization artifacts
        /// </summary>
        public Token[] Tokens { get; }
        /// <summary>
        /// true if an attack was detected during formatting
        /// </summary>
        public bool IsAttackDetected { get; }
        /// <summary>
        /// if  IsAttackDetected == true, contains an index of attacked args element 
        /// </summary>
        public int InjectionPointIndex { get; }
        /// <summary>
        /// result of Formatting (should be null if IsAttackDetected == true)
        /// </summary>
        public string FormattedString { get; }
    }
}
