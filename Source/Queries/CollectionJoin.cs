using System;
using System.Text;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// This condition defines a collection join condition that joins two tables which are connected by collection.
	/// </summary>
	public class CollectionJoin : ConditionList
	{
		/// <summary>
		/// Joins
		/// </summary>
		private readonly Join firstJoin;
		private readonly Join secondJoin;

		/// <summary>
		/// Constructor for a condition that joins two tables which are connected by a collection
		/// </summary>
		/// <param name="parentType">The parentType defines the type of the object who is the owner of the child relation. </param>
		/// <param name="parentField">The parentField defines the name of a property within the parent object type. Two establish a collection join the name of the collection property must be used as the parent field name.</param>
		/// <param name="childType">The childType defines the type of the joined object.</param>
		public CollectionJoin(Type parentType, string parentField, Type childType) 
		{
		    ConditionClause = ConditionClause.WhereClause;
		    string parentTable = Table.GetTableInstance(parentType).DefaultName;

		    var childProjection = ReflectionHelper.GetProjection(childType, null);

		    firstJoin = new Join(
                parentTable + "_" + ReflectionHelper.GetStaticFieldTemplate(parentType, parentField).Name,
				DBConst.PropertyField,
                Table.GetTableInstance(childType).DefaultName,
                childProjection.GetPrimaryKeyDescription().Name);

			secondJoin = new Join(
                parentTable,
                childProjection.GetPrimaryKeyDescription().Name,
                parentTable + "_" + ReflectionHelper.GetStaticFieldTemplate(parentType, parentField).Name,
				DBConst.ParentObjectField);
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Join"/> class.
        /// </summary>
        /// <param name="parentType">The type.</param>
        /// <param name="field">The field.</param>
        /// <param name="child">The child.</param>
        public CollectionJoin(Type parentType, string field, IValueObject child)
            : this(parentType, field, child.GetType())
        {
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
        public CollectionJoin(IValueObject parent, string field, Type childType)
            : this(parent.GetType(), field, childType)
        {
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
        public CollectionJoin(IValueObject parent, string field, IValueObject child)
            : this(parent.GetType(), field, child.GetType())
        {
            var parentProjection = ReflectionHelper.GetProjection(parent.GetType(), null);
            var childProjection = ReflectionHelper.GetProjection(child.GetType(), null);

            string childPrimaryKey = childProjection.GetPrimaryKeyDescription().PropertyName;
            string parentPrimaryKey = parentProjection.GetPrimaryKeyDescription().PropertyName;

            Add(new AndCondition(parent.GetType(), parentPrimaryKey, parent.Id));
            Add(new AndCondition(child.GetType(), childPrimaryKey, child.Id));
        }


		#region ICondition Members

		/// <summary>
		/// Returns the list of tables which are needed to fulfill the condition
		/// </summary>
		public override Set Tables
		{
			get
			{
				var result = new Set();
				result.Merge(firstJoin.Tables);
				result.Merge(secondJoin.Tables);
				result.Merge(base.Tables);

				return result;
			}
		}

		/// <summary>
		/// Returns the first inner join
		/// </summary>
		public Join FirstJoin
		{
			get { return firstJoin; }
		}

		/// <summary>
		/// Returns the second inner join
		/// </summary>
		public Join SecondJoin
		{
			get { return secondJoin; }
		}

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		public override string ConditionString
		{
			get
			{
				var builder = new StringBuilder();

				builder.Append(firstJoin.ConditionString);
				builder.Append(SecondJoin.Type.Operator() + secondJoin.ConditionString);

                string baseResult = base.ConditionString;

                if (baseResult.IsNotNullOrEmpty())
                {
                    builder.Append(" AND ");
                    builder.Append(baseResult);
                }

			    return builder.ToString();
			}
		}

		#endregion
	}
}