using System;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Util
{
	/// <summary>
	/// Const data is stored within this class
	/// </summary>
	public sealed class DBConst
	{
		/// <summary>
		/// Const definition of the Last Update field name
		/// </summary>
		public const string LastUpdateField = "LASTUPDATE";

		/*
		 * LinkList Field Const
		 */
		private static string linkIdField = "LINKID";
		internal const string ParentObjectField = "PARENTOBJECT";
		internal const string PropertyField = "PROPERTY";
		internal const string LinkedToField = "LINKEDTO";

		internal const string TypAddition = "#TYP";

		/*
		 * The smallest time entry that Microsoft Access can store
		 */
		internal static readonly DateTime AccessNullDate = new DateTime(1899, 12, 30);
	    private static SqlCasing globalCasing = SqlCasing.Mixed;

	    /// <summary>
		/// Static Link Id Field definition
		/// </summary>
		public static string LinkIdField
		{
			get { return linkIdField; }
			set { linkIdField = value; }
		}

        /// <summary>
        /// Gets or sets the global casing.
        /// </summary>
        /// <value>The global casing.</value>
        public static SqlCasing GlobalCasing { get { return globalCasing;} set { globalCasing = value;} }

        /// <summary>
        /// Does the global casing.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string DoGlobalCasing(string value)
        {
            switch (globalCasing)
            {
                case SqlCasing.LowerCase:
                    return value.ToLower();
                case SqlCasing.Mixed:
                    return value;
                case SqlCasing.UpperCase:
                    return value.ToUpper();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
	}
}