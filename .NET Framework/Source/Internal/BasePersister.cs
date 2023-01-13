using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Fields;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Language;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Queries;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;

namespace AdFactum.Data.Internal
{
    /// <summary>
    /// This is a base persister class which contains the common lines of code that matches for all persisters.
    /// </summary>
    public abstract class BasePersister : INativePersister, IPersister, ILinqPersister
    {
        #region Join Syntax

        /// <summary>
        /// Defines the join syntax
        /// </summary>
        public enum JoinSyntaxEnum
        {
            /// <summary>
            /// Globals Joins are stored within the FROM clause
            /// </summary>
            FromClauseGlobalJoin,

            /// <summary>
            /// Globals Joins are stored within the WHERE clause
            /// </summary>
            WhereClauseGlobalJoin
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePersister"/> class.
        /// </summary>
        protected BasePersister()
        {
            ExportForeignKeyConstraints = true;
        }

        /// <summary>
        /// Database Connection
        /// </summary>
        public IDbConnection Connection { get; set; }

        /// <summary>
        /// Database Transaction
        /// </summary>
        public IDbTransaction Transaction { get; set; }

        /// <summary>
        /// Gets the concated schema.
        /// </summary>
        /// <value>The concated schema.</value>
        protected string ConcatedSchema
        {
            get
            {
                return !string.IsNullOrEmpty(DatabaseSchema) ? string.Concat(DatabaseSchema, ".") : "";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [export foreign key constraints].
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [export foreign key constraints]; otherwise, <c>false</c>.
        /// </value>
        public bool ExportForeignKeyConstraints { get; set; }

        #region ILinqPersister Members

        /// <summary>
        /// Returns the type of the used linq expression writer
        /// </summary>
        /// <value></value>
        public abstract Type LinqExpressionWriter { get; }

        /// <summary>
        /// Rewrites the expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="backpack"></param>
        /// <param name="groupings"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public abstract Expression RewriteExpression(Expression expression, out ExpressionVisitorBackpack backpack, out List<PropertyTupel> groupings, out int level);

        /// <summary>
        /// Type Mapping class
        /// </summary>
        /// <value></value>
        public ITypeMapper TypeMapper { get; protected set; }


        #endregion

        #region IPersister Members

        /// <summary>
        /// Sql Tracer
        /// </summary>
        public ISqlTracer SqlTracer { get; set; }

        /// <summary>
        /// Database Schema
        /// </summary>
        public string DatabaseSchema { get; set; }

        /// <summary> Returns the Schema Writer </summary>
        public abstract ISchemaWriter Schema
        {
            get;
        }

        /// <summary>
        /// Returns the repository class
        /// </summary>
        public abstract IRepository Repository
        {
            get;
        }

        /// <summary>
        /// Returns the Integrity Checker
        /// </summary>
        public abstract IIntegrity Integrity
        {
            get;
        }

        #endregion

        #endregion

        #region Core SQL methods

        /// <summary>
        /// Executes the query and returns a reader object
        /// </summary>
        /// <param name="command">Command to execute </param>
        /// <returns>Data Reader</returns>
        public IDataReader ExecuteReader(IDbCommand command)
        {
            /*
             * Execute secure call
             */
            var reader = (IDataReader)ExecuteSecureDbCall(command, false);
            return reader;
        }

        /// <summary>
        /// Executes the query and loggs the results
        /// </summary>
        /// <param name="command"></param>
        protected int ExecuteNonQuery(IDbCommand command)
        {
            /*
			 * Execute secure call
			 */
            var rows = (int)ExecuteSecureDbCall(command, true);
            return rows;
        }

        /// <summary>
        /// Tries to safely opens the connection
        /// </summary>
        protected virtual void SavelyOpenConnection()
        {
            if (Connection.State == ConnectionState.Open)
                return;

            bool retry;
            var tries = 0;
            do
            {
                Connection.Open();
                while (Connection.State == ConnectionState.Connecting)
                    Thread.Sleep(100);

                retry = (Connection.State != ConnectionState.Open && tries++ < 1);
                if (retry)
                    Connection.Close();
            } while (retry);
        }

        /// <summary>
        /// Executes the secure db call.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="nonQuery">if set to <c>true</c> [non query].</param>
        /// <returns></returns>
        protected virtual object ExecuteSecureDbCall(IDbCommand command, bool nonQuery)
        {
            command.CommandText = ReplaceStatics(command.CommandText);
            try
            {
                if (nonQuery) return command.ExecuteNonQuery();
                else return command.ExecuteReader();
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
        }

        /// <summary>
        /// Returns the columns of a oracle table
        /// </summary>
        public virtual void GetColumns(
            IDataReader reader,
            Dictionary<string, FieldDescription> fieldTemplates,

            out Dictionary<string, int> fieldIndexDict,
            out Dictionary<int, string> indexFieldDict)
        {
            int fields = reader.FieldCount;

            fieldIndexDict = new Dictionary<string, int>(fields);
            indexFieldDict = new Dictionary<int, string>(fields);

            for (int counter = 0; counter < fields; counter++)
            {
                string columnName = reader.GetName(counter);

                // Find the matching field
                if (fieldTemplates != null && !fieldTemplates.ContainsKey(columnName))
                {
                    string name = columnName;

                    columnName =

                        // Find simple column
                        fieldTemplates.Keys.Where(
                            key => name.Equals(key, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() ??

                        // Find simple column + Type Addition
                        fieldTemplates.Keys.Where(
                            key => name.Equals(key + DBConst.TypAddition, StringComparison.InvariantCultureIgnoreCase)).Select(key => key + DBConst.TypAddition)
                            .FirstOrDefault() ??

                        columnName;
                }

                // Add it to the result dictionary
                if (!fieldIndexDict.ContainsKey(columnName))
                    fieldIndexDict.Add(columnName, counter);

                indexFieldDict.Add(counter, columnName);
            }
        }



        #endregion

        #region Sql Tracer Methods

        /// <summary>
        /// Does an info message output to the tracer interface
        /// </summary>
        /// <param name="exc">Exception</param>
        protected void ErrorMessage(
            Exception exc
            )
        {
            if (SqlTracer == null)
                return;

            if (SqlTracer.TraceErrorEnabled)
                SqlTracer.ErrorMessage(exc.Message, exc.Source);
        }

        #endregion

        #region Sql Clause Helper

        #region Delegates

        /// <summary>
        /// Delegate for writing Where / Having Clauses
        /// </summary>
        public delegate string WriteConditionDelegate(
            ProjectionClass projection, ICondition constraintInterface, IDataParameterCollection parameterCollection,
            IDictionary globalParameter, ref int index, bool first);

        #endregion

        /// <summary>
        /// Gets the concatinator.
        /// </summary>
        /// <value>The concatinator.</value>
        public abstract string Concatinator { get; }

        /// <summary>
        /// Builds a join Qualifier
        /// </summary>
        /// <param name="virtualAlias"></param>
        /// <param name="tableName"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        protected static string BuildJoinQualifier(IDictionary virtualAlias, string tableName, FieldDescription field)
        {
            var vfd = field as VirtualFieldDescription;
            string result;

            if (vfd == null)
            {
                result =
                    string.Concat(Condition.QUOTE_OPEN, tableName, Condition.QUOTE_CLOSE, ".", Condition.QUOTE_OPEN,
                                  field.Name, Condition.QUOTE_CLOSE);
            }
            else
            {
                tableName = (string)virtualAlias[vfd.VirtualIdentifier];
                result =
                    string.Concat(tableName, ".", Condition.QUOTE_OPEN, vfd.ResultField.Name, Condition.QUOTE_CLOSE);
            }

            return result;
        }

        /// <summary>
        /// Builds the virtual from clause
        /// </summary>
        /// <param name="parentTableName">Name of the parent table.</param>
        /// <param name="fieldTemplates">field Templates</param>
        /// <param name="globalParameter">Load Parameter for global fields</param>
        /// <param name="virtualAlias">The virtual alias.</param>
        /// <param name="parameterCollection">The parameter collection.</param>
        /// <param name="index">The index.</param>
        /// <returns>result string</returns>
        protected virtual List<string> PrivateVirtualFromClause(string parentTableName,
                                                                Dictionary<string, FieldDescription> fieldTemplates,
                                                                IDictionary globalParameter, IDictionary virtualAlias,
                                                                IDataParameterCollection parameterCollection,
                                                                ref int index)
        {
            if (fieldTemplates == null)
                return new List<string>();

            var resultList = new List<string>();
            var vfdList = new List<VirtualFieldDescription>();

            int counter = 1;
            string identifier;

            /*
             * Build identifiers
             */
            foreach (var entry in fieldTemplates)
            {
                if (entry.Value is VirtualFieldDescription)
                {
                    var vfd = (VirtualFieldDescription)entry.Value;
                    if (virtualAlias.Contains(vfd.VirtualIdentifier))
                        continue;

                    Table currentTable = vfd.CurrentTable;
                    if (!currentTable.DefaultName.Equals(parentTableName))
                        continue;

                    /*
                     * Register the virtual table
                     */
                    identifier = string.Concat("v", counter);
                    virtualAlias.Add(vfd.VirtualIdentifier, identifier);
                    vfdList.Add(vfd);

                    counter++;
                }
            }

            /*
             * Build Joins
             */
            foreach (VirtualFieldDescription vfd in vfdList)
            {
                identifier = (string)virtualAlias[vfd.VirtualIdentifier];

                if (vfd.GlobalParameter.IsNotNullOrEmpty() &&
                    ((globalParameter == null) || (!globalParameter.Contains(vfd.GlobalParameter))))
                    continue;

                var join1 = new StringBuilder();
                StringBuilder join2 = null;

                if (!vfd.IsLinkTableUsed)
                {
                    /*
		             * Add the table
		             */
                    if (vfd.JoinTable.IsWeakReferenced)
                        join1.Append(" LEFT OUTER JOIN ");
                    else
                        join1.Append(" INNER JOIN ");


                    join1.Append(vfd.SyndicatedJoinTableName);
                    join1.Append(" ");
                    join1.Append(identifier);
                    join1.Append(" ON ");
                    join1.Append(BuildJoinQualifier(virtualAlias, vfd.CurrentTable.DefaultName, vfd.CurrentJoinField));
                    join1.Append(" = ");
                    join1.Append(BuildJoinQualifier(virtualAlias, identifier, vfd.TargetJoinField));
                }
                else
                {
                    join2 = new StringBuilder();

                    if (vfd.JoinTable.IsWeakReferenced)
                        join2.Append(" LEFT OUTER JOIN ");
                    else
                        join2.Append(" INNER JOIN ");

                    join2.Append(Condition.SCHEMA_REPLACE);
                    join2.Append(string.Concat(Condition.QUOTE_OPEN, vfd.LinkTable, Condition.QUOTE_CLOSE));
                    join2.Append(" ");
                    join2.Append(string.Concat(identifier, "lt"));
                    join2.Append(" ON ");
                    join2.Append(BuildJoinQualifier(virtualAlias, vfd.CurrentTable.DefaultName, vfd.CurrentJoinField));
                    join2.Append(" = ");
                    join2.Append(string.Concat(identifier, "lt.", Condition.QUOTE_OPEN, DBConst.PropertyField, Condition.QUOTE_CLOSE));

                    /*
                     * If there is a global parameter specified, than use an inner join
                     */
                    if (
                        // only use the inner join syntax if the JoinSyntax in From clause is choosen.
                        (JoinSyntax == JoinSyntaxEnum.FromClauseGlobalJoin)
                        // Furthermore a global parameter must be specified
                        && vfd.GlobalParameter.IsNotNullOrEmpty()
                        // if it's a primary field, it will be joined via LEFT OUTER with the parent object of the link table
                        && (vfd.GlobalJoinField.IsPrimary == false)
                        // only use the inner join if the global parameter has been found in the global parameter cache
                        && (globalParameter != null) && (globalParameter.Contains(vfd.GlobalParameter))
                        )
                    {
                        join1.Append(" INNER JOIN ");
                    }
                    else
                    {
                        /*
		                 * If not, so use the left outer join if it's a weak refernece
		                 */
                        if (vfd.JoinTable.IsWeakReferenced)
                            join1.Append(" LEFT OUTER JOIN ");
                        else
                            join1.Append(" INNER JOIN ");
                    }

                    join1.Append(vfd.SyndicatedJoinTableName);
                    join1.Append(" ");
                    join1.Append(identifier);
                    join1.Append(" ON ");
                    join1.Append(string.Concat(identifier, "lt.", Condition.QUOTE_OPEN, DBConst.ParentObjectField, Condition.QUOTE_CLOSE));
                    join1.Append(" = ");
                    join1.Append(string.Concat(identifier, ".", Condition.QUOTE_OPEN,
                                               GetPrimaryKeyColumn(fieldTemplates), Condition.QUOTE_CLOSE));
                }

                if (JoinSyntax == JoinSyntaxEnum.FromClauseGlobalJoin)
                {
                    /*
		             * With global language Parameter
		             */
                    if (vfd.GlobalParameter.IsNotNullOrEmpty())
                    {
                        if ((globalParameter == null) || (!globalParameter.Contains(vfd.GlobalParameter)))
                            continue;

                        join1.Append(" AND ");

                        object curValue = globalParameter[vfd.GlobalParameter];
                        if (TypeMapper.ConvertValueToDbType(curValue) != DBNull.Value)
                        {
                            IDbDataParameter parameter = AddParameter(parameterCollection, ref index, curValue,
                                                                      vfd.GlobalJoinField.CustomProperty.MetaInfo);
                            join1.Append(string.Concat(identifier, ".", Condition.QUOTE_OPEN, vfd.GlobalJoinField.Name,
                                                       Condition.QUOTE_CLOSE, "=", GetParameterString(parameter), " "));

                            /*
                             * If it's a join with the ID than add it to the linktable join too
                             */
                            if ((vfd.GlobalJoinField.IsPrimary) && (join2 != null))
                            {
                                parameter = AddParameter(parameterCollection, ref index, curValue, null);

                                join2.Append(" AND ");
                                join2.Append(string.Concat(identifier, "lt.", DBConst.ParentObjectField, "=",
                                                           GetParameterString(parameter), " "));
                            }
                        }
                        else
                        {
                            join1.Append(string.Concat(identifier, ".", Condition.QUOTE_OPEN, vfd.GlobalJoinField.Name,
                                                       Condition.QUOTE_CLOSE, " is null "));

                            /*
                             * If it's a join with the ID than add it to the linktable join too
                             */
                            if ((vfd.GlobalJoinField.IsPrimary) && (join2 != null))
                            {
                                join2.Append(" AND ");
                                join2.Append(string.Concat(identifier, "lt.", DBConst.ParentObjectField, " is null "));
                            }
                        }
                    }
                }

                if (join2 != null) resultList.Add(join2.ToString());
                resultList.Add(join1.ToString());
            }

            return resultList;
        }


        /// <summary>
        /// Builds the virtual from clause
        /// </summary>
        /// <param name="fieldTemplates">field Templates</param>
        /// <param name="globalParameter">Load Parameter for global fields</param>
        /// <param name="virtualAlias">The virtual alias.</param>
        /// <param name="parameterCollection">parameterCollection</param>
        /// <param name="index">The index.</param>
        /// <returns>result string</returns>
        protected virtual string PrivateVirtualWhereClause(Dictionary<string, FieldDescription> fieldTemplates,
                                                           IDictionary globalParameter, IDictionary virtualAlias,
                                                           IDataParameterCollection parameterCollection, ref int index)
        {
            if (JoinSyntax == JoinSyntaxEnum.FromClauseGlobalJoin)
                return "";

            var result = new StringBuilder("(");
            bool first = true;
            IDictionary uniqueList = new HybridDictionary();

            if (fieldTemplates != null)
            {
                foreach (var entry in fieldTemplates)
                {
                    var vfd = entry.Value as VirtualFieldDescription;
                    if ((vfd != null) && (virtualAlias.Contains(vfd.VirtualIdentifier)))
                    {
                        var identifier = (string)virtualAlias[vfd.VirtualIdentifier];

                        /*
                         * Build a unique List, because we only have to join once.
                         */
                        if (uniqueList.Contains(identifier))
                            continue;

                        /*
                         * With global language Parameter
                         */
                        if (vfd.GlobalParameter.IsNotNullOrEmpty())
                        {
                            if ((globalParameter == null) || (!globalParameter.Contains(vfd.GlobalParameter)))
                                continue;

                            if (!first) result.Append(" AND ");

                            object curValue = globalParameter[vfd.GlobalParameter];
                            if (TypeMapper.ConvertValueToDbType(curValue) != DBNull.Value)
                            {
                                if (vfd.JoinTable.IsWeakReferenced)
                                {
                                    result.Append(" (");
                                    IDbDataParameter parameter = AddParameter(parameterCollection, ref index, curValue,
                                                                              vfd.GlobalJoinField.CustomProperty.MetaInfo);

                                    result.Append(string.Concat(identifier, ".", Condition.QUOTE_OPEN,
                                                                vfd.GlobalJoinField.Name, Condition.QUOTE_CLOSE, "=",
                                                                GetParameterString(parameter)));
                                    result.Append(" OR ");
                                    result.Append(string.Concat(identifier, ".", Condition.QUOTE_OPEN,
                                                                vfd.GlobalJoinField.Name, Condition.QUOTE_CLOSE,
                                                                " is null"));
                                    result.Append(") ");
                                }
                                else
                                {
                                    IDbDataParameter parameter = AddParameter(parameterCollection, ref index, curValue,
                                                                              vfd.GlobalJoinField.CustomProperty.MetaInfo);
                                    result.Append(string.Concat(identifier, ".", Condition.QUOTE_OPEN,
                                                                vfd.GlobalJoinField.Name, Condition.QUOTE_CLOSE, "=",
                                                                GetParameterString(parameter)));
                                }
                            }
                            else
                                result.Append(string.Concat(identifier, ".", Condition.QUOTE_OPEN,
                                                            vfd.GlobalJoinField.Name, Condition.QUOTE_CLOSE, " is null"));

                            uniqueList.Add(identifier, null);
                            first = false;
                        }
                    }
                }
            }

            /*
			 * Result string abschliessen
			 */
            result.Append(")");
            if (result.ToString().Equals("()"))
                return "";

            return result.ToString();
        }

        /// <summary>
        /// Privates the with clause.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <param name="virtualAlias">The virtual alias.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        protected virtual string PrivateWithClause(ProjectionClass projection, ICondition whereClause,
                                                   IDataParameterCollection parameters,
                                                   Dictionary<string, FieldDescription> fieldTemplates,
                                                   IDictionary globalParameter, IDictionary virtualAlias, ref int index)
        {
            if (whereClause == null)
                return string.Empty;

            var result = new StringBuilder();
            var withCondition = whereClause as WithClause;

            // * Did we get a with clause condition? 
            if (withCondition != null)
            {
                string conditionString = withCondition.ConditionString;

                // Fix to solve values within a with condition or a from clause
                IList valueList = withCondition.Values;
                conditionString = ReplaceSqlValueParameters(valueList, conditionString, globalParameter, parameters,
                                                            ref index);

                ICondition[] additionals = withCondition.AdditionalConditions;
                for (int subCounter = 0; subCounter < additionals.Length; subCounter++)
                {
                    conditionString = conditionString.ReplaceFirst(Condition.NestedCondition,
                                            PrivateWhereClause(projection, additionals[subCounter], parameters,
                                                               globalParameter, ref index, subCounter == 0));
                }

                result.Append(conditionString);
            }

            // * Add Additional Additions            
            foreach (ICondition condition in whereClause.AdditionalConditions)
                result.Append(PrivateWithClause(projection, condition, parameters, fieldTemplates, globalParameter,
                                                virtualAlias, ref index));

            return result.ToString();
        }

        /// <summary>
        /// Privates the with clause.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="hintClause">The hint clause.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <param name="virtualAlias">The virtual alias.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        protected virtual string PrivateHintClause(ProjectionClass projection, ICondition hintClause,
                                                   IDataParameterCollection parameters,
                                                   Dictionary<string, FieldDescription> fieldTemplates,
                                                   IDictionary globalParameter, IDictionary virtualAlias, ref int index)
        {
            if (hintClause == null)
                return string.Empty;

            var result = new StringBuilder();
            var hintCondition = hintClause as HintCondition;

            // * Did we get a with clause condition? 
            if (hintCondition != null)
            {
                result.Append(hintCondition.ConditionString);
            }

            // * Add Additional Additions            
            foreach (ICondition condition in hintClause.AdditionalConditions)
                result.Append(PrivateHintClause(projection, condition, parameters, fieldTemplates, globalParameter,
                                                virtualAlias, ref index));

            return result.ToString();
        }

        /// <summary>
        /// ReplaceSqlValueParameters
        /// </summary>
        /// <param name="valueList"></param>
        /// <param name="conditionString"></param>
        /// <param name="globalParameter"></param>
        /// <param name="parameters"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private string ReplaceSqlValueParameters(IEnumerable valueList, string conditionString, IDictionary globalParameter,
                                                 IDataParameterCollection parameters, ref int index)
        {
            if (valueList == null) return conditionString;

            IEnumerator valueEnumerator = valueList.GetEnumerator();
            while (valueEnumerator.MoveNext())
            {
                object curValue = valueEnumerator.Current;

                if (conditionString.Contains(Condition.ParameterValue))
                {
                    /*
                     * Replace global Parameters
                     */
                    if ((globalParameter != null) && (globalParameter.Contains(curValue)))
                        curValue = globalParameter[curValue];

                    IDbDataParameter parameter = AddParameter(parameters, ref index, curValue, null);
                    conditionString = conditionString.ReplaceFirst(Condition.ParameterValue, GetParameterString(parameter));
                }
            }
            return conditionString;
        }

        /// <summary>
        /// This method builds new from clauses in order to make a table replacement
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="whereClause">Where Condition</param>
        /// <param name="parameters">Parameter Collection</param>
        /// <param name="fieldTemplates">Field Templates</param>
        /// <param name="globalParameter">Global Parameters</param>
        /// <param name="virtualAlias">The virtual alias.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        protected string PrivateFromClause(ProjectionClass projection, ICondition whereClause,
                                           IDataParameterCollection parameters,
                                           Dictionary<string, FieldDescription> fieldTemplates,
                                           IDictionary globalParameter, IDictionary virtualAlias, ref int index)
        {
            var result = new StringBuilder();
            var innerTables = new Set();
            bool first = true;

            innerTables.Merge(projection.Tables);

            if (whereClause != null)
                innerTables.Merge(whereClause.Tables);
            IEnumerator tableEnumerator = innerTables.GetEnumerator();

            while (tableEnumerator.MoveNext())
            {
                var tupel = (Set.Tupel)tableEnumerator.Current;
                if (tupel.Member != null)
                {
                    string overwrite = PrivateWhereClause(projection, tupel.Member, parameters, globalParameter,
                                                          ref index, true);
                    tupel.Overwrite = overwrite.IndexOf(' ') >= 0
                                          ? string.Concat("(", overwrite, ") ")
                                          : string.Concat(overwrite, " ");
                }

                if (!first) result.Append(", ");

                List<string> joins = PrivateVirtualFromClause(tupel.Key, fieldTemplates, globalParameter, virtualAlias,
                                                              parameters, ref index);
                string join = GetVirtualJoinPart(tupel, joins);

                result.Append(join);
                first = false;
            }

            // Special Handling for Oracle / postgres, if the Casing is lower or uppercase
            if (TypeMapper.SqlCasing != SqlCasing.Mixed)
            {
                var distinct = string.Join(", ", TypeMapper.DoCasing(result.ToString()).Split(',').ToList().Select(x => x.Trim()).Distinct().ToArray());
                return distinct + " ";
            }
            else
            {
                result.Append(" ");
                return result.ToString();
            }
        }

        /// <summary>
        /// Gets the virtual join STMT.
        /// </summary>
        /// <param name="tupel">The tupel.</param>
        /// <param name="joins">The joins.</param>
        /// <returns></returns>
        protected virtual string GetVirtualJoinPart(Set.Tupel tupel, List<string> joins)
        {
            string join = tupel.TupelString();

            foreach (string part in joins)
                join += part;
            return join;
        }

        /// <summary>
        /// Builds the having clause and returns the string
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="constraintInterface">The constraint interface.</param>
        /// <param name="parameterCollection">The parameter collection.</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <param name="index">The index.</param>
        /// <param name="first">if set to <c>true</c> [first].</param>
        /// <returns></returns>
        protected virtual string PrivateHavingClause(ProjectionClass projection, ICondition constraintInterface,
                                                     IDataParameterCollection parameterCollection,
                                                     IDictionary globalParameter, ref int index, bool first)
        {
            var condition = constraintInterface as Condition;

            if (constraintInterface == null
                || (constraintInterface.ConditionClause != ConditionClause.HavingClause
                    && constraintInterface.ConditionClause != ConditionClause.Undefined)
                )
            {
                if (condition == null ||
                    condition.GetContextDependentConditionClause(projection) != ConditionClause.HavingClause)
                    return string.Empty;
            }
            else if (condition != null &&
                     condition.GetContextDependentConditionClause(projection) != ConditionClause.HavingClause)
                return string.Empty;

            return PrivateCoreCondition(PrivateHavingClause, projection, constraintInterface, first, globalParameter,
                                        parameterCollection, ref index);
        }

        /// <summary>
        /// Builds a where Clause and returns the string
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="constraintInterface">Condition collection</param>
        /// <param name="parameterCollection">All parameters are added to the parameterCollection</param>
        /// <param name="globalParameter">Global Parameter for entity localization</param>
        /// <param name="index">index counter to prevent index duplicatino</param>
        /// <param name="first">if set to <c>true</c> [first].</param>
        /// <returns>Where clause represented as string</returns>
        protected virtual string PrivateWhereClause(ProjectionClass projection, ICondition constraintInterface,
                                                    IDataParameterCollection parameterCollection,
                                                    IDictionary globalParameter, ref int index, bool first)
        {
            var condition = constraintInterface as Condition;

            // A HintClause may contain other where clauses. That's why we have to dig deeper into the rabbit burrow
            if (constraintInterface != null && constraintInterface.ConditionClause == ConditionClause.HintClause)
            {
                var sb = new StringBuilder();

                // access addtional conditions
                bool innerFirst = first;
                foreach (var additionalCondition in constraintInterface.AdditionalConditions)
                {
                    sb.Append(PrivateWhereClause(projection, additionalCondition, parameterCollection,
                                                 globalParameter, ref index, innerFirst));
                    innerFirst = false;
                }

                return sb.ToString();
            }

            if (constraintInterface == null
                || (constraintInterface.ConditionClause != ConditionClause.WhereClause
                    && constraintInterface.ConditionClause != ConditionClause.Undefined))
            {
                if (condition == null ||
                    condition.GetContextDependentConditionClause(projection) != ConditionClause.WhereClause)
                    return string.Empty;
            }
            else if (condition != null &&
                     condition.GetContextDependentConditionClause(projection) != ConditionClause.WhereClause)
                return string.Empty;

            return PrivateCoreCondition(PrivateWhereClause, projection, constraintInterface, first, globalParameter,
                                        parameterCollection, ref index);
        }

        /// <summary>
        /// Privates the core condition.
        /// </summary>
        /// <param name="privateWriteCondition">The private write condition delegate.</param>
        /// <param name="projection">The projection.</param>
        /// <param name="constraintInterface">The constraint interface.</param>
        /// <param name="first">if set to <c>true</c> [first].</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <param name="parameterCollection">The parameter collection.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        protected virtual string PrivateCoreCondition(WriteConditionDelegate privateWriteCondition,
                                                      ProjectionClass projection, ICondition constraintInterface,
                                                      bool first, IDictionary globalParameter,
                                                      IDataParameterCollection parameterCollection, ref int index)
        {
            var condition = constraintInterface as Condition;

            string conditionString;

            /*
			 * Handelt es sich zum eine Abfrage?
			 */
            if (condition != null)
            {
                /*
                 * Get a context dependent condition string
                 */
                conditionString = condition.GetContextDependentConditionString(projection);

                if ((!first) && (conditionString.Length > 0))
                    conditionString = constraintInterface.Type.Operator() + conditionString;

                /*
                 * Evaluate condition
                 */
                IList valueList = condition.Values;
                if (valueList != null)
                {
                    IEnumerator valueEnumerator = valueList.GetEnumerator();
                    int valueIndex = -1;
                    while (valueEnumerator.MoveNext())
                    {
                        object curValue = valueEnumerator.Current;
                        valueIndex++;

                        if (conditionString.Contains(Condition.ParameterValue))
                        {
                            /*
                             * Replace global Parameters
                             */
                            if ((globalParameter != null) && (globalParameter.Contains(curValue)))
                                curValue = globalParameter[curValue];

                            /*
                             * Replace the placeholder for Oracle -> make '*' to '%'
                             */
                            string prefix = string.Empty;
                            if ((curValue is string)
                                && ((condition.CompareOperator == QueryOperator.Like)
                                    || (condition.CompareOperator == QueryOperator.Like_NoCaseSensitive)
                                    || (condition.CompareOperator == QueryOperator.NotLike)
                                    || (condition.CompareOperator == QueryOperator.NotLike_NoCaseSensitive))
                                )
                            {
                                var transform = (string)curValue;
                                transform = transform.Replace('*', '%');

                                /*
                                 * Special case to avoid an index search.
                                 */
                                string concat = Concatinator;
                                if (transform.StartsWith("%") && transform.Length > 1 && concat.Length > 0)
                                {
                                    prefix = string.Concat("'%'", concat);
                                    transform = transform.Substring(1);
                                }

                                curValue = transform;
                            }

                            /*
                             * set the bind Parameter
                             */
                            if (condition.GetUseBindParamter(valueIndex))
                            {
                                IDbDataParameter parameter = AddParameter(parameterCollection, ref index, curValue,
                                                                          condition.Field.FieldDescription.
                                                                              CustomProperty.MetaInfo);
                                conditionString = conditionString.ReplaceFirst(Condition.ParameterValue,
                                                        prefix + GetParameterString(parameter));
                            }
                            else
                                conditionString = conditionString.ReplaceFirst(Condition.ParameterValue,
                                                        prefix +
                                                        TypeMapper.GetParamValueAsSQLString(
                                                            TypeMapper.ConvertValueToDbType(curValue)));
                        }
                    }
                }

                if ((globalParameter != null) && (conditionString.Contains(Condition.GlobalJoin)))
                {
                    var vfd = (VirtualFieldDescription)condition.Field.FieldDescription;
                    object curValue = globalParameter[vfd.GlobalParameter];

                    if (condition.GetUseBindParamter(-1))
                    {
                        IDbDataParameter parameter = AddParameter(parameterCollection, ref index, curValue,
                                                                  condition.Field.FieldDescription.CustomProperty.MetaInfo);
                        conditionString = conditionString.ReplaceFirst(Condition.GlobalJoin, GetParameterString(parameter));
                    }
                    else
                        conditionString = conditionString.ReplaceFirst(Condition.GlobalJoin,
                                                TypeMapper.GetParamValueAsSQLString(TypeMapper.ConvertValueToDbType(curValue)));
                }

                var inCondition = condition as InCondition;
                if (inCondition != null)
                {
                    IList subSelects = inCondition.SubSelects;
                    if (subSelects != null)
                        for (int counter = 1; counter <= subSelects.Count; counter++)
                        {
                            var subSelect = (SubSelect)subSelects[counter - 1];
                            var subSelectProjection = new ProjectionClass(subSelect.ResultType);

                            ICondition[] additionals = subSelect.AdditionalConditions;
                            bool useWhere = false;
                            for (int subCounter = 0; subCounter < additionals.Length; subCounter++)
                            {
                                if (additionals[subCounter] is TableReplacement)
                                    conditionString = ReplaceSqlValueParameters(additionals[subCounter].Values,
                                                                                conditionString, globalParameter,
                                                                                parameterCollection, ref index);

                                string replaceWith = privateWriteCondition(subSelectProjection, additionals[subCounter],
                                                                           parameterCollection, globalParameter,
                                                                           ref index, subCounter == 0);

                                // At the last condition, add the grouping clause if necessary
                                if (subCounter == additionals.Length - 1)
                                {
                                    string grouping = subSelectProjection.GetGrouping();
                                    if (!string.IsNullOrEmpty(grouping))
                                        replaceWith = string.Concat(replaceWith, " GROUP BY ", grouping);
                                }

                                conditionString = conditionString.ReplaceFirst(Condition.NestedCondition, replaceWith);
                                if (replaceWith.Length > 0) useWhere = true;
                            }

                            conditionString = conditionString.ReplaceFirst(Condition.WhereCondition, useWhere ? "WHERE" : "");
                        }
                }
            }
            else
            {
                /*
                 * Den Condition string holen
                 */
                conditionString = constraintInterface.ConditionString;

                if ((!first) && (conditionString.Length > 0))
                    conditionString = constraintInterface.Type.Operator() + conditionString;
            }

            /*
			 * Does the string contains nested conditions ? 
			 */
            bool nextFirst = false;
            if (constraintInterface is ConditionList) nextFirst = true;
            if (constraintInterface is Parenthesize) nextFirst = true;
            if (constraintInterface is SubSelect) nextFirst = true;

            ICondition[] additional = constraintInterface.AdditionalConditions;
            for (int subCounter = 0; subCounter < additional.Length; subCounter++)
            {
                string innerWhere = privateWriteCondition(projection, additional[subCounter], parameterCollection,
                                                          globalParameter, ref index, nextFirst);
                conditionString = conditionString.ReplaceFirst(Condition.NestedCondition, innerWhere);

                if (nextFirst && (innerWhere != string.Empty) && (additional[subCounter].ConditionString.Length > 0))
                    nextFirst = false;
            }

            /*
			 * Do the base replacements
			 */
            return conditionString;
        }

        /// <summary>
        /// Returns the join field names to identify the link Ids
        /// </summary>
        /// <param name="clause"></param>
        /// <returns></returns>
        protected static string BuildJoinFields(ICondition clause)
        {
            if (clause == null)
                return "";

            var result = new StringBuilder();

            var parentCondition = clause as HashCondition;
            if (parentCondition != null)
                result.Append(parentCondition.JoinFieldForHashtable);

            foreach (ICondition condition in clause.AdditionalConditions)
                result.Append(BuildJoinFields(condition));

            return result.ToString();
        }

        /// <summary>
        /// Builds the virtual field query
        /// </summary>
        /// <param name="fieldTemplates">field Templates</param>
        /// <param name="globalParameter">Load Parameter for global fields</param>
        /// <param name="virtualAlias">The virtual alias.</param>
        /// <returns>result string</returns>
        protected static string BuildVirtualFields(Dictionary<string, FieldDescription> fieldTemplates,
                                                   IDictionary globalParameter, IDictionary virtualAlias)
        {
            var result = new StringBuilder();

            foreach (var entry in fieldTemplates)
            {
                var virtualField = entry.Value as VirtualFieldDescription;
                if (virtualField != null)
                {
                    if (virtualField.GlobalParameter.IsNotNullOrEmpty() &&
                        ((globalParameter == null) || (!globalParameter.Contains(virtualField.GlobalParameter))))
                        continue;

                    var identifier = (string)virtualAlias[virtualField.VirtualIdentifier];
                    result.Append(string.Concat(", ", identifier, ".", Condition.QUOTE_OPEN,
                                                virtualField.ResultField.Name, Condition.QUOTE_CLOSE, " as ",
                                                virtualField.Name));
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Builds the select function fields.
        /// </summary>
        /// <returns></returns>
        protected string BuildSelectFunctionFields(Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter)
        {
            var result = new StringBuilder();

            foreach (var entry in fieldTemplates)
            {
                FieldDescription field = entry.Value;
                if ((field != null) && (field.CustomProperty.MetaInfo.SelectFunction.IsNotNullOrEmpty()))
                {
                    string selectFunction = field.CustomProperty.MetaInfo.SelectFunction;
                    string identifier = field.Name;

                    if (globalParameter != null)
                    {
                        foreach (DictionaryEntry parameter in globalParameter)
                            selectFunction = selectFunction.Replace((string)parameter.Key, TypeMapper.GetParamValueAsSQLString(parameter.Value));
                    }

                    result.Append(string.Concat(", ", selectFunction, " as ", identifier));
                }
            }

            return result.ToString();
        }

        #endregion

        /// <summary>
        /// Defines the join sytanx
        /// </summary>
        protected abstract JoinSyntaxEnum JoinSyntax { get; }

        #region INativePersister Members

        /// <summary>
        /// Creates the command.
        /// </summary>
        /// <returns></returns>
        public abstract IDbCommand CreateCommand();

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        public abstract IDbDataParameter AddParameter(IDataParameterCollection parameters, ref int numberOfParameter,
                                                      Type type, object value, PropertyMetaInfo metaInfo);

        /// <summary>
        /// Creates an parameter
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="numberOfParameter"></param>
        /// <param name="value"></param>
        /// <param name="metaInfo"></param>
        /// <returns></returns>
        public virtual IDbDataParameter AddParameter(IDataParameterCollection parameters, ref int numberOfParameter,
                                                      object value, PropertyMetaInfo metaInfo)
        {
            return AddParameter(parameters, ref numberOfParameter, value.GetType(), value, metaInfo);
        }

        /// <summary>
        /// Creates the parameter from an existing parameter.
        /// </summary>
        public abstract IDbDataParameter CreateParameter(IDbDataParameter copyFrom, object value);

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        public abstract IDbDataParameter CreateParameter(string parameterName, Type type, object value, PropertyMetaInfo metaInfo);

        /// <summary>
        /// Creates the parameter.
        /// </summary>
        public IDbDataParameter CreateParameter(string parameterName, object value, PropertyMetaInfo metaInfo)
        {
            return CreateParameter(parameterName, value.GetType(), value, metaInfo);
        }

        /// <summary>
        /// Gets the parameter string.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public abstract string GetParameterString(IDbDataParameter parameter);

        static readonly Regex identifierSearch = new Regex(@"\[\-(.*?)-\]", RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex quoteSearch = new Regex(@"\'([^\']*)\'", RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Replaces the statics within a sql statement.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public virtual string ReplaceStatics(string sql)
        {
            var quotedStrings = quoteSearch.Matches(sql);

            // Do the Schema Replace
            sql = sql.Replace(Condition.SCHEMA_REPLACE, ConcatedSchema);

            // Quote the identifier
            sql = identifierSearch.Replace(sql, (match) => TypeMapper.Quote(match.Groups[1].Value));

            // Do casing to the complete string
            sql = TypeMapper.DoCasing(sql);

            // Search Again for the quoted strings and replace them with the original one
            var quoteEnumerator = quotedStrings.GetEnumerator();
            sql = quoteSearch.Replace(sql, match =>
                                               {
                                                   quoteEnumerator.MoveNext();
                                                   return ((Match)quoteEnumerator.Current).Value;
                                               });

            return sql;
        }

        #endregion

        #region Transaction Handling

        /// <summary>
        /// Starts a transaction
        /// </summary>
        public virtual void BeginTransaction()
        {
            if (SqlTracer != null) SqlTracer.BeginTransaction();
            Transaction = Connection.BeginTransaction();
        }

        /// <summary>
        /// Commits a transaction
        /// </summary>
        public virtual void Commit()
        {
            if (SqlTracer != null) SqlTracer.Commit();
            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
        }

        /// <summary>
        /// Rollback the changes, if no commit has been done.
        /// </summary>
        public virtual void Rollback()
        {
            if (SqlTracer != null) SqlTracer.Rollback();
            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
        }

        #endregion

        #region INativePersister Member

        /// <summary>
        /// Fills an data table with an Oracle selection
        /// </summary>
        /// <param name="sqlSelect">sql command</param>
        /// <returns>Filled data table</returns>
        public DataTable FillTable(string sqlSelect)
        {
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            var dataSet = new DataSet("TableX");
            IDbDataAdapter adapter = CreateDataAdapter();
            var rows = 0;

            try
            {
                adapter.SelectCommand = CreateCommand(sqlSelect);
                rows = adapter.Fill(dataSet);

                return dataSet.Tables["Table"];
            }
            finally
            {
                stopwatch.Stop(adapter.SelectCommand, CreateSql(adapter.SelectCommand), rows);
                adapter.SelectCommand.DisposeSafe();
            }
        }

        /// <summary>
        /// Fills the table.
        /// </summary>
        /// <param name="sqlCommand">The SQL command.</param>
        /// <returns></returns>
        public DataTable FillTable(IDbCommand sqlCommand)
        {
            var inserted = 0;

            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            try
            {
                var dataSet = new DataSet("TableX");
                IDbDataAdapter adapter = CreateDataAdapter();

                adapter.SelectCommand = sqlCommand;
                inserted = adapter.Fill(dataSet);

                return dataSet.Tables["Table"];
            }
            finally
            {
                stopwatch.Stop(sqlCommand, CreateSql(sqlCommand), inserted);
            }
        }

        /// <summary>
        /// Executes an Oracle statement
        /// </summary>
        /// <param name="execSql">sql statement</param>
        /// <returns>Affacted rows</returns>
        public int Execute(string execSql)
        {
            IDbCommand command = CreateCommand(execSql);

            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            int count = 1;
            try
            {
                count = ExecuteNonQuery(command);
                return count;
            }
            finally
            {
                stopwatch.Stop(command, CreateSql(command), count);
                command.DisposeSafe();
            }
        }

        /// <summary>
        /// Executes a Oracle statement and replaces the parameter
        /// </summary>
        /// <param name="execSql">sql statement</param>
        /// <param name="parameter">native Oracle parameter</param>
        /// <returns>Affacted rows</returns>
        public int ExecuteWithParameter(string execSql, params object[] parameter)
        {
            IDbCommand command = CreateCommand(execSql);
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);

            int count = 1;
            try
            {
                
                foreach (object curValue in parameter)
                {
                    AddParameter(command.Parameters, ref count, curValue, null);
                }

                count = ExecuteNonQuery(command);
                return count;
            }
            finally
            {
                stopwatch.Stop(command, CreateSql(command), count);
                command.DisposeSafe();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates the name of the child table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="child">The child.</param>
        /// <returns></returns>
        protected static string CreateChildTableName(string tableName, string child)
        {
            return string.Concat(tableName, "_", child);
        }

        /// <summary>
        /// Privates the select.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="startRow">The start row.</param>
        /// <param name="endRow">The end row.</param>
        /// <returns></returns>
        protected virtual List<PersistentProperties> PrivateSelect(IDbCommand command, Dictionary<string, FieldDescription> fieldTemplates, int startRow, int endRow)
        {
            var reader = ExecuteReader(command);
            var result = new List<PersistentProperties>();
            IList ids = new ArrayList();

            try
            {
                /*
                 * Zeilenhash aufbauen
                 */
                Dictionary<string, int> fieldIndexDict;
                Dictionary<int, string> indexFieldDict;
                GetColumns(reader, fieldTemplates, out fieldIndexDict, out indexFieldDict);

                /*
                 * Because Paging is available on every Database, we sometimes have to step forward to the first result
                 */
                int rowCounter = 0;
                while (++rowCounter < startRow)
                    if (!reader.Read()) break;

                // n. Zeile laden
                while (reader.Read() && (rowCounter++ <= endRow))
                {
                    var resultFields = new PersistentProperties();

                    /*
                     * Die Felder durchlaufen
                     */
                    int index;

                    for (index = 0; index < reader.FieldCount; index++)
                    {
                        string column = indexFieldDict[index];
                        if (resultFields.FieldProperties.Contains(column))
                            continue;

                        FieldDescription fieldDescription;
                        if (fieldTemplates.ContainsKey(column))
                            fieldDescription = fieldTemplates[column];
                        else
                            fieldDescription = null;

                        /*
                         * Null Fieldvalue?
                         */
                        object persistField;
                        if (reader.IsDBNull(index))
                            persistField = null;
                        else if (fieldDescription != null)
                            persistField = ConvertSourceToTargetType(reader.GetValue(index), fieldDescription.ContentType);
                        else
                            persistField = reader.GetValue(index);

                        if (fieldDescription != null)
                        {
                            /*
                            * Ist es ein FeldTyp?
                            */
                            if (fieldDescription.FieldType.Equals(typeof(Field)))
                            {
                                /*
                                * Convert ID to Guid 
                                */
                                if (fieldDescription.IsPrimary)
                                {
                                    object rowId = persistField;
                                    ids.Add(rowId);
                                    resultFields.FieldProperties = resultFields.FieldProperties.Add(column, new Field(fieldDescription, rowId));
                                }
                                else
                                    resultFields.FieldProperties = resultFields.FieldProperties.Add(column, new Field(fieldDescription, persistField));

                                continue;
                            }

                            /*
                             * Ist es ein Virtueller Link?
                             */
                            if (fieldDescription.FieldType.Equals(typeof(VirtualLinkAttribute)))
                            {
                                resultFields.FieldProperties = resultFields.FieldProperties.Add(column, new VirtualField((VirtualFieldDescription)fieldDescription, persistField));
                                continue;
                            }

                            /*
                            * Ist es ein Link Typ?
                            */
                            if (fieldDescription.FieldType.Equals(typeof(Link)))
                            {
                                if (persistField == null)
                                    resultFields.FieldProperties = resultFields.FieldProperties.Add(column, new Link(fieldDescription));
                                else if (fieldIndexDict.ContainsKey(column + DBConst.TypAddition))
                                    resultFields.FieldProperties = resultFields.FieldProperties.Add(column,
                                                     new Link(fieldDescription,
                                                              ConvertSourceToTargetType(persistField, typeof(Guid)),
                                                              reader[column + DBConst.TypAddition].ToString()));

                                continue;
                            }

                            /*
                            * Ist es ein Spezialisierter Link?
                            */
                            if (fieldDescription.FieldType.Equals(typeof(SpecializedLink)))
                            {
                                if (persistField == null)
                                    resultFields.FieldProperties = resultFields.FieldProperties.Add(column, new SpecializedLink(fieldDescription));
                                else
                                    resultFields.FieldProperties = resultFields.FieldProperties.Add(column, new SpecializedLink(fieldDescription,
                                        ConvertSourceToTargetType(persistField, typeof(Guid))));
                                continue;
                            }

                        }
                        else
                            resultFields.FieldProperties = resultFields.FieldProperties.Add(column, new UnmatchedField(persistField));
                    }

                    result.Add(resultFields);
                }
            }
            finally
            {
                reader.Close();
                reader.Dispose();
            }

            return result;
        }

        /// <summary>
        /// Builds the complete HAVING Clause with all conditions
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <param name="virtualAlias">The virtual alias.</param>
        /// <param name="parameterCollection">The parameter collection.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        protected virtual string PrivateCompleteHavingClause(ProjectionClass projection,
                                                             Dictionary<string, FieldDescription> fieldTemplates,
                                                             ICondition whereClause, IDictionary globalParameter,
                                                             IDictionary virtualAlias,
                                                             IDataParameterCollection parameterCollection, ref int index)
        {
            string realHaving = PrivateHavingClause(projection, whereClause, parameterCollection, globalParameter,
                                                    ref index, true);
            return string.IsNullOrEmpty(realHaving) ? string.Empty : string.Concat(" HAVING ", realHaving);
        }

        /// <summary>
        /// Builds a complete WHERE Clause with all where statements and virtual fields
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <param name="virtualAlias">The virtual alias.</param>
        /// <param name="parameterCollection">The parameter collection.</param>
        /// <param name="index">Index</param>
        /// <returns></returns>
        protected virtual string PrivateCompleteWhereClause(ProjectionClass projection,
                                                            Dictionary<string, FieldDescription> fieldTemplates,
                                                            ICondition whereClause, IDictionary globalParameter,
                                                            IDictionary virtualAlias,
                                                            IDataParameterCollection parameterCollection, ref int index)
        {
            /*
			 * Query bauen
			 */
            var query = new StringBuilder();
            string realWhere = PrivateWhereClause(projection, whereClause, parameterCollection, globalParameter,
                                                  ref index, true);
            string virtualWhere = (fieldTemplates != null)
                                      ? PrivateVirtualWhereClause(fieldTemplates, globalParameter, virtualAlias,
                                                                  parameterCollection, ref index)
                                      : "";

            if ((realWhere.Length != 0 || virtualWhere.Length != 0))
            {
                query.Append(" WHERE ");
                query.Append(realWhere);

                if ((realWhere.Length != 0 && virtualWhere.Length != 0))
                    query.Append(" AND ");

                query.Append(virtualWhere);
            }
            else if (realWhere.Length != 0)
            {
                query.Append(" WHERE ");
                query.Append(realWhere);
            }

            return query.ToString();
        }

        /// <summary>
        /// Converts the value to the targetType and returns it.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static object ConvertSourceToTargetType(object source, Type targetType)
        {
            if (source == null)
                return source;

            targetType = TypeHelper.GetBaseType(targetType);
            if (source.GetType().Equals(targetType))
                return source;

            if (targetType.IsEnum)
            {
                // Try to convert a string "3" to an integer
                int result;
                if (source is string && int.TryParse((string)source, out result))
                {
                    source = result;
                    return Enum.ToObject(targetType, source);
                }

                // If the source stays a string, than try to interpret it's value
                var valueString = source as string;
                if (valueString != null)
                    return Enum.Parse(targetType, valueString);

                // Try to convert it to an integer
                source = Convert.ToInt32(source);
                return Enum.ToObject(targetType, source);
            }


            if (targetType == typeof(Int32) && source is Decimal)
                source = source != DBNull.Value ? Decimal.Floor((Decimal)source) : 0;

            if (targetType == typeof(Int32))
                return source != DBNull.Value ? Convert.ToInt32(source) : 0;

            if (targetType == typeof(Int64))
                return source != DBNull.Value ? Convert.ToInt64(source) : (Int64)0;

            if (targetType == typeof(Double))
                return source != DBNull.Value ? Convert.ToDouble(source) : 0.0;

            if (targetType == typeof(Decimal))
                return source != DBNull.Value ? Convert.ToDecimal(source) : (Decimal)0;

            if ((targetType == typeof(Guid)) && (source is byte[]))
                return new Guid((byte[])source);

            if (targetType == typeof(bool))
            {
                if (source is Int16)
                    return source.Equals((Int16)1);

                if (source is Decimal)
                    return source.Equals((Decimal)1);

                if (source is int)
                    return source.Equals(1);
            }

            if ((targetType == typeof(string)) && (source == DBNull.Value))
                return string.Empty;

            return source;
        }

        #endregion

        #region IPersister Members

        /// <summary>
        /// Method to insert a new row to database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="fields">Fields to store in database.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <returns></returns>
        public virtual object Insert(string tableName, PersistentProperties fields,
                                     Dictionary<string, FieldDescription> fieldTemplates)
        {
            /*
             * Check if we have fields to insert
             */
            if (fields.AreEmpty)
                return null;

            String query = string.Concat("INSERT INTO ", ConcatedSchema, TypeMapper.Quote(tableName), " ");
            var columns = new List<Field>();
            var values = new ArrayList();
            String colQuery = "";
            String valQuery = "";
            Field primaryKey = null;

            /*
			 * Jetzt die Werte hinzufügen
			 */
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            try
            {
                var enumerator = fields.FieldProperties.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    /*
                     * Nur hinzufügen, wenn es sich um ein Feld oder eine Verknüpfung handelt
                     */
                    if (enumerator.Current.Value is Field)
                    {
                        var field = (Field)enumerator.Current.Value;

                        if (field.FieldDescription.IsPrimary)
                            primaryKey = field;

                        /*
                         * Beim LastUpdate Feld das Datum setzen (sofern es noch nicht gesetzt ist)
                         */
                        if (field.Name.Equals(DBConst.LastUpdateField) && field.Value == null)
                        {
                            DateTime now = DateTime.Now;
                            field.Value = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        }

                        /*
                         * Select Functions can't be inserted
                         */
                        string selectFunction = string.Empty;
                        string insertFunction = string.Empty;
                        if (field.FieldDescription.CustomProperty != null)
                        {
                            selectFunction = field.FieldDescription.CustomProperty.MetaInfo.SelectFunction;
                            insertFunction = field.FieldDescription.CustomProperty.MetaInfo.InsertFunction;
                        }

                        if (selectFunction.IsNotNullOrEmpty())
                            continue;

                        columns.Add(field);

                        if (insertFunction.IsNotNullOrEmpty())
                            values.Add(new InsertFunctionAttribute(insertFunction));
                        else
                            values.Add(field.Value);
                        continue;
                    }


                    /*
                     * ... oder eine Verknüpfung zu einem anderen VO
                     */
                    if (enumerator.Current.Value is Link)
                    {
                        var link = (Link)enumerator.Current.Value;
                        columns.Add(link.Property);
                        values.Add(link.Property.Value);

                        columns.Add(link.LinkedTo);
                        values.Add(link.LinkedTo.Value);

                        continue;
                    }

                    if (enumerator.Current.Value is OneToManyLink)
                    {
                        continue;
                    }

                    /*
                     * ... oder eine Verknüpfung zu einem anderen VO
                     */
                    if (enumerator.Current.Value is SpecializedLink)
                    {
                        var link = (SpecializedLink)enumerator.Current.Value;
                        columns.Add(link.Property);
                        values.Add(link.Property.Value);

                        continue;
                    }
                }

                /*
                 * SQL bauen
                 */
                IEnumerator<Field> colEnumerator = columns.GetEnumerator();
                IEnumerator valEnumerator = values.GetEnumerator();
                bool first = true;
                int counter = 1;

                while (colEnumerator.MoveNext())
                {
                    valEnumerator.MoveNext();
                    Object curValue = valEnumerator.Current;
                    Field column = colEnumerator.Current;

                    if (TypeMapper.IsDbNull(curValue) == false)
                    {
                        var dbFunction = curValue as DatabaseFunction;
                        if (dbFunction == null)
                        {
                            IDbDataParameter parameter = AddParameter(command.Parameters, ref counter, curValue,
                                                                      column.FieldDescription.CustomProperty.MetaInfo);
                            valQuery += (!first ? ", " : " ") + GetParameterString(parameter);
                        }
                        else
                        {
                            valQuery += (!first ? ", " : " ") + dbFunction.Function;
                        }

                        colQuery += (!first ? ", " : " ") + TypeMapper.Quote(column.Name);
                        first = false;
                    }
                }

                query += string.Concat("(", colQuery, ") VALUES (", valQuery, ")");
                command.CommandText = query;


                /*
                 * SQL ausführen
                 */
                int rows = ExecuteNonQuery(command);

                /*
                 * If Primary Key is an auto increment key, the id has to be updated
                 */
                if ((primaryKey != null) && (primaryKey.FieldDescription.IsAutoIncrement))
                    primaryKey.Value = SelectLastAutoId(tableName);

                /*
                 * Insert failed
                 */
                if (rows == 0)
                {
                    var exc = new InvalidOperationException("Could not insert object into table " + tableName);
                    ErrorMessage(exc);
                    throw exc;
                }

                UpdateLinkedObjects(tableName, primaryKey, fields);
                return primaryKey != null ? primaryKey.Value : null;
            }
            finally
            {
                stopwatch.Stop(command, CreateSql(command), 1);
                command.DisposeSafe();
            }
        }

        /// <summary>
        /// Method to update an existing row in database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="fields">Fields to store in database.</param>
        /// <param name="fieldTemplates">Field description.</param>
        public virtual void Update(string tableName, PersistentProperties fields,
                                   Dictionary<string, FieldDescription> fieldTemplates)
        {
            string query = string.Concat("UPDATE ", ConcatedSchema, TypeMapper.Quote(tableName), " SET ");
            var columns = new List<Field>();
            var values = new ArrayList();
            string colQuery = "";
            DateTime checkUpdate = DateTime.MinValue;
            bool hasToCheckUpdate = false;

            /*
			 * Primary Key initialisierung
			 */
            Field primaryId = null;
            Field parentObject = null;

            /*
			 * Jetzt die Werte hinzufügen
			 */
            var enumerator = fields.FieldProperties.GetEnumerator();
            while (enumerator.MoveNext())
            {
                /*
				 * Nur hinzufügen, wenn es sich um ein Feld oder eine Verknüpfung handelt
				 */
                if (enumerator.Current.Value is Field)
                {
                    var field = (Field)enumerator.Current.Value;

                    /*
					 * Set the last update field
					 */
                    if (field.Name.Equals(DBConst.LastUpdateField))
                    {
                        checkUpdate = (DateTime)field.Value;
                        hasToCheckUpdate = !checkUpdate.Equals(DateTime.MinValue);

                        DateTime now = DateTime.Now;
                        field.Value = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                        field.IsModified = true;
                    }

                    /*
                     * Select Functions can't be updated
                     */
                    string selectFunction = string.Empty;
                    string updateFunction = string.Empty;
                    if (field.FieldDescription.CustomProperty != null)
                    {
                        selectFunction = field.FieldDescription.CustomProperty.MetaInfo.SelectFunction;
                        updateFunction = field.FieldDescription.CustomProperty.MetaInfo.UpdateFunction;
                    }

                    /*
                     * Select Functions can't be inserted
                     */
                    if (selectFunction.IsNotNullOrEmpty())
                        continue;

                    /*
                     * If using a update function do always an update
                     */
                    if (updateFunction.IsNotNullOrEmpty())
                    {
                        columns.Add(field);
                        values.Add(new UpdateFunctionAttribute(updateFunction));
                    }
                    else
                        /*
                         * Bei Modifizierung übernehmen
                         */
                        if (field.IsModified)
                        {
                            columns.Add(field);
                            values.Add(field.Value);
                        }

                    /*
					 * Primary Keys merken
					 */
                    if (field.Name == DBConst.ParentObjectField)
                        parentObject = field;
                    else if (field.FieldDescription.IsPrimary)
                        primaryId = field;

                    continue;
                }


                /*
				 * ... oder eine Verknüpfung zu einem anderen VO
				 */
                if (enumerator.Current.Value is Link)
                {
                    var link = (Link)enumerator.Current.Value;
                    if (link.IsModified)
                    {
                        columns.Add(link.Property);
                        values.Add(link.Property.Value);

                        columns.Add(link.LinkedTo);
                        values.Add(link.LinkedTo.Value);
                    }

                    continue;
                }

                /*
				 * ... oder eine Verknüpfung zu einem anderen VO
				 */
                if (enumerator.Current.Value is SpecializedLink)
                {
                    var link = (SpecializedLink)enumerator.Current.Value;
                    if (link.IsModified)
                    {
                        columns.Add(link.Property);
                        values.Add(link.Property.Value);
                    }

                    continue;
                }
            }

            /*
             * If only the LastUpdate field has to be updated, than reset the timestamp to the value before.
             */
            if ((columns.Count == 1) && (hasToCheckUpdate))
            {
                ((Field)fields.FieldProperties.Get(DBConst.LastUpdateField)).Value = checkUpdate;
            }
            else
                /*
                 * Update DB only if minimum one column has changed
                 */
                if (columns.Count > 0)
                {
                    SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
                    IDbCommand command = CreateCommand();
                    try
                    {
                        /*
                        * SQL bauen
                        */
                        IEnumerator<Field> colEnumerator = columns.GetEnumerator();

                        bool first = true;

                        IEnumerator valEnumerator = values.GetEnumerator();
                        int counter = 1;

                        while (colEnumerator.MoveNext())
                        {
                            valEnumerator.MoveNext();
                            object curValue = valEnumerator.Current;
                            Field column = colEnumerator.Current;

                            if (TypeMapper.IsDbNull(curValue) == false)
                            {
                                var dbFunction = curValue as DatabaseFunction;
                                if (dbFunction == null)
                                {
                                    IDbDataParameter parameter = AddParameter(command.Parameters, ref counter, curValue,
                                                                              column.FieldDescription.CustomProperty.MetaInfo);
                                    colQuery += string.Concat((!first ? ", " : " "), TypeMapper.Quote(column.Name),
                                                              " = ",
                                                              GetParameterString(parameter));
                                }
                                else
                                {
                                    colQuery += string.Concat((!first ? ", " : " "), TypeMapper.Quote(column.Name),
                                                              " = ",
                                                              dbFunction.Function);
                                }

                                first = false;
                            }
                            else
                            {
                                colQuery += string.Concat((!first ? ", " : " "), TypeMapper.Quote(column.Name),
                                                          " = null");
                                first = false;
                            }
                        }

                        /*
                         * Primary Keys and so on ...
                         */
                        if (primaryId != null)
                        {
                            IDbDataParameter primary1 = CreateParameter("primary1", primaryId.Value,
                                                                        primaryId.FieldDescription.CustomProperty.MetaInfo);
                            command.Parameters.Add(primary1);

                            if (parentObject != null)
                            {
                                IDbDataParameter primary2 = CreateParameter("primary2", parentObject.Value,
                                                                            parentObject.FieldDescription.CustomProperty.MetaInfo);
                                command.Parameters.Add(primary2);

                                query += string.Concat(colQuery, " WHERE ", ConcatedSchema, TypeMapper.Quote(tableName),
                                                       ".",
                                                       TypeMapper.Quote(primaryId.Name), "=",
                                                       GetParameterString(primary1), " AND ",
                                                       ConcatedSchema, TypeMapper.Quote(tableName), ".",
                                                       TypeMapper.Quote(parentObject.Name), "=",
                                                       GetParameterString(primary2));
                            }
                            else
                                query += string.Concat(colQuery, " WHERE ", ConcatedSchema, TypeMapper.Quote(tableName),
                                                       ".",
                                                       TypeMapper.Quote(primaryId.Name), "=",
                                                       GetParameterString(primary1));
                        }


                        if (hasToCheckUpdate)
                        {
                            IDbDataParameter checkUpdateParam = CreateParameter("checkUpdate", checkUpdate, null);
                            command.Parameters.Add(checkUpdateParam);

                            query += string.Concat(" AND ", TypeMapper.Quote(DBConst.LastUpdateField), "=",
                                                   GetParameterString(checkUpdateParam));
                        }

                        /*
                        * SQL ausführen
                        */
                        command.CommandText = query;
                        int rows = ExecuteNonQuery(command);

                        /*
                         * Update fehlgeschlagen
                         */
                        if (rows == 0)
                        {
                            var exc = new DirtyObjectException("The object " + primaryId + " type " + tableName +
                                                               " has been modified by an other person.") { Source = query };
                            ErrorMessage(exc);
                            throw exc;
                        }
                    }
                    finally
                    {
                        stopwatch.Stop(command, CreateSql(command), 1);
                        command.DisposeSafe();
                    }
                }

            UpdateLinkedObjects(tableName, primaryId, fields);
        }

        /// <summary>
        /// Method to load an existing row from database.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="id">Primary key</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <returns>Hashtable with loaded fields</returns>
        public PersistentProperties Load(ProjectionClass projection, object id,
                                Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter)
        {
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            try
            {
                IDictionary virtualAlias = new HybridDictionary();
                int index = 1;

                /*
                 * CreateSQL
                 */
                string fromClause = PrivateFromClause(projection, null, command.Parameters, fieldTemplates,
                                                      globalParameter,
                                                      virtualAlias, ref index);

                IDbDataParameter parameter = CreateParameter("PrimaryKey", id, null);
                command.Parameters.Add(parameter);
                string virtualWhere = PrivateVirtualWhereClause(fieldTemplates, globalParameter, virtualAlias,
                                                                command.Parameters, ref index);
                string query = string.Concat("SELECT ", projection.GetColumns((ICondition)null, null), " "
                                             , BuildVirtualFields(fieldTemplates, globalParameter, virtualAlias)
                                             , BuildSelectFunctionFields(fieldTemplates, globalParameter)
                                             , " FROM "
                                             , fromClause
                                             , " WHERE "
                                             , projection.PrimaryKeyColumns, " = ", GetParameterString(parameter),
                                             (virtualWhere.Length != 0 ? " AND " + virtualWhere : ""));

                string grouping = projection.GetGrouping();
                if (!string.IsNullOrEmpty(grouping))
                    query = string.Concat(query, " GROUP BY ", grouping);

                command.CommandText = query;

                /*
                 * Execute
                 */
                var result = PrivateSelect(command, fieldTemplates, 0, int.MaxValue);
                return result.FirstOrDefault();
            }
            finally
            {
                stopwatch.Stop(command, CreateSql(command), 1);
                command.DisposeSafe();
            }
        }

        /// <summary>
        /// Method to load child objects from an existing object
        /// </summary>
        /// <param name="parentType">Type of the parent.</param>
        /// <param name="derivedTableName">Name of the derived table.</param>
        /// <param name="id">The id.</param>
        /// <param name="linkIdType">Type of the link id.</param>
        /// <param name="linkedPrimaryKeyType">Type of the linked primary key.</param>
        /// <param name="linkedObjectType">Type of the linked object.</param>
        /// <returns></returns>
        public virtual IDictionary LoadHashChilds(Type parentType, string derivedTableName, Object id, Type linkIdType,
                                                  Type linkedPrimaryKeyType, Type linkedObjectType)
        {
            /*
			* Do select on child Table
			*/
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            int rows = 0;
            try
            {
                IDbDataParameter parameter = CreateParameter("primaryKey", id, null);
                command.Parameters.Add(parameter);

                string linkFields = string.Concat(
                    TypeMapper.Quote(DBConst.LinkIdField), ",",
                    TypeMapper.Quote(DBConst.ParentObjectField), ",",
                    TypeMapper.Quote(DBConst.PropertyField), ",",
                    TypeMapper.Quote(DBConst.LinkedToField));

                string sql = string.Concat("SELECT ", linkFields, " FROM ", ConcatedSchema,
                                           TypeMapper.Quote(derivedTableName)
                                           , " WHERE ", TypeMapper.Quote(DBConst.ParentObjectField), " = ",
                                           GetParameterString(parameter),
                                           " ORDER BY ", TypeMapper.Quote(DBConst.LinkIdField));

                command.CommandText = sql;
                IDataReader reader = ExecuteReader(command);

                try
                {
                    var projection = ReflectionHelper.GetProjection(parentType, null);
                    Type parentPrimaryKeyType = projection.GetPrimaryKeyDescription().ContentType;

                    /*
                    * Add child listings
                    */
                    var resultHash = new SortedList();
                    Dictionary<string, int> fieldIndexDict;
                    Dictionary<int, string> indexFieldDict;
                    GetColumns(reader, null, out fieldIndexDict, out indexFieldDict);
                    while (reader.Read())
                    {
                        object linkId = ConvertSourceToTargetType(reader.GetValue(0), linkIdType);

                        var listlink = new ListLink(
                            null,
                            linkId, parentType,
                            TypeMapper.ConvertToType(parentPrimaryKeyType,
                                                     reader.GetValue(fieldIndexDict[DBConst.ParentObjectField])),
                            // Parent Id

                            TypeMapper.ConvertToType(linkedPrimaryKeyType,
                                                     reader.GetValue(fieldIndexDict[DBConst.PropertyField])),
                            // Property

                            (string)
                            ConvertSourceToTargetType(reader.GetValue(fieldIndexDict[DBConst.LinkedToField]),
                                                      typeof(string)));
                        // Link To

                        resultHash.Add(linkId, listlink);
                    }

                    rows = resultHash.Count;
                    return resultHash;
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

        /// <summary>
        /// Method to load child objects from an existing object
        /// </summary>
        /// <param name="parentType">Type of the parent object.</param>
        /// <param name="derivedTableName">Name of the derived table.</param>
        /// <param name="objectId">The id.</param>
        /// <param name="linkedPrimaryKeyType">Type of the linked primary key.</param>
        /// <param name="linkedObjectType">Type of the linked object.</param>
        /// <returns></returns>
        public virtual IList LoadListChilds(Type parentType, string derivedTableName, Object objectId,
                                            Type linkedPrimaryKeyType, Type linkedObjectType)
        {
            /*
            * Do select on child Table
            */
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            int rows=0;
            try
            {
                IDbDataParameter parameter = CreateParameter("primaryKey", objectId, null);
                command.Parameters.Add(parameter);

                string linkFields = string.Concat(
                    TypeMapper.Quote(DBConst.ParentObjectField), ",",
                    TypeMapper.Quote(DBConst.PropertyField), ",",
                    TypeMapper.Quote(DBConst.LinkedToField));

                string sql = string.Concat("SELECT ", linkFields, " FROM ", ConcatedSchema,
                                           TypeMapper.Quote(derivedTableName)
                                           , " WHERE ", TypeMapper.Quote(DBConst.ParentObjectField), " = ",
                                           GetParameterString(parameter));

                command.CommandText = sql;
                IDataReader reader = ExecuteReader(command);

                try
                {
                    var projection = ReflectionHelper.GetProjection(parentType, null);
                    Type parentPrimaryKeyType = projection.GetPrimaryKeyDescription().ContentType;

                    /*
                    * Add child listings
                    */
                    IList list = new ArrayList();
                    Dictionary<string, int> fieldIndexDict;
                    Dictionary<int, string> indexFieldDict;
                    GetColumns(reader, null, out fieldIndexDict, out indexFieldDict);
                    while (reader.Read())
                    {
                        var listlink = new ListLink(
                            null,
                            (string)
                            ConvertSourceToTargetType(reader.GetValue(fieldIndexDict[DBConst.LinkedToField]),
                                                      typeof(string)),
                            // Link To

                            parentType,
                            TypeMapper.ConvertToType(parentPrimaryKeyType,
                                                     reader.GetValue(fieldIndexDict[DBConst.ParentObjectField])),
                            // Parent Id

                            TypeMapper.ConvertToType(linkedPrimaryKeyType,
                                                     reader.GetValue(fieldIndexDict[DBConst.PropertyField]))
                            // Property
                            );

                        list.Add(listlink);
                    }

                    rows = list.Count;
                    return list;
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

        /// <summary>
        /// Method to delete an existing row from database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="id">Primary key</param>
        /// <param name="fieldTemplates">Field description.</param>
        public virtual void Delete(String tableName, Object id, Dictionary<string, FieldDescription> fieldTemplates)
        {
            IDbCommand command = null;
            IDbDataParameter parameter;

            SqlStopwatch stopwatch;
            foreach (var entry in fieldTemplates)
            {
                stopwatch = new SqlStopwatch(SqlTracer);
                try
                {
                    FieldDescription field = entry.Value;
                    if (!field.FieldType.Equals(typeof(ListLink)))
                        continue;

                    String complexMember = entry.Key;

                    /*
                     * Delete childs on table
                     */
                    command = CreateCommand();
                    parameter = CreateParameter("PrimaryKey", id, null);
                    command.Parameters.Add(parameter);

                    string subTable = string.Concat(ConcatedSchema,
                                                    TypeMapper.Quote(CreateChildTableName(tableName, complexMember)));
                    string subQuery = string.Concat("DELETE FROM ", subTable, " WHERE ", subTable, ".",
                                                    TypeMapper.Quote(DBConst.ParentObjectField), "=",
                                                    GetParameterString(parameter));

                    command.CommandText = subQuery;
                    ExecuteNonQuery(command);
                }
                finally
                {
                    stopwatch.Stop(command, CreateSql(command), -1);
                    command.DisposeSafe();
                }
            }

            stopwatch = new SqlStopwatch(SqlTracer);
            int rows= 0;
            try
            {
                /*
                 * Build query
                 */
                command = CreateCommand();
                parameter = CreateParameter("PrimaryKey", id, null);
                command.Parameters.Add(parameter);

                string primaryKey = GetPrimaryKeyColumn(fieldTemplates);

                string table = string.Concat(ConcatedSchema, TypeMapper.Quote(tableName));
                string query = string.Concat("DELETE FROM ", table, " WHERE ", table, ".", TypeMapper.Quote(primaryKey),
                                             "=",
                                             GetParameterString(parameter));
                command.CommandText = query;

                /*
                 * Execute query
                 */
                rows = ExecuteNonQuery(command);
                if ((rows == 0) && (SqlTracer != null) && (SqlTracer.TraceErrorEnabled))
                {
                    SqlTracer.ErrorMessage(
                        string.Concat("The row with ", id.ToString(), " could not be deleted (table: ", tableName, ")"),
                        "BasePersister");
                }
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
        public virtual IList SelectIDs(ProjectionClass projection, string primaryKeyColumn, ICondition whereClause,
                                       OrderBy orderBy)
        {
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            int rows = 0;
            try
            {
                IDictionary virtualAlias = new HybridDictionary();

                int index = 1;
                string hint = PrivateHintClause(projection, whereClause, command.Parameters, null, null, virtualAlias, ref index);
                string withClause = PrivateWithClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias,
                                                      ref index);
                string fromClause = PrivateFromClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias,
                                                      ref index);
                string query = string.Concat(withClause, "SELECT ", projection.PrimaryKeyColumns, " FROM ", fromClause);

                if (!string.IsNullOrEmpty(hint))
                    query += " " + hint + " ";

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
                }
                finally
                {
                    reader.Close();
                    reader.Dispose();
                }

                rows = ids.Count;
                return ids;
            }
            finally
            {
                stopwatch.Stop(command, CreateSql(command), rows);
                command.DisposeSafe();
            }
        }

        /// <summary>
        /// Checks if the given primary key is stored within the database.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="primaryKeyColumn">The primary key column.</param>
        /// <param name="id">Primary Key</param>
        /// <returns>true, if the record exists in database.</returns>
        public virtual bool Contains(string tableName, string primaryKeyColumn, object id)
        {
            /*
			 * Do selection
			 */
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            try
            {
                IDbDataParameter parameter = CreateParameter("PrimaryKey", id, null);
                command.Parameters.Add(parameter);

                string query = string.Concat("SELECT ", TypeMapper.Quote(primaryKeyColumn), " FROM ", ConcatedSchema,
                                             TypeMapper.Quote(tableName),
                                             " WHERE ", TypeMapper.Quote(primaryKeyColumn), "=",
                                             GetParameterString(parameter));

                command.CommandText = query;

                IDataReader reader = ExecuteReader(command);
                try
                {
                    bool result = reader.Read();
                    return result;
                }
                finally
                {
                    reader.Close();
                    reader.Dispose();
                }
            }
            finally
            {
                stopwatch.Stop(command, CreateSql(command), 0);
                command.DisposeSafe();
            }
        }

        /// <summary>
        /// Returns a list with value objects that matches the search criteria.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="orderBy">Order clause to order the selection.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <param name="distinct">Select only distinct values</param>
        /// <returns>List of value objects</returns>
        public List<PersistentProperties> Select(ProjectionClass projection, ICondition whereClause, OrderBy orderBy,
                            Dictionary<string, FieldDescription> fieldTemplates, IDictionary globalParameter,
                            bool distinct)
        {
            return Select(projection, null, whereClause, orderBy, fieldTemplates, globalParameter, distinct);
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
        public virtual List<PersistentProperties> Select(string tableName, string selectSql, SortedList selectParameter,
                                     Dictionary<string, FieldDescription> fieldTemplates)
        {
            IDbCommand command = CreateCommand(selectSql);

            try
            {
                if (selectParameter != null)
                {
                    IDictionaryEnumerator enumerator = selectParameter.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        IDbDataParameter parameter = CreateParameter(((string)enumerator.Key).Replace("@", ""),
                                                                     enumerator.Value, null);
                        command.Parameters.Add(parameter);
                    }
                }

                List<PersistentProperties> result = PrivateSelect(command, fieldTemplates, 0, int.MaxValue);
                return result;
            }
            finally
            {
                command.DisposeSafe();
            }
        }

        /// <summary>
        /// Returns a list with value objects that matches the search criteria.
        /// </summary>
        /// <param name="tableName">Table Name</param>
        /// <param name="command">The command.</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <returns>List of value objects</returns>
        public List<PersistentProperties> Select(string tableName, IDbCommand command, Dictionary<string, FieldDescription> fieldTemplates)
        {
            List<PersistentProperties> result = PrivateSelect(command, fieldTemplates, 0, int.MaxValue);
            return result;
        }

        /// <summary>
        /// Executes a page select and returns value objects that matches the search criteria and line number is within the min and max values.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="orderBy">Order clause to order the selection.</param>
        /// <param name="minLine">Minimum count</param>
        /// <param name="maxLine">Maximum count</param>
        /// <param name="fieldTemplates">Field description.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <param name="distinct">Select only distinct values</param>
        /// <returns>List of value objects</returns>
        public List<PersistentProperties> PageSelect(ProjectionClass projection, ICondition whereClause, OrderBy orderBy, int minLine,
                                int maxLine, Dictionary<string, FieldDescription> fieldTemplates,
                                IDictionary globalParameter, bool distinct)
        {
            return PageSelect(projection, null, whereClause, orderBy, minLine, maxLine, fieldTemplates, globalParameter,
                              distinct);
        }

        /// <summary>
        /// Counts number of rows that matches the whereclause
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <returns>Number of rows</returns>
        public virtual int Count(ProjectionClass projection, ICondition whereClause,
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

                string hint = PrivateHintClause(projection, whereClause, command.Parameters, null, null, virtualAlias, ref index);

                string withClause = PrivateWithClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias,
                                                      ref index);
                string tables = PrivateFromClause(projection, whereClause, command.Parameters, fieldTemplates,
                                                  globalParameter, virtualAlias, ref index);
                string query = string.Concat(withClause, "SELECT COUNT(",
                                             string.IsNullOrEmpty(grouping) ? "*" : "count(*)",
                                             ") FROM ", tables,
                                             string.IsNullOrEmpty(hint) ? string.Empty : " " + hint + " ",
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
        /// Returns true, if a connection is established
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsConnected
        {
            get { return (Connection != null) && (Connection.State == ConnectionState.Open); }
        }

        #endregion



        #region Schema Write Helper

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static IList GetUniqueIdentifierList(IDictionary source, string tableName)
        {
            if (source[tableName] == null)
                source[tableName] = new ArrayList();

            return source[tableName] as IList;
        }

        /// <summary>
        /// Returns true, if the constraint is unique for the specified table
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tableName"></param>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected static bool ContainedInUniqueIdentifierList(Hashtable source, string tableName, string constraint)
        {
            IList identifierList = GetUniqueIdentifierList(source, tableName);
            if (identifierList.Contains(constraint))
                return true;

            identifierList.Add(constraint);
            return false;
        }

        #endregion

        #region Dispose Pattern

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="T:AdFactum.Data.XmlPersister.XmlPersister"/> is reclaimed by garbage collection.
        /// </summary>
        ~BasePersister()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disconnecting the database
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (Connection != null)
                {
                    Connection.Close();
                    Connection.Dispose();
                    Connection = null;
                }
            }

            // free unmanaged resources
        }

        #endregion

        /// <summary>
        /// Creates the SQL.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public virtual string CreateSql(IDbCommand command)
        {
            if (command == null)
                return string.Empty;

            string sql = command.CommandText;

            IEnumerator enumParam = command.Parameters.GetEnumerator();

            while (enumParam.MoveNext())
            {
                var parameter = (IDbDataParameter)enumParam.Current;
                string paramValue = TypeMapper.GetParamValueAsSQLString(parameter.Value);

                /*
                 * First replace of keyword
                 */
                int index;
                while ((index = sql.IndexOf(parameter.ParameterName, StringComparison.InvariantCultureIgnoreCase)) >= 0)
                {
                    sql = sql.Remove(index, parameter.ParameterName.Length);
                    sql = sql.Insert(index, paramValue);
                }
            }
            return sql;
        }

        /// <summary>
        /// Creates the command object.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public abstract IDbCommand CreateCommand(string sql);

        /// <summary>
        /// Creates the data adapter.
        /// </summary>
        /// <returns></returns>
        protected abstract IDbDataAdapter CreateDataAdapter();

        /// <summary>
        /// This method extracts the unique constraint columns of a table and creates alter table sql methods
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="fieldTemplates">field templates</param>
        /// <param name="uniqueConstraintCount">count the unique constraints</param>
        /// <param name="uniqueConstraints">The unique constraints.</param>
        /// <returns></returns>
        protected string GetUniqueConstraintSql(String tableName, IDictionary fieldTemplates,
                                                Hashtable uniqueConstraintCount, Hashtable uniqueConstraints)
        {
            var resultSql = new StringBuilder();
            string uniqueSql;
            var keyGroupConstraints = new Hashtable();
            int constraintNumber = (uniqueConstraintCount[tableName] == null)
                                       ? 1
                                       : (int)uniqueConstraintCount[tableName];

            foreach (DictionaryEntry entry in fieldTemplates)
            {
                var field = (FieldDescription)entry.Value;

                /*
				 * Is there a unique field?
				 */
                if ((field.CustomProperty != null) && (field.CustomProperty.MetaInfo.IsUnique))
                {
                    string constraint = TypeMapper.Quote(field.Name);
                    if (ContainedInUniqueIdentifierList(uniqueConstraints, tableName, constraint))
                        continue;

                    /*
                     * If the property contains a single unique key
                     */
                    if (field.CustomProperty.ContainsUniqueDefaultGroup)
                    {
                        uniqueSql = GetUniqueConstraintSqlStmt(tableName, constraintNumber, constraint);

                        resultSql.Append(uniqueSql);
                        uniqueConstraintCount[tableName] = ++constraintNumber;
                    }

                    /*
                     * Step through all other key groups, and gather the fields
                     */
                    IEnumerator enumerator = field.CustomProperty.MetaInfo.UniqueKeyGroups.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        var group = (KeyGroup)enumerator.Current;
                        if (group.Number > 0)
                        {
                            if (keyGroupConstraints[group.Number] == null)
                                keyGroupConstraints[group.Number] = new SortedList();

                            int position = ((SortedList)keyGroupConstraints[group.Number]).Count;
                            if (group.Ordering > 0) position = group.Ordering;
                            ((SortedList)keyGroupConstraints[group.Number]).Add(position, constraint);
                        }
                    }
                }
            }

            /*
             * Now add the combined unique keys
             */
            foreach (SortedList sortedConstraint in keyGroupConstraints.Values)
            {
                var constraint = new StringBuilder();
                bool first = true;
                foreach (DictionaryEntry entry in sortedConstraint)
                {
                    if (!first) constraint.Append(", ");
                    constraint.Append(entry.Value);
                    first = false;
                }

                uniqueSql = GetUniqueConstraintSqlStmt(tableName, constraintNumber, constraint.ToString());

                resultSql.Append(uniqueSql);
                uniqueConstraintCount[tableName] = ++constraintNumber;
            }

            return resultSql.ToString();
        }

        /// <summary>
        /// Gets the unique uniqueConstraint SQL STMT.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="constraintNumber">The uniqueConstraint number.</param>
        /// <param name="uniqueConstraint">The uniqueConstraint.</param>
        /// <returns></returns>
        protected virtual string GetUniqueConstraintSqlStmt(string tableName, int constraintNumber,
                                                            string uniqueConstraint)
        {
            string uniqueSql = string.Concat("ALTER TABLE ", ConcatedSchema, TypeMapper.Quote(tableName)
                                             , " ADD CONSTRAINT ", tableName, "_UK", constraintNumber.ToString("00")
                                             , " UNIQUE (", uniqueConstraint, ");\n");
            return uniqueSql;
        }

        /// <summary>
        /// This method extracts the unique inndex constraint columns of a table that links through a virtual link to other tables and create the sql for it
        /// </summary>
        /// <param name="objectType">Type of the object from which the virtual fields shall obtained.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="uniqueConstraintCount">count the foreign key constraints</param>
        /// <param name="uniqueConstraints">The unique constraints.</param>
        /// <returns></returns>
        protected String GetUniqueIndexForVirtualLinksSql(Type objectType,
                                                          Dictionary<string, FieldDescription> fieldTemplates,
                                                          Hashtable uniqueConstraintCount, Hashtable uniqueConstraints)
        {
            var resultSql = new StringBuilder();
            foreach (PropertyInfo info in objectType.GetProperties())
            {
                /*
				 * Is there a virtual field?
				 */
                VirtualLinkAttribute virtualLink = ReflectionHelper.GetVirtualLinkInstance(info);
                if ((virtualLink != null) && (virtualLink.JoinFieldForGlobalParameter != null))
                {
                    string tableName = Table.GetTableInstance(virtualLink.LinkedClass).DefaultName;
                    string fieldForGlobalParameter =
                        Property.GetPropertyInstance(
                            virtualLink.LinkedClass.GetPropertyInfo(virtualLink.JoinFieldForGlobalParameter)).MetaInfo.ColumnName;
                    string fieldForKey =
                        Property.GetPropertyInstance(virtualLink.LinkedClass.GetPropertyInfo(virtualLink.JoinFieldInLinkedClass)).
                            MetaInfo.ColumnName;

                    int constraintNumber = (uniqueConstraintCount[tableName] == null)
                                               ? 1
                                               : (int)uniqueConstraintCount[tableName];

                    string constraint = string.Concat(TypeMapper.Quote(fieldForKey), ", ", TypeMapper.Quote(fieldForGlobalParameter));
                    if (ContainedInUniqueIdentifierList(uniqueConstraints, tableName, constraint))
                        continue;

                    string sql = GetUniqueConstraintSqlStmt(tableName, constraintNumber, constraint);

                    resultSql.Append(sql);
                    uniqueConstraintCount[tableName] = ++constraintNumber;
                }
            }

            return resultSql.ToString();
        }

        /// <summary>
        /// Updates the linked objects.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="primaryId">The primary id.</param>
        /// <param name="fields">The fields.</param>
        private void UpdateLinkedObjects(string tableName, Field primaryId, PersistentProperties fields)
        {
            /*
			 * Nochmal durchlaufen und nur die über Linklisten verknüpften Objekte speichern
			 */
            if (fields.DictProperties != null)
            {
                var enumerator = fields.DictProperties.GetEnumerator();
                while (enumerator.MoveNext())
                    UpdatedLinkedFields(enumerator, tableName, primaryId);
            }

            if (fields.ListProperties != null)
            {
                var enumerator1 = fields.ListProperties.GetEnumerator();
                while (enumerator1.MoveNext())
                    UpdatedLinkedFields(enumerator1, tableName, primaryId);
            }
        }

        /// <summary>
        /// Updates the Linked Fields - that a subroutine of the UpdatedLinkedObjects method
        /// </summary>
        private void UpdatedLinkedFields<TKey>(
            IEnumerator<KeyValuePair<string, Dictionary<TKey, IModification>>> enumerator,
            string tableName,
            Field primaryId)
        {
            var propertyList = enumerator.Current.Value;
            var listEnumerator = propertyList.GetEnumerator();
            while (listEnumerator.MoveNext())
            {
                var dictionaryLink = listEnumerator.Current.Value as ListLink;
                if (dictionaryLink != null)
                {
                    String childTable = CreateChildTableName(tableName, enumerator.Current.Key);

                    dictionaryLink.UpdateParentReferenceId(primaryId.Value);

                    if (dictionaryLink.IsDeleted)
                        DeleteSubLink(childTable, dictionaryLink);
                    else if (dictionaryLink.IsNew)
                        Insert(childTable, dictionaryLink.Fields(this), dictionaryLink.GetTemplates());

                        // It's important to check, if the key != null. 
                    // If true, it is a dictionary link which means, that the content may be changed.
                    // If false, it is a list link. List items can only be deleted or inserted, but never updated.
                    else if (dictionaryLink.IsModified && dictionaryLink.Key != null)
                        Update(childTable, dictionaryLink.Fields(this), dictionaryLink.GetTemplates());

                    continue;
                }

                /*
                 * One To Many Link
                 */
                var oneToManyLink = listEnumerator.Current.Value as OneToManyLink;
                if (oneToManyLink != null)
                    oneToManyLink.UpdateParentReferenceId(primaryId.Value);
            }
        }

        /// <summary>
        /// Gets the name of the primary key.
        /// </summary>
        /// <param name="templates">The templates.</param>
        /// <returns></returns>
        public string GetPrimaryKeyColumn(IDictionary templates)
        {
            foreach (DictionaryEntry de in templates)
            {
                var field = de.Value as FieldDescription;
                if ((field != null) && (field.IsPrimary))
                    return (string)de.Key;
            }

            return string.Empty;
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
        protected virtual List<PersistentProperties> Select(ProjectionClass projection, string additonalColumns, ICondition whereClause,
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
                string hint = PrivateHintClause(projection, whereClause, command.Parameters, null, null, virtualAlias, ref index);
                string withClause = PrivateWithClause(projection, whereClause, command.Parameters, null, null,
                                                      virtualAlias,
                                                      ref index);

                string fromClause = PrivateFromClause(projection, whereClause, command.Parameters, fieldTemplates,
                                                      globalParameter, virtualAlias, ref index);
                string virtualFields = BuildVirtualFields(fieldTemplates, globalParameter, virtualAlias);
                string selectFunctions = BuildSelectFunctionFields(fieldTemplates, globalParameter);

                /*
                 * SQL Bauen
                 */

                string query = string.Concat(withClause
                                             , distinct ? "SELECT DISTINCT " : "SELECT ",
                                             projection.GetColumns(whereClause, additonalColumns), " "
                                             , virtualFields
                                             , selectFunctions
                                             , BuildJoinFields(whereClause)
                                             , " FROM "
                                             , fromClause);

                if (!string.IsNullOrEmpty(hint))
                    query += " " + hint + " ";

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
        /// Pages the select.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="additionalColumns">The additional columns.</param>
        /// <param name="whereClause">The where clause.</param>
        /// <param name="orderBy">The order by.</param>
        /// <param name="minLine">The min line.</param>
        /// <param name="maxLine">The max line.</param>
        /// <param name="fieldTemplates">The field templates.</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <param name="distinct">if set to <c>true</c> [distinct].</param>
        /// <returns></returns>
        protected virtual List<PersistentProperties> PageSelect(ProjectionClass projection, string additionalColumns, ICondition whereClause,
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
                string hint = PrivateHintClause(projection, whereClause, command.Parameters, null, null, virtualAlias, ref index);

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
                
                if (!string.IsNullOrEmpty(hint))
                    query += " " + hint + " ";

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
        /// Return the last auto id
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        protected abstract int SelectLastAutoId(string tableName);

        /// <summary>
        /// Deletes the sub link.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="link">The link.</param>
        public virtual void DeleteSubLink(string tableName, ListLink link)
        {
            SqlStopwatch stopwatch = new SqlStopwatch(SqlTracer);
            IDbCommand command = CreateCommand();

            int rows = 0;
            try
            {

                Field primary1 = link.ParentObject;

                IDbDataParameter primaryKeyParameter1 = CreateParameter("PrimaryKey1", primary1.Value,
                                                                        primary1.FieldDescription.CustomProperty.MetaInfo);
                command.Parameters.Add(primaryKeyParameter1);

                Field primary2 = link.Key ?? link.Property;

                IDbDataParameter primaryKeyParameter2 = CreateParameter("PrimaryKey2",
                                                                        primary2.Value ?? primary2.OldValue,
                                                                        primary2.FieldDescription.CustomProperty.MetaInfo);
                command.Parameters.Add(primaryKeyParameter2);

                /*
                 * Build query
                 */
                string query = string.Concat("DELETE FROM ", ConcatedSchema, TypeMapper.Quote(tableName),
                                             " WHERE ", ConcatedSchema, TypeMapper.Quote(tableName), ".",
                                             TypeMapper.Quote(primary1.Name), "=",
                                             GetParameterString(primaryKeyParameter1),
                                             " AND ", ConcatedSchema, TypeMapper.Quote(tableName), ".",
                                             TypeMapper.Quote(primary2.Name), "=",
                                             GetParameterString(primaryKeyParameter2));

                /*
                 * Execute query
                 */
                command.CommandText = query;
                rows = ExecuteNonQuery(command);
                if ((rows == 0) && (SqlTracer != null) && (SqlTracer.TraceErrorEnabled))
                {
                    SqlTracer.ErrorMessage(
                        string.Concat("The row with ", primary1.Value.ToString(), "/", primary2.Value.ToString(),
                                      " could not be deleted (table: ", tableName, ")"), "BasePersister");
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