using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Xml
{
    /// <summary>
    /// Type Mapping for XML Files
    /// </summary>
    public class XmlTypeMapper : BaseTypeMapper
    {
        /// <summary>
        /// Returns the SQL Casing of the used Database.
        /// </summary>
        public override SqlCasing SqlCasing
        {
            get { return SqlCasing.Mixed; }
        }

        /// <summary>
        /// Gets a value indicating whether [parameter duplication].
        /// </summary>
        /// <value><c>true</c> if [parameter duplication]; otherwise, <c>false</c>.</value>
        public override bool ParameterDuplication
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the string for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public override string GetStringForDDL(FieldDescription field)
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the enum for database.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="metaInfo">property meta information</param>
        /// <returns></returns>
        public override Enum GetEnumForDatabase(Type type, PropertyMetaInfo metaInfo)
        {
            return null;
        }

        /// <summary>
        /// Gets the auto increment identifier.
        /// </summary>
        /// <value>The auto increment identifier.</value>
        public override string AutoIncrementIdentifier
        {
            get { return string.Empty; }
        }
    }
}
