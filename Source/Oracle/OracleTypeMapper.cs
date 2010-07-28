using System;
using System.Collections;
using System.Data;
using System.IO;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;
using Oracle.DataAccess.Client;

namespace AdFactum.Data.Oracle
{
	/// <summary>
	/// Type mapper for oracle data types
	/// </summary>
	[Serializable]
    public class OracleTypeMapper : BaseTypeMapper
	{
		/// <summary>
		/// Oracle Type Mapper 
		/// </summary>
		public OracleTypeMapper()
		{
			DbMappingTable.Add(typeof(Boolean), "NUMBER(1,0)");
			DbMappingTable.Add(typeof(Byte), "NUMBER(3)");
			DbMappingTable.Add(typeof(DateTime), "DATE");
			DbMappingTable.Add(typeof(Decimal), "NUMBER(*,12)");
            DbMappingTable.Add(typeof(Double), "FLOAT({0})");
			DbMappingTable.Add(typeof(Guid), "RAW(16)");
            DbMappingTable.Add(typeof(Int16), "NUMBER(6)");
            DbMappingTable.Add(typeof(Int32), "INTEGER");
			DbMappingTable.Add(typeof(Int64), "INTEGER");
            DbMappingTable.Add(typeof(Single), "FLOAT({0})");
			DbMappingTable.Add(typeof(String), "VARCHAR2({0})");
			DbMappingTable.Add(typeof(TimeSpan), "DATE");
			DbMappingTable.Add(typeof(Enum), "NUMBER(4)");
			DbMappingTable.Add(typeof(Stream), "BLOB");
			DbMappingTable.Add(typeof(Byte[]), "BLOB");
            DbMappingTable.Add(typeof(Char), "CHAR(1)");

			SqlMappingTable.Add(typeof(Boolean), (int) OracleDbType.Byte);
			SqlMappingTable.Add(typeof(Byte), (int) OracleDbType.Int16);
			SqlMappingTable.Add(typeof(DateTime), (int) OracleDbType.Date);
			SqlMappingTable.Add(typeof(Decimal), (int) OracleDbType.Decimal);
			SqlMappingTable.Add(typeof(Double), (int) OracleDbType.Double);
            SqlMappingTable.Add(typeof(Guid), (int) OracleDbType.Raw);
			SqlMappingTable.Add(typeof(Int16), (int) OracleDbType.Int16);
			SqlMappingTable.Add(typeof(Int32), (int) OracleDbType.Int32);
			SqlMappingTable.Add(typeof(Int64), (int) OracleDbType.Int64);
            SqlMappingTable.Add(typeof(Single), (int) OracleDbType.Double);
			SqlMappingTable.Add(typeof(String), (int) OracleDbType.Varchar2);
			SqlMappingTable.Add(typeof(TimeSpan), (int) OracleDbType.Date);
			SqlMappingTable.Add(typeof(Enum), (int) OracleDbType.Int16);
			SqlMappingTable.Add(typeof(Stream), (int) OracleDbType.Blob);
			SqlMappingTable.Add(typeof(Byte[]), (int) OracleDbType.Blob);
		    SqlMappingTable.Add(typeof(Char), (int) OracleDbType.Char);
		}

		#region ITypeMapper Members

        /// <summary>
        /// Gets the string for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public override string GetStringForDDL(FieldDescription field)
		{
			String result = "BLOB";

            Type type = PreTransformTypeForDDL(field);
            int length = field.CustomProperty.MetaInfo.Length;

            /*
             * Strings that are greater than 4000 characters are handled as MEMOS
             */
            if (type.Equals(typeof(string)) && length > 4000)
                return (field.CustomProperty.MetaInfo.IsUnicode) ? "NCLOB" : "CLOB";

            result =  base.GetStringForDDL(field, type, length) ?? result;

            /*
             * Place Unicode N in Front of the field type
             */
            if (field.CustomProperty.MetaInfo.IsUnicode)
                result = string.Concat("N", result);

            return result;
		}

        /// <summary>
        /// Gets the enum for database.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
        /// <returns></returns>
        public override Enum GetEnumForDatabase(Type type, bool isUnicode)
		{
			OracleDbType result = OracleDbType.Blob;
            type = TypeHelper.GetBaseType(type);

			if (SqlMappingTable.ContainsKey(type))
				result = (OracleDbType) SqlMappingTable[type];
			else
			{
				/*
				 * Wenn keine direkte Zuordnung möglich ist, dann nach Implementierungen suchen
				 */
				foreach (var dict in SqlMappingTable)
				{
					if (type.IsDerivedFrom(dict.Key))
					{
					    result = (OracleDbType) dict.Value;
					    break;
					}
				}
			}

            /*
             * Switch to unicode
             */
            if (isUnicode)
            {
                switch (result)
                {
                    case OracleDbType.Char:
                        result = OracleDbType.NChar;
                        break;

                    case OracleDbType.Varchar2:
                        result = OracleDbType.NVarchar2;
                        break;

                    case OracleDbType.Clob:
                        result = OracleDbType.NClob;
                        break;
                }
            }

			return result;
		}

		/// <summary>
		/// Converts the source to a database specific type and returns it.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
        public override object ConvertValue(object value)
		{
            object result = base.ConvertValue(value);

            if (result is Guid)
                result = ((Guid)result).ToByteArray();

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

            if (checkType.IsEnum)               return typeof(Int16);
            if (checkType == typeof(Boolean))   return typeof(Int16);
            if (checkType == typeof(Guid))      return typeof(Byte[]);
            if (checkType == typeof(Stream))    return typeof(Byte[]);
            if (checkType == typeof(byte))      return typeof(short);
            if (checkType == typeof(int))       return typeof(decimal);
            if (checkType == typeof(short))     return typeof(int);
            if (checkType == typeof(long))      return typeof(decimal);
            if (checkType == typeof(Single))    return typeof(double);

	        return base.GetTypeForDatabase(checkType);
		}

        /// <summary>
        /// Gets the auto increment identifier.
        /// </summary>
        /// <value>The auto increment identifier.</value>
        public override string AutoIncrementIdentifier
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the param value as SQL string.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns></returns>
        public override string GetParamValueAsSQLString(object parameterValue)
        {
            /*
             * Check base types
             */
            string baseResult = base.GetParamValueAsSQLString(parameterValue);
            if (baseResult != string.Empty)
                return baseResult;

            /*
             * Check date time
             */
            if (parameterValue is DateTime)
            {
                var time = (DateTime)parameterValue;
                return string.Concat("TO_DATE (", time.ToString(@"\'yyyy-MM-dd HH:mm:ss\'"), ", \'YYYY-MM-DD HH24:MI:SS\')");
            }

            /*
             * Return other
             */
            return parameterValue.ToString();
        }

	    /// <summary>
	    /// Returns the SQL Casing of the used Database.
	    /// </summary>
	    public override SqlCasing SqlCasing
	    {
            get { return Internal.SqlCasing.UpperCase; }
	    }

	    #endregion
    }
}

