using System;

namespace LibProtection.Injections
{
    public static partial class SafeString<T> where T: LanguageProvider
    {
        public static string Format(FormattableString formattable)
        {
            if (TryFormat(formattable, out var formatted))
            {
                return formatted;
            }

            throw new AttackDetectedException();
        }

        public static bool TryFormat(FormattableString formattable, out string formatted)
        {
            return TryFormat(formattable.Format, out formatted, formattable.GetArguments());
        }
    }
}
