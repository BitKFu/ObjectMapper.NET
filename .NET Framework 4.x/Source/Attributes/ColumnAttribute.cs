using System;

namespace AdFactum.Data
{
    /// <summary>
    /// The Column Attribute is used to simplify using the attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public sealed class ColumnAttribute : Attribute
    {
        private readonly string name;
        private readonly bool? isRequired;
        private readonly int? length;

        /// <summary>
        /// Constructs the column attribute
        /// </summary>
        public ColumnAttribute()
        {
        }

        /// <summary>
        /// Constructs the column attribute
        /// </summary>
        public ColumnAttribute(string columnName)
        {
            name = columnName;
        }

        /// <summary>
        /// Constructs the column attribute
        /// </summary>
        public ColumnAttribute(string columnName, bool isRequired)
            :this(columnName)
        {
            this.isRequired = isRequired;
        }

        /// <summary>
        /// Constructs the column attribute
        /// </summary>
        public ColumnAttribute(string columnName, bool isRequired, int length)
            :this(columnName, isRequired)
        {
            this.length = length;
        }

        /// <summary> Name of the column </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary> Defines, if the column is required</summary>
        public bool? IsRequired
        {
            get { return isRequired; }
        }

        /// <summary> Defines the column length </summary>
        public int? Length
        {
            get { return length; }
        }
    }
}
