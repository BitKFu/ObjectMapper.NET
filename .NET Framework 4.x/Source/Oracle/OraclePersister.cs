using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Language;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace AdFactum.Data.Oracle
{
	/// <summary>
	/// Defines an oracle persister
	/// </summary>
	[Serializable]
    public class OraclePersister : BasePersister
	{
		#region protected Const Declarations

		/// <summary>
		/// Connection String to a Microsoft SQL Server
		/// </summary>
		private const String CONNECTION_STRING = "User Id={0};Password={1};Data Source={2};";

		#endregion

		#region protected Members


		/// <summary>
		/// Gets sorting globalization settings.
		/// </summary>
		/// <value>The setting name.</value>
		public string Sorting
		{
			set
			{
				OracleGlobalization oraGlob = ((OracleConnection) Connection).GetSessionInfo();
				oraGlob.Sort = value;
				((OracleConnection) Connection).SetSessionInfo(oraGlob);
			}
		}

		#endregion

		#region Public Constructors

		/// <summary>
		/// Base Constructor
		/// </summary>
		public OraclePersister()
		{
			TypeMapper = new OracleTypeMapper();
		}

        /// <summary>
        /// Constructor that connects directly to an oracle instance using the given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        public OraclePersister(string connectionString)
            :this()
        {
            Connect(connectionString);
        }

		/// <summary>
		/// Constructor that connects directly to a oracle instance.
		/// </summary>
		/// <param name="user">User name</param>
		/// <param name="password">Password</param>
		/// <param name="dbAlias">Database alias</param>
		public OraclePersister(string user, string password, string dbAlias)
			: this()
		{
			Connect(user, password, dbAlias);
		}

		/// <summary>
		/// Constructor that connects directly to a oracle instance.
		/// </summary>
		/// <param name="user">User name</param>
		/// <param name="password">Password</param>
		/// <param name="dbAlias">Database alias</param>
		/// <param name="tracer">Tracer object for sql output</param>
		public OraclePersister(string user, string password, string dbAlias, ISqlTracer tracer)
			: this()
		{
			SqlTracer = tracer;
			Connect(user, password, dbAlias);
		}

		/// <summary>
		/// Constructor that connects directly to a oracle instance.
		/// </summary>
		/// <param name="user">User name</param>
		/// <param name="password">Password</param>
		/// <param name="additionalConnectionParameters">Additional connection parameters</param>
		/// <param name="dbAlias">Database alias</param>
		public OraclePersister(string user, string password, string dbAlias, string additionalConnectionParameters)
			: this()
		{
			Connect(user, password, dbAlias, additionalConnectionParameters);
		}

		/// <summary>
		/// Constructor that connects directly to a oracle instance.
		/// </summary>
		/// <param name="user">User name</param>
		/// <param name="password">Password</param>
		/// <param name="dbAlias">Database alias</param>
		/// <param name="additionalConnectionParameters">Additional connection parameters</param>
		/// <param name="tracer">Tracer object for sql output</param>
		public OraclePersister(string user, string password, string dbAlias, string additionalConnectionParameters, ISqlTracer tracer)
			: this()
		{
			SqlTracer = tracer;
			Connect(user, password, dbAlias, additionalConnectionParameters);
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
                return new OracleSchemaWriter(TypeMapper, DatabaseSchema);
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
                return new OracleIntegrityChecker(this, TypeMapper, DatabaseSchema);
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
                return new OracleRepository(SqlTracer);
            }
        }

		#region Connection Methods

		/// <summary>
		/// Connects to Oracle Instance
		/// </summary>
		/// <param name="user">User name</param>
		/// <param name="password">Password</param>
		/// <param name="dbAlias">Database alias</param>
        public void Connect(string user, string password, string dbAlias)
		{
			Connect(user, password, dbAlias, "");
		}

		/// <summary>
		/// Connects to Oracle Instance
		/// </summary>
		/// <param name="user">User name</param>
		/// <param name="password">Password</param>
		/// <param name="dbAlias">Database alias</param>
		/// <param name="additionalConnectionParameters">Additional connection parameters</param>
		public void Connect(string user, string password, string dbAlias, string additionalConnectionParameters)
		{
			String connectionString = String.Format(CONNECTION_STRING, user, password, dbAlias) + additionalConnectionParameters;
            DatabaseSchema = user;
            Connect(connectionString);
		}

        /// <summary>
        /// Connects to an Oracle Database instance using a connection String
        /// </summary>
        /// <param name="connectionString"></param>
        public void Connect(string connectionString)
        {
            Debug.Assert(Connection == null, "The Connection has already established");
            Connection = new OracleConnection {ConnectionString = connectionString};
            SavelyOpenConnection();

            if (SqlTracer != null)
                SqlTracer.OpenConnection(((OracleConnection)Connection).ServerVersion, Connection.ConnectionString);
        }


		#endregion

		#region IPersister Member

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
                int rowHint = (((maxLine - minLine + 1) / 15) + 1) * 15;

                /*
                 * Build inner tables
                 */

                string hint = PrivateHintClause(projection, whereClause, command.Parameters, null, null, virtualAlias,
                                                ref index);

                string withClause = PrivateWithClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias, ref index);
                string innerTableStr = PrivateFromClause(projection, whereClause, command.Parameters, fieldTemplates,
                                                         globalParameter, virtualAlias, ref index);
                string innerWhere = PrivateCompleteWhereClause(projection, fieldTemplates, whereClause, globalParameter,
                                                               virtualAlias, command.Parameters, ref index);

                string businessSql = string.Concat("SELECT ", projection.GetColumns(whereClause, null), " ",
                                                   BuildVirtualFields(fieldTemplates, globalParameter, virtualAlias),
                                                   BuildSelectFunctionFields(fieldTemplates, globalParameter),
                                                   " FROM ", innerTableStr, innerWhere);

                string grouping = projection.GetGrouping();
                if (!string.IsNullOrEmpty(grouping))
                    businessSql = string.Concat(businessSql, " GROUP BY ", grouping);

                businessSql += PrivateCompleteHavingClause(projection, fieldTemplates, whereClause, globalParameter,
                                                           virtualAlias, command.Parameters, ref index);
                if (orderBy != null)
                {
                    businessSql += string.Concat(" ORDER BY ", orderBy.Columns, " ", orderBy.Ordering);

                    bool isView = (orderBy.ObjectType != null) && Table.GetTableInstance(orderBy.ObjectType).IsView;
                    if (!distinct && !isView)
                        businessSql += string.Concat(", ", orderBy.TableName, ".ROWID ", orderBy.Ordering);
                }

                /*
                * Build outer Select 
                */
                string outerSql = string.Concat(withClause,
                                                distinct
                                                    ? string.Concat("SELECT ", hint, " DISTINCT ")
                                                    : string.Concat("SELECT ", hint)
                                                , " * "
                                                , " FROM "
                                                , " (SELECT /*+ FIRST_ROWS(", rowHint.ToString(),
                                                ")*/ IQ.*, ROWNUM AS Z_R_N FROM (", businessSql
                                                , ") IQ WHERE ROWNUM <= :maxLine) ", "PAGE", " WHERE Z_R_N >= :minLine ");

                IDbDataParameter parameter = CreateParameter("maxLine", maxLine, null);
                command.Parameters.Add(parameter);

                parameter = CreateParameter("minLine", minLine, null);
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
		/// Returns a list with value objects that matches the search criteria.
		/// </summary>
		/// <param name="tableName">Table Name</param>
		/// <param name="selectSql">Complete select string which can be executed directly.</param>
		/// <param name="selectParameter">Parameter used for the placeholders within the select string. 
		/// A placeholder always begins with an @ followed by a defined key.</param>
		/// <param name="fieldTemplates">Field description.</param>
		/// <returns>List of value objects</returns>
        public override List<PersistentProperties> Select(string tableName, string selectSql, SortedList selectParameter, Dictionary<string, FieldDescription> fieldTemplates)
		{
		    return base.Select(tableName, selectSql.Replace("@", ":"), selectParameter, fieldTemplates);
		}

		#endregion


		/// <summary>
		/// Replaces the statics within a sql statement.
		/// </summary>
		/// <param name="sql">The SQL.</param>
		/// <returns></returns>
		public override string ReplaceStatics(string sql)
		{
            sql = base.ReplaceStatics(sql
                      .Replace(Condition.TRIM, "TRIM")
                      .Replace(Condition.UPPER, "UPPER"));

		    sql = sql.Trim();
            if (sql.EndsWith(";") && !sql.ToUpper(CultureInfo.InvariantCulture).EndsWith("END;"))
                sql = sql.Substring(0, sql.Length - 1);

		    return sql;
		}

	    /// <summary>
		/// Creates the command object.
		/// </summary>
		/// <param name="sql">The SQL.</param>
		/// <returns></returns>
		public override IDbCommand CreateCommand(string sql)
		{
			return new OracleCommand(sql, Connection as OracleConnection);
		}

		/// <summary>
		/// Creates the command.
		/// </summary>
		/// <returns></returns>
        public override IDbCommand CreateCommand()
		{
			var command = new OracleCommand {Connection = Connection as OracleConnection};
		    return command;
		}

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="numberOfParameter">The number of parameter.</param>
        /// <param name="type"></param>
        /// <param name="value">The value.</param>
        /// <param name="metaInfo">property meta information</param>
        /// <returns></returns>
        public override IDbDataParameter AddParameter(IDataParameterCollection parameters, ref int numberOfParameter, Type type, object value, PropertyMetaInfo metaInfo)
		{
			const int BUFFER_SIZE = 2048;
            var buffer = value as byte[];
		    object convertedValue = null;
            var dbType = (OracleDbType)TypeMapper.GetEnumForDatabase(type, metaInfo);

            if (buffer == null)
            {
                /*
                 * Extract the value to test
                 */
                object testValue = convertedValue = TypeMapper.ConvertValueToDbType(value);

                /*
                 * look if a parameter with the same value exists.
                 */
                IEnumerator parameterEnum = parameters.GetEnumerator();
                while (parameterEnum.MoveNext())
                {
                    var current = (OracleParameter) parameterEnum.Current;

                    byte[] secondAsByteArray = testValue as byte[];
                    byte[] firstAsByteArray = current.Value as byte[];

                    if ( (current.Value.Equals(testValue))
                      || (secondAsByteArray != null && firstAsByteArray != null 
                       && firstAsByteArray.SequenceEqual(secondAsByteArray)))
                    {
                        IDbDataParameter copyParameter = new OracleParameter(current.ParameterName, current.OracleDbType);
                        copyParameter.Value = current.Value;
                        parameters.Add(copyParameter);
                        return copyParameter;
                    }
                }
            }

			var parameter = new OracleParameter(":p" + numberOfParameter.ToString("00"), dbType);
			if (buffer != null)
			{
				var blob = new OracleBlob((OracleConnection) Connection);	
				
				int startOffset = 0;

			    blob.BeginChunkWrite(); 
				do 
				{
				    int writeBytes = startOffset+BUFFER_SIZE>buffer.Length? buffer.Length-startOffset : BUFFER_SIZE;
					blob.Write(buffer, startOffset, writeBytes);
					startOffset += writeBytes;
				} while (startOffset < buffer.Length);
				blob.EndChunkWrite();

				parameter.OracleDbType = OracleDbType.Blob;
				parameter.Value = blob;
			}
			else
				parameter.Value = convertedValue;

			parameter.Direction = ParameterDirection.Input;
			parameters.Add(parameter);
			numberOfParameter++;

			return parameter;
		}

        /// <summary>
        /// Creates the parameter from an existing parameter, but replaces the value
        /// </summary>
        public override IDbDataParameter CreateParameter(IDbDataParameter copyFrom, object value)
        {
            var op = (OracleParameter) copyFrom;

            string parameterName = copyFrom.ParameterName;
            if (!parameterName.StartsWith(":"))
                parameterName = string.Concat(":", parameterName);

            OracleParameter parameter;
            if (op.CollectionType == OracleCollectionType.None)
            {
                parameter = new OracleParameter(parameterName, op.OracleDbType);
                parameter.Value = TypeMapper.ConvertValueToDbType(value);
                parameter.Direction = ParameterDirection.Input;
            }
            else
            {
                /*
                 * Copy and normalize values 
                 */
                var sourceList = value as IList;
                object[] source;
                object[] target;

                if (sourceList != null)
                {
                    source = new object[sourceList.Count];
                    sourceList.CopyTo(source, 0);
                    target = new object[sourceList.Count];
                }
                else
                {
                    source = (object[])value;
                    target = new object[source != null ? source.Length : 0];
                }
               
                for (int x = 0; x < source.Length; x++)
                    target[x] = TypeMapper.ConvertValueToDbType(source[x]);

                /*
                 * Special handling for arrays
                 */
                parameter = new OracleParameter(parameterName, op.OracleDbType);
                parameter.Direction = ParameterDirection.Input;

                parameter.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
                parameter.Value = target;
                parameter.Size = target.Length;
            }
            return parameter;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        public override IDbDataParameter CreateParameter(string parameterName, Type type, object value, PropertyMetaInfo metaInfo)
		{
            if (!type.IsListType())
            {
                IDbDataParameter parameter = new OracleParameter(":" + parameterName, (OracleDbType)TypeMapper.GetEnumForDatabase(type, metaInfo))
                                                 {
                                                     Value = TypeMapper.ConvertValueToDbType(value),
                                                     Direction = ParameterDirection.Input
                                                 };

                return parameter;
            }
		    else
            {
                /*
                 * Copy and normalize values
                 */
                var sourceList = value as IList;
                object[] source;
                object[] target;

                if (sourceList != null)
                {
                    source = new object[sourceList.Count];
                    sourceList.CopyTo(source, 0);
                    target = new object[sourceList.Count];
                }
                else
                {
                    source = (object[])value;
                    target = new object[source != null ? source.Length : 0];
                }
               

                // Take the type of the first element, or if empty, the given type and reveal the content of the list ( must be generic )
                var parameterType = source != null ? source[0].GetType() : type.UnpackType()[0];

                /*
                 * Special handling for arrays
                 */
                var parameter = new OracleParameter(":" + parameterName, (OracleDbType)TypeMapper.GetEnumForDatabase(parameterType, metaInfo))
                                    {
                                        Direction = ParameterDirection.Input,
                                        CollectionType = OracleCollectionType.PLSQLAssociativeArray,
                                        Value = target,
                                        Size = target.Length
                                    };

                return parameter;
            }
		}

		/// <summary>
		/// Creates the data adapter.
		/// </summary>
		/// <returns></returns>
		protected override IDbDataAdapter CreateDataAdapter()
		{
			return new OracleDataAdapter();
		}

	    /// <summary>
        /// Executes the secure db call.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="nonQuery">if set to <c>true</c> [non query].</param>
        /// <returns></returns>
        protected override object ExecuteSecureDbCall(IDbCommand command, bool nonQuery)
        {
            ConvertRawParameterToString(command);
            return base.ExecuteSecureDbCall(command, nonQuery);
        }

        /// <summary>
        /// Converts the raw parameter to string.
        /// </summary>
        /// <param name="command">The command.</param>
	    private static void ConvertRawParameterToString(IDbCommand command)
	    {
            /*
             * Replace raw parameter type with string type
             */
	        foreach (OracleParameter parameter in command.Parameters)
	            if ((parameter.OracleDbType == OracleDbType.Raw)
                 && (parameter.Value != DBNull.Value))
	            {
	                parameter.Value = CreateGuidString(parameter.Value as byte[]);
	                parameter.OracleDbType = OracleDbType.Varchar2;
	            }
	    }

		/// <summary>
		/// Gets the parameter string.
		/// </summary>
		/// <param name="parameter">The parameter.</param>
		/// <returns></returns>
        public override string GetParameterString(IDbDataParameter parameter)
		{
            var oracleParameter = parameter as OracleParameter;
		    if ((oracleParameter != null) && (oracleParameter.OracleDbType == OracleDbType.Raw))
                return string.Concat("hextoraw(", parameter.ParameterName, ")");

            return parameter.ParameterName;
		}

        /// <summary>
        /// Creates the GUID string.
        /// </summary>
        /// <returns></returns>
        private static string CreateGuidString(byte[] source)
        {
            if (source == null)
                return string.Empty;

            var rv = new StringBuilder();
            IEnumerator byteEnum = source.GetEnumerator();
            while (byteEnum.MoveNext())
                rv.Append(((Byte)byteEnum.Current).ToString("X2"));
            return rv.ToString();
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
	    /// Gets the concatinator.
	    /// </summary>
	    /// <value>The concatinator.</value>
	    public override string Concatinator
	    {
            get { return " || "; }
	    }


	    /// <summary>
	    /// Retrieves the last auto increment Id
	    /// </summary>
	    /// <returns></returns>
	    protected override int SelectLastAutoId(string tableName)
	    {
            int autoId = -1;
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            try
            {
                command.CommandText = string.Concat("SELECT ", tableName, "_SEQ.CURRVAL FROM DUAL");

                IDataReader reader = ExecuteReader(command);
                try
                {
                    if (reader.Read())
                    {
                        object lastId = reader.GetValue(0);
                        if (lastId != DBNull.Value)
                            autoId = (int)ConvertSourceToTargetType(reader.GetValue(0), typeof(Int32));
                    }
                    return autoId;
                }
                finally
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            finally
            {
                stopwatch.Stop(command, CreateSql(command), 1);
                command.DisposeSafe();
            }
	    }

	    #region Dispose Pattern

	    /// <summary>
	    /// Releases unmanaged resources and performs other cleanup operations before the
	    /// </summary>
	    ~OraclePersister()
	    {
	        Dispose(false);
	    }

        #endregion

        /// <summary>
        /// Returns the type of the used Linq Expression Writer
        /// </summary>
        public override Type LinqExpressionWriter
        {
            get { return typeof(OracleExpressionWriter); }
        }

        /// <summary>
        /// Rewrites the Linq Expression
        /// </summary>
        public override Expression RewriteExpression(Expression expression, out ExpressionVisitorBackpack backpack, out List<PropertyTupel> groupings, out int level)
        {
            backpack = new ExpressionVisitorBackpack(TypeMapper);
            var boundExp = PartialEvaluator.Eval(expression);

            boundExp = QueryBinder.Evaluate(boundExp, out groupings, backpack, out level);
            boundExp = MemberBinder.Evaluate(boundExp, backpack);

            //// move aggregate computations so they occur in same select as group-by
            //!!! NOT NEEDED ANYMORE !!! boundExp = AggregateRewriter.Rewrite(boundExp, dynamicCache);

            //// Bind Relationships ( means to solve access to class members, that means to insert a join if necessary)
            //!!! NOT NEEDED ANYMORE !!! boundExp = RelationshipBinder.Bind(boundExp, dynamicCache);

            //// These bundle of Rewriters are all used to get paging mechism in place
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);
            //!!! OBSOLETE HERE      !!! boundExp = RedundantSubqueryRemover.Remove(boundExp, dynamicCache);
            boundExp = OracleTakeToRowNumberRewriter.Rewrite(boundExp, backpack);
            boundExp = SkipToRowNumberRewriter.Rewrite(boundExp, backpack);
            
            //// At last, the correct alias can be set.
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);

            //// Now Check every OrderBy, and move them up into the sql stack, if necessary
            boundExp = OracleOrderByRewriter.Rewrite(boundExp, backpack);

            //// Now have a deep look to the Cross Apply Joins. Because perhaps they aren't valid anymore.
            //// This can be, due removal of selects and replacement with the native table expressions. A INNER JOIN / or CROSS JOIN
            //// is the result of that.
            boundExp = CrossApplyRewriter.Rewrite(boundExp, backpack);

            //// Attempt to rewrite cross joins as inner joins
            boundExp = RedundantSubqueryRemover.Remove(boundExp, backpack);
            boundExp = CrossJoinRewriter.Rewrite(boundExp, backpack);

            ///// Remove unused columns
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);
            //!!! NOT NEEDED ANYMORE !!! boundExp = UnusedColumnRemover.Rewrite(boundExp, dynamicCache);

            //// Do Final
            //!!! OBSOLETE HERE      !!! boundExp = RedundantSubqueryRemover.Remove(boundExp, dynamicCache );
            boundExp = RedundantSubqueryRemover.Remove(boundExp, backpack);
            boundExp = RedundantJoinRemover.Remove(boundExp, backpack);
            boundExp = SqlIdRewriter.Rewrite(boundExp, backpack);
            boundExp = AliasReWriter.Rewrite(boundExp, backpack);

            boundExp = UpdateProjection.Rebind(boundExp, backpack);

            return boundExp;
        }

        /// <summary>
        /// Counts number of rows that matches the whereclause
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <returns>Number of rows</returns>
        public override int Count(ProjectionClass projection, ICondition whereClause,
                                 Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter)
        {
            int numberOfRows = 0;
            int index = 1;
            IDictionary virtualAlias = new HybridDictionary();

            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            try
            {
                string grouping = projection.GetGrouping();

                string hint = PrivateHintClause(projection, whereClause, command.Parameters, null, null, virtualAlias,
                                                ref index);

                string withClause = PrivateWithClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias,
                                                      ref index);
                string tables = PrivateFromClause(projection, whereClause, command.Parameters, fieldTemplates,
                                                  globalParameter, virtualAlias, ref index);
                string query = string.Concat(withClause, "SELECT ", hint, " COUNT(",
                                             string.IsNullOrEmpty(grouping) ? "*" : "count(*)",
                                             ") FROM ", tables,
                                             PrivateCompleteWhereClause(projection, null, whereClause, globalParameter,
                                                                        virtualAlias, command.Parameters, ref index));

                if (!string.IsNullOrEmpty(grouping))
                    query = string.Concat(query, " GROUP BY ", grouping);

                query += PrivateCompleteHavingClause(projection, fieldTemplates, whereClause, globalParameter,
                                                     virtualAlias,
                                                     command.Parameters, ref index);

                command.CommandText = query;

                IDataReader reader = ExecuteReader(command);
                try
                {
                    if (reader.Read())
                        numberOfRows = (int)ConvertSourceToTargetType(reader.GetValue(0), typeof(Int32));

                    return numberOfRows;
                }
                finally
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            finally
            {
                stopwatch.Stop(command, CreateSql(command), numberOfRows);
                command.DisposeSafe();
            }
        }

        /// <summary>
        /// Selects the specified projection.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="additonalColumns">The additonal columns.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <param name="distinct">if set to <c>true</c> [distinct].</param>
        /// <returns></returns>
        protected override List<PersistentProperties> Select(ProjectionClass projection, string additonalColumns, ICondition whereClause,
                                       OrderBy orderBy, Dictionary<string, FieldDescription> fieldTemplates,
                                       IDictionary globalParameter, bool distinct)
        {
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            int rows = 0;
            try
            {
                int index = 1;
                IDictionary virtualAlias = new HybridDictionary();
                string withClause = PrivateWithClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias,
                                                      ref index);

                string hint = PrivateHintClause(projection, whereClause, command.Parameters, null, null, virtualAlias,
                                                ref index);

                string fromClause = PrivateFromClause(projection, whereClause, command.Parameters, fieldTemplates,
                                                      globalParameter, virtualAlias, ref index);
                string virtualFields = BuildVirtualFields(fieldTemplates, globalParameter, virtualAlias);
                string selectFunctions = BuildSelectFunctionFields(fieldTemplates, globalParameter);

                /*
                 * SQL Bauen
                 */

                String query = string.Concat(withClause
                                             ,
                                             distinct
                                                 ? string.Concat("SELECT ", hint, " DISTINCT ")
                                                 : string.Concat("SELECT ", hint),
                                             projection.GetColumns(whereClause, additonalColumns), " "
                                             , virtualFields
                                             , selectFunctions
                                             , BuildJoinFields(whereClause)
                                             , " FROM "
                                             , fromClause);

                /*
                 * Query bauen
                 */
                query += PrivateCompleteWhereClause(projection, fieldTemplates, whereClause, globalParameter,
                                                    virtualAlias,
                                                    command.Parameters, ref index);

                string grouping = projection.GetGrouping();
                if (!string.IsNullOrEmpty(grouping))
                    query = string.Concat(query, " GROUP BY ", grouping);

                query += PrivateCompleteHavingClause(projection, fieldTemplates, whereClause, globalParameter,
                                                     virtualAlias,
                                                     command.Parameters, ref index);
                query += (orderBy != null ? string.Concat(" ORDER BY ", orderBy.Columns, " ", orderBy.Ordering) : "");

                /*
                 * Die IDs selektieren und Objekt laden
                 */
                command.CommandText = query;

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
        /// Retuns a list with primary keys that matches the search criteria.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="primaryKeyColumn">The primary key column.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="orderBy">Order clause to order the selection.</param>
        /// <returns>Returns a list with IDs.</returns>
        public override IList SelectIDs(ProjectionClass projection, string primaryKeyColumn, ICondition whereClause,
                                       OrderBy orderBy)
        {
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            int rows = 0;
            try
            {
                IDictionary virtualAlias = new HybridDictionary();

                int index = 1;
                string withClause = PrivateWithClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias,
                                                      ref index);
                string hint = PrivateHintClause(projection, whereClause, command.Parameters, null, null, virtualAlias,
                                                ref index);
                string fromClause = PrivateFromClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias,
                                                      ref index);

                // WorkItem 64803: Fix the primary id column name. If a with clause is used, the Schema name must not be taken
                var idColumns = string.IsNullOrEmpty(withClause)
                                    ? projection.PrimaryKeyColumns
                                    : string.Concat(Condition.QUOTE_OPEN, projection.TableName(DatabaseType.Oracle),
                                                    Condition.QUOTE_CLOSE, ".",
                                                    Condition.QUOTE_OPEN, projection.GetPrimaryKeyDescription().Name,
                                                    Condition.QUOTE_CLOSE);

                string query = string.Concat(withClause, "SELECT ", hint, idColumns, " FROM ", fromClause);

                /*
                 * Query bauen
                 */
                query += PrivateCompleteWhereClause(projection, null, whereClause, null, virtualAlias,
                                                    command.Parameters,
                                                    ref index);

                string grouping = projection.GetGrouping();
                if (!string.IsNullOrEmpty(grouping))
                    query = string.Concat(query, " GROUP BY ", grouping);

                query += PrivateCompleteHavingClause(projection, null, whereClause, null, virtualAlias,
                                                     command.Parameters,
                                                     ref index);
                query += (orderBy != null ? string.Concat(" ORDER BY ", orderBy.Columns, " ", orderBy.Ordering) : "");

                /*
                 * Die IDs selektieren und Objekt laden
                 */
                command.CommandText = query;

                var ids = new ArrayList();
                IDataReader reader = ExecuteReader(command);
                try
                {
                    while (reader.Read())
                        ids.Add(ConvertSourceToTargetType(reader.GetValue(0), typeof(Guid)));

                    rows = ids.Count;
                    return ids;
                }
                finally
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            finally
            {
                stopwatch.Stop(command, CreateSql(command), rows);
                command.DisposeSafe();
            }
        }
    }

}