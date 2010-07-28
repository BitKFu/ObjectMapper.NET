using System;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Fields
{
	/// <summary>
	/// A Specialized Link is a link that can only point to a specific object type, but not to the derived object types.
	/// Therefore it's a special form of a single link.
	/// </summary>
	[Serializable]
    public class SpecializedLink : IModification
	{
		/// <summary>
		/// Attribute property
		/// </summary>
		private readonly Field property;

		/// <summary>
		/// Returns the attribute property field
		/// </summary>
		public Field Property
		{
			get { return property; }
		}

        /// <summary>
        /// Returns the property Name
        /// </summary>
        public string PropertyName
        {
            get { return property.PropertyName; }
        }
	    /// <summary>
		/// Returns true, if the field object changed since the last save.
		/// </summary>
		public bool IsModified
		{
			get { return property.IsModified; }
			set { property.IsModified = value; }
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fdesc">The fdesc.</param>
        /// <param name="pProperty">Defines the property id</param>
		public SpecializedLink(FieldDescription fdesc, object pProperty)
		{
			property = new Field(fdesc, pProperty);
			IsNew = true;
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fdesc">The fdesc.</param>
		public SpecializedLink(FieldDescription fdesc)
		{
            property = new Field(fdesc, null);
			IsNew = true;
		}

		/// <summary>
		/// Internal used constructor for templates - do not use elsewhere
		/// </summary>
		protected SpecializedLink()
		{
		}

		/// <summary>
		/// Protected copy constructor
		/// </summary>
		protected SpecializedLink(
			SpecializedLink source
			)
		{
			property = (Field) source.property.Clone();
			IsNew = source.IsNew;
			IsDeleted = source.IsDeleted;
		}

		/// <summary>
		/// Copys a link by creating a new object with the copy constructor
		/// </summary>
		public virtual Object Clone()
		{
			return new SpecializedLink(this);
		}

        /// <summary>
        /// Updates the reference id.
        /// </summary>
        /// <param name="vo">The vo.</param>
        internal virtual void UpdateAutoincrementedId(IValueObject vo)
        {
            if (vo == null)
                return;

            if (TypeHelper.GetBaseType(vo.GetType()) == typeof(int))
                property.Value = vo.Id;
        }
        
        /// <summary>
		/// Replaces the current link with a new link, and returns the old link.
		/// </summary>
		/// <param name="po">New persitent object to link</param>
		/// <param name="tempHash">Temporarily used hash for storing the persistent objects</param>
		/// <param name="mapper">Database Mapper. If necessary objects will be reloaded</param>
		/// <returns>Returns the old link</returns>
		public PersistentObject SetLinkedObject(PersistentObject po, ObjectHash tempHash, ObjectMapper mapper)
		{
			PersistentObject returnPO = null;

			/*
			 * A new link has to be created?
			 */
		    Type resultType = property.FieldDescription.ContentType;

		    if (po != null)
			{
				/*
				 * The link counter changes?
				 */
				if (HasValue())
				{
					/*
					 * Only change the link counter, if both objects (old and new one) are not identical.
					 */
					if (!property.Value.ToString().Equals(po.Id.ToString()))
					{
                        returnPO = tempHash.Get(resultType, property.Value);
					}
				}

				property.Value = po.Id;
				IsDeleted = false;
			}
				/*
				* No, than the link has to be deleted
				*/
			else
			{
				/*
				 * Do we have to change the link counter?
				 */
				if (HasValue())
				{
                    returnPO = tempHash.Get(resultType, property.Value);
					if (returnPO == null)
					{
					    var resultProjection = ReflectionHelper.GetProjection(resultType, mapper.MirroredLinqProjectionCache);
						var vo = (IValueObject) mapper.PrivateLoad(
                            resultProjection,
							property.Value, tempHash, int.MaxValue, null);
                        returnPO = tempHash.Get(vo);
					}

					IsDeleted = true;
				}

				property.Value = null;
			}

			IsNew = false;
			return returnPO;
		}

		/// <summary>
		/// Returns true, if the link contains a value
		/// </summary>
		/// <returns></returns>
		public bool HasValue()
		{
			return ((property.Value != null)
				&& (!property.Value.GetType().IsSubclassOf(typeof (Type))));
		}

		private bool isDeleted;

		/// <summary>
		/// Only used for internal checking, if the field has to delete
		/// </summary>
		public bool IsDeleted
		{
			get { return isDeleted; }
			set { isDeleted = value; }
		}

		private bool isNew;

		/// <summary>
		/// Only used for internal checking, if the field has to be created
		/// </summary>
		public bool IsNew
		{
			get { return isNew; }
			set { isNew = value; }
		}

	}
}