using System;
using System.Diagnostics;

namespace LibProtection.Injections
{
    [DebuggerDisplay("[{LowerBound}..{UpperBound}]")]
    public struct Range : IEquatable<Range>
    {
        public int LowerBound { get; }
        public int UpperBound { get; }
        public int Length => UpperBound - LowerBound + 1;

        public Range(int lowerBound, int upperBound)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public bool Contains(int point)
        {
            return LowerBound <= point && UpperBound >= point;
        }

        public bool Contains(Range range)
        {
            return Contains(range.LowerBound) && Contains(range.UpperBound);
        }

        public bool Overlaps(Range range)
        {
            return LowerBound >= range.LowerBound && UpperBound <= range.UpperBound
                   || Contains(range.LowerBound)
                   || Contains(range.UpperBound);
        }

        public override string ToString()
        {
            return $"[{LowerBound}..{UpperBound}]";
        }

        public bool Equals(Range other)
        {
            return LowerBound.Equals(other.LowerBound) && UpperBound.Equals(other.UpperBound);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) { return false; }
            return obj is Range && Equals((Range) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = LowerBound.GetHashCode();
                hashCode = (hashCode * 397) ^ UpperBound.GetHashCode();
                return hashCode;
            }
        }
    }
}