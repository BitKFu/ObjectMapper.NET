using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Language;
using AdFactum.Data.Queries;

namespace AdFactum.Data.SqlServer
{
    /// <summary>
    /// Thats the abstract base class for the SqlServer and SqlServerCE Persister
    /// </summary>
    public abstract class BaseSqlPersister : MicrosoftBasedPersister
    {
        #region IPersister Member


        /// <summary>
        /// Gets the parameter string.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override string GetParameterString(IDbDataParameter parameter)
        {
            return parameter.ParameterName;
        }

        /// <summary>
        /// Defines the join sytanx
        /// </summary>
        /// <value></value>
        protected override JoinSyntaxEnum JoinSyntax
        {
            get { return JoinSyntaxEnum.FromClauseGlobalJoin; }
        }

        /// <summary>
        /// Replaces the statics within a sql statement.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public override string ReplaceStatics(string sql)
        {
            return base.ReplaceStatics(sql
                   .Replace(Condition.TRIM, "RTRIM")
                   .Replace(Condition.UPPER, "UPPER"));
        }

        /// <summary>
        /// Returns the type of the used Linq Expression Writer
        /// </summary>
        public override Type LinqExpressionWriter
        {
            get { return typeof(SqlExpressionWriter); }
        }

        #endregion

    }
}