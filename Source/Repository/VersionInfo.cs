using System;

namespace AdFactum.Data.Repository
{
	/// <summary>
	/// Summary description for VersionInfo.
	/// </summary>
	[Table("OMRE_ORM_VERSIONS")]
	public class VersionInfo : MarkedValueObject
	{
		private int majorVersion;
		private int minorVersion;
		private string application;
		private DateTime created = DateTime.Now;
		private string comment;
		private bool isActive = true;
		private bool isReleased;
		private bool isCurrent;

		/// <summary>
		/// Gets or sets the major version.
		/// </summary>
		/// <value>The major version.</value>
		public int MajorVersion
		{
			get { return majorVersion; }
			set { majorVersion = value; }
		}

		/// <summary>
		/// Gets or sets the minor version.
		/// </summary>
		/// <value>The minor version.</value>
		public int MinorVersion
		{
			get { return minorVersion; }
			set { minorVersion = value; }
		}

		/// <summary>
		/// Gets or sets the comment.
		/// </summary>
		/// <value>The comment.</value>
		[PropertyName("COMMENTS")]
		public string Comment
		{
			get { return comment; }
			set { comment = value; }
		}

		/// <summary>
		/// Gets or sets the created.
		/// </summary>
		/// <value>The created.</value>
		public DateTime Created
		{
			get { return created; }
			set { created = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="VersionInfo"/> is active.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		[PropertyName("IS_ACTIVE")]
		public bool IsActive
		{
			get { return isActive; }
			set { isActive = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="VersionInfo"/> is released.
		/// </summary>
		/// <value><c>true</c> if released; otherwise, <c>false</c>.</value>
		[PropertyName("IS_RELEASED")]
		public bool HasReleased
		{
			get { return isReleased; }
			set { isReleased = value; }
		}

		/// <summary>
		/// Gets or sets the application.
		/// </summary>
		/// <value>The application.</value>
		[PropertyLength(64)]
		[PropertyName("APPLICATION")]
		public string Application
		{
			get { return application; }
			set { application = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is current.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is current; otherwise, <c>false</c>.
		/// </value>
		[PropertyName ("IS_CURRENT")]
		public bool IsCurrent
		{
			get { return isCurrent; }
			set { isCurrent = value; }
		}
	}
}
