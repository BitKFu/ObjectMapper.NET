namespace AdFactum.Data.Repository
{
	/// <summary>
	/// Summary description for FieldIntegrity.
	/// </summary>
	public class FieldIntegrity
	{
		private readonly FieldDescription field;

		private readonly bool uniqueFailure;
		private readonly bool requiredFailure;
		private readonly bool typeFailure;
		private readonly bool fieldIsShorter;
		private readonly bool fieldIsLonger;

		private readonly bool missingField;
		private readonly bool unmatchedField;

		private readonly string name = "";

		/// <summary>
		/// Initializes a new instance of the <see cref="FieldIntegrity"/> class.
		/// </summary>
		/// <param name="_field">The _field.</param>
		/// <param name="_uniqueFailure">if set to <c>true</c> [_unique failure].</param>
		/// <param name="_requiredFailure">if set to <c>true</c> [_required failure].</param>
		/// <param name="_typeFailure">if set to <c>true</c> [_type failure].</param>
		/// <param name="_fieldIsShorter">if set to <c>true</c> [_length failure].</param>
		/// <param name="_fieldIsLonger">if set to <c>true</c> [_field is longer].</param>
		public FieldIntegrity(FieldDescription _field, bool _uniqueFailure, bool _requiredFailure, bool _typeFailure, bool _fieldIsShorter, bool _fieldIsLonger)
		{
			field = _field;
			uniqueFailure = _uniqueFailure;
			typeFailure = _typeFailure;
			fieldIsShorter = _fieldIsShorter;
			fieldIsLonger = _fieldIsLonger;
			requiredFailure = _requiredFailure;
			name = field.Name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FieldIntegrity"/> class.
		/// This constructor is used to set a missing Field
		/// </summary>
		/// <param name="_field">The _field.</param>
		public FieldIntegrity(FieldDescription _field)
		{
			field = _field;
			missingField = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FieldIntegrity"/> class.
		/// This constructor is used to set a unmatched field
		/// </summary>
		/// <param name="_column">The _column.</param>
		public FieldIntegrity(string _column)
		{
			unmatchedField = true;
			name = _column;
		}

		/// <summary>
		/// Gets the field.
		/// </summary>
		/// <value>The field.</value>
		public FieldDescription Field
		{
			get { return field; }
		}

		/// <summary>
		/// Gets a value indicating whether [unique failure].
		/// </summary>
		/// <value><c>true</c> if [unique failure]; otherwise, <c>false</c>.</value>
		public bool UniqueFailure
		{
			get { return uniqueFailure; }
		}

		/// <summary>
		/// Gets a value indicating whether [required failure].
		/// </summary>
		/// <value><c>true</c> if [required failure]; otherwise, <c>false</c>.</value>
		public bool RequiredFailure
		{
			get { return requiredFailure; }
		}

		/// <summary>
		/// Gets a value indicating whether [type failure].
		/// </summary>
		/// <value><c>true</c> if [type failure]; otherwise, <c>false</c>.</value>
		public bool TypeFailure
		{
			get { return typeFailure; }
		}

		/// <summary>
		/// Gets a value indicating whether [length failure].
		/// </summary>
		/// <value><c>true</c> if [length failure]; otherwise, <c>false</c>.</value>
		public bool FieldIsShorter
		{
			get { return fieldIsShorter; }
		}

		/// <summary>
		/// Gets a value indicating whether [missing field].
		/// </summary>
		/// <value><c>true</c> if [missing field]; otherwise, <c>false</c>.</value>
		public bool MissingField
		{
			get { return missingField; }
		}

		/// <summary>
		/// Gets a value indicating whether [unmatched field].
		/// </summary>
		/// <value><c>true</c> if [unmatched field]; otherwise, <c>false</c>.</value>
		public bool UnmatchedField
		{
			get { return unmatchedField; }
		}

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return name; }
		}

		/// <summary>
		/// Gets a value indicating whether [field is longer].
		/// </summary>
		/// <value><c>true</c> if [field is longer]; otherwise, <c>false</c>.</value>
		public bool FieldIsLonger
		{
			get { return fieldIsLonger; }
		}

	}
}