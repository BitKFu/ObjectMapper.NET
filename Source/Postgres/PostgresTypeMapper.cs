using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;
using NpgsqlTypes;

namespace AdFactum.Data.Postgres
{
    public class PostgresTypeMapper : BaseTypeMapper
    {
        public PostgresTypeMapper()
        {
            DbMappingTable.Add(typeof(Boolean), "boolean");
            DbMappingTable.Add(typeof(Byte), "smallint");
            DbMappingTable.Add(typeof(DateTime), "timestamp");
            DbMappingTable.Add(typeof(Decimal), "real");
            DbMappingTable.Add(typeof(Double), "double precision");
            DbMappingTable.Add(typeof(Guid), "uuid");
            DbMappingTable.Add(typeof(Int16), "smallint");
            DbMappingTable.Add(typeof(Int32), "integer");
            DbMappingTable.Add(typeof(Int64), "bigint");
            DbMappingTable.Add(typeof(Single), "real");
            DbMappingTable.Add(typeof(String), "varchar({0})");
            DbMappingTable.Add(typeof(TimeSpan), "interval");
            DbMappingTable.Add(typeof(Enum), "enum");
            DbMappingTable.Add(typeof(Stream), "bytea");
            DbMappingTable.Add(typeof(Byte[]), "bytea");
            DbMappingTable.Add(typeof(Char), "char(1)");

            SqlMappingTable.Add(typeof(Boolean), (int)NpgsqlDbType.Boolean);
            SqlMappingTable.Add(typeof(Byte), (int)NpgsqlDbType.Smallint);
            SqlMappingTable.Add(typeof(DateTime), (int)NpgsqlDbType.Timestamp);
            SqlMappingTable.Add(typeof(Decimal), (int)NpgsqlDbType.Real);
            SqlMappingTable.Add(typeof(Double), (int)NpgsqlDbType.Double);
            SqlMappingTable.Add(typeof(Guid), (int)NpgsqlDbType.Uuid);
            SqlMappingTable.Add(typeof(Int16), (int)NpgsqlDbType.Smallint);
            SqlMappingTable.Add(typeof(Int32), (int)NpgsqlDbType.Integer);
            SqlMappingTable.Add(typeof(Int64), (int)NpgsqlDbType.Bigint);
            SqlMappingTable.Add(typeof(Single), (int)NpgsqlDbType.Real);
            SqlMappingTable.Add(typeof(String), (int)NpgsqlDbType.Varchar);
            SqlMappingTable.Add(typeof(TimeSpan), (int)NpgsqlDbType.Interval);
            SqlMappingTable.Add(typeof(Enum), (int)NpgsqlDbType.Varchar);
            SqlMappingTable.Add(typeof(Stream), (int)NpgsqlDbType.Bytea);
            SqlMappingTable.Add(typeof(Byte[]), (int)NpgsqlDbType.Bytea);
            SqlMappingTable.Add(typeof(Char), (int)NpgsqlDbType.Char);
        }

        /// <summary>
        /// Gets the string for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public override string GetStringForDDL(FieldDescription field)
        {
            String result = "bytea";

            Type type = PreTransformTypeForDDL(field);
            int length = field.CustomProperty.MetaInfo.Length;

            /*
             * Strings that are greater than 4000 characters are handled as MEMOS
             */
            if (type.Equals(typeof(string)) && length > 4000)
                return "text";

            /*
             * Enum Types are supported by Postgres, so why don't use it ;)
             */
            if (type.IsEnum)
                return AddDefaultToDDL(field, Quote(type.Name));

            result = base.GetStringForDDL(field, type, length) ?? result;
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
            NpgsqlDbType result = NpgsqlDbType.Bytea;
            type = TypeHelper.GetBaseType(type);

            if (SqlMappingTable.ContainsKey(type))
                result = (NpgsqlDbType)SqlMappingTable[type];
            else
            {
                /*
                 * Wenn keine direkte Zuordnung möglich ist, dann nach Implementierungen suchen
                 */
                foreach (var dict in SqlMappingTable)
                {
                    if (type.IsDerivedFrom(dict.Key))
                    {
                        result = (NpgsqlDbType)dict.Value;
                        break;
                    }
                }
            }

            return result;
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
        /// Convert every quoted identifier to lower
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public override string Quote(string expression)
        {
            string[] parts = expression.Split(',');
            string result = string.Empty;
            for (int x = 0; x < parts.Length; x++)
            {
                string trim = parts[x].Trim();

                bool isKeyWord = expression.IndexOfAny(new[] {' ', '#'}) >= 0;
                bool containsUpperCase = expression.ToLowerInvariant() != expression;

                result = string.Concat(result, (x > 0 ? "," : ""), isKeyWord || containsUpperCase ? string.Concat("\"", trim, "\"") : trim);
            }

            return result;
        }

        /// <summary>
        /// Returns the SQL Casing of the used Database.
        /// </summary>
        public override SqlCasing SqlCasing
        {
            get { return SqlCasing.Mixed;  }
        }

        /// <summary>
        /// Gets a value indicating whether [parameter duplication].
        /// </summary>
        /// <value><c>true</c> if [parameter duplication]; otherwise, <c>false</c>.</value>
        public override bool ParameterDuplication
        {
            get { return true;}
        }


        /// <summary>
        /// Convert a paramter value to the valid string format
        /// </summary>
        public override string GetParamValueAsSQLString(object parameterValue)
        {
            // Boolean values can be returned as a string (not 0 or 1)
            if (parameterValue is bool)
                return (bool)parameterValue ? "true" : "false";

            // Enums can be evaluated directly
            if (parameterValue != null && parameterValue.GetType().IsEnum)
                return parameterValue.ToString();

            return base.GetParamValueAsSQLString(parameterValue);
        }

        /// <summary>
        /// Converts a type to the typed used from the current database implementation
        /// </summary>
        /// <param name="checkType">Type of the check.</param>
        /// <returns></returns>
        public override Type GetTypeForDatabase(Type checkType)
        {
            if (checkType == typeof(byte))
                return typeof (Int16);

            if (checkType == typeof(decimal))
                return typeof(Single);

            if (checkType == typeof(TimeSpan))
                return typeof (NpgsqlInterval);

            return base.GetTypeForDatabase(checkType);
        }

        /// <summary>
        /// Converts the source to a database specific type and returns it.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override object ConvertValue(object value)
        {
            if (value is TimeSpan)
                return new NpgsqlInterval((TimeSpan)value);

            if (value is DateTime)
                return new NpgsqlDate((DateTime) value);

            if (value is Enum) 
                return value.ToString();

            return base.ConvertValue(value);
        }
    }
}
