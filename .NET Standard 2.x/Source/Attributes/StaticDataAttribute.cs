using System;

namespace AdFactum.Data
{
	/// <summary>
	/// If a class has the static data attribute than the content of the table will not be deleted,
	/// if the parent row that links to the content will be deleted. If the class does not own
	/// a Static Data Attribute all constraints will be cascaded to the class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	[Serializable]
    public class StaticDataAttribute : Attribute
	{
	}
}
