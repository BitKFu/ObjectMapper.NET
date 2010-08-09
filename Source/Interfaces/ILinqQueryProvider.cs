using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.Interfaces
{
    public interface ILinqQueryProvider : IQueryProvider
    {
        /// <summary>
        /// Don't Dispose the command object after executing the command
        /// </summary>
        bool DontDisposeCommand { get; set; }

        /// <summary>
        /// Returns the Linq Persister
        /// </summary>
        IDbCommand PreCompile(Expression expression);

        ///<summary>
        /// Marks the Query as a compiled template query
        ///</summary>
        void MarkAsTemplate();

        /// <summary>
        /// Returns the ObjectMapper .NET
        /// </summary>
        ObjectMapper Mapper { get; }

        /// <summary>
        /// Returns the stored sql command of a compiled query
        /// </summary>
        string StoredSqlCommand { get; }

        /// <summary>
        /// Returns the SQL Id of the stored sql command
        /// </summary>
        string StoredSqlId { get; }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <value>The expression.</value>
        Expression Expression { get; }

        /// <summary>
        /// Gets the dynamic cache.
        /// </summary>
        /// <value>The dynamic cache.</value>
        Cache<Type, ProjectionClass> DynamicCache { get; }

        /// <summary>
        /// Gets the hierarchy level.
        /// </summary>
        /// <value>The hierarchy level.</value>
        int HierarchyLevel { get; }
    }
}
