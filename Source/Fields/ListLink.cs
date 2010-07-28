using System;
using System.Collections.Generic;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Fields
{
	///<summary>
    /// Class that defines a link in a list (e.x. a link in a ArrayList or Hashtable)
	///</summary>
	[Serializable]
    public class ListLink : Link
	{
        /// <summary> Field object that descripes the key column</summary>
		public Field Key { get; private set;}
        
        /// <summary>Field object that descripes the parent property </summary>
        public Field ParentObject { get; private set; }

        /// <summary> True, if it's not a general link, but bound to a special type </summary>
        public bool IsBoundToType { get; private set; }

        /// <summary> Parent Type</summary>
        public Type ParentType { get; private set; }

        /// <summary> Bound Type</summary>
        public Type BoundToType { get; private set; }
        
        /// <summary> Primary key type of the bound type </summary>
        public Type LinkedPrimaryKeyType { get; private set; }

		/// <summary>
		/// Field description for a linked to field
		/// </summary>
		public static readonly FieldDescription LinkedToDescription = new FieldDescription(DBConst.LinkedToField, null, typeof (Field), typeof(string), null, false);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="boundType">Type of the p binded.</param>
        /// <param name="key">New Link Id</param>
        /// <param name="parentTypeParameter">The parent type parameter.</param>
        /// <param name="parenObjectId">Id for the object that contains the link</param>
        /// <param name="boundObjectId">Id for the linked object</param>
        /// <param name="linkedToTypeName">Type of the linked object</param>
        public ListLink(Type boundType, object key, Type parentTypeParameter, object parenObjectId, object boundObjectId, string linkedToTypeName)
		{
			IsBoundToType = boundType != null;
            BoundToType = boundType;
            ParentType = parentTypeParameter;

            if (boundType != null)
            {
                var boundProjection = ReflectionHelper.GetProjection(boundType, null);
                LinkedPrimaryKeyType = boundProjection.GetPrimaryKeyDescription().ContentType;
            }
            else if (boundObjectId != null) LinkedPrimaryKeyType = boundObjectId.GetType();

			IsDeleted = false;
			IsNew = false;

			Key = new Field(new FieldDescription(DBConst.LinkIdField, null, key.GetType(), true), key);
            ParentObject = new Field(GetParentObjectDescription(parenObjectId.GetType()), parenObjectId);
            Property = new Field(GetHashPropertyDescription(LinkedPrimaryKeyType), boundObjectId);
			LinkedTo = new Field(LinkedToDescription, linkedToTypeName);
		}

        /// <summary>
        /// Gets the parent object description.
        /// </summary>
        /// <param name="typeOfPrimaryKey">The type of primary key.</param>
        /// <returns></returns>
        public static FieldDescription GetParentObjectDescription (Type typeOfPrimaryKey)
        {
            var fd =  new FieldDescription(DBConst.ParentObjectField, null, typeof(Field), typeOfPrimaryKey, null, true) {IsAutoIncrement = false};
            return fd;
        }

        /// <summary>
        /// Gets the hash property description.
        /// </summary>
        /// <param name="typeOfPrimaryKey">The type of primary key.</param>
        /// <returns></returns>
        public static FieldDescription GetHashPropertyDescription(Type typeOfPrimaryKey)
        {
            var fd = new FieldDescription(DBConst.PropertyField, null, typeof(Field), typeOfPrimaryKey, null, false) {IsAutoIncrement = false};
            return fd;
        }

        /// <summary>
        /// Gets the hash property description.
        /// </summary>
        /// <param name="typeOfPrimaryKey">The type of primary key.</param>
        /// <returns></returns>
        public static FieldDescription GetListPropertyDescription(Type typeOfPrimaryKey)
        {
            var fd = new FieldDescription(DBConst.PropertyField, null, typeof(Field), typeOfPrimaryKey, null, true) {IsAutoIncrement = false};
            return fd;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="boundType">Type of the binded.</param>
        /// <param name="boundTypeName">Name of the binded type.</param>
        /// <param name="parentTypeParameter">The parent type parameter.</param>
        /// <param name="parentObjectId">The parent object id.</param>
        /// <param name="boundObjectId">The binded object id.</param>
        public ListLink(Type boundType, String boundTypeName, Type parentTypeParameter, object parentObjectId, object boundObjectId)
        {
            IsBoundToType = boundType != null;
            BoundToType = boundType;
            ParentType = parentTypeParameter;

            if (boundType != null)
            {
                var boundProjection = ReflectionHelper.GetProjection(boundType, null);
                LinkedPrimaryKeyType = boundProjection.GetPrimaryKeyDescription().ContentType;
            }
            else if (boundObjectId != null) LinkedPrimaryKeyType = boundObjectId.GetType();

            var parentProjection = ReflectionHelper.GetProjection(parentTypeParameter, null);
            var parentPrimaryKeyType = parentProjection.GetPrimaryKeyDescription().ContentType;

            IsDeleted = false;
            IsNew = false;

            Key = null;
            ParentObject = new Field(GetParentObjectDescription(parentPrimaryKeyType), parentObjectId);
            Property = new Field(GetListPropertyDescription(LinkedPrimaryKeyType), boundObjectId);
            LinkedTo = new Field(LinkedToDescription, boundTypeName);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="boundType">Type of the bound.</param>
        /// <param name="parentObjectPO">Persistent Object that contains the link</param>
        /// <param name="boundObjectPO">Persistent Object that is linked by the parent object</param>
        /// <param name="key">Id for the persistent Object that is linked by the parent object</param>
		public ListLink(Type boundType, PersistentObject parentObjectPO, PersistentObject boundObjectPO, object key)
		{
            IsBoundToType = boundType != null;
            BoundToType = boundType;
	        ParentType = parentObjectPO.ObjectType;

            if (boundType != null)
            {
                var boundProjection = ReflectionHelper.GetProjection(boundType, null);
                LinkedPrimaryKeyType = boundProjection.GetPrimaryKeyDescription().ContentType;
            }
            else 
                if (boundObjectPO != null)
                {
                    var boundPOProjection = ReflectionHelper.GetProjection(boundObjectPO.ObjectType, null);
                    LinkedPrimaryKeyType = boundPOProjection.GetPrimaryKeyDescription().ContentType;
                }

            var parentProjection = ReflectionHelper.GetProjection(parentObjectPO.ObjectType, null);
            var parentPrimaryKeyType = parentProjection.GetPrimaryKeyDescription().ContentType;

            ParentObject = new Field(GetParentObjectDescription(parentPrimaryKeyType), parentObjectPO.Id);
            Key = new Field(new FieldDescription(DBConst.LinkIdField, parentObjectPO.ObjectType, LinkedPrimaryKeyType, true), key);
			IsNew = true;

			if (boundObjectPO != null)
			{
				LinkedTo = new Field(LinkedToDescription, boundObjectPO.ObjectType.FullName);
                Property = new Field(GetHashPropertyDescription(LinkedPrimaryKeyType), boundObjectPO.Id);
				IsDeleted = false;
			}
			else
			{
				LinkedTo = new Field(LinkedToDescription, null);
                Property = new Field(GetHashPropertyDescription(typeof(Guid)), null);
				IsDeleted = false;
			}
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="boundToType">if set to <c>true</c> [bound to type].</param>
        /// <param name="parentObjectPO">Persistent Object that contains the link</param>
        /// <param name="boundObjectPO">Persistent Object that is linked by the parent object</param>
        public ListLink(bool boundToType, PersistentObject parentObjectPO, PersistentObject boundObjectPO)
        {
            IsBoundToType = boundToType;
            BoundToType = boundObjectPO.ObjectType;
            ParentType = parentObjectPO.ObjectType;

            var boundProjection = ReflectionHelper.GetProjection(boundObjectPO.ObjectType, null);
            var parentProjection = ReflectionHelper.GetProjection(parentObjectPO.ObjectType, null);

            LinkedPrimaryKeyType = boundProjection.GetPrimaryKeyDescription().ContentType;
            var parentPrimaryKeyType = parentProjection.GetPrimaryKeyDescription().ContentType;

            ParentObject = new Field(GetParentObjectDescription(parentPrimaryKeyType), parentObjectPO.Id);
            Key = null;
            IsNew = true;

            LinkedTo = new Field(LinkedToDescription, boundObjectPO.ObjectType.FullName);
            Property = new Field(GetListPropertyDescription(LinkedPrimaryKeyType), boundObjectPO.Id);
            IsDeleted = false;
        }
        
	    /// <summary>
		/// Protected copy constructor
		/// </summary>
		protected ListLink(
			ListLink source
			)
			: base(source)
		{
	        if (Key != null)
			    Key = (Field) source.Key.Clone();
	        
			ParentObject = (Field) source.ParentObject.Clone();
			IsBoundToType = source.IsBoundToType;
	        BoundToType = source.BoundToType;
	        ParentType = source.ParentType;
	        LinkedPrimaryKeyType = source.LinkedPrimaryKeyType;
		}

		/// <summary>
		/// Copys the list link object by calling the copy constructor
		/// </summary>
		public override Object Clone()
		{
			return new ListLink(this);
		}

        /// <summary>
        /// Returns the list link property in a field hash table
        /// </summary>
        /// <param name="idType">Type of the primary key</param>
        /// <param name="parentType">Type of the parent.</param>
        /// <param name="generalLinked">if set to <c>true</c> [general linked].</param>
        /// <param name="bindedObjectPrimaryKeyType">Type of the binded object primary key.</param>
        /// <returns></returns>
        public static Dictionary<string, FieldDescription> GetHashTemplates(Type idType, Type parentType, bool generalLinked, Type bindedObjectPrimaryKeyType)
		{
            var hashtable = new Dictionary<string, FieldDescription>(5);

            var parentProjection = ReflectionHelper.GetProjection(parentType, null);
            var poDesc = GetParentObjectDescription(parentProjection.GetPrimaryKeyDescription().ContentType);
            var hpDesc = GetHashPropertyDescription(bindedObjectPrimaryKeyType);

            poDesc.IsAutoIncrement = hpDesc.IsAutoIncrement = false;

            hashtable.Add(DBConst.LinkIdField, new FieldDescription(DBConst.LinkIdField, null, typeof(Field), idType, null, true));
			hashtable.Add(DBConst.ParentObjectField, poDesc);
            hashtable.Add(DBConst.PropertyField, hpDesc);

            if (generalLinked)
				hashtable.Add(DBConst.LinkedToField, LinkedToDescription);

			return hashtable;
		}

        /// <summary>
        /// Returns the list link property in a field hash table
        /// </summary>
        /// <param name="parentType">Type of the parent.</param>
        /// <param name="generalLinked">if set to <c>true</c> [general linked].</param>
        /// <param name="bindedObjectPrimaryKeyType">Type of the binded object primary key.</param>
        /// <returns></returns>
        public static Dictionary<string, FieldDescription> GetListTemplates(Type parentType, bool generalLinked, Type bindedObjectPrimaryKeyType)
        {
            var hashtable = new Dictionary<string, FieldDescription>(4);

            var parentProjection = ReflectionHelper.GetProjection(parentType, null);
            var poDesc = GetParentObjectDescription(parentProjection.GetPrimaryKeyDescription().ContentType);
            var lpDesc = GetListPropertyDescription(bindedObjectPrimaryKeyType);

            poDesc.IsAutoIncrement = lpDesc.IsAutoIncrement = false;

            hashtable.Add(DBConst.ParentObjectField, poDesc);
            hashtable.Add(DBConst.PropertyField, lpDesc);

            if (generalLinked)
            {
                hashtable.Add(DBConst.LinkedToField, LinkedToDescription);
            }

            return hashtable;
        }

        /// <summary>
        /// Gets the list templates.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, FieldDescription> GetTemplates()
        {
            return Key != null 
                ? GetHashTemplates(Key.Value.GetType(), ParentType, !IsBoundToType, LinkedPrimaryKeyType) 
                : GetListTemplates(ParentType, !IsBoundToType, LinkedPrimaryKeyType);
        }

	    /// <summary>
		/// Returns the list link property in a field hash table
		/// </summary>
		/// <returns></returns>
		public PersistentProperties Fields(IPersister persister)
		{
            var properties = new PersistentProperties();

	        if (Key != null)
                properties.FieldProperties = properties.FieldProperties.Add(Key.Name, Key);

            properties.FieldProperties = properties.FieldProperties.Add(ParentObject.Name, ParentObject);
            properties.FieldProperties = properties.FieldProperties.Add(Property.Name, Property);

			if (!IsBoundToType)
                properties.FieldProperties = properties.FieldProperties.Add(LinkedTo.Name, LinkedTo);
			return properties;
		}


		/// <summary>
		/// Override the IsModified Property to set modified state to the fields
		/// </summary>
		public override bool IsModified
		{
			get { return base.IsModified; }
			set
			{
				ParentObject.IsModified =
						Property.IsModified =
							LinkedTo.IsModified = value;
			    
			    if (Key != null)
			        Key.IsModified = value;
			}
		}


	    /// <summary>
        /// Updates the reference id.
        /// </summary>
        /// <param name="parentPrimaryKey">The parent primary key.</param>
        public void UpdateParentReferenceId(object parentPrimaryKey)
	    {
            ParentObject.Value = parentPrimaryKey;
	    }
	}
}