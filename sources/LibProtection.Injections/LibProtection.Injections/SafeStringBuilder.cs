using System;
using System.Collections.Generic;
using System.Text;

namespace LibProtection.Injections
{
    /// <summary>
    /// <para>LibProtection.Injections counterpart to the <c>System.Text.StringBuilder</c> class. 
    /// For every method of the original <c>StringBuilder</c> class <c>LibProtection.Injections.SafeStringBuilder</c> offers two methods:
    /// e.g. <c>Append(string value)</c> and <c>UnchekedAppend(string value)</c>.
    /// Methods without the Unchecked prefix assume that the values passed to them can potentially be controlled an attacker.
    /// On the other hand methods, whose names start with Unchecked, assume that the values passed to them cannot be controlled by an attacker.
    /// When <c>ToString()</c> is called, <c>SafeStringBuilder</c> detects potential injection attacks within user controlled segments.
    /// </summary>
    /// <typeparam name="T">Specifies the grammar of the string.</typeparam>
    public class SafeStringBuilder<T> where T : LanguageProvider
    {
        private readonly StringBuilder internalBuilder;
        internal readonly SortedRangesList taintedRanges = new SortedRangesList();

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the LibProtection.Injections.SafeStringBuilder class.
        /// </summary>
        public SafeStringBuilder()
        {
            internalBuilder = new StringBuilder();
        }

        /// <summary>
        /// Initializes a new instance of the LibProtection.Injections.SafeStringBuilder class using the specified capacity.
        /// </summary>
        /// <param name="capacity">The suggested starting size of this instance.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="capacity"/>capacity is less than zero.</exception>
        public SafeStringBuilder(int capacity)
        {
            internalBuilder = new StringBuilder(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the LibProtection.Injections.SafeStringBuilder class using the specified string.
        /// </summary>
        /// <param name="value">The string used to initialize the value of the instance. If value is <c>null</c>, the 
        /// new instance will contain the empty string (that is, it contains <c>System.String.Empty</c>).</param>
        /// <param name="isSafe">Whether the <paramref name="value"/> can be controlled by an attacker. <c>false</c> by default, 
        /// meaning <paramref name="value"/> is considered attacker controlled.</param>
        public SafeStringBuilder(string value, bool isSafe = false)
        {
            internalBuilder = new StringBuilder(value);
            if (!isSafe && value != null && value.Length != 0)
            {
                taintedRanges.AddLast(new Range(0, value.Length));
            }
        }

        /// <summary>
        /// Initializes a new instance of the LibProtection.Injections.SafeStringBuilder class using the specified capacity and maximum capacity.
        /// </summary>
        /// <param name="capacity">The suggested starting size of this instance.</param>
        /// <param name="maxCapacity">The maximum number of characters the current instance can contain.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"> 
        /// <paramref name="maxCapacity"/> is less than one, <paramref name="capacity"/> is less than zero, 
        /// or <paramref name="capacity"/> is greater than <paramref name="maxCapacity"/>.</exception>
        public SafeStringBuilder(int capacity, int maxCapacity)
        {
            internalBuilder = new StringBuilder(capacity, maxCapacity);
        }

        /// <summary>
        /// Initializes a new instance of the LibProtection.Injections.SafeStringBuilder class using the specified capacity and maximum capacity.
        /// </summary>
        ///<param name="value">The string used to initialize the value of the instance. If value is null, the 
        /// new instance will contain the empty string (that is, it contains <c>System.String.Empty</c>).</param>
        /// <param name="capacity">The suggested starting size of this instance.</param>
        /// <param name="isSafe">Whether the <paramref name="value"/> can be controlled by an attacker. <c>false</c> by default, 
        /// meaning <paramref name="value"/> is considered attacker controlled.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than zero"</exception>
        public SafeStringBuilder(string value, int capacity, bool isSafe = false)
        {
            internalBuilder = new StringBuilder(value, capacity);
            if (!isSafe && value != null && value.Length != 0)
            {
                taintedRanges.AddLast(new Range(0, value.Length));
            }
        }

        /// <summary>
        /// Initializes a new instance of the LibProtection.Injections.SafeStringBuilder class from the specified substring and capacity.
        /// </summary>
        /// <param name="value">The string used to initialize the value of the instance. If value is null, the 
        /// new System.Text.StringBuilder will contain the empty string (that is, it contains System.String.Empty).</param>
        /// <param name="startIndex">The position within <paramref name="value"/> where the substring begins.</param>
        /// <param name="length">The number of characters in the substring.</param>
        /// <param name="capacity">The suggested starting size of this instance.</param>
        /// <param name="isSafe">Whether the substring defined <paramref name="value"/>, <paramref name="startIndex"/> and <paramref name="length"/>
        /// can be controlled by an attacker. <c>false</c> by default, the substring is considered attacker controlled.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than zero, 
        /// or <paramref name="startIndex"/> plus <paramref name="length"/> is not a position within <paramref name="value"/>.</exception>
        public SafeStringBuilder(string value, int startIndex, int length, int capacity, bool isSafe = false)
        {
            internalBuilder = new StringBuilder(value, startIndex, length, capacity);
            if (!isSafe && length != 0)
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
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
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
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
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
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
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
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + repeatCount));
            internalBuilder.Append(value, repeatCount);
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
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(float value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(bool value)
        {
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + (value ? 4 : 5)));
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> UncheckedAppend(bool value)
        {
            internalBuilder.Append(value);
            return this;
        }

        public SafeStringBuilder<T> Append(char value)
        {
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + 1));
            internalBuilder.Append(value);
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
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
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
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
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
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
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
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
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
            taintedRanges.AddLast(new Range(internalBuilder.Length, internalBuilder.Length + strValue.Length));
            internalBuilder.Append(strValue);
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

        public SafeStringBuilder<T> UncheckedAppendFormat(string format, params object[] args)
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
