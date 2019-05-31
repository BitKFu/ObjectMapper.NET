using System;

namespace AdFactum.Data.Internal
{
	/// <summary>
	/// Interally used object struct which will be is stored in the object hash.
	/// </summary>
	public class HashEntry
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HashEntry"/> class.
		/// </summary>
		/// <param name="ppo">The ppo.</param>
		/// <param name="pvo">The pvo.</param>
		public HashEntry(PersistentObject ppo, object pvo)
		{
			po = ppo;
			vo = pvo;
		}

		private PersistentObject po;
		private object vo;

		/// <summary>
		/// Gets the po.
		/// </summary>
		/// <value>The po.</value>
		public PersistentObject Po
		{
			get { return po; }
		}

		/// <summary>
		/// Gets the vo.
		/// </summary>
		/// <value>The vo.</value>
		public object Vo
		{
			get { return vo; }
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// 	<see langword="true"/> if the specified <see cref="T:System.Object"/> is equal to the
		/// current <see cref="T:System.Object"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public override bool Equals(object obj)
		{
			PersistentObject toCompare = obj as PersistentObject;
			if (toCompare != null)
				return po.Id.Equals(toCompare.Id);

			IValueObject voToCompare = obj as IValueObject;
			if (voToCompare != null)
				return voToCompare.Equals(vo);

			if (obj is Guid)
				return po.Id.Equals((Guid) obj);

			if (obj is string)
				return po.Id.ToString().Equals((string) obj);

			return base.Equals(obj);
		}

		/// <summary>
		/// Serves as a hash function for a particular type, suitable
		/// for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return po.GetHashCode();
		}

		/// <summary>
		/// Gets the key.
		/// </summary>
		/// <value>The key.</value>
		public object Id
		{
			get {

                if ((po != null) && (po.Id != null))
                    return po.Id;

                IValueObject ivo = vo as IValueObject;
                if (ivo != null)
                {
                    if (ivo.Id != null)
                        return ivo.Id;
                    else
                        return ivo.InternalId;
                }

                if (vo != null)
                    return vo.GetHashCode();

                return null;
            }
		}

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
	    public Type Type
	    {
	        get
	        {
                if (po != null)
                    return po.ObjectType;

                if (vo != null)
                    return vo.GetType();

                return null;
	        }
	    }

	}
}
