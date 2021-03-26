using System;
using System.Diagnostics;

namespace LibProtection.Injections
{
    [DebuggerDisplay("{ToString()}")]
    public struct Range : IEquatable<Range>
    {
        public int LowerBound { get; private set; }
        public int UpperBound { get; private set; }
        public int Length => UpperBound - LowerBound;

        public Range(int lowerBound, int upperBound)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public void Offset(int value)
        {
            LowerBound += value;
            UpperBound += value;
        }

        public bool Contains(int point)
        {
            return LowerBound <= point && UpperBound > point;
        }

        public bool Contains(Range range)
        {
            return Contains(range.LowerBound) && Contains(range.UpperBound);
        }

        public bool Overlaps(Range range)
        {
            return LowerBound >= range.LowerBound && UpperBound <= range.UpperBound
                   || Contains(range.LowerBound)
                   || Contains(range.UpperBound - 1);
        }

        public override string ToString()
        {
            return Length != 0 ? $"[{LowerBound}..{UpperBound})" : $"[{LowerBound})";
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

        public static bool operator <(Range a, Range b)
        => a.UpperBound <= b.LowerBound;

        public static bool operator >(Range a, Range b)
        => a.LowerBound >= b.UpperBound;

        public Range ConvexHull(Range other)
        {
            return new Range(
                Math.Min(LowerBound, other.LowerBound),
                Math.Max(UpperBound, other.UpperBound)
                );
        }
    }
}