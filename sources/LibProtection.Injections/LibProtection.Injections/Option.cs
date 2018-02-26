using System.Collections.Generic;

namespace LibProtection.Injections
{
    public sealed class Option<T>
    {
        public bool HasValue { get; }
        public T Value { get; }

        public Option(T value)
        {
            HasValue = true;
            Value = value;
        }

        private Option()
        {
        }

        public static Option<T> None = new Option<T>();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Option<T>) obj);
        }

        private bool Equals(Option<T> other)
        {
            return HasValue == other.HasValue && EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (HasValue.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(Value);
            }
        }
    }
}
