using System;
using System.Collections.Generic;
using System.Text;

namespace AdFactum.Data.Queries
{
    /// <summary>
    /// Defines in which clause (Where or Having) the condition resists
    /// </summary>
    public enum ConditionClause
    {
        /// <summary>
        /// Undefined Condition is used as a base that can contain Where and HavingClause elements
        /// </summary>
        Undefined,

        /// <summary>
        /// The condition resists within the With Clause
        /// </summary>
        WithClause,

        /// <summary>
        /// The condition resists within the From Clause
        /// </summary>
        FromClause, 

        /// <summary>
        /// The condition resists within the Where Clause
        /// </summary>
        WhereClause,

        /// <summary>
        /// The condition resists within the Having Clause
        /// </summary>
        HavingClause,

        /// <summary>
        /// Select condition
        /// </summary>
        SelectClause,

        /// <summary>
        /// Used to store informal (information) data
        /// </summary>
        InformalClause,

        /// <summary>
        /// GroupByClause
        /// </summary>
        GroupByClause,

        /// <summary>
        /// JoinClause
        /// </summary>
        JoinClause,

        /// <summary>
        /// HintClause
        /// </summary>
        HintClause
    }
}
