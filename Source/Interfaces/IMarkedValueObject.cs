using System;
using System.Diagnostics;

namespace AdFactum.Data
{
	/// <summary>
	/// Interface that defines a lst Update field.
	/// </summary>
	public interface IMarkedValueObject : IValueObject
	{
		/// <summary>
		/// Returns the latest update function to identify bad update
		/// </summary>
		/// <value>The last update.</value>
		DateTime LastUpdate
		{
			[DebuggerStepThrough]
			get; 
			set;
		}

	}
}