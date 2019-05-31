using System;
using System.Data;
using System.IO;
using AdFactum.Data.Internal;

namespace AdFactum.Data.SqlServer
{
    /// <summary>
    /// Type mapper for sql server data types
    /// </summary>
    public class SqlTypeMapper : BaseSqlTypeMapper
    {
        /// <summary>
        /// SQL Type Mapper Constructor
        /// </summary>
        public SqlTypeMapper()
        {
            DbMappingTable.Add(typeof(Boolean), "BIT");
            DbMappingTable.Add(typeof(Byte), "TINYINT");
            DbMappingTable.Add(typeof(DateTime), "DATETIME");
            DbMappingTable.Add(typeof(Decimal), "DECIMAL(28,12)");
            DbMappingTable.Add(typeof(Double), "FLOAT");
            DbMappingTable.Add(typeof(Guid), "UNIQUEIDENTIFIER");
            DbMappingTable.Add(typeof(Int16), "SMALLINT");
            DbMappingTable.Add(typeof(Int32), "INT");
            DbMappingTable.Add(typeof(Int64), "BIGINT");
            DbMappingTable.Add(typeof(Single), "REAL");
            DbMappingTable.Add(typeof(String), "VARCHAR({0})");
            DbMappingTable.Add(typeof(TimeSpan), "BIGINT");
            DbMappingTable.Add(typeof(Enum), "SMALLINT");
            DbMappingTable.Add(typeof(Stream), "IMAGE");
            DbMappingTable.Add(typeof(Byte[]), "IMAGE");
            DbMappingTable.Add(typeof(Char), "CHAR(1)");

            SqlMappingTable.Add(typeof(Boolean), (int) SqlDbType.Bit);
            SqlMappingTable.Add(typeof(Byte), (int) SqlDbType.TinyInt);
            SqlMappingTable.Add(typeof(DateTime), (int) SqlDbType.DateTime);
            SqlMappingTable.Add(typeof(Decimal), (int) SqlDbType.Decimal);
            SqlMappingTable.Add(typeof(Double), (int) SqlDbType.Float);
            SqlMappingTable.Add(typeof(Guid), (int) SqlDbType.UniqueIdentifier);
            SqlMappingTable.Add(typeof(Int16), (int) SqlDbType.SmallInt);
            SqlMappingTable.Add(typeof(Int32), (int) SqlDbType.Int);
            SqlMappingTable.Add(typeof(Int64), (int) SqlDbType.BigInt);
            SqlMappingTable.Add(typeof(Single), (int) SqlDbType.Real);
            SqlMappingTable.Add(typeof(String), (int) SqlDbType.VarChar);
            SqlMappingTable.Add(typeof(TimeSpan), (int) SqlDbType.BigInt);
            SqlMappingTable.Add(typeof(Enum), (int) SqlDbType.SmallInt);
            SqlMappingTable.Add(typeof(Stream), (int) SqlDbType.Image);
            SqlMappingTable.Add(typeof(Byte[]), (int) SqlDbType.Image);
            SqlMappingTable.Add(typeof(Char), (int) SqlDbType.Char);
        }

        #region ITypeMapper Members

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
            get { return false;}
        }

        /// <summary>
        /// Gets the string for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public override string GetStringForDDL(FieldDescription field)
        {
            Type type = PreTransformTypeForDDL(field);
            int length = field.CustomProperty.MetaInfo.Length;

            /*
			 * Strings that are greater than 8000 characters are handled as TEXT fields
			 */
            if (type.Equals(typeof(string)) && length>8000)
                return (field.CustomProperty.MetaInfo.IsUnicode) ? "NTEXT" : "TEXT";

            string result = GetStringForDDL(field, type, length);

            /*
             * Place Unicode N in Front of the field type
             */
            if (field.CustomProperty.MetaInfo.IsUnicode)
                result = string.Concat("N", result);

            return result;
        }

        /// <summary>
        /// Converts a type to the typed used from the current database implementation
        /// </summary>
        /// <param name="checkType">Type of the check.</param>
        /// <returns></returns>
        public override Type GetTypeForDatabase(Type checkType)
        {
            checkType = TypeHelper.GetBaseType(checkType);

            if (checkType == typeof(TimeSpan))
                return typeof (long);

            return base.GetTypeForDatabase(checkType);
        }

        #endregion
    }
}