using System.Collections.Generic;

namespace LibProtection.Injections.Formatting
{
    internal struct FormatResult
    {
        public FormatResult(Token[] tokens, bool isAttackDetected, int injectionPointIndex, string formattedString)
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

        public override bool Equals(object obj)
        {
            if (!(obj is FormatResult))
            {
                return false;
            }

            var result = (FormatResult)obj;
            return Extensions.ArrayExtensions.ArraysEqual(Tokens, result.Tokens) &&
                   IsAttackDetected == result.IsAttackDetected &&
                   InjectionPointIndex == result.InjectionPointIndex &&
                   FormattedString == result.FormattedString;
        }

        public override int GetHashCode()
        {
            var hashCode = -80248532;
            unchecked
            {
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + Extensions.ArrayExtensions.ArrayGetHashCode(Tokens);
                hashCode = hashCode * -1521134295 + IsAttackDetected.GetHashCode();
                hashCode = hashCode * -1521134295 + InjectionPointIndex.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FormattedString);
            }
            return hashCode;
        }

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
