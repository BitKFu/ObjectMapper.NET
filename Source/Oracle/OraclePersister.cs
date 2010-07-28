using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Language;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;

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

        /// <summary> Returns the Schema Writer </summary>
        public override ISchemaWriter Schema
        {
            get
            {
                return new OracleSchemaWriter(TypeMapper, DatabaseSchema);
            }
        }

        public override IIntegrity Integrity
        {
            get 
            { 
                return new OracleIntegrityChecker(this, TypeMapper, DatabaseSchema);
            }
        }

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
            Connection.Open();

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
			IDbCommand command = CreateCommand();

			int index = 1;
			IDictionary virtualAlias = new HybridDictionary();

			/*
			 * Build outer tables
			 */
            int    rowHint = ( ((maxLine - minLine + 1)/15)+1)*15;

			/*
		 	 * Build inner tables
		 	 */
            string withClause = PrivateWithClause(projection, whereClause, command.Parameters, null, null, virtualAlias, ref index);
            string innerTableStr = PrivateFromClause(projection, whereClause, command.Parameters, fieldTemplates, globalParameter, virtualAlias, ref index);
            string innerWhere = PrivateCompleteWhereClause(projection, fieldTemplates, whereClause, globalParameter, virtualAlias, command.Parameters, ref index);

			string businessSql = string.Concat("SELECT ", projection.GetColumns(whereClause, null), " ", 
			                                   BuildVirtualFields(fieldTemplates, globalParameter, virtualAlias),
			                                   BuildSelectFunctionFields(fieldTemplates, globalParameter),
			                                   " FROM ", innerTableStr, innerWhere);

            string grouping = projection.GetGrouping();
            if (!string.IsNullOrEmpty(grouping))
                businessSql = string.Concat(businessSql, " GROUP BY ", grouping);

            businessSql += PrivateCompleteHavingClause(projection, fieldTemplates, whereClause, globalParameter, virtualAlias, command.Parameters, ref index);
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
            string outerSql = string.Concat(withClause, distinct ? "SELECT DISTINCT " : "SELECT "
                                            , " * "
                                            , " FROM "
                                            , " (SELECT /*+ FIRST_ROWS(", rowHint.ToString(), ")*/ IQ.*, ROWNUM AS Z_R_N FROM (", businessSql
                                            , ") IQ WHERE ROWNUM <= :maxLine) ", "PAGE", " WHERE Z_R_N >= :minLine ");

			IDbDataParameter parameter = CreateParameter("maxLine", maxLine, false);
			command.Parameters.Add(parameter);

			parameter = CreateParameter("minLine", minLine, false);
			command.Parameters.Add(parameter);

			command.CommandText = outerSql;

            List<PersistentProperties> result = PrivateSelect(command, fieldTemplates, 0, int.MaxValue);
			return result;
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
            if (sql.EndsWith(";") && !sql.ToUpper().EndsWith("END;"))
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
        /// <param name="value">The value.</param>
        /// <param name="isUnicode">if set to <c>true</c> [is unicode].</param>
        /// <returns></returns>
        public override IDbDataParameter AddParameter(IDataParameterCollection parameters, ref int numberOfParameter, Type type, object value, bool isUnicode)
		{
			const int BUFFER_SIZE = 2048;
            var buffer = value as byte[];
		    object convertedValue = null;
            var dbType = (OracleDbType)TypeMapper.GetEnumForDatabase(type, isUnicode);

            if (buffer == null)
            {
                /*
                 * Extract the value to test
                 */
                object testValue = convertedValue = TypeMapper.ConvertValue(value);

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
                parameter.Value = TypeMapper.ConvertValue(value);
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
                    target[x] = TypeMapper.ConvertValue(source[x]);

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
        public override IDbDataParameter CreateParameter(string parameterName, Type type, object value, bool isUnicode)
		{
            if (!type.IsListType())
            {
                IDbDataParameter parameter = new OracleParameter(":" + parameterName, (OracleDbType)TypeMapper.GetEnumForDatabase(type, isUnicode))
                                                 {
                                                     Value = TypeMapper.ConvertValue(value),
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
                var parameter = new OracleParameter(":" + parameterName, (OracleDbType)TypeMapper.GetEnumForDatabase(parameterType, isUnicode))
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
            IDbCommand command = CreateCommand();
            command.CommandText = string.Concat("SELECT ", tableName, "_SEQ.CURRVAL FROM DUAL");

            IDataReader reader = ExecuteReader(command);
            if (reader.Read())
            {
                object lastId = reader.GetValue(0);
                if (lastId != DBNull.Value)
                    autoId = (int)ConvertSourceToTargetType(reader.GetValue(0), typeof(Int32));
            }
            reader.Close();

            return autoId;
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
        public override Expression RewriteExpression(Expression expression, Cache<Type, ProjectionClass> dynamicCache, out List<PropertyTupel> groupings, out int level)
        {
            var boundExp = PartialEvaluator.Eval(expression);

            Dictionary<ParameterExpression, MappingStruct> mapping;

            boundExp = QueryBinder.Evaluate(boundExp, out groupings, dynamicCache, TypeMapper, out level, out mapping);
            boundExp = MemberBinder.Evaluate(boundExp, dynamicCache, TypeMapper, mapping);

            //// move aggregate computations so they occur in same select as group-by
            //!!! NOT NEEDED ANYMORE !!! boundExp = AggregateRewriter.Rewrite(boundExp, dynamicCache);

            //// Bind Relationships ( means to solve access to class members, that means to insert a join if necessary)
            //!!! NOT NEEDED ANYMORE !!! boundExp = RelationshipBinder.Bind(boundExp, dynamicCache);

            //// These bundle of Rewriters are all used to get paging mechism in place
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);
            //!!! OBSOLETE HERE      !!! boundExp = RedundantSubqueryRemover.Remove(boundExp, dynamicCache);
            boundExp = OracleTakeToRowNumberRewriter.Rewrite(boundExp, dynamicCache);
            boundExp = SkipToRowNumberRewriter.Rewrite(boundExp, dynamicCache);
            
            //// At last, the correct alias can be set.
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);

            //// Now Check every OrderBy, and move them up into the sql stack, if necessary
            //// Ater that, remove all redundant subqueries now. This is necessary, 
            //// because some sub selects may become unused after the orderBy ist pushed a level higer.
            boundExp = OracleOrderByRewriter.Rewrite(boundExp);

            //// Now have a deep look to the Cross Apply Joins. Because perhaps they aren't valid anymore.
            //// This can be, due removal of selects and replacement with the native table expressions. A INNER JOIN / or CROSS JOIN
            //// is the result of that.
            boundExp = CrossApplyRewriter.Rewrite(boundExp, dynamicCache);

            //// Attempt to rewrite cross joins as inner joins
            boundExp = RedundantSubqueryRemover.Remove(boundExp, dynamicCache);
            boundExp = CrossJoinRewriter.Rewrite(boundExp);

            ///// Remove unused columns
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);
            //!!! NOT NEEDED ANYMORE !!! boundExp = UnusedColumnRemover.Rewrite(boundExp, dynamicCache);

            //// Do Final
            //!!! OBSOLETE HERE      !!! boundExp = RedundantSubqueryRemover.Remove(boundExp, dynamicCache );
            boundExp = RedundantSubqueryRemover.Remove(boundExp, dynamicCache);
            boundExp = RedundantJoinRemover.Remove(boundExp);
            boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);

            boundExp = UpdateProjection.Rebind(boundExp, dynamicCache);

            return boundExp;
        }
    }

}