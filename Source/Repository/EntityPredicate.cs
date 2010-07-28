using System;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Repository
{
	/// <summary>
	/// The entity predicates describes the relation between different entities joined by virtual links
	/// </summary>
	[Table("OMRE_ORM_PREDICATES")]
	public class EntityPredicate : MarkedValueObject
	{
		#region Private Attributes

		private VersionInfo _versionInfo;

		private string leftSideTableName;
		private string leftSideColumn;

		private string rightSideTableName;
		private string rightSideColumn;

		private string joinOperator;

		private bool weakJoin;

		private DateTime created;

		#endregion

		#region Public Members

		/// <summary>
		/// Gets or sets the name of the left side table.
		/// </summary>
		/// <value>The name of the left side table.</value>
		[PropertyName("L_TABLE_NAME")]
		public string LeftSideTableName
		{
			get { return leftSideTableName; }
			set { leftSideTableName = value; }
		}

		/// <summary>
		/// Gets or sets the left side column.
		/// </summary>
		/// <value>The left side column.</value>
		[PropertyName("L_COLUMN_NAME")]
		public string LeftSideColumn
		{
			get { return leftSideColumn; }
			set { leftSideColumn = value; }
		}

		/// <summary>
		/// Gets or sets the name of the right side table.
		/// </summary>
		/// <value>The name of the right side table.</value>
		[PropertyName("R_TABLE_NAME")]
		public string RightSideTableName
		{
			get { return rightSideTableName; }
			set { rightSideTableName = value; }
		}

		/// <summary>
		/// Gets or sets the join operator.
		/// </summary>
		/// <value>The join operator.</value>
		[PropertyName("OPERATOR")]
		public string JoinOperator
		{
			get { return joinOperator; }
			set { joinOperator = value; }
		}

		/// <summary>
		/// Gets or sets the right side column.
		/// </summary>
		/// <value>The right side column.</value>
		[PropertyName("R_COLUMN_NAME")]
		public string RightSideColumn
		{
			get { return rightSideColumn; }
			set { rightSideColumn = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [weak join].
		/// </summary>
		/// <value><c>true</c> if [weak join]; otherwise, <c>false</c>.</value>
		[PropertyName("WEAK_JOIN")]
		public bool WeakJoin
		{
			get { return weakJoin; }
			set { weakJoin = value; }
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
		/// Gets or sets the version info.
		/// </summary>
		/// <value>The version info.</value>
		[PropertyName("VERSION_ID")]
		public VersionInfo VersionInfo
		{
			get { return _versionInfo; }
			set { _versionInfo = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityPredicate"/> class.
		/// </summary>
		public EntityPredicate()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityPredicate"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		/// <param name="vfd">The virtual field</param>
		public EntityPredicate(VersionInfo version, VirtualFieldDescription vfd)
		{
			VersionInfo = version;

			LeftSideTableName = vfd.CurrentTable.Name;
			LeftSideColumn = vfd.CurrentJoinField.Name;

		    Table targetClassInstance = vfd.JoinTable;
		    RightSideTableName = targetClassInstance.Name;
			RightSideColumn = vfd.TargetJoinField.Name;

			WeakJoin = targetClassInstance.IsWeakReferenced;
			JoinOperator = "=";

			Created = DateTime.Now;
		}

		#endregion

		#region Overwritten Methods

		/// <summary>
		/// Two value Entity Predicates equals, if the business columns are the same
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			EntityPredicate ep = obj as EntityPredicate;
			if (ep != null)
			{
				return (LeftSideColumn == ep.LeftSideColumn)
					&& (LeftSideTableName == ep.LeftSideTableName)
					&& (RightSideColumn == ep.RightSideColumn)
					&& (RightSideTableName == ep.RightSideTableName);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a Hashcode, that helps identifiying the object
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}
}