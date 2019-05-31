namespace AdFactum.Data.Internal
{
	/// <summary>
	/// The validator type defines, if a property is always valid or if the validation
	/// depends on the version of the database model.
	/// </summary>
	public enum ValidatorType
	{
		/// <summary>
		/// The property is always valid.
		/// </summary>
		AlwaysValid,

		/// <summary>
		/// The property is only valid until a special database model version.
		/// </summary>
		ValidUntil,

		/// <summary>
		/// The property is only valid since a special database model version.
		/// </summary>
		ValidSince
	}
}
