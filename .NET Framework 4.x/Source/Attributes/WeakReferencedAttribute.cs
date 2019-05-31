using System;

namespace AdFactum.Data
{
	/// <summary>
	/// This attribute describes if the content always matches the constraint within the parent table.
	/// If a weak referenced attribute is set to a class, than the object mapper will always use an outer join
	/// to join this table.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	[Serializable]
    public class WeakReferencedAttribute : Attribute
	{
	}
}
