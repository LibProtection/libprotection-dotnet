using System;
using System.Collections.Generic;
using System.Text;

namespace LibProtection.Injections
{
    public class SafeStringBuilder<T> where T : LanguageProvider
    {
        private StringBuilder internalBuilder;
        //TODO: implement sorted storage for taintedRanges
        //TODO: when new range is added check if it can be merged with one or both of its neighbours
        internal readonly SortedRangesList taintedRanges = new SortedRangesList();

        #region Constructors
        public SafeStringBuilder()
        {
            internalBuilder = new StringBuilder();
        }

        public SafeStringBuilder(int capacity)
        {
            internalBuilder = new StringBuilder(capacity);
        }

        public SafeStringBuilder(string value, bool isSafe = false)
        {
            internalBuilder = new StringBuilder(value);
            if (!isSafe)
            {
                taintedRanges.AddLast(new Range(0, value.Length));
            }
        }

        public SafeStringBuilder(int capacity, int maxCapacity)
        {
            internalBuilder = new StringBuilder(capacity, maxCapacity);
        }

        public SafeStringBuilder(string value, int capacity, bool isSafe = false)
        {
            internalBuilder = new StringBuilder(value, capacity);
            if (!isSafe)
            {
                taintedRanges.AddLast(new Range(0, value.Length));
            }
        }

        public SafeStringBuilder(string value, int startIndex, int length, int capacity, bool isSafe = false)
        {
            internalBuilder = new StringBuilder(value, startIndex, length, capacity);
            if (!isSafe)
            {
                taintedRanges.AddLast(new Range(0, length));
            }
        }
        #endregion constructors

        #region Append
        public SafeStringBuilder<T> Append(StringBuilder value)
        {
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + value.Length));
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(StringBuilder value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(UInt16 value)
        {
            var strValue = value.ToString();
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(UInt16 value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(UInt32 value)
        {
            var strValue = value.ToString();
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(UInt32 value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(UInt64 value)
        {
            var strValue = value.ToString();
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(UInt64 value)
        {
            internalBuilder.Append(value);
            return this;
        }


        public SafeStringBuilder<T> Append(char[] value, int startIndex, int charCount)
        {
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + charCount));
            internalBuilder.Append(value, startIndex, charCount);
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(char[] value, int startIndex, int charCount)
        {
            internalBuilder.Append(value, startIndex, charCount);
            return this;
        }

        public SafeStringBuilder<T> Append(string value)
        {
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + value.Length));
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(string value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(string value, int startIndex, int count)
        {
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + count));
            internalBuilder.Append(value, startIndex, count);
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(string value, int startIndex, int count)
        {
            internalBuilder.Append(value, startIndex, count);
            return this;
        }

        public SafeStringBuilder<T> Append(Char value, Int32 repeatCount)
        {
            internalBuilder.Append(value, repeatCount);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + repeatCount));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(Char value, Int32 repeatCount)
        {
            internalBuilder.Append(value, repeatCount);
            return this;
        }

        public SafeStringBuilder<T> Append(float value)
        {
            var strValue = value.ToString();
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(float value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(bool value)
        {
            internalBuilder.Append(value);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + (value ? 4 : 5)));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(bool value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(char value)
        {
            internalBuilder.Append(value);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + 1));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(char value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(char[] value)
        {
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + value.Length));
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(char[] value)
        {
            internalBuilder.Append(value);
            return this;
        }
        public SafeStringBuilder<T> Append(decimal value)
        {
            var strValue = value.ToString();
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            return this;
        }

        public SafeStringBuilder<T> UnchekedAppend(decimal value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(byte value)
        {
            var strValue = value.ToString();
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(byte value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(Int16 value)
        {
            var strValue = value.ToString();
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(Int16 value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(Int32 value)
        {
            var strValue = value.ToString();
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(Int32 value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(Int64 value)
        {
            var strValue = value.ToString();
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(Int64 value)
        {
            internalBuilder.Append(value);
            return this;
        }


        public SafeStringBuilder<T> Append(object value)
        {
            var strValue = value.ToString();
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(object value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(double value)
        {
            var strValue = value.ToString();
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(double value)
        {
            internalBuilder.Append(value);
            return this;
        }
        #endregion Append

        #region AppendFormat

        public SafeStringBuilder<T> AppendFormat(string format, params object[] args)
        {
            internalBuilder.Append(SafeString<T>.Format(format, args));
            return this;
        }

        public SafeStringBuilder<T> UnsafeAppendFormat(string format, params object[] args)
        {
            internalBuilder.AppendFormat(format, args);
            return this;
        }

        #endregion AppendFormat

        #region AppendLine
        public SafeStringBuilder<T> AppendLine()
        {
            internalBuilder.AppendLine();
            return this;
        }

        public SafeStringBuilder<T> AppendLine(string value)
        {
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + value.Length));
            internalBuilder.AppendLine(value);
            internalBuilder.AppendLine();
            return this;
        }
        #endregion AppendLine

        public SafeStringBuilder<T> Clear()
        {
            taintedRanges.Clear();
            internalBuilder.Clear();
            return this;
        }

        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            internalBuilder.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        public int EnsureCapacity(int capacity)
            => internalBuilder.EnsureCapacity(capacity);

        //TODO: add all insert overloads
        #region Insert
        public SafeStringBuilder<T> Insert(int index, string value)
        {
            var newRange = new Range(index, index + value.Length);
            taintedRanges.SafeInsert(newRange);
            internalBuilder.Insert(index, value);
            return this;
        }

        public SafeStringBuilder<T> UncheckedInsert(int index, string value)
        {
            var newRange = new Range(index, index + value.Length);
            taintedRanges.UncheckedInsert(newRange);
            internalBuilder.Insert(index, value);
            return this;
        }
        #endregion Insert

        #region Remove
        public SafeStringBuilder<T> Remove(int startIndex, int length)
        {
            var newRange = new Range(startIndex, startIndex + length);

            taintedRanges.Remove(newRange);
            return this;

        }
        #endregion Remove 

        #region Replace

        public SafeStringBuilder<T> Replace(string oldValue, string newValue)
        {
            var s = internalBuilder.ToString();
            taintedRanges.Replace(s, oldValue, newValue);
            internalBuilder.Replace(oldValue, newValue);
            return this;
        }

        #endregion Replace

        public override string ToString()
        {
            var value = internalBuilder.ToString();
            var sanitizeResult = LanguageService.TrySanitize(Single<T>.Instance, value, taintedRanges.ToList());

            if (sanitizeResult.Success)
            {
                return sanitizeResult.SanitizedText;
            }
            else
            {
                throw new AttackDetectedException();
            }
        }

    }
}
