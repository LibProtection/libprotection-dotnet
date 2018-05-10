namespace LibProtection.Injections.Formatting
{
    class Fragment
    {
        public string FormattedValue { get; }
        public bool IsSafe { get; }
        public int FragmentArgumentIndex { get; }

        public Fragment(string formattedValue, bool isSafe, int fragmentArgumentIndex)
        {
            FormattedValue = formattedValue;
            IsSafe = isSafe;
            FragmentArgumentIndex = fragmentArgumentIndex;
        }
    }
}