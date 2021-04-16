using LibProtection.Injections.Formatting;
using System;
using System.Text;

namespace LibProtection.Injections
{
    /// <summary>
    /// <para> 
    /// LibProtection.Injections counterpart to the <see cref="StringBuilder"/> class. 
    /// For every method of the original <see cref="StringBuilder"/> class <see cref="SafeStringBuilder{T}"/> offers two methods:
    /// e.g. <see cref="Append(string)"/> and <see cref="UncheckedAppend(string)"/>.
    /// </para>
    /// <para>
    /// Methods without the Unchecked prefix assume that the values passed to them can potentially be controlled an attacker.
    /// On the other hand methods, whose names start with Unchecked, assume that the values passed to them cannot be controlled by an attacker.
    /// When <see cref="ToString"/> is called, <see cref="StringBuilder"/> detects potential injection attacks within user controlled segments.
    /// </para>
    /// </summary>
    /// <typeparam name="T">Specifies the grammar of the string.</typeparam>
    public class SafeStringBuilder<T> where T : LanguageProvider
    {
        private readonly StringBuilder internalBuilder;
        internal readonly SortedRangesList taintedRanges = new SortedRangesList();

        #region Properties
        /// <summary>
        /// Gets or sets the maximum number of characters that can be contained in the memory allocated by the current instance.
        /// </summary>
        /// <value>The maximum number of characters that can be contained in the memory allocated by the current instance. 
        /// Its value can range from <see cref="Length"/> to <see cref="MaxCapacity"/>.</value>
        /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than the current length of this instance.
        /// -or- The value specified for a set operation is greater than the maximum capacity.</exception>
        public int Capacity 
        { 
            get 
                { 
                    return internalBuilder.Capacity; 
            } 
            set 
            { 
                internalBuilder.Capacity = value; 
            } 
        }

        /// <summary>
        /// Gets the character at the specified character position in this instance.
        /// </summary>
        /// <param name="index">The position of the character.</param>
        /// <value>The Unicode character at position index <paramref name="index"/>.</value>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside the bounds of this instance while setting a character.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the bounds of this instance while getting a character.</exception>
        public char this[int index]
        {
            get { return internalBuilder[index]; }
            //TODO add set when replace at position is implemented
        }

        /// <summary>
        /// Gets the length of the current instance.
        /// </summary>
        /// <value>THe length of this instance</value>
        /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than zero or greater than <see cref="MaxCapacity"/>.</exception>
        public int Length
        {
            get { return internalBuilder.Length; }
            set 
            { 
                internalBuilder.Length = value;
                taintedRanges.CutOff(value);
            }
        }

        /// <summary>
        /// Gets the maximum capacity of this instance.
        /// </summary>
        /// <value>The maximum number of characters this instance can hold.</value>
        public int MaxCapacity 
        { 
            get { return internalBuilder.MaxCapacity; } 
        }
        #endregion Properties

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeStringBuilder{T}"/> class.
        /// </summary>
        public SafeStringBuilder()
        {
            internalBuilder = new StringBuilder();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeStringBuilder{T}"/> class using the specified capacity.
        /// </summary>
        /// <param name="capacity">The suggested starting size of this instance.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="capacity"/>capacity is less than zero.</exception>
        public SafeStringBuilder(int capacity)
        {
            internalBuilder = new StringBuilder(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeStringBuilder{T}"/> class using the specified string.
        /// </summary>
        /// <param name="value">The string used to initialize the value of the instance. If value is <c>null</c>, the 
        /// new instance will contain the empty string (that is, it contains <see cref="String.Empty"/>).</param>
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
        /// Initializes a new instance of the <see cref="SafeStringBuilder{T}"/> class using the specified capacity and maximum capacity.
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
        /// Initializes a new instance of the <see cref="SafeStringBuilder{T}"/> class using the specified capacity and maximum capacity.
        /// </summary>
        ///<param name="value">The string used to initialize the value of the instance. If value is null, the 
        /// new instance will contain the empty string (that is, it contains <see cref="String.Empty"/>).</param>
        /// <param name="capacity">The suggested starting size of this instance.</param>
        /// <param name="isSafe">Whether the <paramref name="value"/> can be controlled by an attacker. <c>false</c> by default, 
        /// meaning <paramref name="value"/> is considered attacker controlled.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="capacity"/> is less than zero."</exception>
        public SafeStringBuilder(string value, int capacity, bool isSafe = false)
        {
            internalBuilder = new StringBuilder(value, capacity);
            if (!isSafe && value != null && value.Length != 0)
            {
                taintedRanges.AddLast(new Range(0, value.Length));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeStringBuilder{T}"/> class from the specified substring and capacity.
        /// </summary>
        /// <param name="value">The string used to initialize the value of the instance. If value is null, the 
        /// new System.Text.StringBuilder will contain the empty string (that is, it contains <see cref="String.Empty"/>).</param>
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
        /// <summary>
        /// Appends the string representation of a specified string builder to this instance. 
        /// The content of the string builder is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The string builder to append.</param>
        /// <returns>A reference to this instance after the append operation is completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(StringBuilder value)
        {
            var length = internalBuilder.Length;
            internalBuilder.Append(value);
            if (value != null && value.Length != 0)
            {
                taintedRanges.AddLast(new Range(length, length + value.Length));
            }

            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified string builder to this instance.
        /// The content of the string builder is NOT considered to be user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The string builder to append.</param>
        /// <returns>A reference to this instance after the append operation is completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(StringBuilder value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified a specified 16-bit unsigned integer to this instance. 
        /// This representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The 16-bit unsigned integer to append.</param>
        /// <returns>A reference to this instance after the append operation is completed.</returns>\
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(UInt16 value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(length, length + strValue.Length));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified a specified 16-bit unsigned integer to this instance. 
        /// This representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The 16-bit unsigned integer to append.</param>
        /// <returns>A reference to this instance after the append operation is completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(UInt16 value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified a specified 32-bit unsigned integer to this instance. 
        /// This representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The 32-bit unsigned integer to append.</param>
        /// <returns>A reference to this instance after the append operation is completed.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(UInt32 value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(length, length + strValue.Length));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified a specified 32-bit unsigned integer to this instance. 
        /// This representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The 32-bit unsigned integer to append.</param>
        /// <returns>A reference to this instance after the append operation is completed.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(UInt32 value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified a specified 64-bit unsigned integer to this instance. 
        /// This representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The 64-bit unsigned integer to append.</param>
        /// <returns>A reference to this instance after the append operation is completed.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(UInt64 value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(length, length + strValue.Length));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified a specified 64-bit unsigned integer to this instance. 
        /// This representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The 64-bit unsigned integer to append.</param>
        /// <returns>A reference to this instance after the append operation is completed.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(UInt64 value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified subarray of Unicode characters to this instance. 
        /// Appended subarray is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">A character array.</param>
        /// <param name="startIndex">The starting position in <paramref name="value"/>.</param>
        /// <param name="charCount">The number of characters to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>, 
        /// and <paramref name="startIndex"/> and <paramref name="charCount"/> are not zero</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="charCount"/> is less than zero. -or- <paramref name="startIndex"/> is less than zero. 
        /// -or- <paramref name="startIndex"/>+<paramref name="charCount"/> is greater than the length of <paramref name="value"/>. 
        /// -or- Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(char[] value, int startIndex, int charCount)
        {
            int length = internalBuilder.Length;
            internalBuilder.Append(value, startIndex, charCount);
            if (charCount != 0)
            {
                taintedRanges.AddLast(new Range(length, length + charCount));
            }
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified subarray of Unicode characters to this instance. 
        /// Appended subarray is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">A character array.</param>
        /// <param name="startIndex">The starting position in <paramref name="value"/>.</param>
        /// <param name="charCount">The number of characters to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>, 
        /// and <paramref name="startIndex"/> and <paramref name="charCount"/> are not zero</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="charCount"/> is less than zero. -or- <paramref name="startIndex"/> is less than zero. 
        /// -or- <paramref name="startIndex"/>+<paramref name="charCount"/> is greater than the length of <paramref name="value"/>. 
        /// -or- Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(char[] value, int startIndex, int charCount)
        {
            internalBuilder.Append(value, startIndex, charCount);
            return this;
        }

        /// <summary>
        /// Appends the specified string to this instance. Appended string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">A string to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(string value)
        {
            var length = internalBuilder.Length;
            internalBuilder.Append(value);
            if (value != null && value.Length != 0)
            {
                taintedRanges.AddLast(new Range(length, length + value.Length));
            }
            return this;
        }

        /// <summary>
        /// Appends the specified string to this instance. Appended string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(string value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends a copy of a specified substring to this instance. Appended substring is considered user controlled for the purpose of attack detection. 
        /// </summary>
        /// <param name="value">The string that contains the substring to append.</param>
        /// <param name="startIndex">The starting position of the substring within <paramref name="value"/>.</param>
        /// <param name="count">The number of characters in <paramref name="value"/> to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>, 
        /// and <paramref name="startIndex"/> and <paramref name="charCount"/> are not zero</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="charCount"/> is less than zero. -or- <paramref name="startIndex"/> is less than zero. 
        /// -or- <paramref name="startIndex"/>+<paramref name="charCount"/> is greater than the length of <paramref name="value"/>. 
        /// -or- Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(string value, int startIndex, int count)
        {
            var length = internalBuilder.Length;
            internalBuilder.Append(value, startIndex, count);
            if (count != 0)
            {
                taintedRanges.AddLast(new Range(length, length + count));
            }
            return this;
        }

        /// <summary>
        /// Appends a copy of a specified substring to this instance. Appended substring is NOT considered user controlled for the purpose of attack detection. 
        /// </summary>
        /// <param name="value">The string that contains the substring to append.</param>
        /// <param name="startIndex">The starting position of the substring within <paramref name="value"/>.</param>
        /// <param name="count">The number of characters in <paramref name="value"/> to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>, 
        /// and <paramref name="startIndex"/> and <paramref name="charCount"/> are not zero</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="charCount"/> is less than zero. -or- <paramref name="startIndex"/> is less than zero. 
        /// -or- <paramref name="startIndex"/>+<paramref name="charCount"/> is greater than the length of <paramref name="value"/>. 
        /// -or- Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(string value, int startIndex, int count)
        {
            internalBuilder.Append(value, startIndex, count);
            return this;
        }

        /// <summary>
        /// Appends a specified number of copies of the string representation of a Unicode character to this instance.
        /// Appended segment is considered user controlled for the purpose of attack detection.  
        /// </summary>
        /// <param name="value">The character to append.</param>
        /// <param name="repeatCount">The number of times to append <paramref name="value"/>.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatCount"/> is less than zero.
        /// -or- Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        public SafeStringBuilder<T> Append(Char value, Int32 repeatCount)
        {
            var length = internalBuilder.Length;
            internalBuilder.Append(value, repeatCount);
            if (repeatCount != 0)
            {
                taintedRanges.AddLast(new Range(length, length + repeatCount));
            }
            return this;
        }

        /// <summary>
        /// Appends a specified number of copies of the string representation of a Unicode character to this instance.
        /// Appended segment is NOT considered user controlled for the purpose of attack detection.  
        /// </summary>
        /// <param name="value">The character to append.</param>
        /// <param name="repeatCount">The number of times to append <paramref name="value"/>.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="repeatCount"/> is less than zero.
        /// -or- Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        public SafeStringBuilder<T> UncheckedAppend(Char value, Int32 repeatCount)
        {
            internalBuilder.Append(value, repeatCount);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified single-precision floating-point number to this instance. 
        /// Appended characters are considered user controlled for the purpose of attack detection.  
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(float value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(length, length + strValue.Length));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified single-precision floating-point number to this instance. 
        /// Appended characters are NOT considered user controlled for the purpose of attack detection.  
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(float value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified <see cref="Boolean"/> value to this instance. 
        /// Appended representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(bool value)
        {
            var length = internalBuilder.Length;
            internalBuilder.Append(value);
            taintedRanges.AddLast(new Range(length, length + (value ? 4 : 5)));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified <see cref="Boolean"/> value to this instance. 
        /// Appended representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(bool value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified <see cref="Char"/> value to this instance. 
        /// Appended representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(char value)
        {
            var length = internalBuilder.Length;
            internalBuilder.Append(value);
            taintedRanges.AddLast(new Range(length, length + 1));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified <see cref="Char"/> value to this instance. 
        /// Appended representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The UTF-16-encoded code unit to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(char value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of the Unicode characters in a specified array to this instance. 
        /// Appended representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The array of characters to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(char[] value)
        {
            var length = internalBuilder.Length;
            internalBuilder.Append(value);
            if (value != null && value.Length != 0)
            {
                taintedRanges.AddLast(new Range(length, length + value.Length));
            }
            return this;
        }

        /// <summary>
        /// Appends the string representation of the Unicode characters in a specified array to this instance. 
        /// Appended representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The array of characters to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(char[] value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified decimal number to this instance. 
        /// Appended representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The decimal number to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(decimal value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(length, length + strValue.Length));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified decimal number to this instance. 
        /// Appended representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The decimal number to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UnchekedAppend(decimal value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified 8-bit unsigned integer to this instance. 
        /// Appended representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(byte value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(length, length + strValue.Length));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified 8-bit unsigned integer to this instance. 
        /// Appended representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(byte value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified 16-bit signed integer to this instance. 
        /// Appended representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(Int16 value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(length, length + strValue.Length));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified 16-bit signed integer to this instance. 
        /// Appended representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(Int16 value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified 32-bit signed integer to this instance. 
        /// Appended representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(Int32 value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(length, length + strValue.Length));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified 32-bit signed integer to this instance. 
        /// Appended representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(Int32 value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified 64-bit signed integer to this instance. 
        /// Appended representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(Int64 value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(length, length + strValue.Length));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified 64-bit signed integer to this instance. 
        /// Appended representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(Int64 value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified object to this instance.
        /// Appended representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The object to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(object value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            if (strValue.Length != 0)
            {
                taintedRanges.AddLast(new Range(length, length + strValue.Length));
            }
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified object to this instance.
        /// Appended representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The object to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(object value)
        {
            internalBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified double-precision floating-point number to this instance.
        /// Appended representation is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Append(double value)
        {
            var strValue = value.ToString();
            var length = internalBuilder.Length;
            internalBuilder.Append(strValue);
            taintedRanges.AddLast(new Range(length, length + strValue.Length));
            return this;
        }

        /// <summary>
        /// Appends the string representation of a specified double-precision floating-point number to this instance.
        /// Appended representation is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The value to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppend(double value)
        {
            internalBuilder.Append(value);
            return this;
        }
        #endregion Append

        #region AppendFormat
        /// <summary>
        /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.
        /// The string itself is NOT considered to be user controlled for the purpose of attack detection, while string representation of the arguments are.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        /// <returns>A reference to this instance with <paramref name="format"/> appended. Each format item in <paramref name="format"/> is replaced 
        /// by the string representation of the corresponding object argument.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException"><paramref name="format"/> is invalid or the index of a format item is less than 0 (zero), or
        /// greater than or equal to length of the <paramref name="args"/> array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> AppendFormat(string format, params object[] args)
        {
            if (format == null || args == null)
            {
                throw new ArgumentNullException("One of the arguments is null.");
            }

            var formattedString = Formatter.Format(format, args, out var newTaintedRanges, out var _);
            internalBuilder.Append(formattedString);
            foreach (var taintedRange in newTaintedRanges)
            {
                if (taintedRange.Length != 0)
                {
                    taintedRanges.AddLast(taintedRange);
                }
            }

            return this;
        }

        /// <summary>
        /// Appends the string returned by processing a composite format string, which contains zero or more format items, to this instance.
        /// Both the string itself and the string representation of the arguments are NOT considered to be user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        /// <returns>A reference to this instance with <paramref name="format"/> appended. Each format item in <paramref name="format"/> is replaced 
        /// by the string representation of the corresponding object argument.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="format"/> or <paramref name="args"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException"><paramref name="format"/> is invalid or the index of a fromat item is less than 0 (zero), or
        /// greater than or equal to length of the <paramref name="args"/> array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The length of the expanded string would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedAppendFormat(string format, params object[] args)
        {
            internalBuilder.AppendFormat(format, args);
            return this;
        }

        //TODO: add all AppendFormat overloads
        #endregion AppendFormat

        #region AppendLine
        /// <summary>
        /// Appends the default line terminator to the end of the current instance.
        /// </summary>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> AppendLine()
        {
            internalBuilder.AppendLine();
            return this;
        }

        /// <summary>
        /// Appends a copy of the specified string followed by the default line terminator to the end of the current instance.
        /// Appended string is considered user controlled for the purpose of attack detection, while the line terminator is not.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>

        public SafeStringBuilder<T> AppendLine(string value)
        {
            var length = internalBuilder.Length;
            internalBuilder.AppendLine(value);
            if (value != null && value.Length != 0)
            {
                taintedRanges.AddLast(new Range(length, length + value.Length));
            }
            return this;
        }

        /// <summary>
        /// Appends a copy of the specified string followed by the default line terminator to the end of the current instance.
        /// Appended string and line terminator are NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="value">The string to append.</param>
        /// <returns>A reference to this instance after the append operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UnsafeAppendLine(string value)
        {
            internalBuilder.AppendLine(value);
            return this;
        }
        #endregion AppendLine

        #region Clear
        /// <summary>
        /// Removes all characters from the current instance.
        /// </summary>
        /// <returns>An instance whose <see cref="Length"/> is 0 (zero).</returns>
        public SafeStringBuilder<T> Clear()
        {
            taintedRanges.Clear();
            internalBuilder.Clear();
            return this;
        }
        #endregion Clear

        #region CopyTo
        /// <summary>
        /// Copies the characters from a specified segment of this instance to a specified segment of a destination <see cref="Char"/> array.
        /// </summary>
        /// <param name="sourceIndex">The starting position in this instance where characters will be copied from. The index is zero-based.</param>
        /// <param name="destination">The array where characters will be copied.</param>
        /// <param name="destinationIndex">The starting position in <paramref name="destination"/> where characters will be copied. The index is zero-based.</param>
        /// <param name="count">The number of characters to be copied.</param>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="sourceIndex"/>, <paramref name="destinationIndex"/> or <paramref name="count"/> is less than zero.
        /// -or- <paramref name="sourceIndex"/> is greater than the length of this instance.</exception>
        /// <exception cref="ArgumentException"><paramref name="sourceIndex"/>+<paramref name="count"/> is greater than the length of this instance.
        /// -or- <paramref name="destinationIndex"/>+<paramref name="count"/> is greater than the length of <paramref name="destination"/>.</exception>
        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            internalBuilder.CopyTo(sourceIndex, destination, destinationIndex, count);
        }
        #endregion CopyTo

        #region EnsureCapacity
        /// <summary>
        /// Ensures that the capacity of this instance is at least the specified value.
        /// </summary>
        /// <param name="capacity">The minimum capacity to ensure.</param>
        /// <returns>The new capacity of this instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than zero
        /// -or- Enlarging the value of this instance would exceed <paramref name="capacity"/>.</exception>
        public int EnsureCapacity(int capacity)
            => internalBuilder.EnsureCapacity(capacity);
        #endregion EnsureCapacity

        #region Insert
        /// <summary>
        /// Inserts one or more copies of a specified string into this instance at the specified character position. 
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The string to insert.</param>
        /// <param name="count">The number of time two insert <paramref name="value"/>.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance,
        /// -or- the current <see cref="Length"/> plus the length of <paramref name="value"/> exceeds <see cref="MaxCapacity"/>.</exception>
        /// <exception cref=" OutOfMemoryException">The current length of this instance plus the length of <paramref name="value"/> times <paramref name="count"/>
        /// exceeds <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, string value, int count)
        {
            internalBuilder.Insert(index, value, count);
            if (value != null && value.Length != 0)
            {
                taintedRanges.SafeInsert(new Range(index, index + value.Length * count));
            }
            return this;
        }

        /// <summary>
        /// Inserts one or more copies of a specified string into this instance at the specified character position. 
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The string to insert.</param>
        /// <param name="count">The number of time two insert <paramref name="value"/>.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance,
        /// -or- the current <see cref="Length"/> plus the length of <paramref name="value"/> exceeds <see cref="MaxCapacity"/>.</exception>
        /// <exception cref=" OutOfMemoryException">The current length of this instance plus the length of <paramref name="value"/> times <paramref name="count"/>
        /// exceeds <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, string value, int count)
        {
            internalBuilder.Insert(index, value, count);
            return this;
        }

        /// <summary>
        /// Inserts the string representation of a 64-bit unsigned integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, ulong value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a 64-bit unsigned integer into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, ulong value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a 32-bit unsigned integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, uint value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a 32-bit unsigned integer into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, uint value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a 16-bit unsigned integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, ushort value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a 16-bit unsigned integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, ushort value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts a string into this instance at the specified character position. 
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The string to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance,
        /// -or- the current <see cref="Length"/> plus the length of <paramref name="value"/> exceeds <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, string value)
        {
            internalBuilder.Insert(index, value);
            if (value != null && value.Length != 0)
            {
                taintedRanges.SafeInsert(new Range(index, index + value.Length));
            }
            return this;
        }

        /// <summary>
        /// Inserts a string into this instance at the specified character position. 
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The string to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance,
        /// -or- the current <see cref="Length"/> plus the length of <paramref name="value"/> exceeds <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, string value)
        {
            internalBuilder.Insert(index, value);
            if (value != null && value.Length != 0)
            {
                taintedRanges.UncheckedInsert(new Range(index, index + value.Length));
            }
            return this;
        }

        /// <summary>
        /// Inserts the string representation of a single-precision floating point number into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, float value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a single-precision floating point number into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, float value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified subarray of Unicode characters to this instance at the specified character position. 
        /// Inserted subarray is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="=index">The position in this instance where insertion begins.</param>
        /// <param name="value">A character array.</param>
        /// <param name="startIndex">The starting position in <paramref name="value"/>.</param>
        /// <param name="charCount">The number of characters to append.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>, 
        /// and <paramref name="startIndex"/> and <paramref name="charCount"/> are not zero</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/>, <paramref name="startIndex"/> or <paramref name="charCount"/> is less than zero. 
        /// -or- <paramref name="startIndex"/> is greater than the length of this instance.
        /// -or- <paramref name="startIndex"/>+<paramref name="charCount"/> is not a position within <paramref name="value"/>. 
        /// -or- Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, char[] value, int startIndex, int charCount)
        {
            return Insert(index, new string(value, startIndex, charCount));
        }

        /// <summary>
        /// Inserts the string representation of a specified subarray of Unicode characters to this instance at the specified character position. 
        /// Inserted subarray is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="=index">The position in this instance where insertion begins.</param>
        /// <param name="value">A character array.</param>
        /// <param name="startIndex">The starting position in <paramref name="value"/>.</param>
        /// <param name="charCount">The number of characters to append.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>, 
        /// and <paramref name="startIndex"/> and <paramref name="charCount"/> are not zero</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/>, <paramref name="startIndex"/> or <paramref name="charCount"/> is less than zero. 
        /// -or- <paramref name="startIndex"/> is greater than the length of this instance.
        /// -or- <paramref name="startIndex"/>+<paramref name="charCount"/> is not a position within <paramref name="value"/>. 
        /// -or- Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, char[] value, int startIndex, int charCount)
        {
            return UncheckedInsert(index, new string(value, startIndex, charCount));
        }

        /// <summary>
        /// Inserts the string representation of a specified 8-bit signed integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, sbyte value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified 8-bit signed integer into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, sbyte value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified 16-bit signed integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, short value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified 16-bit signed integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, short value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified 64-bit signed integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, long value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified 64-bit signed integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, long value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified 32-bit signed integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, int value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified 32-bit signed integer into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, int value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified double-precision floating-point number into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, double value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified double-precision floating-point number into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, double value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified decimal number into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, decimal value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified decimal number into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, decimal value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified array of Unicode characters into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The character array to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, char[] value)
        {
            return Insert(index, new string(value));
        }

        /// <summary>
        /// Inserts the string representation of a specified array of Unicode characters into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The character array to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, char[] value)
        {
            return UncheckedInsert(index, new string(value));
        }

        /// <summary>
        /// Inserts the string representation of a specified 8-bit unsigned integer into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, byte value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified 8-bit unsigned integer into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, byte value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified Boolean value into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, bool value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified Boolean value into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, bool value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified object into this instance at the specified character position.
        /// The inserted string is considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The object to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Insert(int index, object value)
        {
            return Insert(index, value.ToString());
        }

        /// <summary>
        /// Inserts the string representation of a specified object into this instance at the specified character position.
        /// The inserted string is NOT considered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins.</param>
        /// <param name="value">The object to insert.</param>
        /// <returns>A reference to this instance after the insert operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero or greater than the current length of this instance.</exception>
        /// <exception cref=" OutOfMemoryException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> UncheckedInsert(int index, object value)
        {
            return UncheckedInsert(index, value.ToString());
        }

        #endregion Insert

        #region Remove
        /// <summary>
        /// Removes the specified range of characters from this instance.
        /// </summary>
        /// <param name="startIndex">The zero-based position in this instance where removal begins.</param>
        /// <param name="length">The number of characters to remove.</param>
        /// <returns>A reference to this instance after the excise operation has completed.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="startIndex"/> or <paramref name="length"/> is less than zero.
        /// -or- <paramref name="startIndex"/> + <paramref name="length"/> is greater than the length of this instance.</exception>
        public SafeStringBuilder<T> Remove(int startIndex, int length)
        {
            var newRange = new Range(startIndex, startIndex + length);
            internalBuilder.Remove(startIndex, length);
            taintedRanges.Remove(newRange);
            return this;
        }
        #endregion Remove 

        #region Replace
        /// <summary>
        /// Replaces all occurrences of a specified string in this instance with another specified string. 
        /// Replacement segments are contsidered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="oldValue">The string to replace.</param>
        /// <param name="newValue">The string that replaces <paramref name="oldValue"/>, or <c>null</c>.</param>
        /// <returns>A reference to this instance with all instances of <paramref name="oldValue"/> replaced by <paramref name="newValue"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="oldValue"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The length of <paramref name="oldValue"/> is zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Replace(string oldValue, string newValue)
        {
            var str = internalBuilder.ToString();
            internalBuilder.Replace(oldValue, newValue);
            taintedRanges.Replace(str, oldValue, newValue, 0, str.Length);
            return this;
        }

        /// <summary>
        /// Replaces, within a substring of this instance, all occurrences of a specified string with another specified string.
        /// Replacement segments are contsidered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="oldValue">The string to replace.</param>
        /// <param name="newValue">The string that replaces <paramref name="oldValue"/>, or <c>null</c>.</param>
        /// <param name="startIndex">The position in this instance where the substring begins.</param>
        /// <param name="count">The length of the substring.</param>
        /// <returns>A reference to this instance with all instances of <paramref name="oldValue"/> replaced by <paramref name="newValue"/> in the range 
        /// from <paramref name="startIndex"/> to <paramref name="startIndex"/> + <paramref name="count"/> - 1.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="oldValue"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The length of <paramref name="oldValue"/> is zero.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex"/> or <paramref name="count"/> is less than zero.
        /// -or- <paramref name="startIndex"/>+<paramref name="count"/> is greater than <see cref="Length"/> of this instance.
        /// -or- Enlarging the value of this instance would exceed <see cref="MaxCapacity"/>.</exception>
        public SafeStringBuilder<T> Replace(string oldValue, string newValue, int startIndex, int count)
        {
            var str = internalBuilder.ToString();
            internalBuilder.Replace(oldValue, newValue);
            taintedRanges.Replace(str, oldValue, newValue, startIndex, count);
            return this;
        }

        /// <summary>
        /// Replaces, within a substring of this instance, all occurrences of a specified character with another specified character.
        /// Replacement characters are contsidered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="oldChar">The character to replace.</param>
        /// <param name="newChar">The characherthat replaces <paramref name="oldChar"/>, or <c>null</c>.</param>
        /// <param name="startIndex">The position in this instance where the substring begins.</param>
        /// <param name="count">The length of the substring.</param>
        /// <returns>A reference to this instance with all instances of <paramref name="oldChar"/> replaced by <paramref name="newChar"/> in the range 
        /// from <paramref name="startIndex"/> to <paramref name="startIndex"/> + <paramref name="count"/> - 1.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex"/> or <paramref name="count"/> is less than zero.
        /// -or- <paramref name="startIndex"/>+<paramref name="count"/> is greater than <see cref="Length"/> of this instance.</exception>
        public SafeStringBuilder<T> Replace(char oldChar, char newChar, int startIndex, int count)
        {
            return Replace(oldChar.ToString(), newChar.ToString(), startIndex, count);
        }

        /// <summary>
        /// Replaces all occurrences of a specified character in this instance with another specified character.
        /// Replacement characters are contsidered user controlled for the purpose of attack detection.
        /// </summary>
        /// <param name="oldChar">The character to replace.</param>
        /// <param name="newChar">The characherthat replaces <paramref name="oldChar"/>, or <c>null</c>.</param>
        /// <returns>A reference to this instance with <paramref name="oldChar"/> replaced by <paramref name="newChar"/>.</returns>
        public SafeStringBuilder<T> Replace(char oldChar, char newChar)
        {
            return Replace(oldChar.ToString(), newChar.ToString());
        }

        #endregion Replace

        #region ToString
        /// <summary>
        /// Converts the value of this instance to a <see cref="String"/>.
        /// </summary>
        /// <returns>String whose attacker controlled substrings have been sanitized.</returns>
        /// <exception cref="AttackDetectedException">An injection attack was detected.</exception>
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
        #endregion ToString
    }
}
