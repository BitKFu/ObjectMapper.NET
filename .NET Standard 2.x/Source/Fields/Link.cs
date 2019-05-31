using System;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Fields
{
	/// <summary>
	/// Class that defines a link to an other value object.
	/// </summary>
	[Serializable]
    public class Link : IModification
	{
		/// <summary>
		/// Property Field object
		/// </summary>
		private Field property;

		/// <summary>
		/// Returns the property Field object
		/// </summary>
		public Field Property
		{
			get { return property; }
			set { property = value; }
		}

		/// <summary>
		/// Property Field LinkedTo
		/// </summary>
		private Field linkedTo;

		/// <summary>
		/// Returns the property field LinkedTo
		/// </summary>
		public Field LinkedTo
		{
			get { return linkedTo; }
			set { linkedTo = value; }
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
		public virtual bool IsModified
		{
			get { return property.IsModified; }
			set { property.IsModified = value; }
		}

		/// <summary>
		/// Internal used constructor for templates - do not use elsewhere
		/// </summary>
		protected Link()
		{
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fdesc">The fdesc.</param>
        /// <param name="pProperty">Defines the property id</param>
        /// <param name="pLinkedTo">Defines the class name the property is linking to</param>
        public Link(FieldDescription fdesc, object pProperty, String pLinkedTo)
		{
			property = new Field(fdesc, pProperty);
            linkedTo = new Field(new FieldDescription(fdesc.Name + DBConst.TypAddition, null, pLinkedTo.GetType(), false), pLinkedTo);

			IsNew = true;
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fdesc">The fdesc.</param>
        public Link(FieldDescription fdesc)
		{
			property = new Field(fdesc, null);
			linkedTo = new Field(new FieldDescription(fdesc.Name + DBConst.TypAddition, null, typeof(string), false), null);

			IsNew = true;
		}

		/// <summary>
		/// Protected copy constructor
		/// </summary>
		protected Link(
			Link source
			)
		{
			property = (Field) source.property.Clone();
			linkedTo = (Field) source.linkedTo.Clone();
			IsNew = source.IsNew;
			IsDeleted = source.IsDeleted;
		}

		/// <summary>
		/// Copys a link by creating a new object with the copy constructor
		/// </summary>
		public virtual Object Clone()
		{
			return new Link(this);
		}

        /// <summary>
        /// Updates the reference id.
        /// </summary>
        /// <param name="vo">The vo.</param>
        internal void UpdateAutoincrementedId(IValueObject vo)
        {
            if (vo == null)
                return;

            if (TypeHelper.GetBaseType(vo.GetType()) == typeof(int))
            {
                property.Value = vo.Id;
                linkedTo.Value = vo.GetType().FullName;
            }
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
					if (!property.Value.ToString().Equals(po.Id))
					{
                        Type resultType = mapper.ObjectFactory.GetType((string)linkedTo.Value);
					    var resultProjection = ReflectionHelper.GetProjection(resultType, mapper.MirroredLinqProjectionCache);

                        returnPO = tempHash.Get(resultType, property.Value);
						if (returnPO == null)
						{
                            var vo = (IValueObject)mapper.PrivateLoad(resultProjection,
								property.Value, tempHash, int.MaxValue, null);
							returnPO = tempHash.Get(vo);
						}
					}
				}

				property.Value = po.Id;
				linkedTo.Value = po.ObjectType.FullName;

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
				    Type resultType = mapper.ObjectFactory.GetType((string) linkedTo.Value);
                    returnPO = tempHash.Get(resultType, property.Value);
                    if (returnPO == null)
                    {
                        var resultProjection = ReflectionHelper.GetProjection(resultType, mapper.MirroredLinqProjectionCache);
                        var vo = (IValueObject)mapper.PrivateLoad(resultProjection,
                            property.Value, tempHash, int.MaxValue, null);
                        returnPO = tempHash.Get(vo);
                    }

					IsDeleted = true;
				}

				property.Value = null;        
//			    property.IsModified = true;
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
				&& (!property.Value.GetType().IsSubclassOf(typeof (Type))))
			    
		        && (!isDeleted);
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