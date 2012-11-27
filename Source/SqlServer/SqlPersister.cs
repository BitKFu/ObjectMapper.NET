using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;

namespace AdFactum.Data.SqlServer
{
    /// <summary>
    /// Defines a sql server persister.
    /// </summary>
    public class SqlPersister : Sql2000Persister
    {

        #region Public Constructors

        /// <summary>
        /// Base Constructor
        /// </summary>
        public SqlPersister()
        {
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server using a Connection String
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlPersister(string connectionString)
            :base(connectionString)
        {
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server Name</param>
        public SqlPersister(string database, string server)
            : base(database, server)
        {
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server on a special username
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server name</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        public SqlPersister(string database, string server, string user, string password)
            : base(database, server, user, password)
        {
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server Name</param>
        /// <param name="tracer">Tracer object for sql output</param>
        public SqlPersister(string database, string server, ISqlTracer tracer)
            : base(database, server, tracer)
        {
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server on a special username
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server name</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        /// <param name="tracer">Tracer object for sql output</param>
        public SqlPersister(string database, string server, string user, string password, ISqlTracer tracer)
            : base(database, server, user, password, tracer)
        {
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server Name</param>
        /// <param name="additionalConnectionParameters">Additional connection parameters</param>
        public SqlPersister(string database, string server, string additionalConnectionParameters)
            : base(database, server, additionalConnectionParameters)
        {
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server on a special username
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server name</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        /// <param name="additionalConnectionParameters">Additional connection parameters</param>
        public SqlPersister(string database, string server, string user, string password, string additionalConnectionParameters)
            : base(database, server, user, password, additionalConnectionParameters)
        {
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server Name</param>
        /// <param name="additionalConnectionParameters">Additional connection parameters</param>
        /// <param name="tracer">Tracer object for sql output</param>
        public SqlPersister(string database, string server, string additionalConnectionParameters, ISqlTracer tracer)
            : base(database, server, additionalConnectionParameters, tracer)
        {
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server on a special username
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server name</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        /// <param name="additionalConnectionParameters">Additional connection parameters</param>
        /// <param name="tracer">Tracer object for sql output</param>
        public SqlPersister(string database, string server, string user, string password, string additionalConnectionParameters, ISqlTracer tracer)
            : base(database, server, user, password, additionalConnectionParameters, tracer)
        {
        }

        #endregion

        /// <summary>
        /// Executes a page select and returns value objects that matches the search criteria and line number is within the min and max values.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="additionalColumns">The additional columns.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="orderBy">Order clause to order the selection.</param>
        /// <param name="minLine">Minimum count</param>
        /// <param name="maxLine">Maximum count</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <param name="distinct">Select only distinct values</param>
        /// <returns>List of value objects</returns>
        protected override List<PersistentProperties> PageSelect(ProjectionClass projection, string additionalColumns, ICondition whereClause, OrderBy orderBy, int minLine, int maxLine, Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter, bool distinct)
        {
            // Use the TOP SQL of the base class, if the minLine is 1 or less
            if (minLine <= 1)
                return base.PageSelect(projection, additionalColumns, whereClause, orderBy, minLine, maxLine,
                                       fieldTemplates, globalParameter, distinct);

            IDbCommand command = CreateCommand();

            try
            {

                int index = 1;
                IDictionary virtualAlias = new HybridDictionary();

                /*
                 * Build outer tables
                 */
                string orderByString;
                if (orderBy != null)
                    orderByString = string.Concat(" ORDER BY ", orderBy.GetColumn(false), " ", orderBy.Ordering);
                else
                    orderByString = string.Concat(" ORDER BY ", projection.PrimaryKeyColumns);

                /*
                 * Build inner tables
                 */
                string withClause = PrivateWithClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias, ref index);
                string innerTableStr = PrivateFromClause(projection, whereClause, command.Parameters, fieldTemplates,
                                                         globalParameter, virtualAlias, ref index);
                string innerWhere = PrivateCompleteWhereClause(projection, fieldTemplates, whereClause, globalParameter,
                                                               virtualAlias, command.Parameters, ref index);

                string businessSql = string.Concat("SELECT ", projection.GetColumns(whereClause, null),
                                                   BuildVirtualFields(fieldTemplates, globalParameter, virtualAlias),
                                                   BuildSelectFunctionFields(fieldTemplates, globalParameter),
                                                   ", ROW_NUMBER() OVER(", orderByString, ") as Z_R_N ",
                                                   " FROM ", innerTableStr, innerWhere);

                string grouping = projection.GetGrouping();
                if (!string.IsNullOrEmpty(grouping))
                    businessSql = string.Concat(businessSql, " GROUP BY ", grouping);
                businessSql += PrivateCompleteHavingClause(projection, fieldTemplates, whereClause, globalParameter,
                                                           virtualAlias, command.Parameters, ref index);

                /*
                * Build outer Select 
                */
                string outerSql = string.Concat(withClause, distinct ? "SELECT DISTINCT " : "SELECT ",
                                                projection.ColumnsOnly
                                                , " FROM ("
                                                , businessSql
                                                , ") "
                                                , " PAGE"
                                                , " WHERE Z_R_N BETWEEN @minLine AND @maxLine ");

                IDbDataParameter parameter = CreateParameter("minLine", minLine, false);
                command.Parameters.Add(parameter);

                parameter = CreateParameter("maxLine", maxLine, false);
                command.Parameters.Add(parameter);
                command.CommandText = outerSql;

                List<PersistentProperties> result = PrivateSelect(command, fieldTemplates, 0, int.MaxValue);
                return result;
            }
            finally
            {
                command.DisposeSafe();
            }
        }

    }
}