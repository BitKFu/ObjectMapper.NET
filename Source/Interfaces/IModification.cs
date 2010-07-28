using System;
using System.Diagnostics;

namespace AdFactum.Data
{
	/// <summary>
	/// Modification Interface
	/// </summary>
    public interface IModification : ICloneable
	{
        /// <summary>
        /// Returns the property name of the field or link
        /// </summary>
        string PropertyName
        { 
            get;
        }

		/// <summary>
		/// Gets or sets a value indicating whether this instance is modified.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is modified; otherwise, <c>false</c>.
		/// </value>
		bool IsModified
		{
			[DebuggerStepThrough]
			get; 
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is deleted.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is deleted; otherwise, <c>false</c>.
		/// </value>
		bool IsDeleted
		{
			[DebuggerStepThrough]
			get; 
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is new.
		/// </summary>
		/// <value><c>true</c> if this instance is new; otherwise, <c>false</c>.</value>
		bool IsNew 
		{
			[DebuggerStepThrough]
			get; 
			set; }

	}
}