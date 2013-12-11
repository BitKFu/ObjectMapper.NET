using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Access
{
	/// <summary>
	/// Class for mapping the .NET types to OleDbTypes
	/// </summary>
	[Serializable]
    public class AccessTypeMapper : BaseTypeMapper
    {
        #region Keywords

        /// <summary>
        /// Special Characters
        /// </summary>
        internal static readonly char[] SpecialCharacters = new[] { ' ', '#' };

	    /// <summary>
	    /// Access Keywords
	    /// </summary>
        internal static readonly HashSet<string> Keywords = new HashSet<string>
        {
            "ADD","ALL","ALPHANUMERIC","ALTER","AND","ANY","APPLICATION","AS","ASC","ASSISTANT","AUTOINCREMENT","AVG",
            "BETWEEN","BINARY","BIT","BOOLEAN","BY","BYTE",
            "CHAR", "CHARACTER","COLUMN","COMPACTDATABASE","CONSTRAINT","CONTAINER","COUNT","COUNTER","CREATE","CREATEDATABASE",
            "CREATEFIELD","CREATEGROUP","CREATEINDEX","CREATEOBJECT","CREATEPROPERTY","CREATERELATION","CREATETABLEDEF","CREATEUSER",
            "CREATEWORKSPACE","CURRENCY","CURRENTUSER",
            "DATABASE","DATE","DATETIME","DELETE","DESC","DESCRIPTION","DISALLOW","DISTINCT","DISTINCTROW","DOCUMENT","DOUBLE","DROP",
            "ECHO","ELSE","END","EQV","ERROR","EXISTS","EXIT",
            "FALSE","FIELD", "FIELDS","FILLCACHE","FLOAT", "FLOAT4", "FLOAT8","FOREIGN","FORM", "FORMS","FROM","FULL","FUNCTION",
            "GENERAL","GETOBJECT","GETOPTION","GOTOPAGE","GROUP","GROUP BY","GUID",
            "HAVING","IDLE","IEEEDOUBLE", "IEEESINGLE","IF","IGNORE","IMP","IN","INDEX", "INDEXES","INNER","INSERT","INSERTTEXT",
            "INT", "INTEGER", "INTEGER1", "INTEGER2", "INTEGER4","INTO","IS",
            "JOIN","KEY","LASTMODIFIED","LEFT","LEVEL","LIKE","LOGICAL", "LOGICAL1","LONG", "LONGBINARY", "LONGTEXT",
            "MACRO", "MATCH", "MAX", "MIN", "MOD", "MEMO", "MODULE", "MONEY", "MOVE", 
            "NAME", "NEWPASSWORD", "NO", "NOT", "NOTE", "NULL", "NUMBER", "NUMERIC",
            "OBJECT", "OLEOBJECT", "OFF", "ON", "OPENRECORDSET", "OPTION", "OR", "ORDER", "ORIENTATION", "OUTER", "OWNERACCESS",
            "PARAMETER", "PARAMETERS", "PARTIAL", "PERCENT", "PIVOT", "PRIMARY", "PROCEDURE", "PROPERTY", 
            "QUERIES", "QUERY", "QUIT",
            "REAL","RECALC","RECORDSET","REFERENCES","REFRESH","REFRESHLINK","REGISTERDATABASE","RELATION","REPAINT","REPAIRDATABASE",
            "REPORT","REPORTS","REQUERY","RIGHT",
            "SCREEN","SECTION","SELECT","SET","SETFOCUS","SETOPTION","SHORT","SINGLE","SMALLINT","SOME","SQL","STDEV", "STDEVP","STRING","SUM",
            "TABLE","TABLEDEF", "TABLEDEFS","TABLEID","TEXT","TIME", "TIMESTAMP","TOP","TRANSFORM","TRUE","TYPE",
            "UNION","UNIQUE","UPDATE","USER",
            "VALUE","VALUES","VAR", "VARP","VARBINARY", "VARCHAR","VERSION",
            "WHERE","WITH","WORKSPACE",
            "XOR","YEAR","YES","YESNO","GLOBAL","PRECISION","DAY","MONTH","YEAR","REFERENCE","WEEKDAY","REVERS","TRANSLATION"
        };
        #endregion

        /// <summary>
		/// Type Mapping class UINT16
		/// </summary>
		public AccessTypeMapper()
		{
			DbMappingTable.Add(typeof(Boolean), "BIT");
			DbMappingTable.Add(typeof(Byte), "BYTE");
			DbMappingTable.Add(typeof(DateTime), "DATETIME");
            DbMappingTable.Add(typeof(Decimal), "DOUBLE");
			DbMappingTable.Add(typeof(Double), "DOUBLE");
			DbMappingTable.Add(typeof(Guid), "GUID");
			DbMappingTable.Add(typeof(Int16), "SMALLINT");
			DbMappingTable.Add(typeof(Int32), "INT");
			DbMappingTable.Add(typeof(Int64), "LONG");
			DbMappingTable.Add(typeof(Single), "SINGLE");
			DbMappingTable.Add(typeof(String), "VARCHAR({0})");
			DbMappingTable.Add(typeof(TimeSpan), "DATETIME");
            DbMappingTable.Add(typeof(Enum), "SMALLINT");
			DbMappingTable.Add(typeof(Stream), "IMAGE");
			DbMappingTable.Add(typeof(Byte[]), "IMAGE");
            DbMappingTable.Add(typeof(Char), "CHAR(1)");

			SqlMappingTable.Add(typeof(Boolean), (int) OleDbType.Boolean);
			SqlMappingTable.Add(typeof(Byte), (int) OleDbType.UnsignedTinyInt);
			SqlMappingTable.Add(typeof(DateTime), (int) OleDbType.Date);
            SqlMappingTable.Add(typeof(Decimal), (int) OleDbType.Double);
			SqlMappingTable.Add(typeof(Double), (int) OleDbType.Double);
			SqlMappingTable.Add(typeof(Guid), (int) OleDbType.Guid);
			SqlMappingTable.Add(typeof(Int16), (int) OleDbType.SmallInt);
			SqlMappingTable.Add(typeof(Int32), (int) OleDbType.Integer);
            SqlMappingTable.Add(typeof(Int64), (int) OleDbType.Integer);
			SqlMappingTable.Add(typeof(Single), (int) OleDbType.Single);
			SqlMappingTable.Add(typeof(String), (int) OleDbType.VarChar);
			SqlMappingTable.Add(typeof(TimeSpan), (int) OleDbType.Date);
            SqlMappingTable.Add(typeof(Enum), (int) OleDbType.SmallInt);
			SqlMappingTable.Add(typeof(Stream), (int) OleDbType.LongVarBinary);
			SqlMappingTable.Add(typeof(Byte[]), (int) OleDbType.LongVarBinary);
            SqlMappingTable.Add(typeof(Char), (int) OleDbType.Char);
        }

		#region ITypeMapper Members

        /// <summary>
        /// Gets the string for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        public override string GetStringForDDL(FieldDescription field)
		{
			string result = "IMAGE";

            Type type = PreTransformTypeForDDL(field);
            int length = field.CustomProperty.MetaInfo.Length;
            
            /*
             * Strings that are greater than 255 characters are handled as MEMOS
             */
            if (type.Equals(typeof(string)) && length > 255)
                return "MEMO";

            result = base.GetStringForDDL(field, type, length) ?? result;

            /*
             * Place Unicode N in Front of the field type
             */
            if (field.CustomProperty.MetaInfo.IsUnicode)
            {
                if (result.StartsWith("CHAR") || result.StartsWith("VARCHAR"))
                    result = string.Concat("N", result);
            }

            return result;
		}

        /// <summary>
        /// Gets the enum for database.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
        /// <returns></returns>
        public override Enum GetEnumForDatabase(Type type, int size, bool isUnicode)
		{
			OleDbType result = OleDbType.LongVarBinary;
            type = TypeHelper.GetBaseType(type);

			if (SqlMappingTable.ContainsKey(type))
				result = (OleDbType) SqlMappingTable[type];
			else
			{
				/*
				 * Wenn keine direkte Zuordnung möglich ist, dann nach Implementierungen suchen
				 */
				foreach (var dict in SqlMappingTable)
				{
					if (type.IsDerivedFrom(dict.Key))
						return (OleDbType) dict.Value;
				}
			}

            /*
             * Switch to unicode
             */
            if (isUnicode)
            {
                switch (result)
                {
                    case OleDbType.Char:
                        result = OleDbType.WChar;
                        break;

                    case OleDbType.VarChar:
                        result = OleDbType.VarWChar;
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
            get { return "COUNTER"; }
	    }

        /// <summary>
        /// Converts a type to the typed used from the current database implementation
        /// </summary>
        /// <param name="checkType">Type of the check.</param>
        /// <returns></returns>
        public override Type GetTypeForDatabase(Type checkType)
        {
            checkType = TypeHelper.GetBaseType(checkType);

            if (checkType == typeof(long))
                return typeof (int);

            if (checkType == typeof(decimal))
                return typeof(double);

            return base.GetTypeForDatabase(checkType);
        }

        /// <summary>
        /// Gets the param value as SQL string.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns></returns>
        public override string GetParamValueAsSQLString(object parameterValue)
        {
            // Check bool 
            if (parameterValue is bool)
                return parameterValue.Equals(true) ? "true" : "false";

            /*
             * Check base types
             */
            string baseResult = base.GetParamValueAsSQLString(parameterValue);
            if (baseResult != string.Empty)
                return baseResult;

            /*
             * Check Guid
             */
            if (parameterValue is Guid)
            {
                var guid = (Guid)parameterValue;
                return string.Concat("{guid {" , guid , "}}");
            }

            /*
             * Check date time
             */
            if (parameterValue is DateTime)
            {
                var time = (DateTime)parameterValue;
                return time.ToString(@"#MM\/dd\/yyyy HH:mm:ss#");
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
	        get { return true;}
	    }

	    #endregion
	}

}