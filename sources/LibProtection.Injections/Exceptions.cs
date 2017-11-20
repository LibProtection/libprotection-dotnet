using System;

namespace LibProtection.Injections
{
    public class AttackDetectedException : Exception { }

    public class LanguageNotSupportedException : Exception
    {
        public LanguageNotSupportedException(string message) : base(message) { }
    }
}
