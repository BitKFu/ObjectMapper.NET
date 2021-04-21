using System;
using AdFactum.Data;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Test entity for unicode.
    /// </summary>
    public class UnicodeTestEntity : ValueObject
    {
        public const char constUnicodeChar = 'л';
        public const string constUnicodeString = "Маленькая зарплата";
        public const string constUnicodeMemo = "Маленькая зарплата";

        private char unicodeChar;
        private string unicodeString;
        private string unicodeMemo;

        /// <summary>
        /// Gets or sets the unicode char.
        /// </summary>
        /// <value>The unicode char.</value>
        [Unicode]
        public char UnicodeChar
        {
            get { return unicodeChar; }
            set { unicodeChar = value; }
        }

        /// <summary>
        /// Gets or sets the unicode string.
        /// </summary>
        /// <value>The unicode string.</value>
        [Unicode]
        [PropertyLength(30)]
        public string UnicodeString
        {
            get { return unicodeString; }
            set { unicodeString = value; }
        }

        /// <summary>
        /// Gets or sets the unicode memo.
        /// </summary>
        /// <value>The unicode memo.</value>
        [Unicode]
        [PropertyLength(int.MaxValue)]
        public string UnicodeMemo
        {
            get { return unicodeMemo; }
            set { unicodeMemo = value; }
        }
    }
}
