using System.Diagnostics.CodeAnalysis;

namespace LibProtection.Injections
{
    public struct FormatCacheItem
    {
        public readonly string Format;
        public readonly object[] Args;

        public FormatCacheItem(string format, object[] args)
        {
            Format = format;
            Args = args;
        }

        [SuppressMessage("ReSharper", "ArrangeThisQualifier")]
        public override bool Equals(object obj)
        {
            if (!(obj is FormatCacheItem) || GetHashCode() != obj.GetHashCode()) { return false; }

            var that = (FormatCacheItem)obj;

            if (!Equals(this.Format, that.Format)) { return false; }

            if (this.Args == null && that.Args != null) { return false; }
            if (this.Args != null && that.Args == null) { return false; }
            if (that.Args == null) { return true; }
            if (this.Args.Length != that.Args.Length) { return false; }
            for (int i = 0; i < this.Args.Length; i++)
            {
                if (!Equals(this.Args[i], that.Args[i])) { return false; }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Format != null ? Format.GetHashCode() : 0);
                if (Args != null)
                {
                    for (int i = 0; i < Args.Length; i++)
                    {
                        hash = hash * 23 + (Args[i] != null ? Args[i].GetHashCode() : 0);
                    }
                }

                return hash;
            }
        }
    }
}
