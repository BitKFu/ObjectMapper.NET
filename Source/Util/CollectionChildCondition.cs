using System;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;

namespace AdFactum.Data.Util
{
	/// <summary>
	/// The collection condition checks if a collection contains the given child ID
	/// </summary>
	public class CollectionChildCondition : Condition
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
		public CollectionChildCondition(ConditionOperator conditionOperator, Type pTypeOfQueryObject, string collectionPropertyName, Type typeOfCollectionElement, Guid elementId)
			: base(
                Table.GetTableInstance(pTypeOfQueryObject).Name + "_" + ReflectionHelper.GetStaticFieldTemplate(pTypeOfQueryObject, collectionPropertyName).Name,
				conditionOperator,
				QueryOperator.Equals,
				new Field(ListLink.GetListPropertyDescription(elementId.GetType()), elementId))
		{
            parentTable = Table.GetTableInstance(pTypeOfQueryObject).Name;
            childTable = Table.GetTableInstance(typeOfCollectionElement).Name;

			Add(new CollectionJoin(pTypeOfQueryObject, collectionPropertyName, typeOfCollectionElement));
		}

		/// <summary>
		/// The collection condition checks if a collection contains the given child ID
		/// </summary>
		/// <param name="pTypeOfQueryObject">Type of the object that has to be queried</param>
		/// <param name="collectionPropertyName">Collection property name</param>
		/// <param name="typeOfCollectionElement">Type of the collection element</param>
		/// <param name="elementId">Unique identifier of the element within the collection</param>
		public CollectionChildCondition(Type pTypeOfQueryObject, string collectionPropertyName, Type typeOfCollectionElement, Guid elementId)
			: base(
                Table.GetTableInstance(pTypeOfQueryObject).Name + "_" + ReflectionHelper.GetStaticFieldTemplate(pTypeOfQueryObject, collectionPropertyName).Name,
				ConditionOperator.AND,
				QueryOperator.Equals,
				new Field(ListLink.GetListPropertyDescription(elementId.GetType()), elementId))
		{
            parentTable = Table.GetTableInstance(pTypeOfQueryObject).Name;
            childTable = Table.GetTableInstance(typeOfCollectionElement).Name;

			Add(new CollectionJoin(pTypeOfQueryObject, collectionPropertyName, typeOfCollectionElement));
		}

	}
}