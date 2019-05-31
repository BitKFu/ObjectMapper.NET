using System;
using System.Collections.Generic;
using System.Text;

namespace AdFactum.Data.Queries
{
    /// <summary>
    /// A Union join for two or more SubSelects
    /// </summary>
    public class UnionAll : Union
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnionAll"/> class.
        /// </summary>
        /// <param name="_subSelects">A Union joins two or more SubSelects. The subSelect parameter is used to put in the nested SubSelect objects.</param>
        public UnionAll(params SubSelect[] _subSelects)
            :base(_subSelects)
        		{
        		}

        /// <summary>
        /// Gets the connector.
        /// </summary>
        /// <value>The connector.</value>
        public override string Connector
        {
            get
            {
                return "UNION ALL";
            }
        }
    }
}
