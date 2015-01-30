using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;

namespace AdFactum.Data.SqlServer
{
    /// <summary>
    /// Defines a sql server persister.
    /// </summary>
    public class Sql2000Persister : BaseSqlPersister
    {
        #region Private Const Declarations

        /// <summary>
        /// Connection String to a Microsoft SQL Server
        /// </summary>
        private const String CONNECTION_STRING = "Persist Security Info=False;Integrated Security=False;Initial Catalog={0};Data Source={1};User Id={2};Password={3};";
        private const String CONNECTION_STRING_TRUSTED = "Persist Security Info=False;Integrated Security=SSPI;Initial Catalog={0};Data Source={1};";

        #endregion

        #region Public Constructors

        /// <summary>
        /// Base Constructor
        /// </summary>
        public Sql2000Persister()
        {
            TypeMapper = new SqlTypeMapper();
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server using a connection string
        /// </summary>
        /// <param name="connectionString"></param>
        public Sql2000Persister(string connectionString)
            :this()
        {
            Connect(connectionString);
        }


        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server Name</param>
        public Sql2000Persister(string database, string server)
            : this()
        {
            Connect(database, server);
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server on a special username
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server name</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        public Sql2000Persister(string database, string server, string user, string password)
            : this()
        {
            Connect(database, server, user, password);
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server Name</param>
        /// <param name="tracer">Tracer object for sql output</param>
        public Sql2000Persister(string database, string server, ISqlTracer tracer)
            : this()
        {
            SqlTracer = tracer;
            Connect(database, server);
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server on a special username
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server name</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        /// <param name="tracer">Tracer object for sql output</param>
        public Sql2000Persister(string database, string server, string user, string password, ISqlTracer tracer)
            : this()
        {
            SqlTracer = tracer;
            Connect(database, server, user, password);
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server Name</param>
        /// <param name="additionalConnectionParameters">Additional connection parameters</param>
        public Sql2000Persister(string database, string server, string additionalConnectionParameters)
            : this()
        {
            Connect(database, server, additionalConnectionParameters);
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server on a special username
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server name</param>
        /// <param name="user">User</param>
        /// <param name="password">Password</param>
        /// <param name="additionalConnectionParameters">Additional connection parameters</param>
        public Sql2000Persister(string database, string server, string user, string password, string additionalConnectionParameters)
            : this()
        {
            Connect(database, server, user, password, additionalConnectionParameters);
        }

        /// <summary>
        /// Constructor to connect with a Microsoft SQL Server
        /// </summary>
        /// <param name="database">Database Name</param>
        /// <param name="server">Server Name</param>
        /// <param name="additionalConnectionParameters">Additional connection parameters</param>
        /// <param name="tracer">Tracer object for sql output</param>
        public Sql2000Persister(string database, string server, string additionalConnectionParameters, ISqlTracer tracer)
            : this()
        {
            SqlTracer = tracer;
            Connect(database, server, additionalConnectionParameters);
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
        public Sql2000Persister(string database, string server, string user, string password, string additionalConnectionParameters, ISqlTracer tracer)
            : this()
        {
            SqlTracer = tracer;
            Connect(database, server, user, password, additionalConnectionParameters);
        }

        #endregion

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

        #region Connection Methods

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
            String connectionString = String.Format(CONNECTION_STRING, database, server, user, password) + additionalConnectionParameters;

            Connection = new SqlConnection {ConnectionString = connectionString};
            SavelyOpenConnection();

            if (SqlTracer != null)
                SqlTracer.OpenConnection(((SqlConnection) Connection).ServerVersion, Connection.ConnectionString);
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
            /*
             * Use base SQL, if the frame starts greater than 1
             */
            if (maxLine == int.MaxValue || minLine > 1)
                return
                    base.PageSelect(projection, additionalColumns, whereClause, orderBy, minLine, maxLine,
                                    fieldTemplates, globalParameter, distinct);


            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            int rows = 0;
            try
            {
                IDictionary virtualAlias = new HybridDictionary();

                int index = 1;
                string withClause = PrivateWithClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias, ref index);
                string tables = PrivateFromClause(projection, whereClause, command.Parameters, fieldTemplates,
                                                  globalParameter, virtualAlias, ref index);

                /*
                 * SQL Bauen
                 */
                String query = string.Concat(withClause, distinct ? "SELECT DISTINCT " : "SELECT "
                                             , "TOP ", maxLine, " "
                                             , projection.GetColumns(whereClause, additionalColumns), " "
                                             , BuildVirtualFields(fieldTemplates, globalParameter, virtualAlias)
                                             , BuildSelectFunctionFields(fieldTemplates, globalParameter)
                                             , " FROM " + tables);

                /*
                 * Query bauen
                 */
                query += PrivateCompleteWhereClause(projection, fieldTemplates, whereClause, globalParameter,
                                                    virtualAlias, command.Parameters, ref index);

                string grouping = projection.GetGrouping();
                if (!string.IsNullOrEmpty(grouping))
                    query = string.Concat(query, " GROUP BY ", grouping);

                query += PrivateCompleteHavingClause(projection, fieldTemplates, whereClause, globalParameter,
                                                     virtualAlias, command.Parameters, ref index);
                query += (orderBy != null ? " ORDER BY " + orderBy.Columns + " " + orderBy.Ordering : "");

                /*
                 * Die IDs selektieren und Objekt laden
                 */
                command.CommandText = query;

                List<PersistentProperties> result = PrivateSelect(command, fieldTemplates, minLine, maxLine);
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

        /// <summary>
        /// Gets the concatinator.
        /// </summary>
        /// <value>The concatinator.</value>
        public override string Concatinator
        {
            get { return " + "; }
        }

        #region Dispose Pattern

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="T:AdFactum.Data.XmlPersister.XmlPersister"/> is reclaimed by garbage collection.
        /// </summary>
        ~Sql2000Persister()
        {
            Dispose(false);
        }

        #endregion

    }
}