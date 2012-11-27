using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Language;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using Npgsql;
using NpgsqlTypes;

namespace AdFactum.Data.Postgres
{
    /// <summary>
    /// PostgresPersister
    /// </summary>
    public class PostgresPersister : BasePersister
    {
        /// <summary>
        /// Default Connection String
        /// </summary>
        public const string CONNECTION_STRING = "Server={0};Port={1};User Id={2};Password={3};Database={4};";

        /// <summary>
        /// Constructor
        /// </summary>
        public PostgresPersister()
        {
            TypeMapper = new PostgresTypeMapper();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString"></param>
        public PostgresPersister(string connectionString)
            :this()
        {
            Connect(connectionString);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PostgresPersister(string server, string port, string userId, string password, string database)
            :this()
        {
            Connect(server, port, userId, password, database);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PostgresPersister(string server, string userId, string password, string database)
            :this()
        {
            Connect(server, userId, password, database);
        }

        /// <summary>
        /// Connect to the database by using the standard port 5432
        /// </summary>
        public void Connect(string server, string userId, string password, string database)
        {
            Connect(server, "5432", userId, password, database);
        }

        /// <summary>
        /// Connect to the database with the given parameters
        /// </summary>
        public void Connect(string server, string port, string id, string password, string database)
        {
            string connectionString = string.Format(CONNECTION_STRING, server, port, id, password, database);
            Connect(connectionString);
        }

        /// <summary>
        /// Connect with the given connection string
        /// </summary>
        public void Connect(string connectionString)
        {
            Debug.Assert(Connection == null, "The Connection has already established");
            Connection = new NpgsqlConnection (connectionString);
            Connection.Open();

            if (SqlTracer != null)
                SqlTracer.OpenConnection(((NpgsqlConnection)Connection).ServerVersion, Connection.ConnectionString);
        }

        /// <summary>
        /// Gets or sets the SQL casing.
        /// </summary>
        /// <value>The SQL casing.</value>
        public SqlCasing SqlCasing
        {
            get { return TypeMapper.SqlCasing; }
            set
            {
                var ptm = TypeMapper as PostgresTypeMapper;
                if (ptm != null)
                    ptm.SetSqlCasing( value );
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

        /// <summary>
        /// Creates the command object.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public override IDbCommand CreateCommand(string sql)
        {
            return new NpgsqlCommand(sql, (NpgsqlConnection) Connection, (NpgsqlTransaction) Transaction);
        }

        /// <summary>
        /// Creates the data adapter.
        /// </summary>
        /// <returns></returns>
        protected override IDbDataAdapter CreateDataAdapter()
        {
            return new NpgsqlDataAdapter();
        }

        /// <summary>
        /// Return the last auto id
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        protected override int SelectLastAutoId(string tableName)
        {
            int autoId = -1;
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();
            command.CommandText = string.Concat("SELECT CURRVAL('", TypeMapper.Quote(TypeMapper.DoCasing(tableName+ "_seq")) ,"')");

            try
            {
                IDataReader reader = ExecuteReader(command);
                try
                {
                    if (reader.Read())
                    {
                        object lastId = reader.GetValue(0);
                        if (lastId != DBNull.Value)
                            autoId = (int) ConvertSourceToTargetType(reader.GetValue(0), typeof (Int32));
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

        /// <summary> Returns the Schema Writer </summary>
        public override ISchemaWriter Schema
        {
            get { return new PostgresSchemaWriter(TypeMapper, DatabaseSchema); }
        }

        /// <summary>
        /// Returns the repository class
        /// </summary>
        public override IRepository Repository
        {
            get { return new PostgresRepository(SqlTracer);  }
        }

        /// <summary>
        /// Returns the Integrity Checker
        /// </summary>
        public override IIntegrity Integrity
        {
            get { return new PostgresIntegrityChecker(this, TypeMapper, DatabaseSchema); }
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
        /// Defines the join sytanx
        /// </summary>
        protected override JoinSyntaxEnum JoinSyntax
        {
            get { return JoinSyntaxEnum.FromClauseGlobalJoin; }
        }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <returns></returns>
        public override IDbCommand CreateCommand()
        {
            var command = new NpgsqlCommand
                              {
                                  Connection = (NpgsqlConnection) Connection,
                                  Transaction = (NpgsqlTransaction) Transaction 
                              };
            return command;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        public override IDbDataParameter AddParameter(IDataParameterCollection parameters, ref int numberOfParameter, Type type, object value, bool isUnicode)
        {
            var buffer = value as byte[];
            object convertedValue = null;
            var dbType = (NpgsqlDbType)TypeMapper.GetEnumForDatabase(type, isUnicode);

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
                    var current = (NpgsqlParameter)parameterEnum.Current;

                    var secondAsByteArray = testValue as byte[];
                    var firstAsByteArray = current.Value as byte[];

                    if ((current.Value.Equals(testValue))
                      || (secondAsByteArray != null && firstAsByteArray != null
                       && firstAsByteArray.SequenceEqual(secondAsByteArray)))
                    {
                        IDbDataParameter copyParameter = new NpgsqlParameter(current.ParameterName, current.NpgsqlDbType);
                        copyParameter.Value = current.Value;
                        parameters.Add(copyParameter);
                        return copyParameter;
                    }
                }
            }
            else
                convertedValue = buffer;

            var parameter = new NpgsqlParameter(":p" + numberOfParameter.ToString("00"), dbType)
                                {
                                    Value = convertedValue,
                                    Direction = ParameterDirection.Input
                                };
            parameters.Add(parameter);
            numberOfParameter++;

            return parameter;
        }


        /// <summary>
        /// Gets the parameter string.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override string GetParameterString(IDbDataParameter parameter)
        {
            if (parameter.Value != null && parameter.Value.GetType().IsEnum)
            {
                string ps = "CAST(" + parameter.ParameterName + " AS " +
                            TypeMapper.Quote(parameter.Value.GetType().Name) + ")";

                // Convert enum to a string
                parameter.Value = parameter.Value.ToString();
                return ps;
            }
            else
                return parameter.ParameterName;
        }

        /// <summary>
        /// Replaces the statics within a sql statement.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public override string ReplaceStatics(string sql)
        {
            sql = sql.Replace(TypeMapper.DoCasing(Condition.SCHEMA_REPLACE), ConcatedSchema);
            sql = base.ReplaceStatics(sql
                      .Replace(Condition.TRIM, "TRIM")
                      .Replace(Condition.UPPER, "UPPER"));

            sql = sql.Trim();
            if (sql.EndsWith(";") && !sql.ToUpper(CultureInfo.InvariantCulture).EndsWith("END;"))
                sql = sql.Substring(0, sql.Length - 1);

            return sql;
        }

        /// <summary>
        /// Creates the parameter from an existing parameter.
        /// </summary>
        public override IDbDataParameter CreateParameter(IDbDataParameter copyFrom, object value)
        {
            var op = (NpgsqlParameter)copyFrom;

            string parameterName = copyFrom.ParameterName;
            if (!parameterName.StartsWith(":"))
                parameterName = string.Concat(":", parameterName);

            var parameter = new NpgsqlParameter(parameterName, op.NpgsqlDbType)
                                {
                                    Value = TypeMapper.ConvertValueToDbType(value),
                                    Direction = ParameterDirection.Input
                                };

            return parameter;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        public override IDbDataParameter CreateParameter(string parameterName, Type type, object value, bool isUnicode)
        {
            IDbDataParameter parameter = new NpgsqlParameter(":" + parameterName, (NpgsqlDbType)TypeMapper.GetEnumForDatabase(type, isUnicode))
            {
                Value = TypeMapper.ConvertValueToDbType(value),
                Direction = ParameterDirection.Input
            };

            return parameter;
        }

        /// <summary>
        /// Returns the Expression Writer
        /// </summary>
        public override Type LinqExpressionWriter
        {
            get
            {
                return typeof (PostgresExpressionWriter);
            }
        }

        /// <summary>
        /// Rewrites the LINQ Expressions
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="backpack"></param>
        /// <param name="groupings"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public override Expression RewriteExpression(Expression expression, out ExpressionVisitorBackpack backpack, out List<AdFactum.Data.Linq.Expressions.PropertyTupel> groupings, out int level)
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
            //boundExp = SkipToRowNumberRewriter.Rewrite(boundExp, dynamicCache);

            //// At last, the correct alias can be set.
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);

            //// Now Check every OrderBy, and move them up into the sql stack, if necessary
            boundExp = PostgresOrderByRewriter.Rewrite(boundExp, backpack);

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
        /// Pages the select.
        /// </summary>
        protected override List<PersistentProperties> PageSelect(ProjectionClass projection, string additionalColumns, ICondition whereClause,
                                           OrderBy orderBy, int minLine, int maxLine,
                                           Dictionary<string, FieldDescription> fieldTemplates,
                                           IDictionary globalParameter, bool distinct)
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
                string tables = PrivateFromClause(projection, whereClause, command.Parameters, fieldTemplates,
                                                  globalParameter, virtualAlias, ref index);

                /*
                 * SQL Bauen
                 */
                String query = string.Concat(withClause, distinct ? "SELECT DISTINCT " : "SELECT ",
                                             projection.GetColumns(whereClause, additionalColumns), " "
                                             , BuildVirtualFields(fieldTemplates, globalParameter, virtualAlias)
                                             , BuildSelectFunctionFields(fieldTemplates, globalParameter)
                                             , " FROM " + tables);

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
                query += (orderBy != null ? " ORDER BY " + orderBy.Columns + " " + orderBy.Ordering : "");

                if (minLine > 0)
                    query += " OFFSET " + (minLine - 1);

                if (maxLine < int.MaxValue)
                    query += " LIMIT " + (maxLine - minLine + 1);

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
    }
}
