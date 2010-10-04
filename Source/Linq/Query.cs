using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq.Expressions;
using AdFactum.Data.Linq.Language;
using AdFactum.Data.Linq.Translation;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;

namespace AdFactum.Data.Linq
{
    internal class ParameterNameComparer : IEqualityComparer<IDataParameter>
    {
        #region Implementation of IEqualityComparer<IDataParameter>

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <returns>
        /// true if the specified objects are equal; otherwise, false.
        /// </returns>
        /// <param name="x">The first object of type <paramref name="T"/> to compare.
        ///                 </param><param name="y">The second object of type <paramref name="T"/> to compare.
        ///                 </param>
        public bool Equals(IDataParameter x, IDataParameter y)
        {
            return x.ParameterName == y.ParameterName;
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <returns>
        /// A hash code for the specified object.
        /// </returns>
        /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.
        ///                 </param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.
        ///                 </exception>
        public int GetHashCode(IDataParameter obj)
        {
            return obj.ParameterName.GetHashCode();
        }

        #endregion
    }

    /// <summary>
    /// Implementation of the Linq Query Provider for the ObjectMapper .NET
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Query<T> : IOrderedQueryable<T>, ILinqQueryProvider
    {
        private readonly Dictionary<string, int> parameterMapping = new Dictionary<string, int>();
        private Expression compiledExpression;
        private List<PropertyTupel> groupings;
        private IDataParameterCollection storedParamterCollection;
        private string storedSqlCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="Query&lt;T&gt;"/> class.
        /// </summary>
        public Query(Expression expression, ObjectMapper mapper, Cache<Type, ProjectionClass> cache)
        {
            Expression = expression;
            Mapper = mapper;
            DynamicCache = cache;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Query&lt;T&gt;"/> class.
        /// </summary>
        public Query(ObjectMapper mapper)
        {
            Mapper = mapper;
            Expression = Expression.Constant(this);
        }

        /// <summary>
        /// Initializes a Query object with a dedicated name
        /// </summary>
        public Query(ObjectMapper mapper, string tableName)
            : this(mapper)
        {
            if (DynamicCache == null) // If we don't have a dynamic cache, we need one to store the overwritten table name
                DynamicCache = new Cache<Type, ProjectionClass>("Linq Dynamic Cache");

            ProjectionClass projection = ReflectionHelper.GetProjection(typeof (T), DynamicCache);

            // If it's not a LinkBridge, than don't insert it into the global cache
            Type genericTypeDefinition = null;
            if (typeof(T).IsGenericType)
                genericTypeDefinition = typeof (T).GetGenericTypeDefinition();

            if (genericTypeDefinition == null || !typeof(LinkBridge<,>).IsAssignableFrom(genericTypeDefinition))
            {
                projection = (ProjectionClass) projection.Clone();
                projection.TableNameOverwrite = tableName;
                DynamicCache.Insert(typeof(T), projection);
            }
            else
                projection.TableNameOverwrite = tableName;
        }

        /// <summary>
        /// Gets or sets the dynamic cache.
        /// </summary>
        /// <value>The dynamic cache.</value>
        public Cache<Type, ProjectionClass> DynamicCache{ get; private set;}

        private int level;

        /// <summary> Gets or sets the Hierarchy Level. </summary>
        public IQueryable<T> Level(Expression expression, int hierarchyLevel)
        {
            level = hierarchyLevel;
            return this;
        }

        /// <summary>
        /// Gets the hierarchy level.
        /// </summary>
        /// <value>The hierarchy level.</value>
        public int HierarchyLevel {get{ return level;}}

        /// <summary>
        /// Returns the Linq persister
        /// </summary>
        public ILinqPersister Persister
        {
            get { return (ILinqPersister) Mapper.Persister; }
        }

        #region ILinqQueryProvider Members

        /// <summary> Gets or sets the mapper. </summary>
        public ObjectMapper Mapper { get; set; }

        /// <summary>
        /// Constructs an <see cref="T:System.Linq.IQueryable"/> object that can evaluate the query represented by a specified expression tree.
        /// </summary>
        public IQueryable CreateQuery(Expression expression)
        {
            return CreateQuery<T>(expression);
        }

        /// <summary>
        /// Creates the query.
        /// </summary>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new Query<TElement>(expression, Mapper, DynamicCache);
        }

        /// <summary>
        /// Executes the query represented by a specified expression tree.
        /// </summary>
        public object Execute(Expression expression)
        {
            return Execute<List<T>>(expression);
        }

        /// <summary>
        /// Executes the specified expression.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public TResult Execute<TResult>(Expression expression)
        {
            IDbCommand command = PreCompile(expression);
            ReplaceSqlParamter(command);

            return ExecuteCommand<TResult>(command);
        }

        /// <summary>
        /// PreCompiles the command
        /// </summary>
        /// <returns></returns>
        public IDbCommand PreCompile(Expression expression)
        {
            ILinqPersister linqPersister = Persister;
            int i;
            ExpressionVisitorBackpack backpack;
            compiledExpression = linqPersister.RewriteExpression(expression, out backpack, out groupings, out i);
            level = i;

            var lambda = compiledExpression as LambdaExpression;
            if (lambda != null)
            {
                compiledExpression = lambda.Body;
                for (int x = 0; x < lambda.Parameters.Count; x++)
                    parameterMapping.Add(lambda.Parameters[x].Name, x);
            }

#if TRACE
            Console.WriteLine("\nSql Expression");
            Console.WriteLine("----------------");
#endif
            IDbCommand command = LinqMethodInspector.Evaluate(linqPersister.LinqExpressionWriter,
                    lambda ?? compiledExpression, compiledExpression, groupings, linqPersister, backpack);

#if TRACE
            Console.WriteLine(command.CommandText);
#endif

            // Store the current Commands in order to rebind them, if necessary
            DynamicCache = backpack.ProjectionCache;
            storedParamterCollection = command.Parameters;
            storedSqlCommand = command.CommandText;

            return command;
        }

        /// <summary>
        /// Used to mark the query as a template object for compiled queries
        /// </summary>
        public void MarkAsTemplate()
        {
            if (string.IsNullOrEmpty(StoredSqlCommand))
                throw new NotSupportedException(
                    "You can only mark the query as a template, if it contains a command that can be made to a template.");

            Mapper = null; // Remove Mapper Reference
            Expression = Expression.Constant(this); // Remove Expression Reference

            // Strip duplicated parameters
            List<IDataParameter> newParameters = null;
            if (storedParamterCollection.Count > 1)
            {
                List<IDataParameter> parameters = new List<IDataParameter>(new ListAdapter<IDataParameter>(storedParamterCollection));
                newParameters = parameters.Distinct(new ParameterNameComparer()).ToList();
            }

            if (newParameters != null && storedParamterCollection.Count != newParameters.Count)
            {
                storedParamterCollection.Clear();
                newParameters.ForEach(x => storedParamterCollection.Add(x));
            }
        }

        /// <summary> True, if the command shall not be disposed </summary>
        public bool DontDisposeCommand
        {
            get { return Persister.DontDisposeCommand; }
            set { Persister.DontDisposeCommand = value; }
        }

        #endregion

        #region IOrderedQueryable<T> Members

        /// <summary> Returns the current expression </summary>
        public Expression Expression { get; private set; }

        ///<summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>) Execute(Expression)).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary> 
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable"/> is executed.
        /// </summary>
        public Type ElementType
        {
            get
            {
                if (typeof (T).IsGenericType && (typeof (T).GetGenericTypeDefinition() == typeof (List<>)))
                    return typeof (T).GetGenericArguments().First();

                return typeof (T);
            }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        public IQueryProvider Provider
        {
            get { return this; }
        }

        /// <summary>
        /// Returns the stored sql command
        /// </summary>
        public string StoredSqlCommand
        {
            get { return storedSqlCommand; }
        }

        #endregion

        /// <summary>
        /// Executes the command and returns the result
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        internal TResult ExecuteCommand<TResult>(IDbCommand command)
        {
            var selectExp = compiledExpression as SelectExpression;
            if ((selectExp != null && selectExp.SelectResult == SelectResultType.SingleAggregate) || (compiledExpression is ScalarExpression))
                return GetAggregate<TResult>(command);

            IList resultList = GetResult(command);

            // Check if a single result is expected
            if (selectExp != null && (selectExp.SelectResult == SelectResultType.SingleObject
                                      || selectExp.SelectResult == SelectResultType.SingleObjectOrDefault))
            {
                if (resultList != null && resultList.Count > 0)
                    return (TResult) resultList[0];

                // Throw an exception, if nothing could be found
                if (selectExp.SelectResult == SelectResultType.SingleObject)
                    throw new NoDataFoundException();

                return default(TResult);
            }

            if (resultList == null)
                return default(TResult);

            return (TResult) resultList;
        }

        /// <summary>
        /// Determines whether [is projection result] [the specified result type].
        /// </summary>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="dynamicCache">The dynamic cache.</param>
        /// <returns>
        /// 	<c>true</c> if [is projection result] [the specified result type]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsProjectionResult(Type resultType, Cache<Type, ProjectionClass> dynamicCache)
        {
            return resultType.IsValueObjectType()
                   || resultType.IsProjectedType(dynamicCache); //QueryData.ProjectionCache);
        }

        /// <summary>
        /// Returns the result
        /// </summary>
        /// <returns></returns>
        private IList GetResult(IDbCommand command)
        {
            if (IsProjectionResult(ElementType, DynamicCache) && !ElementType.IsGroupingType())
            {
                try
                {
                    // Re-Direct Projection Cache to Linq emulation
                    Mapper.MirroredLinqProjectionCache = DynamicCache;

                    IList mapperResult = Mapper.SelectNative(ElementType, command, level);

                    Type listAdapterType = typeof (ListAdapter<>).MakeGenericType(ElementType);
                    object listAdapter = Activator.CreateInstance(listAdapterType, mapperResult);
                    Type genericListType = typeof (List<>).MakeGenericType(ElementType);
                    object genericList = Activator.CreateInstance(genericListType, listAdapter);

                    return (IList) genericList;
                }
                finally
                {
                    // Remove Projection Cache emulation
                    Mapper.MirroredLinqProjectionCache = null;
                }
            }
            else
            {
                var nativePersister = (INativePersister) Mapper.Persister;

                // Read the resultset
                var resultSet = new ArrayList();
                IDataReader reader = nativePersister.ExecuteReader(command);
                while (reader.Read())
                    resultSet.Add(reader.GetValue(0));

                reader.Close();
                reader.Dispose();

                // Convert result
                Type genericListType = typeof (List<>).MakeGenericType(ElementType);
                var genericList = (IList) Activator.CreateInstance(genericListType, resultSet.Count);
                bool isGrouping = ElementType.IsGroupingType();

                if (isGrouping) // The complte object shall be grouped
                {
                    Type groupBy = ElementType.GetGenericArguments()[1];
                    PropertyTupel groupingType = groupings.Find(tupel => tupel.Source.ReflectedType == groupBy);

                    foreach (object row in resultSet)
                    {
                        object groupCriteria = row;
                        IList detail = Mapper.Select(groupBy,
                                                     new AndCondition(groupBy, groupingType.Source.Name, groupCriteria),
                                                     null,
                                                     level);

                        Type grouping = typeof (Grouping<,>).MakeGenericType(groupingType.SourceType, groupBy);
                        object groupByInstance = Activator.CreateInstance(grouping, new[] {groupCriteria, detail});
                        genericList.Add((T) groupByInstance);
                    }
                }
                else
                {
                    foreach (object row in resultSet)
                        genericList.Add((T) Persister.TypeMapper.ConvertToType(typeof (T), row));
                }

                return genericList;
            }
        }

        /// <summary>
        /// Gets the aggregate.
        /// </summary>
        /// <typeparam name="TResult">The type of the aggregate.</typeparam>
        /// <returns></returns>
        private TResult GetAggregate<TResult>(IDbCommand command)
        {
            IDataReader reader = null;

            try
            {
                // Re-Direct Projection Cache to Linq emulation
                Mapper.MirroredLinqProjectionCache = DynamicCache;

                if (IsProjectionResult(typeof (TResult), DynamicCache) && !typeof (TResult).IsGroupingType())
                {
                    IList mapperResult = Mapper.SelectNative(typeof (TResult), command, level);

#if DEBUG
                    // If an aggregation contains more than one, than the aggregation is wrong
                    Debug.Assert(mapperResult.Count <= 1, "Aggregation returned more than one row.");
#endif
                    return (mapperResult.Count == 1) ? (TResult) mapperResult[0] : default(TResult);
                }
                else
                {
                    TResult aggregateResult = default(TResult);
                    var nativePersister = (INativePersister) Mapper.Persister;
                    reader = nativePersister.ExecuteReader(command);
                    if (reader.Read())
                    {
                        aggregateResult =
                            (TResult) BasePersister.ConvertSourceToTargetType(reader.GetValue(0), typeof (TResult));

#if DEBUG
                        // If an aggregation contains more than one, than the aggregation is wrong
                        Debug.Assert(!reader.Read(), "Aggregation returned more than one row.");
#endif
                    }

                    return aggregateResult;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }

                // Remove Projection Cache emulation
                Mapper.MirroredLinqProjectionCache = null;
            }
        }

        /// <summary>
        /// Returns the stored sql id
        /// </summary>
        public string StoredSqlId
        {
            get
            {
                SelectExpression originalSelect = ExpressionTypeFinder.Find(compiledExpression, typeof(SelectExpression)) as SelectExpression;
                return originalSelect != null ? originalSelect.SqlId : null;
            }
        }

        /// <summary>
        /// Replaces the Sql Parameter in the string to convert them into valid sql parameters
        /// </summary>
        private void ReplaceSqlParamter(IDbCommand command)
        {
            HashSet<string> alreadyProcessed = new HashSet<string>();
            foreach (IDbDataParameter parameter in command.Parameters)
            {
                // Avoid replacing duplicated parameter names
                var replaceThis = parameter.ParameterName;
                if (alreadyProcessed.Contains(replaceThis))
                    continue;

                alreadyProcessed.Add(replaceThis);

                // count the amount of the same paramter within the sql
                var startIndex = 0;
                while ((startIndex = command.CommandText.IndexOf(replaceThis, startIndex)) > -1)
                {
                    var replaceWith = Persister.GetParameterString(parameter);

                    command.CommandText = command.CommandText.Remove(startIndex, replaceThis.Length);
                    command.CommandText = command.CommandText.Insert(startIndex, replaceWith);

                    startIndex += replaceWith.Length;
                }
            }
        }

        /// <summary>
        /// Rebinds the statement
        /// </summary>
        internal Query<T> RebindStatement(ObjectMapper mapper, object[] args, out IDbCommand command)
        {
            var result = new Query<T>(mapper);

            // Bind sql statement
            var persister = (ILinqPersister) mapper.Persister;
            IDbCommand newCommand = persister.CreateCommand();
            newCommand.CommandText = StoredSqlCommand;

            // Find first select expression
            string sqlId = StoredSqlId;
            if (!string.IsNullOrEmpty(sqlId))
                newCommand.CommandText = ExpressionOverride.Rewrite(sqlId, StoredSqlCommand);

            int localCounter = 0;
            // Bind sql parameter
            for (int parameterCounter = 0; parameterCounter < storedParamterCollection.Count; parameterCounter++)
            {
                var oldParameter = (IDbDataParameter) storedParamterCollection[parameterCounter];
                int argCounter = parameterMapping[GetPlainParameterName(oldParameter.ParameterName)];

                // Check if Values are explicit defined
                if (newCommand.CommandText.IndexOf("${" + oldParameter.ParameterName + "}") >= 0)
                {
                    // replace the parameter with the concrete value
                    newCommand.CommandText =
                        newCommand.CommandText.Replace("${" + oldParameter.ParameterName + "}",
                                                       persister.TypeMapper.GetParamValueAsSQLString(args[argCounter]));
                }

                // Check for NULL Values
                if (persister.TypeMapper.IsDbNull(args[argCounter]))
                {
                    // replace the parameter name
                    newCommand.CommandText =
                        newCommand.CommandText.Replace("= " + oldParameter.ParameterName, "IS NULL").Replace(
                            "<> " + oldParameter.ParameterName, "IS NOT NULL");

                    // And including the HEXTORAW Helper for Oracle !
                    newCommand.CommandText =
                        newCommand.CommandText.Replace("= HEXTORAW(" + oldParameter.ParameterName+")", "IS NULL").Replace(
                            "<> HEXTORAW(" + oldParameter.ParameterName + ")", "IS NOT NULL");
                    continue;
                }

                // Maybe the parameter is an associative array
                if (args[argCounter].GetType().IsListType())
                {
                    string replaceWith = string.Empty;
                    bool first = true;
                    foreach (object parameter in (IEnumerable) args[argCounter])
                    {
                        if (!first) replaceWith += ", ";
                        IDbDataParameter dbParameter = persister.AddParameter(newCommand.Parameters, ref localCounter,
                                                                              parameter.GetType(), parameter, false);
                        replaceWith += persister.GetParameterString(dbParameter);
                        first = false;
                    }

                    newCommand.CommandText = newCommand.CommandText.Replace(oldParameter.ParameterName, replaceWith);
                    continue;
                }

                bool duplicateParameter = mapper.Persister.TypeMapper.ParameterDuplication;

                // count the amount of the same paramter within the sql
                var startIndex = 0;
                var count = 0;
                while ((startIndex = newCommand.CommandText.IndexOf(oldParameter.ParameterName, startIndex)) > -1)
                {
                    count++;

                    var replaceWith = persister.GetParameterString(oldParameter);
                    startIndex += replaceWith.Length;

                    newCommand.CommandText = newCommand.CommandText.ReplaceFirst(oldParameter.ParameterName, replaceWith);
                }
    
                // Only duplicate parameter, if that is allowed
                if (!duplicateParameter && count > 1)
                    count = 1;

                // add the parameter
                for (int x = 0; x < count; x++)
                {
                    IDbDataParameter newParameter = persister.CreateParameter(oldParameter, args[argCounter]);
                    newCommand.Parameters.Add(newParameter);
                }
            }

            command = newCommand;
            result.compiledExpression = compiledExpression;
            result.DynamicCache = DynamicCache;
            result.level = level;
            return result;
        }

        /// <summary>
        /// Gets the plain parameter name, without the leading 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private string GetPlainParameterName(string name)
        {
            char firstChar = name[0];
            if (firstChar == ':' || firstChar == '@' || firstChar == '?')
                return name.Substring(1);

            return name;
        }

        #region Query Extension Methods

        /// <summary>
        /// Placeholder query for the SqlId
        /// </summary>
        private IQueryable<T> SqlId(Expression expression, string sqlId)
        {
            return this; // This method is only evaluated by the QueryBinder
        }

        /// <summary>
        /// Placeholder query for hinting sqls
        /// </summary>
        private IQueryable<T> Hint(Expression expression, string hint)
        {
            return this; // This method is only evaluated by the QueryBinder
        }

        #endregion
    }
}