using System;

namespace AdFactum.Data
{
	/// <summary>
	/// If this attribute is set, the property is requiered and marked as not null on database
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	[Serializable]
    public sealed class RequiredAttribute : Attribute
	{
	}
}