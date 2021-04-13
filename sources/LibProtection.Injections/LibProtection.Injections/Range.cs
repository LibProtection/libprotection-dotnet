using System;
using System.Diagnostics;

namespace LibProtection.Injections
{
    [DebuggerDisplay("{ToString()}")]
    public struct Range : IEquatable<Range>
    {
        public int LowerBound { get; }
        public int UpperBound { get; }
        public int Length => UpperBound - LowerBound;

        public Range(int lowerBound, int upperBound)
        {
            LowerBound = lowerBound;
            UpperBound = upperBound;
        }

        public Range Offset(int value)
            => new Range(LowerBound + value, UpperBound + value);
        

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
        /// Substraction a ranges from the current one.
        /// </summary>
        /// <param name="range">Range to be subtracted from this one.</param>
        /// <param name="modifiedRange">First part of the range after substraction.</param>
        /// <param name="newRange">Second part of the range after substraction, if present; otherwise <c>default</c>.</param>
        /// <returns><c>true</c> If substraction results in two disjoiinted ranges; otherwise, <c>false</c>.</returns>
        internal bool TrySubstract(Range range, out Range modifiedRange, out Range newRange)
        {
            if (!Overlaps(range))
            {
                modifiedRange = this;
                newRange = new Range();
                return false;
            }

            if (LowerBound <= range.LowerBound)
            {
                modifiedRange = new Range(LowerBound, range.LowerBound);

                if (UpperBound > range.UpperBound)
                {
                    newRange = new Range(range.UpperBound, UpperBound);
                    return true;
                }
            }
            else
            {
                var newLowerBound = UpperBound > range.UpperBound ? range.UpperBound : LowerBound;
                var newUpperBound = UpperBound > range.UpperBound ? UpperBound : LowerBound;
                
                modifiedRange = new Range(newLowerBound, newUpperBound);
            }
            newRange = new Range();
            return false;
        }
    }
}