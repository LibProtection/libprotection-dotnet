using System;

namespace LibProtection.Injections
{
    public static partial class SafeString<T> where T: LanguageProvider
    {
        /// <summary>
        /// Formats the composite format string by using the formatting conventions of the current culture,
        /// detecting injection attacks along the way.
        /// </summary>
        /// <param name="formattable">A composite format string.</param>
        /// <returns>A result string formatted by using the conventions of the current culture.</returns>
        /// <exception cref="AttackDetectedException">Injection attack is detected.</exception>
        public static string Format(FormattableString formattable)
        {
            if (TryFormat(formattable, out var formatted))
            {
                return formatted;
            }

            throw new AttackDetectedException();
        }

        /// <summary>
        /// Formats the composite format string by using the formatting conventions of the current culture,
        /// detecting injection attacks along the way.
        /// </summary>
        /// <param name="formattable">A composite format string.</param>
        /// <param name="formatted">A result string formatted by using the using the conventions of the current culture, if no attack is detected; otherwise, <c>null</c>.
        /// This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if no attack is detected; otherwise, <c>false</c>.</returns>
        public static bool TryFormat(FormattableString formattable, out string formatted)
        {
            return TryFormat(formattable.Format, out formatted, formattable.GetArguments());
        }
    }
}
