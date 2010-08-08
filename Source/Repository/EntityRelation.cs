using System;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Repository
{
	/// <summary>
	/// Summary description for EntityRelation.
	/// </summary>
	[Table("OMRE_ORM_RELATIONS")]
	public class EntityRelation : MarkedValueObject
	{
        private const string ExclusiveReferenced = "ASSOCIATIVE";
        private const string ExclusiveReferencedCascade = "EXCLUSIVE_AGGREGATE_CASCADE";
	    private const string Linked = "LINKED";
	    private const string AggregatedCascade = "AGGREGATED_CASCADE";
	    private const string LinkedCascade = "LINKED_CASCADE";

	    #region Private Fields

		private string parentTable;
		private string parentColumn = "ID";

		private string childTable;
		private string childColumn;

		private string linkTable;
		private string linkColumn;
		
		private VersionInfo versionInfo;

	    public EntityRelation()
	    {
	        Created = DateTime.Now;
	    }

	    #endregion

		#region Private Enumerationen

		/// <summary>
		/// Defines the orm relation tyep
		/// </summary>
		public enum OrmType
		{
			/// <summary>
			/// O/R Relation without an action
			/// </summary>
			Association,

			/// <summary>
			/// O/R Relation without an action
			/// </summary>
			ExclusiveReferencedCascade,

			/// <summary>
			/// Linked 
			/// </summary>
			Linked,

			/// <summary>
			/// Aggregate Cascade
			/// </summary>
			AggregatedCascade,

			/// <summary>
			/// Linked Cascade
			/// </summary>
			LinkedCascade
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRelation"/> class.
		/// </summary>
		/// <param name="versionInfo">The version info.</param>
		/// <param name="mapper">The mapper.</param>
		/// <param name="field">The field.</param>
		public void Initialize(
			VersionInfo versionInfo,
			ObjectMapper mapper,
			FieldDescription field
			)
		{
			VersionInfo = versionInfo;

            if (field.FieldType.Equals(typeof(OneToManyLink)))
                SetOneToManyLink(mapper, field);
            else
			if (field.FieldType.Equals(typeof (ListLink)))
				SetListLink(mapper, field);
			else
				SetLink(mapper, field);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRelation"/> class.
		/// </summary>
		/// <param name="versionInfo">The version info.</param>
		/// <param name="mapper">The mapper.</param>
		/// <param name="parentType">Type of the parent.</param>
		/// <param name="childColumn">The child column.</param>
		/// <param name="childType">Type of the child.</param>
		/// <param name="ormType">Type of the orm.</param>
        public void Initialize(
			VersionInfo versionInfo,
			ObjectMapper mapper,
			Type parentType,
			string childColumn,
			Type childType,
			OrmType ormType
			)
		{
			VersionInfo = versionInfo;
			SetLink(mapper, parentType, childColumn, childType, ormType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRelation"/> class.
		/// </summary>
		/// <param name="versionInfo">The version info.</param>
		/// <param name="mapper">The mapper.</param>
		/// <param name="field">The field.</param>
		/// <param name="ormType">Type of the orm.</param>
        public void Initialize(
			VersionInfo versionInfo,
			ObjectMapper mapper,
			VirtualFieldDescription field,
			OrmType ormType
			)
		{
			VersionInfo = versionInfo;
			SetLink(mapper, field, ormType);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EntityRelation"/> class.
		/// </summary>
		/// <param name="versionInfo">The version info.</param>
		/// <param name="mapper">The mapper.</param>
		/// <param name="parentType">Type of the parent.</param>
		/// <param name="parentColumn">The parent column.</param>
		/// <param name="childType">Type of the child.</param>
		/// <param name="childColumn">The child column.</param>
		/// <param name="ormType">Type of the orm.</param>
		public void Initialize(
			VersionInfo versionInfo,
			ObjectMapper mapper,
			Type parentType,
			string parentColumn,
			Type childType,
			string childColumn,
			OrmType ormType
			)
		{
			VersionInfo = versionInfo;
			SetLink(mapper, parentType, parentColumn, childType, childColumn, ormType);
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRelation"/> class.
        /// </summary>
        /// <param name="versionInfo">The version info.</param>
        /// <param name="parentTable">The parent table.</param>
        /// <param name="parentColumn">The parent column.</param>
        /// <param name="childTable">The child table.</param>
        /// <param name="childColumn">The child column.</param>
        /// <param name="ormType">Type of the orm.</param>
        public void Initialize(
            VersionInfo versionInfo,
            string parentTable,
            string parentColumn,
            string childTable,
            string childColumn,
            OrmType ormType
            )
        {
            VersionInfo = versionInfo;
            SetLink(parentTable, parentColumn, childTable, childColumn, ormType);
        }
        
#endregion

		#region Public Methods

		/// <summary>
		/// This method is used for setting meta information
		/// for an object that has an aggregation to an other object.
		/// In opposite to the other SetLink Method,
		/// with this method the OR Mapper Relation Type can be specified directly.
		/// </summary>
		/// <param name="mapper">Database Mapper object</param>
		/// <param name="parentType">Type of the parent.</param>
		/// <param name="childColumn">The child column.</param>
		/// <param name="childType">Type of the child.</param>
		/// <param name="ormType">OR Mapper Relation Type</param>
		protected virtual void SetLink(
			ObjectMapper mapper,
			Type parentType,
			string childColumn,
			Type childType,
			OrmType ormType
			)
		{
            ParentTable = Table.GetTableInstance(parentType).Name;
			ChildColumn = childColumn;

            ChildTable = Table.GetTableInstance(childType).Name;
			OrmRelation = ormType;

		    var parentProjection = ReflectionHelper.GetProjection(parentType, null);
            ParentColumn = parentProjection.GetPrimaryKeyDescription().Name;
		}

        /// <summary>
        /// This method is used for setting meta information
        /// for an object that has an aggregation to an other object.
        /// In opposite to the other SetLink Method,
        /// with this method the OR Mapper Relation Type can be specified directly.
        /// </summary>
        /// <param name="parentTable">The parent table.</param>
        /// <param name="parentColumn">The parent column.</param>
        /// <param name="childTable">The child table.</param>
        /// <param name="childColumn">The child column.</param>
        /// <param name="ormType">OR Mapper Relation Type</param>
        protected virtual void SetLink(
            string parentTable,
            string parentColumn,
            string childTable,
            string childColumn,
            OrmType ormType
            )
        {
            ParentTable = parentTable;
            ChildColumn = childColumn;

            ChildTable = childTable;
            OrmRelation = ormType;

            ParentColumn = parentColumn;
        }

        
        /// <summary>
		/// This method is used for setting meta information
		/// for an object that has an aggregation to an other object.
		/// In opposite to the other SetLink Method, 
		/// with this method the OR Mapper Relation Type can be specified directly.
		/// </summary>
		/// <param name="mapper">Database Mapper object</param>
		/// <param name="parentType">Type of the parent object that holds the child aggregation</param>
		/// <param name="childColumn">Name of the property in order to access the child object</param>
		/// <param name="childType">Type of the child object which can be accessed through the property -ChildColumn- </param>
		/// <param name="parentColumn">Defines the linked column Name</param>
		/// <param name="ormType">OR Mapper Relation Type</param>
		public void SetLink(
			ObjectMapper mapper,
			Type parentType,
			string parentColumn,
			Type childType,
			string childColumn,
			OrmType ormType
			)
		{
            ParentTable = Table.GetTableInstance(parentType).Name;
			ChildColumn = childColumn;

            ChildTable = Table.GetTableInstance(childType).Name;
			OrmRelation = ormType;

			ParentColumn = parentColumn;
		}

		/// <summary>
		/// This method is used for setting meta information
		/// for an object that has an aggregation to an other object.
		/// In opposite to the other SetLink Method,
		/// with this method the OR Mapper Relation Type can be specified directly.
		/// </summary>
		/// <param name="mapper">Database Mapper object</param>
		/// <param name="field">The field.</param>
		/// <param name="ormType">OR Mapper Relation Type</param>
		protected virtual void SetLink(
			ObjectMapper mapper,
			VirtualFieldDescription field,
			OrmType ormType
			)
		{
            ParentTable = field.JoinTable.Name;
			ParentColumn = field.TargetJoinField.Name;

            ChildTable = field.CurrentTable.Name;
			ChildColumn = field.CurrentJoinField.Name;

			OrmRelation = ormType;
		}

		/// <summary>
		/// This method is used for setting meta information
		/// for an object that has an aggregation to an other object.
		/// </summary>
		/// <param name="mapper">Database Mapper object</param>
		/// <param name="field">The field.</param>
		protected virtual void SetLink(
			ObjectMapper mapper,
			FieldDescription field)
		{
			if (Table.GetTableInstance(field.ContentType).IsStatic)
			{
                ChildTable = Table.GetTableInstance(field.ParentType).Name;
				ChildColumn = field.Name;

                ParentTable = Table.GetTableInstance(field.ContentType).Name;
                OrmRelation = field.CustomProperty.MetaInfo.RelationType ?? OrmType.Association;
			}
			else
			{
                ChildTable = Table.GetTableInstance(field.ParentType).Name;
				ChildColumn = field.Name;

                ParentTable = Table.GetTableInstance(field.ContentType).Name;
				OrmRelation = field.CustomProperty.MetaInfo.RelationType ?? OrmType.AggregatedCascade;
			}

            var parentProjection = ReflectionHelper.GetProjection(field.ContentType, null);
            ParentColumn = parentProjection.GetPrimaryKeyDescription().Name;
		}

        /// <summary>
        /// This method is used for setting meta information
        /// for an object that has an aggregation to an other object.
        /// </summary>
        /// <param name="mapper">Database Mapper object</param>
        /// <param name="field">The field.</param>
        protected virtual void SetOneToManyLink(
            ObjectMapper mapper,
            FieldDescription field)
        {
            var source = field;
            var contentType = field.ContentType;

            var targetProperty = field.CustomProperty.MetaInfo.LinkedTargetProperty;
            var targetType = field.CustomProperty.MetaInfo.LinkTarget;
            field = ReflectionHelper.GetStaticFieldTemplate(targetType, targetProperty);
            if (field.CustomProperty.MetaInfo.IsGeneralLinked)
                throw new NotSupportedException("The target [" + field.CustomProperty.PropertyInfo.ReflectedType.Name + "." + field.CustomProperty.PropertyInfo.Name + "] of the OneToMany Link ["
                                                               + source.CustomProperty.PropertyInfo.ReflectedType.Name + "." + source.CustomProperty.PropertyInfo.Name + "] is bound to an interface, which is not allowed using a OneToMany Link.");

            if (Table.GetTableInstance(field.ContentType).IsStatic)
            {
                ChildTable = Table.GetTableInstance(field.ParentType).Name;
                ChildColumn = field.Name;

                ParentTable = Table.GetTableInstance(field.ContentType).Name;
                OrmRelation = source.CustomProperty.MetaInfo.RelationType ?? OrmType.Association;
            }
            else
            {
                ChildTable = Table.GetTableInstance(field.ParentType).Name;
                ChildColumn = field.Name;

                ParentTable = Table.GetTableInstance(field.ContentType).Name;
                OrmRelation = source.CustomProperty.MetaInfo.RelationType ?? OrmType.ExclusiveReferencedCascade;
            }

            var parentProjection = ReflectionHelper.GetProjection(contentType, null);
            ParentColumn = parentProjection.GetPrimaryKeyDescription().Name;
        }

		/// <summary>
		/// This method is used for setting meta information
		/// for a object type that owns a collection of child objects.
		/// </summary>
		/// <param name="mapper">Database Mapper object</param>
		/// <param name="field">The description.</param>
		protected virtual void SetListLink(
			ObjectMapper mapper,
			FieldDescription field)
		{
            ChildTable = Table.GetTableInstance(field.ParentType).Name;
			LinkTable = ChildTable + "_" + field.Name;
			ChildColumn = "PARENTOBJECT";
			LinkColumn = "PROPERTY";
			ParentTable = LinkTable;

            Type propertyTarget = field.CustomProperty.MetaInfo.LinkTarget;
            if (field.CustomProperty.MetaInfo.LinkTarget == null)
				return;

		    Table tableInstance = Table.GetTableInstance(propertyTarget);
		    OrmRelation = tableInstance.IsStatic ? OrmType.Linked : OrmType.LinkedCascade;
            ParentTable = tableInstance.Name;
			ParentColumn = "ID";
		}

		#endregion

		#region Overwritte Methods

		/// <summary>
		/// Returns the textual representation of an OR mapper relation
		/// </summary>
		/// <returns></returns>
		public string  DebugInfo()
		{
			return string.Concat(Relation, ":" , ParentTable , "." , ParentColumn , " over " , LinkTable , "." , LinkColumn , " to " , ChildTable , "." , ChildColumn) ;
		}

		/// <summary>
		/// Gets the unique identifier.
		/// </summary>
		/// <value>The unique identifier.</value>
		[Ignore]
		public string UniqueIdentifierKey
		{
			get
			{
                return string.Concat(VersionInfo.MajorVersion.ToString(), ".", VersionInfo.MinorVersion.ToString(), "_",
                    ChildTable, ".", ChildColumn, ".", ParentTable, ".", ParentColumn, ".", LinkTable);
            }
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets or sets the parent table.
		/// </summary>
		/// <value>The parent table.</value>
		[PropertyName("PARENT_TABLE")]
		public string ParentTable
		{
			get { return parentTable; }
			set { parentTable = value; }
		}

		/// <summary>
		/// Gets or sets the child table.
		/// </summary>
		/// <value>The child table.</value>
		[PropertyName("CHILD_TABLE")]
		public string ChildTable
		{
			get { return childTable; }
			set { childTable = value; }
		}


		/// <summary>
		/// Gets or sets the child column.
		/// </summary>
		/// <value>The child column.</value>
		[PropertyName("CHILD_COLUMN")]
		public string ChildColumn
		{
			get { return childColumn; }
			set { childColumn = value; }
		}

		/// <summary>
		/// Gets or sets the parent column.
		/// </summary>
		/// <value>The parent column.</value>
		[PropertyName("PARENT_COLUMN")]
		public string ParentColumn
		{
			get { return parentColumn; }
			set { parentColumn = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		[PropertyName("ORM_RELATION")]
		public string Relation
		{
			get
			{
			    switch (OrmRelation)
			    {
			        case OrmType.Association:
                        return ExclusiveReferenced;

                    case OrmType.ExclusiveReferencedCascade:
			            return ExclusiveReferencedCascade;

                    case OrmType.Linked:
			            return Linked;

                    case OrmType.AggregatedCascade:
			            return AggregatedCascade;

			        case OrmType.LinkedCascade:
			            return LinkedCascade;

			        default:
                        throw new NotSupportedException(OrmRelation.ToString());
			    }
			}
			set
			{
                if (ExclusiveReferenced.Equals(value, StringComparison.InvariantCultureIgnoreCase))
					OrmRelation = OrmType.Association;

                if (ExclusiveReferencedCascade.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    OrmRelation = OrmType.ExclusiveReferencedCascade;

                if (Linked.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    OrmRelation = OrmType.Linked;

                if (AggregatedCascade.Equals(value, StringComparison.InvariantCultureIgnoreCase))
					OrmRelation = OrmType.AggregatedCascade;

                if (LinkedCascade.Equals(value, StringComparison.InvariantCultureIgnoreCase))
					OrmRelation = OrmType.LinkedCascade;
			}
		}

		/// <summary>
		/// Gets or sets the link table.
		/// </summary>
		/// <value>The link table.</value>
		[PropertyName("LINK_TABLE")]
		public string LinkTable
		{
			get { return linkTable; }
			set {
			    linkTable = value != null ? value : "";
			}
		}

	    /// <summary>
	    /// Stores the creation time of the object
	    /// </summary>
	    public DateTime Created { get; set; }

	    /// <summary>
		/// Gets or sets the version info.
		/// </summary>
		/// <value>The version info.</value>
		[PropertyName("VERSION_ID")]
		public VersionInfo VersionInfo
		{
			get { return versionInfo; }
			set
			{
			    versionInfo = value;
			    Application = versionInfo != null ? versionInfo.Application : string.Empty;
			}
		}

		/// <summary>
		/// Gets or sets the link column.
		/// </summary>
		/// <value>The link column.</value>
		[PropertyName("LINK_COLUMN")]
		public string LinkColumn
		{
			get { return linkColumn; }
			set {
			    linkColumn = value != null ? value : "";
			}
		}

	    /// <summary>
	    /// Gets or sets the orm relation.
	    /// </summary>
	    /// <value>The orm relation.</value>
	    [Ignore]
	    public OrmType OrmRelation { get; set; }

	    /// <summary>
	    /// Gets or sets the application.
	    /// </summary>
	    /// <value>The application.</value>
	    [PropertyLength(64)]
	    public string Application { get; set; }

	    #endregion
	}
}