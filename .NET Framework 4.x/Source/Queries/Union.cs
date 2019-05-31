using System.Collections;

namespace AdFactum.Data.Queries
{
	/// <summary>
	/// A Union joins two or more SubSelects
	/// </summary>
	public class Union
	{
		private ArrayList subSelects;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="_subSelects">A Union joins two or more SubSelects. The subSelect parameter is used to put in the nested SubSelect objects.</param>
		public Union(params SubSelect[] _subSelects)
		{
			subSelects = new ArrayList();
			foreach (SubSelect select in _subSelects)
				subSelects.Add(select);
		}

		/// <summary>
		/// Accessor for Sub Selects
		/// </summary>
		public ArrayList SubSelects
		{
			get { return subSelects; }
		}

        /// <summary>
        /// Gets the connector.
        /// </summary>
        /// <value>The connector.</value>
	    public virtual string Connector
	    {
            get { return "UNION"; }
	    }
	}
}