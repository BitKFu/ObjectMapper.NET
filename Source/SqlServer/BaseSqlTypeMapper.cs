using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.SqlServer
{
    /// <summary>
    /// Abstract base class for the concrete sql type mapper used for the SqlServer and SqlServerCE
    /// </summary>
    public abstract class BaseSqlTypeMapper : BaseTypeMapper
    {
        #region KEYWORDS

        /// <summary>
        /// Special Characters
        /// </summary>
        internal static readonly char[] SpecialCharacters = new[] { ' ', '#' };

        /// <summary>
        /// SQL Keywords
        /// </summary>
        internal static readonly HashSet<string> Keywords = new HashSet<string> {
                                                                                    "ADD", "EXCEPT", "PERCENT",
                                                                                    "ALL", "EXEC", "PLAN",
                                                                                    "ALTER", "EXECUTE", "PRECISION",
                                                                                    "AND", "EXISTS", "PRIMARY",
                                                                                    "ANY", "EXIT", "PRINT",
                                                                                    "AS", "FETCH", "PROC",
                                                                                    "ASC", "FILE", "PROCEDURE",
                                                                                    "AUTHORIZATION", "FILLFACTOR", "PUBLIC",
                                                                                    "BACKUP", "FOR", "RAISERROR",
                                                                                    "BEGIN", "FOREIGN", "READ",
                                                                                    "BETWEEN", "FREETEXT", "READTEXT",
                                                                                    "BREAK", "FREETEXTTABLE", "RECONFIGURE",
                                                                                    "BROWSE", "FROM", "REFERENCES",
                                                                                    "BULK", "FULL", "REPLICATION",
                                                                                    "BY", "FUNCTION", "RESTORE",
                                                                                    "CASCADE", "GOTO", "RESTRICT",
                                                                                    "CASE", "GRANT", "RETURN",
                                                                                    "CHECK", "GROUP", "REVOKE",
                                                                                    "CHECKPOINT", "HAVING", "RIGHT",
                                                                                    "CLOSE", "HOLDLOCK", "ROLLBACK",
                                                                                    "CLUSTERED", "IDENTITY", "ROWCOUNT",
                                                                                    "COALESCE", "IDENTITY_INSERT", "ROWGUIDCOL",
                                                                                    "COLLATE", "IDENTITYCOL", "RULE",
                                                                                    "COLUMN", "IF", "SAVE",
                                                                                    "COMMIT", "IN", "SCHEMA",
                                                                                    "COMPUTE", "INDEX", "SELECT",
                                                                                    "CONSTRAINT", "INNER", "SESSION_USER",
                                                                                    "CONTAINS", "INSERT", "SET",
                                                                                    "CONTAINSTABLE", "INTERSECT", "SETUSER",
                                                                                    "CONTINUE", "INTO", "SHUTDOWN",
                                                                                    "CONVERT", "IS", "SOME",
                                                                                    "CREATE", "JOIN", "STATISTICS",
                                                                                    "CROSS", "KEY", "SYSTEM_USER",
                                                                                    "CURRENT", "KILL", "TABLE",
                                                                                    "CURRENT_DATE", "LEFT", "TEXTSIZE",
                                                                                    "CURRENT_TIME", "LIKE", "THEN",
                                                                                    "CURRENT_TIMESTAMP", "LINENO", "TO",
                                                                                    "CURRENT_USER", "LOAD", "TOP",
                                                                                    "CURSOR", "NATIONAL", "TRAN",
                                                                                    "DATABASE", "NOCHECK", "TRANSACTION",
                                                                                    "DBCC", "NONCLUSTERED", "TRIGGER",
                                                                                    "DEALLOCATE", "NOT", "TRUNCATE",
                                                                                    "DECLARE", "NULL", "TSEQUAL",
                                                                                    "DEFAULT", "NULLIF", "UNION",
                                                                                    "DELETE", "OF", "UNIQUE",
                                                                                    "DENY", "OFF", "UPDATE",
                                                                                    "DESC", "OFFSETS", "UPDATETEXT",
                                                                                    "DISK", "ON", "USE",
                                                                                    "DISTINCT", "OPEN", "USER",
                                                                                    "DISTRIBUTED", "OPENDATASOURCE", "VALUES",
                                                                                    "DOUBLE", "OPENQUERY", "VARYING",
                                                                                    "DROP", "OPENROWSET", "VIEW",
                                                                                    "DUMMY", "OPENXML", "WAITFOR",
                                                                                    "DUMP", "OPTION", "WHEN",
                                                                                    "ELSE", "OR", "WHERE",
                                                                                    "END", "ORDER", "WHILE",
                                                                                    "ERRLVL", "OUTER", "WITH",
                                                                                    "ESCAPE", "OVER", "WRITETEXT",
        
                                                                                    "ABSOLUTE", "EXEC", "OVERLAPS",
                                                                                    "ACTION", "EXECUTE", "PAD",
                                                                                    "ADA", "EXISTS", "PARTIAL",
                                                                                    "ADD", "EXTERNAL", "PASCAL",
                                                                                    "ALL", "EXTRACT", "POSITION",
                                                                                    "ALLOCATE", "FALSE", "PRECISION",
                                                                                    "ALTER", "FETCH", "PREPARE",
                                                                                    "AND", "FIRST", "PRESERVE",
                                                                                    "ANY", "FLOAT", "PRIMARY",
                                                                                    "ARE", "FOR", "PRIOR",
                                                                                    "AS", "FOREIGN", "PRIVILEGES",
                                                                                    "ASC", "FORTRAN", "PROCEDURE",
                                                                                    "ASSERTION", "FOUND", "PUBLIC",
                                                                                    "AT", "FROM", "READ",
                                                                                    "AUTHORIZATION", "FULL", "REAL",
                                                                                    "AVG", "GET", "REFERENCES",
                                                                                    "BEGIN", "GLOBAL", "RELATIVE",
                                                                                    "BETWEEN", "GO", "RESTRICT",
                                                                                    "BIT", "GOTO", "REVOKE",
                                                                                    "BIT_LENGTH", "GRANT", "RIGHT",
                                                                                    "BOTH", "GROUP", "ROLLBACK",
                                                                                    "BY", "HAVING", "ROWS",
                                                                                    "CASCADE", "HOUR", "SCHEMA",
                                                                                    "CASCADED", "IDENTITY", "SCROLL",
                                                                                    "CASE", "IMMEDIATE", "SECOND",
                                                                                    "CAST", "IN", "SECTION",
                                                                                    "CATALOG", "INCLUDE", "SELECT",
                                                                                    "CHAR", "INDEX", "SESSION",
                                                                                    "CHAR_LENGTH", "INDICATOR", "SESSION_USER",
                                                                                    "CHARACTER", "INITIALLY", "SET",
                                                                                    "CHARACTER_LENGTH", "INNER", "SIZE",
                                                                                    "CHECK", "INPUT", "SMALLINT",
                                                                                    "CLOSE", "INSENSITIVE", "SOME",
                                                                                    "COALESCE", "INSERT", "SPACE",
                                                                                    "COLLATE", "INT", "SQL",
                                                                                    "COLLATION", "INTEGER", "SQLCA",
                                                                                    "COLUMN", "INTERSECT", "SQLCODE",
                                                                                    "COMMIT", "INTERVAL", "SQLERROR",
                                                                                    "CONNECT", "INTO", "SQLSTATE",
                                                                                    "CONNECTION", "IS", "SQLWARNING",
                                                                                    "CONSTRAINT", "ISOLATION", "SUBSTRING",
                                                                                    "CONSTRAINTS", "JOIN", "SUM",
                                                                                    "CONTINUE", "KEY", "SYSTEM_USER",
                                                                                    "CONVERT", "LANGUAGE", "TABLE",
                                                                                    "CORRESPONDING", "LAST", "TEMPORARY",
                                                                                    "COUNT", "LEADING", "THEN",
                                                                                    "CREATE", "LEFT", "TIME",
                                                                                    "CROSS", "LEVEL", "TIMESTAMP",
                                                                                    "CURRENT", "LIKE", "TIMEZONE_HOUR",
                                                                                    "CURRENT_DATE", "LOCAL", "TIMEZONE_MINUTE",
                                                                                    "CURRENT_TIME", "LOWER", "TO",
                                                                                    "CURRENT_TIMESTAMP", "MATCH", "TRAILING",
                                                                                    "CURRENT_USER", "MAX", "TRANSACTION",
                                                                                    "CURSOR", "MIN", "TRANSLATE",
                                                                                    "DATE", "MINUTE", "TRANSLATION",
                                                                                    "DAY", "MODULE", "TRIM",
                                                                                    "DEALLOCATE", "MONTH", "TRUE",
                                                                                    "DEC", "NAMES", "UNION",
                                                                                    "DECIMAL", "NATIONAL", "UNIQUE",
                                                                                    "DECLARE", "NATURAL", "UNKNOWN",
                                                                                    "DEFAULT", "NCHAR", "UPDATE",
                                                                                    "DEFERRABLE", "NEXT", "UPPER",
                                                                                    "DEFERRED", "NO", "USAGE",
                                                                                    "DELETE", "NONE", "USER",
                                                                                    "DESC", "NOT", "USING",
                                                                                    "DESCRIBE", "NULL", "VALUE",
                                                                                    "DESCRIPTOR", "NULLIF", "VALUES",
                                                                                    "DIAGNOSTICS", "NUMERIC", "VARCHAR",
                                                                                    "DISCONNECT", "OCTET_LENGTH", "VARYING",
                                                                                    "DISTINCT", "OF", "VIEW",
                                                                                    "DOMAIN", "ON", "WHEN",
                                                                                    "DOUBLE", "ONLY", "WHENEVER",
                                                                                    "DROP", "OPEN", "WHERE",
                                                                                    "ELSE", "OPTION", "WITH",
                                                                                    "END", "OR", "WORK",
                                                                                    "END-EXEC", "ORDER", "WRITE",
                                                                                    "ESCAPE", "OUTER", "YEAR",
                                                                                    "EXCEPT", "OUTPUT", "ZONE",
                                                                                    "EXCEPTION", 

                                                                                    "ABSOLUTE", "FOUND", "PRESERVE",
                                                                                    "ACTION", "FREE", "PRIOR",
                                                                                    "ADMIN", "GENERAL", "PRIVILEGES",
                                                                                    "AFTER", "GET", "READS",
                                                                                    "AGGREGATE", "GLOBAL", "REAL",
                                                                                    "ALIAS", "GO", "RECURSIVE",
                                                                                    "ALLOCATE", "GROUPING", "REF",
                                                                                    "ARE", "HOST", "REFERENCING",
                                                                                    "ARRAY", "HOUR", "RELATIVE",
                                                                                    "ASSERTION", "IGNORE", "RESULT",
                                                                                    "AT", "IMMEDIATE", "RETURNS",
                                                                                    "BEFORE", "INDICATOR", "ROLE",
                                                                                    "BINARY", "INITIALIZE", "ROLLUP",
                                                                                    "BIT", "INITIALLY", "ROUTINE",
                                                                                    "BLOB", "INOUT", "ROW",
                                                                                    "BOOLEAN", "INPUT", "ROWS",
                                                                                    "BOTH", "INT", "SAVEPOINT",
                                                                                    "BREADTH", "INTEGER", "SCROLL",
                                                                                    "CALL", "INTERVAL", "SCOPE",
                                                                                    "CASCADED", "ISOLATION", "SEARCH",
                                                                                    "CAST", "ITERATE", "SECOND",
                                                                                    "CATALOG", "LANGUAGE", "SECTION",
                                                                                    "CHAR", "LARGE", "SEQUENCE",
                                                                                    "CHARACTER", "LAST", "SESSION",
                                                                                    "CLASS", "LATERAL", "SETS",
                                                                                    "CLOB", "LEADING", "SIZE",
                                                                                    "COLLATION", "LESS", "SMALLINT",
                                                                                    "COMPLETION", "LEVEL", "SPACE",
                                                                                    "CONNECT", "LIMIT", "SPECIFIC",
                                                                                    "CONNECTION", "LOCAL", "SPECIFICTYPE",
                                                                                    "CONSTRAINTS", "LOCALTIME", "SQL",
                                                                                    "CONSTRUCTOR", "LOCALTIMESTAMP", "SQLEXCEPTION",
                                                                                    "CORRESPONDING", "LOCATOR", "SQLSTATE",
                                                                                    "CUBE", "MAP", "SQLWARNING",
                                                                                    "CURRENT_PATH", "MATCH", "START",
                                                                                    "CURRENT_ROLE", "MINUTE", "STATE",
                                                                                    "CYCLE", "MODIFIES", "STATEMENT",
                                                                                    "DATA", "MODIFY", "STATIC",
                                                                                    "DATE", "MODULE", "STRUCTURE",
                                                                                    "DAY", "MONTH", "TEMPORARY",
                                                                                    "DEC", "NAMES", "TERMINATE",
                                                                                    "DECIMAL", "NATURAL", "THAN",
                                                                                    "DEFERRABLE", "NCHAR", "TIME",
                                                                                    "DEFERRED", "NCLOB", "TIMESTAMP",
                                                                                    "DEPTH", "NEW", "TIMEZONE_HOUR",
                                                                                    "DEREF", "NEXT", "TIMEZONE_MINUTE",
                                                                                    "DESCRIBE", "NO", "TRAILING",
                                                                                    "DESCRIPTOR", "NONE", "TRANSLATION",
                                                                                    "DESTROY", "NUMERIC", "TREAT",
                                                                                    "DESTRUCTOR", "OBJECT", "TRUE",
                                                                                    "DETERMINISTIC", "OLD", "UNDER",
                                                                                    "DICTIONARY", "ONLY", "UNKNOWN",
                                                                                    "DIAGNOSTICS", "OPERATION", "UNNEST",
                                                                                    "DISCONNECT", "ORDINALITY", "USAGE",
                                                                                    "DOMAIN", "OUT", "USING",
                                                                                    "DYNAMIC", "OUTPUT", "VALUE",
                                                                                    "EACH", "PAD", "VARCHAR",
                                                                                    "END-EXEC", "PARAMETER", "VARIABLE",
                                                                                    "EQUALS", "PARAMETERS", "WHENEVER",
                                                                                    "EVERY", "PARTIAL", "WITHOUT",
                                                                                    "EXCEPTION", "PATH", "WORK",
                                                                                    "EXTERNAL", "POSTFIX", "WRITE",
                                                                                    "FALSE", "PREFIX", "YEAR",
                                                                                    "FIRST", "PREORDER", "ZONE",
                                                                                    "FLOAT", "PREPARE"};

        #endregion

        /// <summary>
        /// Gets the string for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="type">The type.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected override string GetStringForDDL(FieldDescription field, Type type, int length)
        {
            return base.GetStringForDDL(field, type,length) ?? "IMAGE";
        }

        /// <summary>
        /// Gets the enum for database.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
        /// <returns></returns>
        public override Enum GetEnumForDatabase(Type type, bool isUnicode)
        {
            SqlDbType result = SqlDbType.Image;
            type = TypeHelper.GetBaseType(type);

            if (SqlMappingTable.ContainsKey(type))
                result = (SqlDbType)SqlMappingTable[type];
            else
            {
                /*
                 * Wenn keine direkte Zuordnung möglich ist, dann nach Implementierungen suchen
                 */
                foreach (var dict in SqlMappingTable)
                {
                    if (type.IsDerivedFrom(dict.Key))
                    {
                        result = (SqlDbType) dict.Value;
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
                    case SqlDbType.Char:
                        result = SqlDbType.NChar;
                        break;

                    case SqlDbType.VarChar:
                        result = SqlDbType.NVarChar;
                        break;

                    case SqlDbType.Text :
                        result = SqlDbType.NText;
                        break;
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
            get { return "IDENTITY"; }
        }

        /// <summary>
        /// Converts the source to a database specific type and returns it.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override object ConvertValueToDbType(object value)
        {
            if (value is TimeSpan)
                return ((TimeSpan)value).Ticks;

            return base.ConvertValueToDbType(value);
        }

        /// <summary>
        /// Gets the param value as SQL string.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns></returns>
        public override string GetParamValueAsSQLString(object parameterValue)
        {
            string result;

            /*
             * Check guid
             */
            var byteArray = parameterValue as Byte[];
            if ((byteArray != null) && (byteArray.Length == 16))
            {
                result = string.Concat("'", new Guid(byteArray).ToString(), "'");
                return result;
            }

            if (parameterValue is Guid)
            {
                result = string.Concat("'", ((Guid)parameterValue).ToString(), "'");
                return result;
            }

            /*
             * Check base types
             */
            result = base.GetParamValueAsSQLString(parameterValue);
            if (result != string.Empty)
                return result;

            /*
             * Check date time
             */
            if (parameterValue is DateTime)
            {
                var time = (DateTime)parameterValue;
                return string.Concat("CONVERT (DATETIME, '", time.ToString(@"yyyy/MM/dd HH:mm:ss,20"), "')");
            }

            /*
             * Return other
             */
            return parameterValue.ToString();
        }

        /// <summary>
        /// Quotes the string, if necessary
        /// </summary>
        /// <returns></returns>
        public override string Quote(string column)
        {
            string[] parts = column.Split(',');
            string result = string.Empty;
            for (int x = 0; x < parts.Length; x++)
            {
                string trim = parts[x].Trim();

                bool isKeyWord = Keywords.Contains(trim.ToUpper(CultureInfo.InvariantCulture));

                result = string.Concat(result, (x > 0 ? "," : ""),
                                       isKeyWord || trim.IndexOfAny(SpecialCharacters) >= 0 ? string.Concat("[", trim, "]") : trim);
            }

            return result;
        }
    }
}