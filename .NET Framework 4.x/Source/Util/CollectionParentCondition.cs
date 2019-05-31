using System;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;

namespace AdFactum.Data.Util
{
	/// <summary>
	/// The collection condition checks if a collection contains the given child ID
	/// </summary>
	public class CollectionParentCondition : Condition
	{
		private string parentTable;
		private string childTable;

		/// <summary>
		/// Gets the parent table.
		/// </summary>
		/// <value>The parent table.</value>
		public string ParentTable
		{
			get { return parentTable; }
		}

		/// <summary>
		/// Gets the child table.
		/// </summary>
		/// <value>The child table.</value>
		public string ChildTable
		{
			get { return childTable; }
		}

		/// <summary>
		/// The collection condition checks if a collection contains the given child ID
		/// </summary>
		/// <param name="conditionOperator">Defines the condition (AND or OR)</param>
		/// <param name="pTypeOfQueryObject">Type of the object that has to be queried</param>
		/// <param name="collectionPropertyName">Collection property name</param>
		/// <param name="typeOfCollectionElement">Type of the collection element</param>
		/// <param name="elementId">Unique identifier of the element within the collection</param>
		public CollectionParentCondition(ConditionOperator conditionOperator, Type pTypeOfQueryObject, string collectionPropertyName, Type typeOfCollectionElement, Guid elementId)
			: base(
				Table.GetTableInstance(pTypeOfQueryObject).DefaultName + "_" + ReflectionHelper.GetStaticFieldTemplate(pTypeOfQueryObject, collectionPropertyName).Name,
				conditionOperator,
				QueryOperator.Equals,
				new Field(ListLink.GetParentObjectDescription(elementId.GetType()), elementId))
		{
		    Initialize(collectionPropertyName, pTypeOfQueryObject, typeOfCollectionElement);
		}

	    /// <summary>
		/// The collection condition checks if a collection contains the given child ID
		/// </summary>
		/// <param name="pTypeOfQueryObject">Type of the object that has to be queried</param>
		/// <param name="collectionPropertyName">Collection property name</param>
		/// <param name="typeOfCollectionElement">Type of the collection element</param>
		/// <param name="elementId">Unique identifier of the element within the collection</param>
		public CollectionParentCondition(Type pTypeOfQueryObject, string collectionPropertyName, Type typeOfCollectionElement, object elementId)
			: base(
                Table.GetTableInstance(pTypeOfQueryObject).DefaultName + "_" + ReflectionHelper.GetStaticFieldTemplate(pTypeOfQueryObject, collectionPropertyName).Name,
				ConditionOperator.AND,
				QueryOperator.Equals,
				new Field(ListLink.GetParentObjectDescription(elementId.GetType()), elementId))
		{
            Initialize(collectionPropertyName, pTypeOfQueryObject, typeOfCollectionElement);
		}

        /// <summary>
        /// Initializes the specified collection property name.
        /// </summary>
        /// <param name="collectionPropertyName">Name of the collection property.</param>
        /// <param name="pTypeOfQueryObject">The p type of query object.</param>
        /// <param name="typeOfCollectionElement">The type of collection element.</param>
        private void Initialize(string collectionPropertyName, Type pTypeOfQueryObject, Type typeOfCollectionElement)
        {
            parentTable = Table.GetTableInstance(pTypeOfQueryObject).DefaultName;
            childTable = Table.GetTableInstance(typeOfCollectionElement).DefaultName;

            var collectionProjection = ReflectionHelper.GetProjection(typeOfCollectionElement, null);

            Add(new Join(
                     parentTable + "_" + ReflectionHelper.GetStaticFieldTemplate(pTypeOfQueryObject, collectionPropertyName).Name,
                     DBConst.PropertyField,
                     childTable,
                     collectionProjection.GetPrimaryKeyDescription().Name));
        }

    }
}