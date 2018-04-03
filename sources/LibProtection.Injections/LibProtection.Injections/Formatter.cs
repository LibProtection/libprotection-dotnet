using System;
using System.Globalization;

namespace LibProtection.Injections
{
    internal class ArgumentWrapper
    {
        public object Argument { get; set; }
        public int Index { get; set; }
    }

    internal class Formatter<T> : ICustomFormatter where T: LanguageProvider
    {
        private readonly char _complementaryChar;

        public Formatter(char complementaryChar)
        {
            _complementaryChar = complementaryChar;
        }

        public string Format(string format, object argumentWrapperObj, IFormatProvider formatProvider)
        {
            if (!(formatProvider is FormatProvider<T> provider))
            {
                throw new FormatException($"Invalid format provider: {formatProvider}");
            }

            var argumentWrapper = (ArgumentWrapper)argumentWrapperObj;
            var arg = argumentWrapper.Argument;

            bool isSafe = false;
            string formattedValue = null;
            
            if (format != null)
            {
                var modifiers = format.Split(':');
                var modifierIndex = 0;
                
                while (modifierIndex < modifiers.Length)
                {
                    var modifier = modifiers[modifierIndex].ToLowerInvariant();
                    
                    switch (modifier)
                    {
                        case "safe":
                            isSafe = true;
                            break;
                            
                        default:
                            if (formattedValue == null && arg is IFormattable)
                            {
                                formattedValue = ((IFormattable) arg).ToString(modifier, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                throw new FormatException($"Invalid format modifier: {modifier}");
                            }
                            break;
                    }
                    
                    modifierIndex++;
                }                
            }

            if (formattedValue == null) { formattedValue = arg.ToString(); }
            
            provider.AddFragment(
                new Fragment(formattedValue, isSafe, argumentWrapper.Index)
            );

            return new string(_complementaryChar, formattedValue.Length);
        }
    }
}