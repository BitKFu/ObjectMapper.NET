using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;

namespace AdFactum.Data.SqlServer
{
    /// <summary>
    /// Defines a sql server persister.
    /// </summary>
    public class SqlPersister : BaseSqlPersister
    {
        /// <summary>
        /// Connection String to a Microsoft SQL Server
        /// </summary>
        private const string CONNECTION_STRING = "Persist Security Info=False;Integrated Security=False;Initial Catalog={0};Data Source={1};User Id={2};Password={3};";
        private const string CONNECTION_STRING_TRUSTED = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog={0};Data Source={1};";

        public SqlPersister()
        {
            TypeMapper = new SqlTypeMapper();
        }

        /// <summary>
        /// Returns the Schema Writer
        /// </summary>
        /// <value></value>
        public override ISchemaWriter Schema
        {
            get
            {
                return new SqlSchemaWriter(TypeMapper, DatabaseSchema);
            }
        }

        /// <summary>
        /// Returns the Integrity Checker
        /// </summary>
        /// <value></value>
        public override IIntegrity Integrity
        {
            get
            {
                return new SqlIntegrityChecker(this, TypeMapper, DatabaseSchema);
            }
        }

        /// <summary>
        /// Returns the repository class
        /// </summary>
        /// <value></value>
        public override IRepository Repository
        {
            get
            {
                return new SqlRepository(SqlTracer);
            }
        }

        /// <summary>
        /// Gets the concatinator.
        /// </summary>
        /// <value>The concatinator.</value>
        public override string Concatinator
        {
            get { return " + "; }
        }

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

            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            int rows = 0;
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

                IDbDataParameter parameter = CreateParameter("minLine", minLine, null);
                command.Parameters.Add(parameter);

                parameter = CreateParameter("maxLine", maxLine, null);
                command.Parameters.Add(parameter);
                command.CommandText = outerSql;

                List<PersistentProperties> result = PrivateSelect(command, fieldTemplates, 0, int.MaxValue);
                rows = result.Count;
                return result;
            }
            finally
            {
                stopwatch.Stop(command, CreateSql(command), rows);
                command.DisposeSafe();
            }
        }

        /// <summary>
        /// Connects to a Microsoft SQL Server
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server Name</param>
        public virtual void Connect(string database, string server)
        {
            Connect(database, server, "");
        }

        /// <summary>
        /// Connects to a Microsoft SQL Server
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server Name</param>
        /// <param name="additionalConnectionParameters">Additional connection parameters</param>
        public virtual void Connect(string database, string server, string additionalConnectionParameters)
        {
            String connectionString = String.Format(CONNECTION_STRING_TRUSTED, database, server) + additionalConnectionParameters;
            Connect(connectionString);
        }

        /// <summary>
        /// Connects to a Microsoft SQL Server using an Connection String
        /// </summary>
        /// <param name="connectionString"></param>
        public virtual void Connect(string connectionString)
        {
            Connection = new SqlConnection {ConnectionString = connectionString};
            SavelyOpenConnection();

            if (SqlTracer != null)
                SqlTracer.OpenConnection(((SqlConnection) Connection).ServerVersion, Connection.ConnectionString);
        }

        /// <summary>
        /// Connects to a Microsoft SQL Server on a special username
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server name</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        public virtual void Connect(string database, string server, string user, string password)
        {
            Connect(database, server, user, password, "");
        }

        /// <summary>
        /// Connects to a Microsoft SQL Server on a special username
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server name</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        /// <param name="additionalConnectionParameters">Additional connection parameters</param>
        public virtual void Connect(string database, string server, string user, string password, string additionalConnectionParameters)
        {
            var connectionString = String.Format(CONNECTION_STRING, database, server, user, password) + additionalConnectionParameters;
            Connect(connectionString);
        }

        /// <summary>
        /// Creates the command object.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public override IDbCommand CreateCommand(string sql)
        {
            var command = new SqlCommand(sql, (SqlConnection) Connection)
            {
                Transaction = (SqlTransaction) Transaction
            };

            return command;
        }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <returns></returns>
        public override IDbCommand CreateCommand()
        {
            var command = new SqlCommand
            {
                Connection = (SqlConnection) Connection, 
                Transaction = (SqlTransaction) Transaction
            };
            return command;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="numberOfParameter">The number of parameter.</param>
        /// <param name="type">Type of the value object</param>
        /// <param name="value">The value.</param>
        /// <param name="metaInfo">property meta information</param>
        /// <returns></returns>
        public override IDbDataParameter AddParameter(IDataParameterCollection parameters, ref int numberOfParameter, Type type, object value, PropertyMetaInfo metaInfo)
        {
            object convertedValue = TypeMapper.ConvertValueToDbType(value);
            var dbType = (SqlDbType)TypeMapper.GetEnumForDatabase(type, metaInfo);
		    
            /*
			 * look if a parameter with the same value exists.
			 */
            IEnumerator parameterEnum = parameters.GetEnumerator();
            while (parameterEnum.MoveNext())
            {
                var current = (SqlParameter) parameterEnum.Current;
                if ( (current.Value.Equals(convertedValue)) && (current.SqlDbType.Equals(dbType)) )
                    return current;
            }

            var parameter = new SqlParameter("@p" + numberOfParameter.ToString("00"), dbType) {Value = convertedValue};
            parameters.Add(parameter);
            numberOfParameter++;

            return parameter;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        public override IDbDataParameter CreateParameter(string parameterName, Type type, object value, PropertyMetaInfo metaInfo)
        {
            if (!parameterName.StartsWith("@"))
                parameterName = string.Concat("@", parameterName);

            IDbDataParameter parameter = new SqlParameter(parameterName, (SqlDbType)TypeMapper.GetEnumForDatabase(type, metaInfo))
            {Value = TypeMapper.ConvertValueToDbType(value)};

            return parameter;
        }

        /// <summary>
        /// Creates the parameter from an existing parameter, but replaces the value
        /// </summary>
        public override IDbDataParameter CreateParameter(IDbDataParameter copyFrom, object value)
        {
            var copy = (SqlParameter) copyFrom;

            string parameterName = copy.ParameterName;
            if (!parameterName.StartsWith("@"))
                parameterName = string.Concat("@", parameterName);

            IDbDataParameter parameter = new SqlParameter(parameterName, copy.SqlDbType)
            {Value = TypeMapper.ConvertValueToDbType(value)};

            return parameter;
        }

        /// <summary>
        /// Creates the data adapter.
        /// </summary>
        /// <returns></returns>
        protected override IDbDataAdapter CreateDataAdapter()
        {
            return new SqlDataAdapter();
        }
    }
}