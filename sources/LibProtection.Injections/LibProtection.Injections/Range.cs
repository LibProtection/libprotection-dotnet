using System;
using System.Diagnostics;

namespace LibProtection.Injections
{
    [DebuggerDisplay("{ToString()}")]
    public struct Range : IEquatable<Range>
    {
        public int LowerBound { get; set; }
        public int UpperBound { get; set; }
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

        public bool Touches(Range range)
        {
            return Touches(range.LowerBound) || Touches(range.UpperBound);
        }

        public bool Touches(int point)
        {
            return LowerBound-1 <= point && point <= UpperBound;
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
            return obj is Range && Equals((Range)obj);
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

        public Range ConvexHull(Range other)
        {
            return new Range(
                Math.Min(LowerBound, other.LowerBound),
                Math.Max(UpperBound, other.UpperBound)
                );
        }

        /// <summary>
        /// Substracts range from the current range assuming ranges do overlap. Returns true if substraction splits existing range in two.
        /// New range is assinged to out parameter newRange.
        /// </summary>
        internal bool TrySubstract(Range range, out Range newRange)
        {
            if (LowerBound <= range.LowerBound)
            {
                var oldUpperBound = UpperBound;
                UpperBound = range.LowerBound;

                if (oldUpperBound > range.UpperBound)
                {
                    newRange = new Range(range.UpperBound, oldUpperBound);
                    return true;
                }
            }
            else
            {
                if (UpperBound > range.UpperBound)
                {
                    LowerBound = range.UpperBound;
                }
                else
                {
                    UpperBound = LowerBound;
                }
            }
            newRange = default;
            return false;
        }
    }
}