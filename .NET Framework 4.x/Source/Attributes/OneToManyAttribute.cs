using System;
using AdFactum.Data.Repository;

namespace AdFactum.Data
{
    /// <summary>
    /// The One To Many Attribute is used to bind collections
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    [Serializable]
    public sealed class OneToManyAttribute : Attribute
    {
        /// <summary>
        /// Type of the defined relation
        /// </summary>
        private readonly EntityRelation.OrmType? relationType;

        /// <summary>
        /// Defines the Joined Type
        /// </summary>
        private readonly Type joinedType;

        /// <summary>
        /// Defines the Joined Property
        /// </summary>
        private readonly string joinedProperty;

        /// <summary>
        /// Internal Constructor
        /// </summary>
        public OneToManyAttribute()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OneToManyAttribute(Type joinedType, string joinedProperty)
        {
            this.joinedType = joinedType;
            this.joinedProperty = joinedProperty;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OneToManyAttribute(Type joinedType, string joinedProperty, EntityRelation.OrmType relationType)
            :this(joinedType, joinedProperty)
        {
            this.relationType = relationType;
        }

        /// <summary> Returns the Joined Type </summary>
        public Type JoinedType
        {
            get { return joinedType; }
        }

        /// <summary> Returns the Joined Property  </summary>
        public string JoinedProperty
        {
            get { return joinedProperty; }
        }

        /// <summary> Gets the type of the relation. </summary>
        public EntityRelation.OrmType? RelationType
        {
            get { return relationType; }
        }
    }
}
