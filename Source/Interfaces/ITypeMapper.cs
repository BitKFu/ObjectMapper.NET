using System;
using System.Collections.Generic;
using AdFactum.Data.Internal;

namespace AdFactum.Data
{
	/// <summary>
	/// This is a interface to map .NET types to the database
	/// </summary>
	public interface ITypeMapper
	{
        /// <summary>
        /// Gets the string for DDL.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        string GetStringForDDL(FieldDescription field);

        /// <summary>
        /// Gets the enum for database.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="size">Size of the value</param>
        /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
        /// <returns></returns>
        Enum GetEnumForDatabase(Type type, int size, bool isUnicode);

		/// <summary>
		/// Converts the source to a database specific type and returns it.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		object ConvertValueToDbType	(object value);

        /// <summary>
        /// Convert the given value to the specific type.
        /// </summary>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
	    object ConvertToType(Type returnType, object value);

		/// <summary>
		/// Converts a type to the typed used from the current database implementation
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		Type GetTypeForDatabase (Type type);

        /// <summary>
        /// Determines whether [is db null] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if [is db null] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
	    bool IsDbNull(object value);

        /// <summary>
        /// Gets the auto increment identifier.
        /// </summary>
        /// <value>The auto increment identifier.</value>
        string AutoIncrementIdentifier { get; }

        /// <summary>
        /// Gets the param value as SQL string.
        /// </summary>
        /// <param name="parameterValue">The parameter value.</param>
        /// <returns></returns>
        string GetParamValueAsSQLString(object parameterValue);

        /// <summary>
        /// Quotes the expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
	    string Quote(string expression);

        /// <summary> Returns a type mapping table in order to map types to database types </summary>
        Dictionary<Type, string> DbMappingTable { get; }

        /// <summary>
        /// Returns the SQL Casing of the used Database.
        /// </summary>
        SqlCasing SqlCasing { get; }

        /// <summary>
        /// Gets a value indicating whether [parameter duplication].
        /// </summary>
        /// <value><c>true</c> if [parameter duplication]; otherwise, <c>false</c>.</value>
	    bool ParameterDuplication { get; }

	    /// <summary>
        /// Returns the correct cased string
        /// </summary>
	    string DoCasing(string sql);
	}
}
