using System;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
    internal class HashCondition : CollectionParentCondition
    {
        /// <summary>
        /// The collection condition checks if a collection contains the given child ID
        /// </summary>
        /// <param name="pTypeOfQueryObject">Type of the object that has to be queried</param>
        /// <param name="collectionPropertyName">Collection property name</param>
        /// <param name="typeOfCollectionElement">Type of the collection element</param>
        /// <param name="elementId">Unique identifier of the element within the collection</param>
        public HashCondition(Type pTypeOfQueryObject, string collectionPropertyName, Type typeOfCollectionElement, object elementId)
            : base(pTypeOfQueryObject, collectionPropertyName, typeOfCollectionElement, elementId)
        {
            
        }
        
        /// <summary>
        /// Returns the join field used for filling the hashtable
        /// </summary>
        public string JoinFieldForHashtable
        {
            get
            {
                return string.Concat(
                    ", ",
                    TableName,
                    ".",
                    DBConst.LinkIdField,
                    " AS ",
                    ReflectionHelper.GetJoinField(ParentTable, ChildTable)
                    );
            }
        }
    }
}
