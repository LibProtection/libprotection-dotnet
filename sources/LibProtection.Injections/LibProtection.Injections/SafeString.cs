namespace LibProtection.Injections
{
    public static partial class SafeString<T> where T : LanguageProvider
    {
        public static string Format(string format, params object[] args) 
        {
            if (TryFormat(format, out var formatted, args))
            {
                return formatted;
            }
            
            throw new AttackDetectedException();
        }

        public static bool TryFormat(string format, out string formatted, params object[] args) 
        {
            return FormatProvider<T>.TryFormat(format, out formatted, args);
        }
    }
}