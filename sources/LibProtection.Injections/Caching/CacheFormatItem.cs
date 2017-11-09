namespace LibProtection.Injections.Caching
{
    public struct CacheFormatItem
    {
        public string Format;
        public object[] Args;

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is CacheFormatItem) || this.GetHashCode() != obj.GetHashCode()) { return false; }

            var that = (CacheFormatItem)obj;

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
                hash = hash * 23 + this.Format != null ? this.Format.GetHashCode() : 0;
                if (this.Args != null)
                {
                    for (int i = 0; i < this.Args.Length; i++)
                    {
                        hash = hash * 23 + (Args[i] != null ? Args[i].GetHashCode() : 0);
                    }
                }

                return hash;
            }
        }
    }
}
