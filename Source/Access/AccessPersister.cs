using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Language;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Queries;
using AdFactum.Data.SqlServer;

namespace AdFactum.Data.Access
{
    /// <summary>
    /// Defines a persister for a Microsoft Access Database
    /// </summary>
    [Serializable]
    public class AccessPersister : MicrosoftBasedPersister
    {
        #region Private Const Declarations

        /// <summary>
        /// Connection string to a Microsoft Access Database
        /// </summary>
        private const string CONNECTION_STRING = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}; Jet OLEDB:Database Password={1};";
        private const string CONNECTION_STRING_OPWD = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0};";

        private const int UPDATED_RESTRICTED = -72156238;

        #endregion

        private string databaseFile;

        /// <summary>
        /// File of Access Database
        /// </summary>
        public string DatabaseFile
        {
            get { return databaseFile; }
        }

        /// <summary>
        /// Defines the join sytanx
        /// </summary>
        /// <value></value>
        protected override JoinSyntaxEnum JoinSyntax
        {
            get { return JoinSyntaxEnum.WhereClauseGlobalJoin; }
        }

        /// <summary>
        /// Gets the concatinator.
        /// </summary>
        /// <value>The concatinator.</value>
        public override string Concatinator
        {
            get { return " & "; }
        }

        /// <summary>
        /// Returns the type for the Linq Expression Writer
        /// </summary>
        public override Type LinqExpressionWriter
        {
            get { return typeof (AccessExpressionWriter); }
        }

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public AccessPersister()
        {
            TypeMapper = new AccessTypeMapper();
        }

        /// <summary>
        /// Constructor that opens an existing database by using the connection string
        /// </summary>
        public AccessPersister(string connectionString)
            : this()
        {
            Connect(connectionString);
        }

        /// <summary>
        /// Constructor that opens an existing database
        /// </summary>
        public AccessPersister(string pDatabaseFile, string pPassword)
            : this()
        {
            Connect(pDatabaseFile, pPassword);
        }

        /// <summary>
        /// Constructor that opens an existing database
        /// </summary>
        public AccessPersister(string pDatabaseFile, string pPassword, string additionalConnectionParameters)
            : this()
        {
            Connect(pDatabaseFile, pPassword, additionalConnectionParameters);
        }

        /// <summary>
        /// Constructor that opens an existing database
        /// </summary>
        public AccessPersister(string pDatabaseFile, ISqlTracer tracer)
            : this()
        {
            SqlTracer = tracer;
            Connect(pDatabaseFile);
        }

        /// <summary>
        /// Constructor that opens an existing database
        /// </summary>
        public AccessPersister(string pDatabaseFile, string pPassword, ISqlTracer tracer)
            : this()
        {
            SqlTracer = tracer;
            Connect(pDatabaseFile, pPassword);
        }

        /// <summary>
        /// Constructor that opens an existing database
        /// </summary>
        public AccessPersister(string pDatabaseFile, string pPassword, string additionalConnectionParameters,
                               ISqlTracer tracer)
            : this()
        {
            SqlTracer = tracer;
            Connect(pDatabaseFile, pPassword, additionalConnectionParameters);
        }

        /// <summary>
        /// Returns the Schema Writer
        /// </summary>
        /// <value></value>
        public override ISchemaWriter Schema
        {
            get
            {
                return new AccessSchemaWriter(TypeMapper, DatabaseSchema);
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
                return new AccessIntegrityChecker(this, TypeMapper, DatabaseSchema);
            }
        }

        /// <summary>
        /// Returns the repository class
        /// </summary>
        public override IRepository Repository
        {
            get
            {
                return new AccessRepository(SqlTracer);
            }
        }

        #endregion

        /// <summary>
        /// Connecting to an existing database
        /// </summary>
        /// <param name="pDatabaseFile">Database file name</param>
        /// <param name="password">Database password</param>
        /// <param name="additionalConnectionParameters">Additional connection parameters</param>
        public virtual void Connect(string pDatabaseFile, string password, string additionalConnectionParameters)
        {
            databaseFile = pDatabaseFile;
            string connectionString;

            if (!string.IsNullOrEmpty(password))
                connectionString = string.Format(CONNECTION_STRING, databaseFile, password) +
                                   additionalConnectionParameters;
            else
                connectionString = string.Format(CONNECTION_STRING_OPWD, databaseFile) + additionalConnectionParameters;

            Connect(connectionString);
        }

        /// <summary>
        /// Connecting to an existing database
        /// </summary>
        /// <param name="pDatabaseFile">Database file name</param>
        /// <param name="password">Database password</param>
        public virtual void Connect(string pDatabaseFile, string password)
        {
            Connect(pDatabaseFile, password, string.Empty);
        }

        /// <summary>
        /// Connecting to an existing database
        /// </summary>
        public virtual void Connect(string connectionString)
        {
            Debug.Assert(Connection == null, "The Connection has already established");
            Connection = new OleDbConnection {ConnectionString = connectionString};
            Connection.Open();

            if (SqlTracer != null)
                SqlTracer.OpenConnection(((OleDbConnection) Connection).ServerVersion, Connection.ConnectionString);
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
        public override List<PersistentProperties> Select(string tableName, string selectSql, SortedList selectParameter,
                                                          Dictionary<string, FieldDescription> fieldTemplates)
        {
            IDbCommand command = CreateCommand();

            try
            {
                /*
                 * Die IDs selektieren und Objekt laden
                 */
                string businessSql = selectSql;

                if (selectParameter != null)
                {
                    IDictionaryEnumerator enumerator = selectParameter.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        businessSql = businessSql.Replace((string)enumerator.Key, "?");
                        IDbDataParameter parameter =
                            CreateParameter(((string)enumerator.Key).Replace("@", string.Empty),
                                            enumerator.Value, false);
                        command.Parameters.Add(parameter);
                    }
                }

                command.CommandText = businessSql;

                List<PersistentProperties> result = PrivateSelect(command, fieldTemplates, 0, int.MaxValue);
                return result;
            }
            finally
            {
                command.DisposeSafe();
            }
        }

        /// <summary>
        /// Gets the virtual join STMT.
        /// </summary>
        /// <param name="tupel">The tupel.</param>
        /// <param name="joins">The joins.</param>
        /// <returns></returns>
        protected override string GetVirtualJoinPart(Set.Tupel tupel, List<string> joins)
        {
            var result = new StringBuilder();
            result.Append('(', joins.Count);
            result.Append(tupel.TupelString());

            foreach (string join in joins)
            {
                result.Append(join);
                result.Append(")");
            }
            return result.ToString();
        }

        /// <summary>
        /// Replaces the statics within a sql statement.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public override string ReplaceStatics(string sql)
        {
            return base.ReplaceStatics(sql
                .Replace(Condition.TRIM, "TRIM")
                .Replace(Condition.UPPER, "UCASE"));
        }

        /// <summary>
        /// Creates the SQL.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        protected override string CreateSql(IDbCommand command)
        {
            string sql = command.CommandText;

            int startPos = 0;
            IEnumerator enumParam = command.Parameters.GetEnumerator();

            while (enumParam.MoveNext())
            {
                var parameter = (IDbDataParameter) enumParam.Current;
                string paramValue = TypeMapper.GetParamValueAsSQLString(parameter.Value);

                /*
				 * First replace of keyword
				 */
                int index = sql.IndexOf("?", startPos);
                if (index >= 0)
                {
                    sql = sql.Remove(index, 1);
                    sql = sql.Insert(index, paramValue);
                }

                startPos = index + 1;
            }
            return sql;
        }

        /// <summary>
        /// Creates the command object.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public override IDbCommand CreateCommand(string sql)
        {
            var command = new OleDbCommand(sql, (OleDbConnection) Connection)
                              {Transaction = (OleDbTransaction) Transaction};

            return command;
        }

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <returns></returns>
        public override IDbCommand CreateCommand()
        {
            var command = new OleDbCommand
                              {
                                  Connection = (OleDbConnection) Connection,
                                  Transaction = (OleDbTransaction) Transaction
                              };

            return command;
        }

        /// <summary>
        /// Executes the secure db call.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="nonQuery">if set to <c>true</c> [non query].</param>
        /// <returns></returns>
        protected override object ExecuteSecureDbCall(IDbCommand command, bool nonQuery)
        {
            /*
             * Replace Static Placeholder
             */
            command.CommandText = ReplaceStatics(command.CommandText);

            /*
             * SQL ausführen und loggen
             */
            DateTime start = DateTime.Now;
            object result = null;
            int trys = 0;
            try
            {
                bool tryAgain;
                do
                {
                    tryAgain = false;

                    try
                    {
                        if (nonQuery) result = command.ExecuteNonQuery();
                        else result = command.ExecuteReader();
                    }
                    catch (OleDbException exc)
                    {
                        if (exc.Errors.Count > 0)
                        {
                            OleDbError err = exc.Errors[0];
                            tryAgain = (err.NativeError == UPDATED_RESTRICTED) && (trys++ < 10);

                            if (!tryAgain)
                                throw new SqlCoreException(exc.Errors[0].Message, exc.Errors[0].NativeError, CreateSql(command));
                        }
                        else
                            throw;
                    }
                } while (tryAgain);

                return result;
            }
            catch (SqlCoreException)
            {
                throw;
            }
            catch (DbException exc)
            {
                ErrorMessage(exc);
                throw new SqlCoreException(exc, exc.ErrorCode, CreateSql(command));
            }
            catch (Exception exc)
            {
                ErrorMessage(exc);
                throw new SqlCoreException(exc, 0, CreateSql(command));
            }
            finally
            {
                SqlOutput(command, 0, DateTime.Now.Subtract(start));
                command.Dispose();
            }
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        public override IDbDataParameter AddParameter(IDataParameterCollection parameters, ref int numberOfParameter,
                                                      Type type, object value, bool isUnicode)
        {
            IDbDataParameter parameter = new OleDbParameter(
                "?p" + numberOfParameter.ToString("00"), 
                (OleDbType) TypeMapper.GetEnumForDatabase(type, isUnicode))
                                             {
                                                 Value = TypeMapper.ConvertValueToDbType(value)
                                             };
            parameters.Add(parameter);

            numberOfParameter++;
            return parameter;
        }

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        public override IDbDataParameter CreateParameter(string parameterName, Type type, object value, bool isUnicode)
        {
            IDbDataParameter parameter = new OleDbParameter("?" + parameterName,
                                                            (OleDbType) TypeMapper.GetEnumForDatabase(type, isUnicode))
                                             {Value = TypeMapper.ConvertValueToDbType(value)};

            return parameter;
        }

        /// <summary>
        /// Creates the parameter from an existing parameter.
        /// </summary>
        /// <param name="copyFrom"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public override IDbDataParameter CreateParameter(IDbDataParameter copyFrom, object value)
        {
            var copy = (OleDbParameter) copyFrom;

            IDbDataParameter parameter = new OleDbParameter(copyFrom.ParameterName, copy.OleDbType)
                                             {Value = TypeMapper.ConvertValueToDbType(value)};

            return parameter;
        }


        /// <summary>
        /// Creates the data adapter.
        /// </summary>
        /// <returns></returns>
        protected override IDbDataAdapter CreateDataAdapter()
        {
            return new OleDbDataAdapter();
        }

        /// <summary>
        /// Gets the parameter string.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public override string GetParameterString(IDbDataParameter parameter)
        {
            return "?";
        }

        /// <summary>
        /// Rewrites the Linq Expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="backpack"></param>
        /// <param name="groupings"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public override System.Linq.Expressions.Expression RewriteExpression(System.Linq.Expressions.Expression expression, out ExpressionVisitorBackpack backpack, out List<AdFactum.Data.Linq.Expressions.PropertyTupel> groupings, out int level)
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
            boundExp = SkipToRowNumberRewriter.Rewrite(boundExp, backpack);

            //// At last, the correct alias can be set.
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);

            //// Now Check every OrderBy, and move them up into the sql stack, if necessary
            boundExp = SqlOrderByRewriter.Rewrite(boundExp, backpack);

            //// Now have a deep look to the Cross Apply Joins. Because perhaps they aren't valid anymore.
            //// This can be, due removal of selects and replacement with the native table expressions. A INNER JOIN / or CROSS JOIN
            //// is the result of that.
            boundExp = CrossApplyRewriter.Rewrite(boundExp, backpack);

            //// Attempt to rewrite cross joins as inner joins
            boundExp = RedundantSubqueryRemover.Remove(boundExp, backpack);
            boundExp = CrossJoinRewriter.Rewrite(boundExp, backpack);
            boundExp = SortAccessJoins.Sort(boundExp, backpack);

            ///// Remove unused columns
            //!!! OBSOLETE HERE      !!! boundExp = AliasReWriter.Rewrite(boundExp, dynamicCache);
            //!!! NOT NEEDED ANYMORE !!! boundExp = UnusedColumnRemover.Rewrite(boundExp, dynamicCache);

            //// Do Final
            //!!! OBSOLETE HERE      !!! boundExp = RedundantSubqueryRemover.Remove(boundExp, dynamicCache );
            boundExp = RedundantSubqueryRemover.Remove(boundExp, backpack);
            boundExp = RedundantJoinRemover.Remove(boundExp, backpack);
            boundExp = AliasReWriter.Rewrite(boundExp, backpack);

            boundExp = UpdateProjection.Rebind(boundExp, backpack);

            return boundExp;
        }

        #region Dispose Pattern

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// </summary>
        ~AccessPersister()
        {
            Dispose(false);
        }

        #endregion
    }
}