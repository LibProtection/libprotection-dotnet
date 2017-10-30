namespace LibProtection.Injections
{
    internal class Fragment
    {
        public string FormattedValue { get; }
        public bool IsSafe { get; }

        public Fragment(string formattedValue, bool isSafe)
        {
            FormattedValue = formattedValue;
            IsSafe = isSafe;
        }
    }
}