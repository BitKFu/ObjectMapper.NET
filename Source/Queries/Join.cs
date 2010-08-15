using System;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// This condition defines a regular join. 
	/// </summary>
	public class Join : ConditionList
	{
		private readonly string parentTable;
		private readonly string parentField;

		private readonly string childTable;
		private readonly string childField;

		/// <summary>
		/// Constructor for a condition that joins two tables 
		/// </summary>
		internal Join(string parentTable, string parentField, string childTable, string childField) 
		{
            ConditionClause = ConditionClause.WhereClause;

			this.childTable = childTable;
			this.childField = childField;

			this.parentTable = parentTable;
			this.parentField = parentField;
		}

		/// <summary>
		/// Constructor for a regular join condition that joins two tables which are connected by a key / value pair.
		/// </summary>
		/// <param name="parentType">The parentType defines the type of the object who is the owner of the child relation. </param>
		/// <param name="parentField">The parentField defines a name of a property within the parent object type. The parent field is a key value that refers to the child.</param>
		/// <param name="childType">The childType defines the type of the joined object.</param>
		/// <param name="childField">The childField defines a name of a property within the child object type. This field is the direct pendant to the parentField which holds the key value. The content of the parentField and the content of the childField must map in order to establish the join.</param>
		public Join(Type parentType, string parentField, Type childType, string childField) 
		{
            ConditionClause = ConditionClause.WhereClause;
            
            childTable = Table.GetTableInstance(childType).DefaultName;
			this.childField = ReflectionHelper.GetStaticFieldTemplate(childType, childField).Name;

            parentTable = Table.GetTableInstance(parentType).DefaultName;
			this.parentField = ReflectionHelper.GetStaticFieldTemplate(parentType, parentField).Name;
		}

		/// <summary>
		/// Constructor for a regular join condition that joins two tables which are connected by a key / value pair. The two classes
		/// are directly joined by using the primary key of the child type.
		/// </summary>
		/// <param name="parentType">The parentType defines the type of the object who is the owner of the child relation. </param>
		/// <param name="parentField">The parentField defines a name of a property within the parent object type. The parent field is a key value that refers to the child.</param>
		/// <param name="childType">The childType defines the type of the joined object.</param>
		public Join(Type parentType, string parentField, Type childType) 
		{
            ConditionClause = ConditionClause.WhereClause;

		    var childProjection = ReflectionHelper.GetProjection(childType, null);
            childTable = Table.GetTableInstance(childType).DefaultName;
            childField = childProjection.GetPrimaryKeyDescription().Name;

            parentTable = Table.GetTableInstance(parentType).DefaultName;
			this.parentField = ReflectionHelper.GetStaticFieldTemplate(parentType, parentField).Name;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Join"/> class.
        /// </summary>
        /// <param name="parentType">The type.</param>
        /// <param name="field">The field.</param>
        /// <param name="child">The child.</param>
        public Join(Type parentType, string field, IValueObject child)
            : this(parentType, field, child.GetType())
        {
            ConditionClause = ConditionClause.WhereClause;

            var childProjection = ReflectionHelper.GetProjection(child.GetType(), null);
            string primaryKey = childProjection.GetPrimaryKeyDescription().PropertyName;
            Add(new AndCondition(child.GetType(), primaryKey, child.Id));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Join"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="field">The field.</param>
        /// <param name="childType">Type of the child.</param>
        public Join(IValueObject parent, string field, Type childType)
            : this(parent.GetType(), field, childType)
        {
            ConditionClause = ConditionClause.WhereClause;

            var parentProjection = ReflectionHelper.GetProjection(parent.GetType(), null);
            string primaryKey = parentProjection.GetPrimaryKeyDescription().PropertyName;
            Add(new AndCondition(parent.GetType(), primaryKey, parent.Id));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Join"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="field">The field.</param>
        /// <param name="child">The child.</param>
        public Join(IValueObject parent, string field, IValueObject child)
            : this(parent.GetType(), field, child.GetType())
        {
            ConditionClause = ConditionClause.WhereClause;

            var childProjection = ReflectionHelper.GetProjection(child.GetType(), null);
            var parentProjection = ReflectionHelper.GetProjection(parent.GetType(), null);

            string childPrimaryKey = childProjection.GetPrimaryKeyDescription().PropertyName;
            string parentPrimaryKey = parentProjection.GetPrimaryKeyDescription().PropertyName;

            Add(new AndCondition(parent.GetType(), parentPrimaryKey, parent.Id));
            Add(new AndCondition(child.GetType(), childPrimaryKey, child.Id));
        }

		/// <summary>
		/// Returns the left side
		/// </summary>
		public string LeftSide
		{
			get { return string.Concat(Condition.QUOTE_OPEN, ParentTable, Condition.QUOTE_CLOSE , "." ,
									   Condition.QUOTE_OPEN, parentField, Condition.QUOTE_CLOSE); }
		}

		/// <summary>
		/// Returns the Right side
		/// </summary>
		public string RightSide
		{
			get 
		{
			return string.Concat(Condition.QUOTE_OPEN, ChildTable, Condition.QUOTE_CLOSE , "." ,
								 Condition.QUOTE_OPEN, childField, Condition.QUOTE_CLOSE); }
		}

		#region ICondition Members

		/// <summary>
		/// Returns the list of tables which are needed to fulfill the condition
		/// </summary>
		public override Set Tables
		{
			get
			{
				var tables = new Set {ParentTable, ChildTable};
			    tables.Merge(base.Tables);

				return tables;
			}
		}

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		public override string ConditionString
		{
			get
			{
			    var result = string.Concat(LeftSide , "=" , RightSide);
			    var baseResult = base.ConditionString;

                if (baseResult.IsNotNullOrEmpty() )
                    result = string.Concat(result, " AND ", baseResult);

			    return result;
			}
		}

		/// <summary>
		/// Returns the parent table
		/// </summary>
		public string ParentTable
		{
			get { return parentTable; }
		}

		/// <summary>
		/// Returns the child table
		/// </summary>
		public string ChildTable
		{
			get { return childTable; }
		}

		#endregion
	}
}