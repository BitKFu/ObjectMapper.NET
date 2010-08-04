using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
    /// <summary> Defines the Casing of the Database </summary>
    public enum SqlCasing
    {
        /// <summary> Database uses lower casing </summary>
        LowerCase,

        /// <summary> Database uses mixed casing </summary>
        Mixed,

        /// <summary> Database uses upper casing</summary>
        UpperCase
    }

    /// <summary>
    /// Base class used by every type mapper derivation
    /// </summary>
    public abstract class BaseTypeMapper : ITypeMapper
    {
        private readonly Dictionary<Type, string> dbMappingTable = new Dictionary<Type, string>();
        private readonly Dictionary<Type, int> sqlMappingTable = new Dictionary<Type, int>();

        /// <summary>
        /// Gets the db mapping table.
        /// </summary>
        /// <value>The db mapping table.</value>
        public Dictionary<Type, string> DbMappingTable
        {
            get { return dbMappingTable; }
        }

        /// <summary>
        /// Returns the SQL Casing of the used Database.
        /// </summary>
        public abstract SqlCasing SqlCasing { get; }

        /// <summary>
        /// Gets a value indicating whether [parameter duplication].
        /// </summary>
        /// <value><c>true</c> if [parameter duplication]; otherwise, <c>false</c>.</value>
        public abstract bool ParameterDuplication { get; }

        /// <summary>
        /// Gets the SQL mapping table.
        /// </summary>
        /// <value>The SQL mapping table.</value>
        protected Dictionary<Type, int> SqlMappingTable
        {
            get { return sqlMappingTable; }
        }

        /// <summary>
        /// Gets the string for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="type">The type.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected virtual string GetStringForDDL(FieldDescription field, Type type, int length)
        {
            var result = string.Empty;

            /*
             * Get database mappings
             */
            if (DbMappingTable.ContainsKey(type))
                result = string.Concat(result, DbMappingTable[type]);
            else
            {
                /*
                 * Wenn keine direkte Zuordnung möglich ist, dann nach Implementierungen suchen
                 */
                foreach (var dict in DbMappingTable)
                {
                    if (!type.IsDerivedFrom(dict.Key)) continue;
                    result = string.Concat(result, dict.Value);
                    break;
                }
            }

            /*
             * Replace length 
             */
            if (length > 0)
                result = string.Format(result, length);

            /*
             * Add default value and not null extension
             */
            result = AddDefaultToDDL(field, result);

            return result;
        }

        /// <summary>
        /// Adds an default value to the DDL
        /// </summary>
        /// <param name="result"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        protected string AddDefaultToDDL(FieldDescription field, string result)
        {
            object defaultValue = field.CustomProperty.MetaInfo.DefaultValue;
            if (defaultValue != null)
                result = string.Concat(result, " DEFAULT ", GetParamValueAsSQLString(ConvertValue(defaultValue)));

            if (field.CustomProperty.MetaInfo.IsRequiered)
                result = string.Concat(result, " NOT NULL");
            return result;
        }

        /// <summary>
        /// Pres the transform type for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        protected static Type PreTransformTypeForDDL(FieldDescription field)
        {
            Type type = TypeHelper.GetBaseType(field.ContentType);

            /*
             * If it's a list binding, retrieve the link target
             */
            if (type.IsListType())
                type = field.CustomProperty.MetaInfo.LinkTarget;

            return type;
        }

        /// <summary>
        /// Gets the string for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public abstract string GetStringForDDL(FieldDescription field);

        /// <summary>
        /// Gets the enum for database.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
        /// <returns></returns>
        public abstract Enum GetEnumForDatabase(Type type, bool isUnicode);

        /// <summary>
        /// Converts the source to a database specific type and returns it.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual object ConvertValue(object value)
        {
            if (IsDbNull(value))
                return DBNull.Value;

            if (value is TimeSpan)
                return DateTime.MinValue.Add((TimeSpan)value);

            if (value is Enum)
                return (int)value;

            var vo = value as IValueObject;
            if (vo != null)
                return vo.Id;

            var stream = value as Stream;
            if (stream != null)
            {
                var content = new byte[stream.Length];
                var length = (int)stream.Length;
                stream.Seek(0, SeekOrigin.Begin);
                int readed = stream.Read(content, 0, length);
                Debug.Assert(readed == length, "Could not read stream.");

                return content;
            }

            return value;
        }

        /// <summary>
        /// Converts a type to the typed used from the current database implementation
        /// </summary>
        /// <param name="checkType">Type of the check.</param>
        /// <returns></returns>
        public virtual Type GetTypeForDatabase(Type checkType)
        {
            if (checkType.IsEnum)               checkType = typeof(Int16); else
            if (checkType == typeof(Stream))    checkType = typeof(Byte[]);else
            if (checkType == typeof(TimeSpan))  checkType = typeof(DateTime);  

            return checkType;
        }

        /// <summary>
        /// Determines whether [is db null] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if [is db null] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsDbNull(object value)
        {
            if (value == null)
                return true;

            if ((value is DateTime) && (value.Equals(DateTime.MinValue)))
                return true;

            if ((value is char) && (value.Equals('\0')))
                return true;

            if ((value is Guid) && (((Guid)value).Equals(Guid.Empty)))
                return true;

            var stream = value as Stream;
            if (stream != null)
            {
                if (stream.Length == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the auto increment identifier.
        /// </summary>
        /// <value>The auto increment identifier.</value>
        public abstract string AutoIncrementIdentifier { get; }

        /// <summary>
        /// Gets the param value as SQL string.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns></returns>
        public virtual string GetParamValueAsSQLString(object parameterValue)
        {
            var byteArray = parameterValue as Byte[];

            /*
             * Check byte Array
             */
            if (byteArray != null)
            {
                var sb = new StringBuilder();
                int counter = 0;

                IEnumerator byteEnum = byteArray.GetEnumerator();
                sb.Append("hextoraw('");
                while ((byteEnum.MoveNext()) && (counter++ < 100))
                    sb.Append(((Byte)byteEnum.Current).ToString("X2"));

                if (counter < byteArray.Length)
                    sb.Append(" ... ");

                sb.Append("')");
                return sb.ToString();
            }

            string result = string.Empty;

            /*
             * Check base types
             */
            if (parameterValue == null || parameterValue.Equals(DBNull.Value))
                result = "NULL";
            else if (parameterValue is bool)
                result = parameterValue.Equals(true) ? "1" : "0";
            else if (parameterValue is int)
                result = parameterValue.ToString();
            else if (parameterValue.GetType().IsEnum)
                result = ((int)parameterValue).ToString();
            else if (parameterValue is string || parameterValue is char)
                result = string.Concat("'", parameterValue.ToString().Replace("'","''"), "'");
            else if (parameterValue is double || parameterValue is float || parameterValue is Single || parameterValue is Decimal)
                result = parameterValue.ToString().Replace(',', '.');

            return result;
        }

        /// <summary>
        /// Converts the identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public string DoCasing(string identifier)
        {
            switch (SqlCasing)
            {
                case SqlCasing.LowerCase:
                    return identifier.ToLower();
                case SqlCasing.UpperCase:
                    return identifier.ToUpper();
            }

            return identifier;
        }

        /// <summary>
        /// Quotes the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public virtual string Quote(string expression)
        {
            string[] parts = expression.Split(',');
            string result = string.Empty;
            for (int x = 0; x < parts.Length; x++)
            {
                string trim = parts[x].Trim();

                bool isKeyWord = expression.Contains(" ");

                result = string.Concat(result, (x > 0 ? "," : ""),
                                       isKeyWord || trim.Contains(" ") ? string.Concat("[", trim, "]") : trim);
            }

            return result;
        }
    }
}
