using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Fields;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Queries;
using AdFactum.Data.Internal;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;
#if VS2008
using AdFactum.Data.Linq;
#endif

namespace AdFactum.Data
{
    /// <summary>
    /// This is the Database Mapper. The mapper offers all functionality to store objects in database, load objects and manage transactions.
    /// </summary>
    public class ObjectMapper : ITransactionContext
    {
        #region Private members

        /// <summary>
        /// Points to an object Factory that is used to create the business entities.
        /// </summary>
        private IObjectFactory objectFactory;

        /// <summary>
        /// Defines a transaction
        /// </summary>
        private ITransactionContext transactionContext;

        /// <summary>
        /// Defines, if the ObjectMapper .NET uses a borrowed transaction.
        /// If true, the ObjectMapper .NET won't dispose the transaction context
        /// because it may be used within another ObjectMapper
        /// </summary>
        private readonly bool borrowedTransaction;

        #endregion


        #region Public Getter and Setter

        /// <summary>
        /// Returns the object factory in order to create new business entities.
        /// </summary>
        public IObjectFactory ObjectFactory
        {
            get { return objectFactory; }
            set { objectFactory = value; }
        }

        #endregion

        #region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ObjectMapper"/> class.
		/// </summary>
		protected ObjectMapper ()
		{
		}

        /// <summary>
        /// Constructor that creates a AdFactum object mapper instance with automatic
        /// transactions and single process automation.
        /// Non referenced objects will be deleted automaticly from database.
        /// </summary>
        /// <param name="pPersister">An persister handles the core tasks to access the database instance. The AdFactum object mapper knows four kind of persisters.</param>
        public ObjectMapper(
            IPersister pPersister)
            : this(new UniversalFactory(),
                   pPersister,
                   Transactions.Automatic)
        {
        }
        
        /// <summary>
        /// Constructor that creates a AdFactum object mapper instance with automatic
        /// transactions and single process automation.
        /// Non referenced objects will be deleted automaticly from database.
        /// </summary>
        /// <param name="pObjectFactory">An object factory is used by the AdFactum Object Mapper in order to create new instances of value objects.</param>
        /// <param name="pPersister">An persister handles the core tasks to access the database instance. The AdFactum object mapper knows four kind of persisters.</param>
        public ObjectMapper(
            IObjectFactory pObjectFactory,
            IPersister pPersister)
            : this(pObjectFactory,
                   pPersister,
                   Transactions.Automatic)
        {
        }

        /// <summary>
        /// This constructor is used for creating an Ad-Factum object mapper instance.
        /// </summary>
        /// <param name="pPersister">An persister handles the core tasks to access the database instance. The AdFactum object mapper knows four kind of persisters.</param>
        /// <param name="pTransaction">The transaction settings are used to define how the AdFactum Object mapper handles transactions. </param>
        public ObjectMapper(
            IPersister pPersister,
            Transactions pTransaction)
            : this(new UniversalFactory(), pPersister, pTransaction)
        {
        }

        /// <summary>
        /// This constructor is used for creating an Ad-Factum object mapper instance.
        /// </summary>
        /// <param name="pObjectFactory">An object factory is used by the AdFactum Object Mapper in order to create new instances of value objects.</param>
        /// <param name="pPersister">An persister handles the core tasks to access the database instance. The AdFactum object mapper knows four kind of persisters.</param>
        /// <param name="pTransaction">The transaction settings are used to define how the AdFactum Object mapper handles transactions. </param>
        public ObjectMapper(
            IObjectFactory pObjectFactory,
            IPersister pPersister,
            Transactions pTransaction)
        {
            transactionContext = new TransactionContext(pPersister, pTransaction);
            objectFactory = pObjectFactory;
        }

        /// <summary>
        /// This constructor is used for creating an Ad-Factum object mapper instance.
        /// </summary>
        /// <param name="pObjectFactory">An object factory is used by the AdFactum Object Mapper in order to create new instances of value objects.</param>
        /// <param name="pPersister">An persister handles the core tasks to access the database instance. The AdFactum object mapper knows four kind of persisters.</param>
        /// <param name="pTransaction">The transaction settings are used to define how the AdFactum Object mapper handles transactions.</param>
        /// <param name="pApplicationName">Name of the p application.</param>
        public ObjectMapper(
            IObjectFactory pObjectFactory,
            IPersister pPersister,
            Transactions pTransaction,
            string pApplicationName
            )
        {
            transactionContext = new TransactionContext(pPersister, pTransaction);
            objectFactory = pObjectFactory;
            ApplicationName = pApplicationName;
        }

        /// <summary>
        /// This constructor is used for creating an Ad-Factum object mapper instance.
        /// </summary>
        /// <param name="pObjectFactory">An object factory is used by the AdFactum Object Mapper in order to create new instances of value objects.</param>
        /// <param name="pPersister">An persister handles the core tasks to access the database instance. The AdFactum object mapper knows four kind of persisters.</param>
        /// <param name="pTransaction">The transaction settings are used to define how the AdFactum Object mapper handles transactions.</param>
        /// <param name="pApplicationName">Name of the application.</param>
        /// <param name="pDatabaseVersion">The version information.</param>
        public ObjectMapper(
            IObjectFactory pObjectFactory,
            IPersister pPersister,
            Transactions pTransaction,
            string pApplicationName,
            double pDatabaseVersion
            )
        {
            transactionContext = new TransactionContext(pPersister, pTransaction);
            objectFactory = pObjectFactory;
            ApplicationName = pApplicationName;
            DatabaseVersion = pDatabaseVersion;
        }

        /// <summary>
        /// This constructor is used for creating an Ad-Factum object mapper instance.
        /// </summary>
        /// <param name="pObjectFactory">An object factory is used by the AdFactum Object Mapper in order to create new instances of value objects.</param>
        /// <param name="pPersister">An persister handles the core tasks to access the database instance. The AdFactum object mapper knows four kind of persisters.</param>
        /// <param name="pTransaction">The transaction settings are used to define how the AdFactum Object mapper handles transactions.</param>
        /// <param name="pApplicationName">Name of the application.</param>
        /// <param name="pDatabaseMajorVersion">The p database major version.</param>
        /// <param name="pDatabaseMinorVersion">The version information.</param>
        public ObjectMapper(
            IObjectFactory pObjectFactory,
            IPersister pPersister,
            Transactions pTransaction,
            string pApplicationName,
            string pDatabaseMajorVersion,
            string pDatabaseMinorVersion
            )
        {
            transactionContext = new TransactionContext(pPersister, pTransaction);
            objectFactory = pObjectFactory;
            ApplicationName = pApplicationName;
            DatabaseMajorVersion = pDatabaseMajorVersion;
            DatabaseMinorVersion = pDatabaseMinorVersion;
        }

        /// <summary>
        /// Constructor for creating an adfactum object mapper.
        /// </summary>
        /// <param name="pObjectFactory">Object Factory</param>
        /// <param name="transactionContextHandler">Transaction Handler</param>
        public ObjectMapper(
            IObjectFactory pObjectFactory,
            ITransactionContext transactionContextHandler
            )
        {
            transactionContext = transactionContextHandler;
            objectFactory = pObjectFactory;
            borrowedTransaction = true;
        }

        #endregion

        #region Methods that returns a single object

        /// <summary>
        /// This method is used for flat loading value objects.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="id">The id parameter defines the primary id of the object that has to be loaded. Every ValueObject has a primary key property (named as Id) which is from type GUID.</param>
        /// <returns>A value object of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public object FlatLoad(Type type, object id)
        {
            return Load(type, id, 0);
        }

        /// <summary>
        /// This method is used for flat loading value objects.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="id">The id parameter defines the primary id of the object that has to be loaded. Every ValueObject has a primary key property (named as Id) which is from type GUID.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A value object of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public object FlatLoad(Type type, object id, IDictionary globalParameter)
        {
            return Load(type, id, HierarchyLevel.FlatObject, globalParameter);
        }

        /// <summary>
        /// This method is used for flat loading value objects.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="condition">The condition.</param>
        /// <returns>
        /// A value object of the parameterized type.
        /// </returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public object FlatLoad(Type type, ICondition condition)
        {
            return Load(type, condition, 0);
        }

        /// <summary>
        /// This method is used for flat loading value objects.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>
        /// A value object of the parameterized type.
        /// </returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public object FlatLoad(Type type, ICondition condition, IDictionary globalParameter)
        {
            return Load(type, condition, HierarchyLevel.FlatObject, globalParameter);
        }

        /// <summary>
        /// This method is used for deep loading a value object.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="condition">The condition.</param>
        /// <returns>
        /// A value object of the parameterized type.
        /// </returns>
        public object Load(Type type, ICondition condition)
        {
            return Load(type, condition, int.MaxValue);
        }

        /// <summary>
        /// This method is used for deep loading a value object.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>
        /// A value object of the parameterized type.
        /// </returns>
        public object Load(Type type, ICondition condition, IDictionary globalParameter)
        {
            return Load(type, condition, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This method is used for deep loading a value object.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the object shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <returns>
        /// A value object of the parameterized type.
        /// </returns>
        public object Load(Type type, ICondition condition, int hierarchyLevel)
        {
            return Load(type, condition, hierarchyLevel, null);
        }

        /// <summary>
        /// This method is used for deep loading a value object.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the object shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>
        /// A value object of the parameterized type.
        /// </returns>
        public object Load(Type type, ICondition condition, int hierarchyLevel, IDictionary globalParameter)
        {
            IList result = Paging(type, condition, null, 1, 1, hierarchyLevel, globalParameter);
            if (result.Count > 0)
                return result[0];

            return null;
        }

        /// <summary>
        /// This method is used for deep loading a value object.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="id">The id parameter defines the primary id of the object that has to be loaded. Every ValueObject has a primary key property (named as Id) which is from type GUID.</param>
        /// <returns>A value object of the parameterized type.</returns>
        public object Load(Type type, object id)
        {
            return Load(type, id, int.MaxValue);
        }

        /// <summary>
        /// This method is used for deep loading a value object.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="id">The id parameter defines the primary id of the object that has to be loaded. Every ValueObject has a primary key property (named as Id) which is from type GUID.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A value object of the parameterized type.</returns>
        public object Load(Type type, object id, IDictionary globalParameter)
        {
            return Load(type, id, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This method is used for deep loading a value object.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="id">The id parameter defines the primary id of the object that has to be loaded. Every ValueObject has a primary key property (named as Id) which is from type GUID.</param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the object shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <returns>A value object of the parameterized type.</returns>
        public object Load(Type type, object id, int hierarchyLevel)
        {
            return Load(type, id, hierarchyLevel, null);
        }

        /// <summary>
        /// This method is used for deep loading a value object.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="id">The id parameter defines the primary id of the object that has to be loaded. Every ValueObject has a primary key property (named as Id) which is from type GUID.</param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the object shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A value object of the parameterized type.</returns>
        public object Load(Type type, object id, int hierarchyLevel, IDictionary globalParameter)
        {
            /*
			 * Neuen Ladehash anlegen, um Rekursionen zu vermeiden
			 */
            ObjectHash tempHash = UpdateHash();

            /*
			 * Das Objekt laden
			 */
            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
            object vo = PrivateLoad(projection, id, tempHash, hierarchyLevel, globalParameter);

            MergeHash(tempHash);
            return vo;
        }

        #endregion

        #region Methods that returns an object list

        /// <summary>
        /// This method is used for flat loading value objects.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="ids">This parameter is used for loading a list of objects. Therefore the list has to contain only GUID ids for all the value objects that shall be loaded.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatLoad(Type type, IList ids)
        {
            return Load(type, ids, 0);
        }

        /// <summary>
        /// This method is used for flat loading value objects.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="ids">This parameter is used for loading a list of objects. Therefore the list has to contain only GUID ids for all the value objects that shall be loaded.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatLoad(Type type, IList ids, IDictionary globalParameter)
        {
            return Load(type, ids, HierarchyLevel.FlatObject, globalParameter);
        }

        /// <summary>
        /// This method is used for deep loading value objects.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="ids">This parameter is used for loading a list of objects. Therefore the list has to contain only GUID ids for all the value objects that shall be loaded.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList Load(Type type, IList ids)
        {
            return Load(type, ids, int.MaxValue);
        }

        /// <summary>
        /// This method is used for deep loading value objects.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="ids">This parameter is used for loading a list of objects. Therefore the list has to contain only GUID ids for all the value objects that shall be loaded.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList Load(Type type, IList ids, IDictionary globalParameter)
        {
            return Load(type, ids, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This method is used for deep loading value objects.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="ids">This parameter is used for loading a list of objects. Therefore the list has to contain only GUID ids for all the value objects that shall be loaded.</param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the object shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <returns>Returns a list with objects.</returns>
        public IList Load(Type type, IList ids, int hierarchyLevel)
        {
            return Load(type, ids, hierarchyLevel, null);
        }

        /// <summary>
        /// This method is used for deep loading value objects.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the result value shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="ids">This parameter is used for loading a list of objects. Therefore the list has to contain only GUID ids for all the value objects that shall be loaded.</param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the object shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a list with objects.</returns>
        public IList Load(Type type, IList ids, int hierarchyLevel, IDictionary globalParameter)
        {
            IList result = new ArrayList();

            /*
			 * Neuen Ladehash anlegen, um Rekursionen zu vermeiden
			 */
            ObjectHash tempHash = UpdateHash();
            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
            
            /*
			 * Das Objekte laden
			 */
            foreach (object id in ids)
            {
                object vo = PrivateLoad(projection, id, tempHash, hierarchyLevel, globalParameter);
                result.Add(vo);
            }

            MergeHash(tempHash);
            return result;
        }

        #endregion

        #region Methods for using regular object mapper selects

        /// <summary>
        /// This method is used for flat selecting a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatDistinctSelect(Type type)
        {
            return FlatDistinctSelect(type, null);
        }

        /// <summary>
        /// This method is used for flat selecting a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatDistinctSelect(Type type, ICondition whereClause)
        {
            return DistinctSelect(type, whereClause, null, 0);
        }

        /// <summary>
        /// This method is used for flat selecting a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatSelect(Type type)
        {
            return FlatSelect(type, null);
        }

        /// <summary>
        /// This method is used for flat selecting a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatSelect(Type type, ICondition whereClause)
        {
            return Select(type, whereClause, null, 0);
        }

        /// <summary>
        /// This method is used for flat selecting a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatDistinctSelect(Type type, ICondition whereClause, OrderBy orderBy)
        {
            return DistinctSelect(type, whereClause, orderBy, 0);
        }

        /// <summary>
        /// This method is used for flat selecting a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatSelect(Type type, ICondition whereClause, OrderBy orderBy)
        {
            return Select(type, whereClause, orderBy, 0);
        }

        /// <summary>
        /// This method is used for flat selecting a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatDistinctSelect(Type type, ICondition whereClause, IDictionary globalParameter)
        {
            return DistinctSelect(type, whereClause, null, HierarchyLevel.FlatObject, globalParameter);
        }

        /// <summary>
        /// This method is used for flat selecting a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>
        /// A list with value objects of the parameterized type.
        /// </returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatSelect(Type type, ICondition whereClause, OrderBy orderBy, IDictionary globalParameter)
        {
            return Select(type, whereClause, orderBy, HierarchyLevel.FlatObject, globalParameter);
        }

        /// <summary>
        /// This method is used for flat selecting a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>
        /// A list with value objects of the parameterized type.
        /// </returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatDistinctSelect(Type type, ICondition whereClause, OrderBy orderBy, IDictionary globalParameter)
        {
            return DistinctSelect(type, whereClause, orderBy, HierarchyLevel.FlatObject, globalParameter);
        }

        /// <summary>
        /// This method is used for flat selecting a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatSelect(Type type, ICondition whereClause, IDictionary globalParameter)
        {
            return Select(type, whereClause, null, HierarchyLevel.FlatObject, globalParameter);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList DistinctSelect(Type type)
        {
            return DistinctSelect(type, null, null, int.MaxValue);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList Select(Type type)
        {
            return Select(type, null, null, int.MaxValue);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList DistinctSelect(Type type, ICondition whereClause)
        {
            return DistinctSelect(type, whereClause, null, int.MaxValue);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList Select(Type type, ICondition whereClause)
        {
            return Select(type, whereClause, null, int.MaxValue);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList DistinctSelect(Type type, ICondition whereClause, IDictionary globalParameter)
        {
            return DistinctSelect(type, whereClause, null, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList Select(Type type, ICondition whereClause, IDictionary globalParameter)
        {
            return Select(type, whereClause, null, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList DistinctSelect(Type type, ICondition whereClause, OrderBy orderBy)
        {
            return DistinctSelect(type, whereClause, orderBy, int.MaxValue);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList Select(Type type, ICondition whereClause, OrderBy orderBy)
        {
            return Select(type, whereClause, orderBy, int.MaxValue);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList DistinctSelect(Type type, ICondition whereClause, OrderBy orderBy, IDictionary globalParameter)
        {
            return DistinctSelect(type, whereClause, orderBy, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList Select(Type type, ICondition whereClause, OrderBy orderBy, IDictionary globalParameter)
        {
            return Select(type, whereClause, orderBy, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList DistinctSelect(Type type, ICondition whereClause, OrderBy orderBy, int hierarchyLevel)
        {
            return DistinctSelect(type, whereClause, orderBy, hierarchyLevel, null);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList Select(Type type, ICondition whereClause, OrderBy orderBy, int hierarchyLevel)
        {
            return Select(type, whereClause, orderBy, hierarchyLevel, null);
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList Select(Type type, ICondition whereClause, OrderBy orderBy, int hierarchyLevel,
                            IDictionary globalParameter)
        {
            IList result = new ArrayList();

            /*
			 * Neuen Ladehash anlegen, um Rekursionen zu vermeiden
			 */
            ObjectHash tempHash = UpdateHash();

            /*
			 * Eine Liste der POs laden
			 */
            IList resultPOs =
                PrivateSelect(type, whereClause, orderBy, tempHash, hierarchyLevel, globalParameter, false);

            /*
			 * Die Liste durchlaufen und die VOs holen
			 */
            foreach (PersistentObject po in resultPOs)
                result.Add(po.GetTemporaryCreated() ?? tempHash.GetVO(po.ObjectType, po.Id));

            MergeHash(tempHash);
            return result;
        }

        /// <summary>
        /// This methods selects a list of objects that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList DistinctSelect(Type type, ICondition whereClause, OrderBy orderBy, int hierarchyLevel,
                                    IDictionary globalParameter)
        {
            IList result = new ArrayList();

            /*
			 * Neuen Ladehash anlegen, um Rekursionen zu vermeiden
			 */
            ObjectHash tempHash = UpdateHash();

            /*
			 * Eine Liste der POs laden
			 */
            IList resultPOs = PrivateSelect(type, whereClause, orderBy, tempHash, hierarchyLevel, globalParameter, true);

            /*
			 * Die Liste durchlaufen und die VOs holen
			 */
            foreach (PersistentObject po in resultPOs)
                result.Add(po.GetTemporaryCreated() ?? tempHash.GetVO(po.ObjectType, po.Id));

            MergeHash(tempHash);
            return result;
        }

        #endregion

        #region Methods for using native SQL selects or stored procedures

        /// <summary>
        /// This methods selects a list of objects from the result of the given select.
        /// This method is strongly dependend from the persister that is used and therefore it should be used very carefully.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="select">This parameter defines a native SQL call. It can be either a select string like 'select * from xyz where ...' or a call to a stored procedure in database. However the result must match the columns of the select parameter type in order to map the result correctly to the instance of an object.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatSelectNative(Type type, string select)
        {
            return SelectNative(type, select, null, 0);
        }

        /// <summary>
        /// Select the objects native by using a pre-defined IDbCommand
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public IList FlatSelectNative(Type type, IDbCommand command)
        {
            return SelectNative(type, command, 0);
        }


        /// <summary>
        /// This methods selects a list of objects from the result of the given select.
        /// The placeholder within the select string will be replaced by the used Persister. This method is strongly
        /// dependend from the persister that is used and therefore it should be used very carefully.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="select">This parameter defines a native SQL call. It can be either a select string like 'select * from xyz where ...' or a call to a stored procedure in database. However the result must match the columns of the select parameter type in order to map the result correctly to the instance of an object.</param>
        /// <param name="parameter">This parameter is used parameterize the native select string. It's possible to provide template strings and fill the strings up with the parameters. The list is sorted, because it is indispensable that the parameters are used in the correct ordering. Additionally it's mandatory that the parameter always starts with an @.
        /// <code>
        /// string     selectTemplate = "SELECT * FROM CONTACT WHERE Locale=@param1";
        /// SortedList parameter = new SortedList();
        /// 
        /// parameter.Add ("@param1", "de-DE");
        ///	IList result = mapper.Select (typeof(Contact), selectTemplate, parameter);
        /// </code>
        /// </param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        /// <remarks>Only the core attributes of the value objects are loaded. Nested value objects or dependencies are kept untouched.</remarks>
        public IList FlatSelectNative(Type type, string select, SortedList parameter)
        {
            return SelectNative(type, select, parameter, 0);
        }


        /// <summary>
        /// This methods selects a list of objects from the result of the given select.
        /// This method is strongly dependend from the persister that is used and therefore it should be used very carefully.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="select">This parameter defines a native SQL call. It can be either a select string like 'select * from xyz where ...' or a call to a stored procedure in database. However the result must match the columns of the select parameter type in order to map the result correctly to the instance of an object.</param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList SelectNative(Type type, string select)
        {
            return SelectNative(type, select, null, int.MaxValue);
        }

        /// <summary>
        /// This methods selects a list of objects from the result of the given select.
        /// The placeholder within the select string will be replaced by the used Persister. This method is strongly
        /// dependend from the persister that is used and therefore it should be used very carefully.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="select">This parameter defines a native SQL call. It can be either a select string like 'select * from xyz where ...' or a call to a stored procedure in database. However the result must match the columns of the select parameter type in order to map the result correctly to the instance of an object.</param>
        /// <param name="parameter">This parameter is used parameterize the native select string. It's possible to provide template strings and fill the strings up with the parameters. The list is sorted, because it is indispensable that the parameters are used in the correct ordering. Additionally it's mandatory that the parameter always starts with an @.
        /// <code>
        /// string     selectTemplate = "SELECT * FROM CONTACT WHERE Locale=@param1";
        /// SortedList parameter = new SortedList();
        /// 
        /// parameter.Add ("@param1", "de-DE");
        ///	IList result = mapper.Select (typeof(Contact), selectTemplate, parameter);
        /// </code>
        /// </param>
        /// <returns>A list with value objects of the parameterized type.</returns>
        public IList SelectNative(Type type, string select, SortedList parameter)
        {
            return SelectNative(type, select, parameter, int.MaxValue);
        }

        /// <summary>
        /// This methods selects a list of objects from the result of the given select.
        /// The placeholder within the select string will be replaced by the used Persister. This method is strongly
        /// dependend from the persister that is used and therefore it should be used very carefully.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="select">This parameter defines a native SQL call. It can be either a select string like 'select * from xyz where ...' or a call to a stored procedure in database. However the result must match the columns of the select parameter type in order to map the result correctly to the instance of an object.</param>
        /// <param name="parameter">This parameter is used parameterize the native select string. It's possible to provide template strings and fill the strings up with the parameters. The list is sorted, because it is indispensable that the parameters are used in the correct ordering. Additionally it's mandatory that the parameter always starts with an @.
        /// 	<code>
        /// string     selectTemplate = "SELECT * FROM CONTACT WHERE Locale=@param1";
        /// SortedList parameter = new SortedList();
        /// parameter.Add ("@param1", "de-DE");
        /// IList result = mapper.Select (typeof(Contact), selectTemplate, parameter);
        /// </code></param>
        /// <param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <returns>
        /// A list with value objects of the parameterized type.
        /// </returns>
        public IList SelectNative(Type type, string select, SortedList parameter, int hierarchyLevel)
        {
            IList result = new ArrayList();

            /*
			 * Neuen Ladehash anlegen, um Rekursionen zu vermeiden
			 */
            ObjectHash tempHash = UpdateHash();

            /*
			 * Eine Liste der POs laden
			 */
            IList resultPOs = PrivateSelect(type, select, parameter, tempHash, hierarchyLevel);

            /*
			 * Die Liste durchlaufen und die VOs holen
			 */
            foreach (PersistentObject po in resultPOs)
                result.Add(po.GetTemporaryCreated() ?? tempHash.GetVO(po.ObjectType, po.Id));

            MergeHash(tempHash);

            return result;
        }

        /// <summary>
        /// Select the objects native by using a pre-defined IDbCommand
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="command">The command.</param>
        /// <param name="hierarchyLevel"></param>
        /// <returns></returns>
        public IList SelectNative(Type type, IDbCommand command, int hierarchyLevel)
        {
            IList result = new ArrayList();

            /*
			 * Neuen Ladehash anlegen, um Rekursionen zu vermeiden
			 */
            ObjectHash tempHash = UpdateHash();

            /*
			 * Eine Liste der POs laden
			 */
            IList resultPOs = PrivateSelect(type, command, tempHash, hierarchyLevel);

            /*
			 * Die Liste durchlaufen und die VOs holen
			 */
            foreach (PersistentObject po in resultPOs)
                result.Add(po.GetTemporaryCreated() ?? tempHash.GetVO(po.ObjectType, po.Id));

            MergeHash(tempHash);

            return result;
        }

        #endregion

        #region Methods for handling collections

        /// <summary>
        /// Add a new element to an existing object collection
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper the type of the object, that contains the collection, to which the new element shall be added.</param>
        /// <param name="propertyName">This parameter defines the name of the collection property within the typed class. The type of the collection property must be derived from IList.</param>
        /// <param name="parentObjectId">This parameter defines the unique object id of the parent object to which the new value object shall be added.</param>
        /// <param name="elementId">This parameter contains id of the new value object that shall be added to a collection.</param>
        /// <param name="childType">Type of the child element which has to be removed</param>
        /// <remarks>Only one element can be added with this method to a collection within one transaction.
        /// That's because the array counter is only incremented after an commmit.</remarks>
        public void AddToCollection(Type type, string propertyName, object parentObjectId, Type childType, object elementId)
        {
            FieldDescription collectionFieldDescription =
                ReflectionHelper.GetStaticFieldTemplate(type, propertyName,
                                                        (this as ITransactionContext).DatabaseMajorVersion,
                                                        (this as ITransactionContext).DatabaseMinorVersion);

            /*
			 * Prüfen, ob eine Transaction geöffnet wurde
			 */
            if (TransactionSetting == Transactions.Manual)
                CheckOpenTransaction();

            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
            var childProjection = ReflectionHelper.GetProjection(childType, MirroredLinqProjectionCache);

            /*
			 * Laden durchführen
			 */
            ObjectHash tempHash = UpdateHash();
            PrivateLoad(childProjection, elementId, tempHash, HierarchyLevel.FlatObject, null);
            PersistentObject linkPo = tempHash.Get(childType, elementId);

            /*
			 * Neuen Index selektieren
			 */
            FieldDescription listDescription =
                ReflectionHelper.GetStaticFieldTemplate(type, propertyName,
                                                        (this as ITransactionContext).DatabaseMajorVersion,
                                                        (this as ITransactionContext).DatabaseMinorVersion);

            /*
			 * Neues PO mit Parent Guid anlegen
			 */
            PersistentObject virtualPO = null;
            object virtualLoaded = null;
            if (tempHash.Contains(type, parentObjectId))
            {
                virtualPO = tempHash.Get(type, parentObjectId);
                virtualLoaded = tempHash.GetVO(type, parentObjectId);
            }

            if ((virtualPO == null) || (virtualPO.IsFlatLoaded))
            {
                virtualLoaded = PrivateLoad(projection, parentObjectId, tempHash, HierarchyLevel.Dependend1stLvl, null);
                virtualPO = tempHash.Get(type, parentObjectId);
            }

            /*
			 * Existiert schon eine Liste für einen Listentyp ? 
			 */
            Dictionary<string, IModification> propertyList;
            if (!virtualPO.Properties.ListProperties.TryGetValue(collectionFieldDescription.Name, out propertyList))
            {
                propertyList = new Dictionary<string, IModification>();
                virtualPO.Properties.ListProperties = virtualPO.Properties.ListProperties.Add(collectionFieldDescription.Name, propertyList);
            }

            /*
             * Add it to the property List, if it does not exist
             */
            string key = ConstraintSaveList.CalculateKey(childType, elementId);
            IModification existingLink;
            if (!propertyList.TryGetValue(key, out existingLink))
            {
                var link = new ListLink(listDescription.CustomProperty.MetaInfo.LinkTarget!=null, virtualPO, linkPo);
                propertyList.Add(key, link);
            }
            else
                existingLink.IsDeleted = false;

            var virtualVO = virtualLoaded as IValueObject;
            if (virtualVO != null)
                tempHash.Add(virtualPO, virtualVO);

            /*
			 * In die Datenbank schreiben
			 */
            AutoTransaction(tempHash, null);
        }


        /// <summary>
        /// Removes an element from an existing object collection
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper the type of the object, that contains the collection, from which the element shall be removed.</param>
        /// <param name="propertyName">This parameter defines the name of the collection property within the typed class. The type of the collection property must be derived from IList.</param>
        /// <param name="parentObjectId">This parameter defines the unique object id of the parent object from which the new value object shall be removed.</param>
        /// <param name="childType">Type of the child.</param>
        /// <param name="elementId">This parameter contains the new value object that shall be added to a collection.</param>
        public void RemoveFromCollection(Type type, string propertyName, object parentObjectId, Type childType, object elementId)
        {
            FieldDescription collectionFieldDescription =
                ReflectionHelper.GetStaticFieldTemplate(type, propertyName,
                                                        (this as ITransactionContext).DatabaseMajorVersion,
                                                        (this as ITransactionContext).DatabaseMinorVersion);

            /*
			 * Prüfen, ob eine Transaction geöffnet wurde
			 */
            CheckOpenTransaction();

            /*
			 * Speichervorgang durchführen
			 */
            ObjectHash tempHash = UpdateHash();

            /*
			 * Neues PO mit Parent Guid anlegen
			 */
            PersistentObject virtualPO = null;
            object virtualLoaded = null;
            if (tempHash.Contains(type, parentObjectId))
            {
                virtualPO = tempHash.Get(type, parentObjectId);
                virtualLoaded = tempHash.GetVO(type, parentObjectId);
            }

            if ((virtualPO == null) || (virtualPO.IsFlatLoaded))
            {
                var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
                virtualLoaded = PrivateLoad(projection, parentObjectId, tempHash, HierarchyLevel.Dependend1stLvl, null);
                virtualPO = tempHash.Get(type, parentObjectId);
            }

            /*
			 * Existiert schon eine Liste für einen Listentyp ? 
			 */
            Dictionary<string, IModification> propertyList;
            if (!virtualPO.Properties.ListProperties.TryGetValue(collectionFieldDescription.Name, out propertyList))
            {
                propertyList = new Dictionary<string, IModification>();
                virtualPO.Properties.ListProperties = virtualPO.Properties.ListProperties.Add(collectionFieldDescription.Name, propertyList);
            }

            /*
			 * Search the item
			 */
            string key = ConstraintSaveList.CalculateKey(childType, elementId);
            IModification existingLink;
            if (propertyList.TryGetValue(key, out existingLink))
            {
                var element = (ListLink)existingLink;

                /*
                 * Remove Link
                 */
                element.SetLinkedObject(null, tempHash, this);
                
                /*
				 * Persistent Object zum Hash hinzufügen
				 */
                var virtualVO = virtualLoaded as IValueObject;
                if (virtualVO != null)
                    tempHash.Add(virtualPO, virtualVO);

                /*
				 * In die Datenbank schreiben
				 */
                AutoTransaction(tempHash, null);
            }
        }

        #endregion

        #region Methods for handling hashtables

        /// <summary>
        /// Add a new element to an existing object hashtable
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper the type of the object, that contains the collection, to which the new element shall be added.</param>
        /// <param name="propertyName">This parameter defines the name of the collection property within the typed class. The type of the collection property must be derived from IDictionary.</param>
        /// <param name="parentObjectId">This parameter defines the unique object id of the parent object to which the new value object shall be added.</param>
        /// <param name="key">This parameter is only used for properties that implements the IDictionary interface. It's used instead of a sequential index like in a collection in order to access the value objects by using a key.</param>
        /// <param name="childType">Type of the child element which has to be removed</param>
        /// <param name="elementId">This parameter contains the new value object id that shall be added to a collection.</param>
        public void AddToHashtable(Type type, string propertyName, object parentObjectId, object key, Type childType,
                                   object elementId)
        {
            FieldDescription collectionFieldDescription =
                ReflectionHelper.GetStaticFieldTemplate(type, propertyName,
                                                        (this as ITransactionContext).DatabaseMajorVersion,
                                                        (this as ITransactionContext).DatabaseMinorVersion);

            /*
			 * Prüfen, ob eine Transaction geöffnet wurde
			 */
            CheckOpenTransaction();

            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
            var childProjection = ReflectionHelper.GetProjection(childType, MirroredLinqProjectionCache);

            /*
			 * Load Object
			 */
            ObjectHash tempHash = UpdateHash();
            PrivateLoad(childProjection, elementId, tempHash, HierarchyLevel.FlatObject, null);
            PersistentObject linkPo = tempHash.Get(childType, elementId);

            /*
			 * Neues PO mit Parent Guid anlegen
			 */
            PersistentObject virtualPO;
            if (tempHash.Contains(type, parentObjectId))
                virtualPO = tempHash.Get(type, parentObjectId);
            else
            {
                virtualPO = new PersistentObject(ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache), parentObjectId)
                                {
                                    IsNew = false,
                                    IsModified = true,
                                    IsFlatLoaded = true
                                };
            }

            /*
			 * Existiert schon eine Liste für einen Listentyp ? 
			 */
            Dictionary<object, IModification> propertyList;
            if (!virtualPO.Properties.DictProperties.TryGetValue(collectionFieldDescription.Name, out propertyList))
            {
                propertyList = new Dictionary<object, IModification>();
                virtualPO.Properties.DictProperties = virtualPO.Properties.DictProperties.Add(collectionFieldDescription.Name, propertyList);
            }

            /*
			 * Existiert der Counter Eintrag bereits ? 
			 */
            ListLink dictionaryLink;
            if (propertyList.ContainsKey(key))
            {
                /*
				 * Dann den bestehenden Eintrag überschreiben
				 */
                dictionaryLink = (ListLink) propertyList[key];
                dictionaryLink.SetLinkedObject(linkPo, tempHash, this);
            }
            else
            {
                /*
				 * Einen neuen anlegen und einfügen
				 */
                dictionaryLink = new ListLink(collectionFieldDescription.CustomProperty.MetaInfo.LinkTarget,
                                              virtualPO, linkPo, key);
                propertyList.Add(key, dictionaryLink);
            }


            /*
			 * Ist das element neu?
             */
            var cpc = new ConditionList(
                new AndCondition(type, projection.GetPrimaryKeyDescription().PropertyName, parentObjectId),
                new CollectionJoin(type, propertyName, childType),
                new AndCondition(childType, childProjection.GetPrimaryKeyDescription().PropertyName, elementId));
          
            dictionaryLink.IsNew = Count(childType, cpc) == 0;

            /*
			 * Persistent Object zum Hash hinzufügen
			 */
            tempHash.Add(virtualPO);

            /*
			 * In die Datenbank schreiben
			 */
            AutoTransaction(tempHash, null);
        }

        /// <summary>
        /// Removes an element from an existing object hashtable
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper the type of the object, that contains the collection, from which the element shall be removed.</param>
        /// <param name="propertyName">This parameter defines the name of the collection property within the typed class. The type of the collection property must be derived from IDictionary.</param>
        /// <param name="parentObjectId">This parameter defines the unique object id of the parent object from which the new value object shall be removed.</param>
        /// <param name="key">This parameter is only used for properties that implements the IDictionary interface. It's used instead of a sequential index like in a collection in order to access the value objects by using a key.</param>
        public void RemoveFromHashtable(Type type, string propertyName, object parentObjectId, object key)
        {
            FieldDescription collectionFieldDescription =
                ReflectionHelper.GetStaticFieldTemplate(type, propertyName,
                                                        (this as ITransactionContext).DatabaseMajorVersion,
                                                        (this as ITransactionContext).DatabaseMinorVersion);

            /*
			 * Prüfen, ob eine Transaction geöffnet wurde
			 */
            CheckOpenTransaction();

            /*
			 * Speichervorgang durchführen
			 */
            ObjectHash tempHash = UpdateHash();

            /*
			 * Neues PO mit Parent Guid anlegen
			 */
            PersistentObject virtualPO = null;
            if (tempHash.Contains(type, parentObjectId))
                virtualPO = tempHash.Get(type, parentObjectId);

            if ((virtualPO == null) || (virtualPO.IsFlatLoaded))
            {
                var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
                PrivateLoad(projection, parentObjectId, tempHash, HierarchyLevel.Dependend1stLvl, null);
                virtualPO = tempHash.Get(type, parentObjectId);
            }

            /*
			 * Existiert schon eine Liste für einen Listentyp ? 
			 */
            Dictionary<object, IModification> propertyList;
            if (!virtualPO.Properties.DictProperties.TryGetValue(collectionFieldDescription.Name, out propertyList))
            {
                propertyList = new Dictionary<object, IModification>();
                virtualPO.Properties.DictProperties = virtualPO.Properties.DictProperties.Add(collectionFieldDescription.Name, propertyList);
            }

            /*
			 * Existiert der Counter Eintrag bereits ? 
			 */
            IModification existingLink;
            if (propertyList.TryGetValue(key, out existingLink))
            {
                /*
				 * Dann den bestehenden Eintrag überschreiben
				 */
                var dictionaryLink = (ListLink) existingLink;
                dictionaryLink.SetLinkedObject(null, tempHash, this);

                /*
			     * Persistent Object zum Hash hinzufügen
			     */
                tempHash.Add(virtualPO);

                /*
			     * In die Datenbank schreiben
			     */
                AutoTransaction(tempHash, null);
            }
        }

        #endregion

        #region Methods for saving value objects

        /// <summary>
        /// Stores a single value object (without any dependencies) into the database.
        /// </summary>
        /// <param name="vo">This parameter contains the object that shall be stored within the database. It's mandatory that a value object implements the IValueObject interface in order to ensure that there is a valid Id property.</param>
        public void FlatSave(IValueObject vo)
        {
            Save(vo, 0);
        }

        /// <summary>
        /// Stores a single value object into the database. 
        /// </summary>
        /// <param name="vo">This parameter contains the object that shall be stored within the database. It's mandatory that a value object implements the IValueObject interface in order to ensure that there is a valid Id property.</param>
        public void Save(IValueObject vo)
        {
            Save(vo, int.MaxValue);
        }

        /// <summary>
        /// Stores a single value object into the database.
        /// </summary>
        /// <param name="vo">This parameter contains the object that shall be stored within the database. It's mandatory that a value object implements the IValueObject interface in order to ensure that there is a valid Id property.</param>
        ///	<param name="hierarchyLevel">The hierarchy level tells the Ad-Factum object mapper how deep the new object shall be stored in database. A hierarchy level of 1 tells the mapper to store only the object itself and the first nested objects.</param>
        public void Save(IValueObject vo, int hierarchyLevel)
        {
            /*
             * Prüfen, ob eine Transaction geöffnet wurde
             */
            CheckOpenTransaction();

            /*
             * Speichervorgang durchführen
             */
            ObjectHash tempHash = UpdateHash();
            ObjectHash secondTempHash = null;

            bool secondStepUpdate = false;
            PrivateSave(vo, tempHash, hierarchyLevel, null, new ConstraintSaveList(), ref secondStepUpdate);

            if (secondStepUpdate)
            {
                bool exceptionUpdate = false;
                secondTempHash = SecondUpdateHash();
                secondTempHash.MergeHash(tempHash);
                PrivateSave(vo, secondTempHash, hierarchyLevel, null, new ConstraintSaveList(), ref exceptionUpdate);
            }

            /*
             * In die Datenbank schreiben
             */
            AutoTransaction(tempHash, secondTempHash);
        }

        #endregion

        #region Methods for selecting object ids

        /// <summary>
        /// This methods selects the primary keys of a object type that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper from which type the result ids shall be selected. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <returns>List of primary keys</returns>
        public IList SelectIDs(Type type, ICondition whereClause)
        {
            return SelectIDs(type, whereClause, null);
        }

        /// <summary>
        /// This methods selects the primary keys of a object type that matches the filter criteria in the where clause.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper from which type the result ids shall be selected. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type Condition (e.g ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <returns>List of primary keys</returns>
        public IList SelectIDs(Type type, ICondition whereClause, OrderBy orderBy)
        {
            ProjectionClass projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
            IList result = Persister.SelectIDs(projection, projection.GetPrimaryKeyDescription().Name, whereClause, orderBy);
            return result;
        }

        #endregion

        #region Methods for counting objects

        /// <summary>
        /// Counts number of rows that matches the whereclause 
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <returns>Number of rows</returns>
        public int Count(Type type, ICondition whereClause)
        {
            return Count(type, whereClause, null);
        }

        /// <summary>
        /// Counts number of rows that matches the whereclause 
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="whereClause">Where clause to filter the selection.</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <returns>Number of rows</returns>
        public int Count(Type type, ICondition whereClause, IDictionary globalParameter)
        {
            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
            return Persister.Count(projection, whereClause,
                                   projection.GetFieldTemplates(true), globalParameter);
        }

        #endregion

        #region Methods for deleting objects

        /// <summary>
        /// Deletes the value object from database. The second parameter can be used to force the object mapper to delete the dependend objects.
        /// </summary>
        /// <param name="vo">This parameter contains the object that shall be deleted from the database. It's mandatory that a value object implements the IValueObject interface in order to ensure that there is a valid Id property.</param>
        public void Delete(IValueObject vo)
        {
            DeleteRecursive(vo, HierarchyLevel.FlatObject);
        }

        /// <summary>
        /// Deletes all objects recursive.
        /// </summary>
        /// <param name="vo">The vo.</param>
        /// <param name="hierarchyLevel">The hierarchy level.</param>
        public void DeleteRecursive(IValueObject vo, int hierarchyLevel)
        {
            /*
             * Prüfen, ob eine Transaction geöffnet wurde
             */
            CheckOpenTransaction();

            PersistentObject po = null;

            /*
             * Temporärer Löschhash neu anlegen
             */
            ObjectHash tempHash = UpdateHash();

            Type voType = vo.GetType();

            if (tempHash.Contains(vo))
                po = tempHash.Get(vo);

            /*
             * If no Persistent Object does exist create/load a temporary persistent object
             */
            if (po == null)
            {
                var projection = ReflectionHelper.GetProjection(voType, MirroredLinqProjectionCache);

                if (HierarchyLevel.IsFlatLoaded(hierarchyLevel))
                    po = new PersistentObject(projection, vo.Id);
                else
                {
                    PrivateLoad(projection, vo.Id, tempHash, hierarchyLevel, null);
                    po = tempHash.Get(voType, vo.Id);
                }
            }

            PrivateDelete(po, hierarchyLevel, tempHash);

            /*
             * In die Datenbank schreiben
             */
            AutoTransaction(tempHash, null);
        }

        /// <summary>
        /// Deletes all objects that are within the result of the select where clause.
        /// </summary>
        /// <param name="typeToDelete">The typeToDelete parameter tells the Ad-Factum object mapper which object typeToDelete shall be deleted. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="primaryKey">This parameter defines a single unique identifier of an object that shall be deleted.</param>
        public void Delete(Type typeToDelete, object primaryKey)
        {
            DeleteRecursive(typeToDelete, primaryKey, HierarchyLevel.FlatObject);
        }

        /// <summary>
        /// Deletes all objects recursive.
        /// </summary>
        /// <param name="typeToDelete">The typeToDelete.</param>
        /// <param name="primaryKey">The primary key.</param>
        /// <param name="hierarchyLevel">The hierarchy level.</param>
        public void DeleteRecursive(Type typeToDelete, object primaryKey, int hierarchyLevel)
        {
            /*
			 * Prüfen, ob eine Transaction geöffnet wurde
			 */
            CheckOpenTransaction();

            /*
             * Temporärer Löschhash neu anlegen
             */
            ObjectHash tempHash = UpdateHash();

            PersistentObject po = null;
            if (tempHash.Contains(typeToDelete, primaryKey))
                po = tempHash.Get(typeToDelete, primaryKey);

            /*
             * If no Persistent Object does exist create/load a temporary persistent object
             */
            if (po == null)
            {
                var projection = ReflectionHelper.GetProjection(typeToDelete, MirroredLinqProjectionCache);
                if (HierarchyLevel.IsFlatLoaded(hierarchyLevel))
                    po = new PersistentObject(projection, primaryKey);
                else
                {
                    PrivateLoad(projection, primaryKey, tempHash, hierarchyLevel, null);
                    po = tempHash.Get(typeToDelete, primaryKey);
                }
            }

            PrivateDelete(po, hierarchyLevel, tempHash);

            /*
             * In die Datenbank schreiben
             */
            AutoTransaction(tempHash, null);
        }

        /// <summary>
        /// Deletes all objects that are within the result of the where condition.
        /// </summary>
        /// <param name="typeToDelete">The type parameter tells the Ad-Factum object mapper which object type shall be deleted. Because of this parameter does the mapper know from which table the data shall be read.</param>
        public void Delete(Type typeToDelete)
        {
            Delete(typeToDelete, null);
        }

        /// <summary>
        /// Deletes all objects that are within the result of the where condition.
        /// </summary>
        /// <param name="typeToDelete">The type parameter tells the Ad-Factum object mapper which object type shall be deleted. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="hierarchyLevel">The hierarchy level.</param>
        public void DeleteRecursive(Type typeToDelete, int hierarchyLevel)
        {
            DeleteRecursive(typeToDelete, null, hierarchyLevel);
        }
        
        /// <summary>
        /// Deletes all objects that are within the result of the where condition.
        /// </summary>
        /// <param name="typeToDelete">The type parameter tells the Ad-Factum object mapper which object type shall be deleted. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="conditions">This parameter defines the where-conditions of the delete selection. Normally a instance of type Condition (e.g. ConditionList) is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        public void Delete(Type typeToDelete, ICondition conditions)
        {
            DeleteRecursive(typeToDelete, conditions, HierarchyLevel.FlatObject);
        }

        /// <summary>
        /// Deletes all objects recursive.
        /// </summary>
        /// <param name="typeToDelete">The selected objects.</param>
        /// <param name="conditions">The conditions.</param>
        /// <param name="hierarchyLevel">The hierarchy level.</param>
        public void DeleteRecursive(Type typeToDelete, ICondition conditions, int hierarchyLevel)
        {
            /*
             * Prüfen, ob eine Transaction geöffnet wurde
             */
            CheckOpenTransaction();

            /*
             * Temporärer Löschhash neu anlegen
             */
            ObjectHash tempHash = UpdateHash();

            /*
             * Die zu löschenden Ids selektieren
             */
            IList idList = SelectIDs(typeToDelete, conditions);
            ProjectionClass projection = ReflectionHelper.GetProjection(typeToDelete, MirroredLinqProjectionCache);
            
            /*
             * Die Liste durchlaufen und löschen
             */
            IEnumerator idEnumerator = idList.GetEnumerator();
            while (idEnumerator.MoveNext())
            {
                object id = idEnumerator.Current;
                PersistentObject po = null;

                if (tempHash.Contains(typeToDelete, id))
                    po = tempHash.Get(typeToDelete, id);

                /*
                 * If no Persistent Object does exist create/load a temporary persistent object
                 */
                if (po == null)
                {
                    if (HierarchyLevel.IsFlatLoaded(hierarchyLevel))
                    {
                        po = new PersistentObject(projection, id);
                    }
                    else
                    {
                        PrivateLoad(projection, id, tempHash, hierarchyLevel, null);
                        po = tempHash.Get(typeToDelete, id);
                    }
                }

                PrivateDelete(po, hierarchyLevel, tempHash);
            }

            /*
             * In die Datenbank schreiben
             */
            AutoTransaction(tempHash, null);
        }

        #endregion

        #region Methods for paging value objects

        /// <summary>
        /// This method is used for database driven paging. 
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList FlatDistinctPaging(Type type, ICondition whereClause, int minRow, int maxRow)
        {
            return DistinctPaging(type, whereClause, null, minRow, maxRow, 0);
        }

        /// <summary>
        /// This method is used for database driven paging. 
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList FlatPaging(Type type, ICondition whereClause, int minRow, int maxRow)
        {
            return Paging(type, whereClause, null, minRow, maxRow, 0);
        }

        /// <summary>
        /// This method is used for database driven paging. 
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList FlatDistinctPaging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow)
        {
            return DistinctPaging(type, whereClause, orderBy, minRow, maxRow, 0);
        }

        /// <summary>
        /// This method is used for database driven paging. 
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList FlatPaging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow)
        {
            return Paging(type, whereClause, orderBy, minRow, maxRow, 0);
        }

        /// <summary>
        /// This method is used for database driven paging. 
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList FlatDistinctPaging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow,
                                        IDictionary globalParameter)
        {
            return DistinctPaging(type, whereClause, orderBy, minRow, maxRow, HierarchyLevel.FlatObject, globalParameter);
        }

        /// <summary>
        /// This method is used for database driven paging. 
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList FlatPaging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow,
                                IDictionary globalParameter)
        {
            return Paging(type, whereClause, orderBy, minRow, maxRow, HierarchyLevel.FlatObject, globalParameter);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList DistinctPaging(Type type, ICondition whereClause, int minRow, int maxRow)
        {
            return DistinctPaging(type, whereClause, null, minRow, maxRow, int.MaxValue);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList Paging(Type type, ICondition whereClause, int minRow, int maxRow)
        {
            return Paging(type, whereClause, null, minRow, maxRow, int.MaxValue);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList DistinctPaging(Type type, ICondition whereClause, int minRow, int maxRow,
                                    IDictionary globalParameter)
        {
            return DistinctPaging(type, whereClause, null, minRow, maxRow, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList Paging(Type type, ICondition whereClause, int minRow, int maxRow, IDictionary globalParameter)
        {
            return Paging(type, whereClause, null, minRow, maxRow, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList DistinctPaging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow)
        {
            return DistinctPaging(type, whereClause, orderBy, minRow, maxRow, int.MaxValue);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList Paging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow)
        {
            return Paging(type, whereClause, orderBy, minRow, maxRow, int.MaxValue);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList DistinctPaging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow,
                                    IDictionary globalParameter)
        {
            return DistinctPaging(type, whereClause, orderBy, minRow, maxRow, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList Paging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow,
                            IDictionary globalParameter)
        {
            return Paging(type, whereClause, orderBy, minRow, maxRow, int.MaxValue, globalParameter);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="hierarchieLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList DistinctPaging(Type type, ICondition whereClause, int minRow, int maxRow, int hierarchieLevel)
        {
            return DistinctPaging(type, whereClause, null, minRow, maxRow, hierarchieLevel);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="hierarchieLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList Paging(Type type, ICondition whereClause, int minRow, int maxRow, int hierarchieLevel)
        {
            return Paging(type, whereClause, null, minRow, maxRow, hierarchieLevel);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="hierarchieLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList DistinctPaging(Type type, ICondition whereClause, int minRow, int maxRow, int hierarchieLevel,
                                    IDictionary globalParameter)
        {
            return DistinctPaging(type, whereClause, null, minRow, maxRow, hierarchieLevel, globalParameter);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="hierarchieLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList Paging(Type type, ICondition whereClause, int minRow, int maxRow, int hierarchieLevel,
                            IDictionary globalParameter)
        {
            return Paging(type, whereClause, null, minRow, maxRow, hierarchieLevel, globalParameter);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="hierarchieLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList DistinctPaging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow,
                                    int hierarchieLevel)
        {
            return DistinctPaging(type, whereClause, orderBy, minRow, maxRow, hierarchieLevel, null);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="hierarchieLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList Paging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow,
                            int hierarchieLevel)
        {
            return Paging(type, whereClause, orderBy, minRow, maxRow, hierarchieLevel, null);
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="hierarchieLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList Paging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow,
                            int hierarchieLevel, IDictionary globalParameter)
        {
            IList result = new ArrayList();

            /*
			 * Neuen Ladehash anlegen, um Rekursionen zu vermeiden
			 */
            ObjectHash tempHash = UpdateHash();

            /*
			 * Eine Liste der POs laden
			 */
            IList resultPOs;
            if ((maxRow == int.MaxValue) && (minRow <= 1))
                resultPOs = PrivateSelect(type, whereClause, orderBy, tempHash, hierarchieLevel, globalParameter, false);
            else
                resultPOs =
                    PrivatePaging(type, whereClause, orderBy, minRow, maxRow, tempHash, hierarchieLevel, globalParameter,
                                  false);

            /*
			 * Die Liste durchlaufen und die VOs holen
			 */
            foreach (PersistentObject po in resultPOs)
                result.Add(po.GetTemporaryCreated() ?? tempHash.GetVO(po.ObjectType, po.Id));

            MergeHash(tempHash);
            return result;
        }

        /// <summary>
        /// This method is used for database driven paging.
        /// </summary>
        /// <param name="type">The type parameter tells the Ad-Factum object mapper of which type the content of the result list shall be. Because of this parameter does the mapper know from which table the data shall be read.</param>
        /// <param name="whereClause">This parameter defines the where-conditions of the selection. Normally a instance of type WhereClause is used to fill this parameter. If no where-conditions are needed, the parameter can be set to NULL.</param>
        /// <param name="orderBy">The orderBy parameter defines which column is used for ordering the result list. If no order is needed, this parameter can be set to NULL.</param>
        /// <param name="minRow">The minRow defines the starting row within the database selection. If you want to select the first ten rows the minRow would be set to 1.</param>
        /// <param name="maxRow">The maxRow defines the ending row number within the database selection. If you want to select rows 11 to 20. The maxRow variable must set to 20.</param>
        /// <param name="hierarchieLevel">The hierarchy level tells the Ad-Factum object mapper how deep the objects shall be loaded. A hierarchy level of 1 tells the mapper to step only into the first nested relation.</param>
        /// <param name="globalParameter">The global parameters are used for value object which have virtual links to other objects. A prime example for using virtual links are value objects which have dynamically translated attributes.</param>
        /// <returns>Returns a paged list with value objects</returns>
        public IList DistinctPaging(Type type, ICondition whereClause, OrderBy orderBy, int minRow, int maxRow,
                                    int hierarchieLevel, IDictionary globalParameter)
        {
            IList result = new ArrayList();

            /*
			 * Neuen Ladehash anlegen, um Rekursionen zu vermeiden
			 */
            ObjectHash tempHash = UpdateHash();

            /*
			 * Eine Liste der POs laden
			 */
            IList resultPOs;
            if ((maxRow == int.MaxValue) && (minRow <= 1))
                resultPOs = PrivateSelect(type, whereClause, orderBy, tempHash, hierarchieLevel, globalParameter, true);
            else
                resultPOs =
                    PrivatePaging(type, whereClause, orderBy, minRow, maxRow, tempHash, hierarchieLevel, globalParameter,
                                  true);

            /*
			 * Die Liste durchlaufen und die VOs holen
			 */
            foreach (PersistentObject po in resultPOs)
                result.Add(po.GetTemporaryCreated() ?? tempHash.GetVO(po.ObjectType, po.Id));

            MergeHash(tempHash);
            return result;
        }

        #endregion

        #region Methods for nested objects

        /// <summary>
        /// Returns a nested object flat loaded
        /// </summary>
        /// <param name="type">Type of a parent object that contains the nested object, which has to be returned</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="objectId">Parent object id</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <returns>
        /// Returns the nested object or NULL if the object could not be found or loaded.
        /// </returns>
        public object FlatGetNestedObject(Type type, string propertyName, object objectId, IDictionary globalParameter)
        {
            return GetNestedObject(type, propertyName, objectId, 0, globalParameter);
        }

        /// <summary>
        /// Returns a nested object flat loaded
        /// </summary>
        /// <param name="type">Type of a parent object that contains the nested object, which has to be returned</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="objectId">Parent object id</param>
        /// <returns>Returns the nested object or NULL if the object could not be found or loaded.</returns>
        public object FlatGetNestedObject(Type type, string propertyName, object objectId)
        {
            return GetNestedObject(type, propertyName, objectId, 0);
        }

        /// <summary>
        /// Used for loading child collections of an parent object
        /// </summary>
        /// <param name="type">Type of a parent object that contains the nested object, which has to be returned</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="objectId">Parent object id</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <returns></returns>
        public IList FlatGetNestedCollection(Type type, string propertyName, object objectId,  IDictionary globalParameter)
        {
            return GetNestedCollection(type, propertyName, objectId, 0, globalParameter);
        }

        /// <summary>
        /// Used for loading child collections of an parent object
        /// </summary>
        /// <param name="type">Type of a parent object that contains the nested object, which has to be returned</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="objectId">Parent object id</param>
        /// <returns></returns>
        public IList FlatGetNestedCollection(Type type, string propertyName, object objectId)
        {
            return GetNestedCollection(type, propertyName, objectId, 0);
        }

        /// <summary>
        /// Returns a nested object
        /// </summary>
        /// <param name="type">Type of a parent object that contains the nested object, which has to be returned</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="objectId">Parent object id</param>
        /// <returns>Returns the nested object or NULL if the object could not be found or loaded.</returns>
        public object GetNestedObject(Type type, string propertyName, object objectId)
        {
            return GetNestedObject(type, propertyName, objectId, int.MaxValue);
        }

        /// <summary>
        /// Returns a nested object
        /// </summary>
        /// <param name="type">Type of a parent object that contains the nested object, which has to be returned</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="objectId">Parent object id</param>
        /// <param name="hierarchyLevel"></param>
        /// <returns>Returns the nested object or NULL if the object could not be found or loaded.</returns>
        public object GetNestedObject(Type type, string propertyName, object objectId, int hierarchyLevel)
        {
            return GetNestedObject(type, propertyName, objectId, hierarchyLevel, null);    
        }

        /// <summary>
        /// Returns a nested object
        /// </summary>
        /// <param name="type">Type of a parent object that contains the nested object, which has to be returned</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="objectId">Parent object id</param>
        /// <param name="hierarchyLevel">The hierarchy level.</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <returns>
        /// Returns the nested object or NULL if the object could not be found or loaded.
        /// </returns>
        public object GetNestedObject(Type type, string propertyName, object objectId, int hierarchyLevel, IDictionary globalParameter)
        {
            object result = null;
            FieldDescription field =
                ReflectionHelper.GetStaticFieldTemplate(type, propertyName,
                                                        (this as ITransactionContext).DatabaseMajorVersion,
                                                        (this as ITransactionContext).DatabaseMinorVersion);

            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);

            if (field.FieldType.Equals(typeof(SpecializedLink)))
            {
                ICondition nestedJoin;

                /*
				 * If the types are the same, we must work with a sub select
				 */

                if (field.ContentType.Equals(type))
                    nestedJoin = new InCondition(type, projection.GetPrimaryKeyDescription().PropertyName,
                                                 new SubSelect(type, propertyName,
                                                               new AndCondition(type, projection.GetPrimaryKeyDescription().PropertyName,
                                                                                QueryOperator.Equals, objectId)));
                else
                {
                    var contentProjection = ReflectionHelper.GetProjection(field.ContentType, MirroredLinqProjectionCache);
                    nestedJoin = new ConditionList(
                        new AndCondition(type, projection.GetPrimaryKeyDescription().PropertyName, QueryOperator.Equals, objectId),
                        new Join(type, propertyName, field.ContentType, contentProjection.GetPrimaryKeyDescription().PropertyName));
                }

                IList list = Select(field.ContentType, nestedJoin, null, hierarchyLevel, globalParameter);

                if (list.Count > 0)
                    result = list[0];
            }
            else
            if (field.FieldType.Equals(typeof(Link)))
            {
                /*
                 * Neuen Ladehash anlegen, um Rekursionen zu vermeiden
                 */
                var tempHash  = UpdateHash();
                var templates = new Dictionary<string, FieldDescription>(projection.GetFieldTemplates(false));
                var dictEnum = templates.GetEnumerator();
                var removeKeys = new ArrayList();
                while (dictEnum.MoveNext())
                {
                    FieldDescription fieldDesc = dictEnum.Current.Value;
                    if ((fieldDesc != null) &&
                        (fieldDesc.FieldType == typeof(Link)
                        || fieldDesc.FieldType == typeof(ListLink)
                        || fieldDesc.FieldType == typeof(SpecializedLink)
                        ) && (dictEnum.Current.Key != propertyName))
                        removeKeys.Add(dictEnum.Current.Key);
                }

                /*
                 * Filter out all unwanted childs
                 */
                foreach (string key in removeKeys)
                    templates.Remove(key);

                PersistentProperties fields = Persister.Load(projection, objectId, templates, globalParameter);
                PersistentObject po = fields != null ? new PersistentObject(projection, false, fields, objectId) : null;

                /*
			     * Daten in das VO setzen und in den temporären Hash
			     */
                if (po != null)
                {
                    int level = hierarchyLevel == int.MaxValue ? int.MaxValue : HierarchyLevel.Dependend1stLvl + hierarchyLevel;
                    object vo = po.CreateVO(this, objectFactory, tempHash, level, null);
                    Property property = templates[propertyName].CustomProperty;
                    result = property.GetValue(vo);
                }
            }

            return result;
        }

        /// <summary>
        /// Used for loading child collections of an parent object
        /// </summary>
        /// <param name="type">Type of a parent object that contains the nested object, which has to be returned</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="objectId">Parent object id</param>
        /// <returns></returns>
        public IList GetNestedCollection(Type type, string propertyName, object objectId)
        {
            return GetNestedCollection(type, propertyName, objectId, int.MaxValue);
        }

        /// <summary>
        /// Gets the nested collection.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="objectId">The object id.</param>
        /// <param name="hierarchyLevel">The hierarchy level.</param>
        /// <returns></returns>
        public IList GetNestedCollection(Type type, string propertyName, object objectId, int hierarchyLevel)
        {
            return GetNestedCollection(type, propertyName, objectId, hierarchyLevel, null);
        }

        /// <summary>
        /// Used for loading child collections of an parent object
        /// </summary>
        /// <param name="type">Type of a parent object that contains the nested object, which has to be returned</param>
        /// <param name="propertyName">Name of the property</param>
        /// <param name="objectId">Parent object id</param>
        /// <param name="hierarchyLevel">The hierarchy level.</param>
        /// <param name="globalParameter">The global parameter.</param>
        /// <returns></returns>
        public IList GetNestedCollection(Type type, string propertyName, object objectId, 
                                         int hierarchyLevel, IDictionary globalParameter)
        {
            IList result = null;
            FieldDescription field = ReflectionHelper.GetStaticFieldTemplate(type, propertyName,
                                                        (this as ITransactionContext).DatabaseMajorVersion,
                                                        (this as ITransactionContext).DatabaseMinorVersion);

            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);

            if (field.FieldType.Equals(typeof (ListLink)))
            {
                bool isGeneralLinked = field.CustomProperty.MetaInfo.IsGeneralLinked;

                /*
                 * If it's not a gernal link, try to load the collection directly
                 */
                if (!isGeneralLinked)
                {
                    Type typeOfCollectionChild = field.CustomProperty.MetaInfo.LinkTarget;

                    ICondition dependendJoin;
                    if (type.Equals(typeOfCollectionChild))
                        dependendJoin =
                            new CollectionParentCondition(type, propertyName, typeOfCollectionChild, objectId);
                    else
                        dependendJoin = new ConditionList(
                            new AndCondition(type, projection.GetPrimaryKeyDescription().PropertyName, QueryOperator.Equals, objectId),
                            new CollectionJoin(type, propertyName, typeOfCollectionChild));

                    result = Select(typeOfCollectionChild, dependendJoin, null, hierarchyLevel, globalParameter);
                }
                /*
                 * If it's a general link, we have to use a more generic method
                 */
                else
                {
                    /*
                     * Neuen Ladehash anlegen, um Rekursionen zu vermeiden
                     */
                    var tempHash  = UpdateHash();
                    var templates = new Dictionary<string, FieldDescription>(projection.GetFieldTemplates(false));
                    IDictionaryEnumerator dictEnum = templates.GetEnumerator();
                    var removeKeys = new ArrayList();
                    while (dictEnum.MoveNext())
                    {
                        var fieldDesc = dictEnum.Value as FieldDescription;
                        if ((fieldDesc != null) && 
                            (fieldDesc.FieldType == typeof(Link) 
                            || fieldDesc.FieldType == typeof(ListLink) 
                            || fieldDesc.FieldType == typeof(SpecializedLink)
                            ) && ((string)dictEnum.Entry.Key != propertyName))
                            removeKeys.Add(dictEnum.Key);
                    }

                    /*
                     * Filter out all unwanted childs
                     */
                    foreach (string key in removeKeys)
                        templates.Remove(key);

                    PersistentProperties fields = Persister.Load(projection, objectId, templates, globalParameter);
                    PersistentObject po = fields != null ? new PersistentObject(projection, false, fields, objectId) : null;

                    /*
			         * Daten in das VO setzen und in den temporären Hash
			         */
                    if (po != null)
                    {
                        int level = hierarchyLevel == int.MaxValue ? int.MaxValue : HierarchyLevel.Dependend1stLvl + hierarchyLevel;

                        object vo = po.CreateVO(this, objectFactory, tempHash, level, null);
                        Property property = templates[propertyName].CustomProperty;
                        result = (IList) property.GetValue(vo);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Set a single property of a defined value object with a new value.
        /// </summary>
        /// <param name="type">Type of the object of which the property shall be set</param>
        /// <param name="propertyName">Name of the property that shall be set</param>
        /// <param name="objectId">Unique object id</param>
        /// <param name="propertyValueId">New value which shall be set to the property</param>
        /// <param name="propertyType">Type of the nested property</param>
        public void SetNestedObject(Type type, string propertyName, object objectId, Type propertyType,
                                    object propertyValueId)
        {
            /*
			 * Prüfen, ob eine Transaction geöffnet wurde
			 */
            CheckOpenTransaction();

            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
            var propertyProjection = ReflectionHelper.GetProjection(propertyType, MirroredLinqProjectionCache);

            /*
			 * Load object first
			 */
            ObjectHash tempHash = UpdateHash();
            object propertyValue = PrivateLoad(propertyProjection, propertyValueId, tempHash, HierarchyLevel.FlatObject, null);

            /*
			 * Neues PO mit Parent Guid anlegen
			 */
            PersistentObject virtualPO;
            if (tempHash.Contains(type, objectId))
                virtualPO = tempHash.Get(type, objectId);
            else
            {
                PrivateLoad(projection, objectId, tempHash, HierarchyLevel.FlatObject, null);
                virtualPO = tempHash.Get(type, objectId);
            }

            /*
			 * Set the update field
			 */
            PropertyInfo property = type.GetPropertyInfo(propertyName);
            Property propertyCustomInfo = Property.GetPropertyInstance(property);
            VirtualLinkAttribute virtualLinkCustomInfo = ReflectionHelper.GetVirtualLinkInstance(property);

            bool secondStepUpdate = false;
            virtualPO.InternalUpdateField(property, propertyCustomInfo, virtualLinkCustomInfo,
                                          propertyValue, this, tempHash, HierarchyLevel.FlatObjectWithLinks, 
                                          null, new ConstraintSaveList(), ref secondStepUpdate, false);
            string primaryKey = projection.GetPrimaryKeyDescription().Name; 
            var idField = virtualPO.Properties.FieldProperties.Get(primaryKey) as Field;
            if (idField != null)
                idField.IsModified = idField.IsNew = false;

            /*
			 * Persistent Object zum Hash hinzufügen
			 */
            tempHash.Add(virtualPO);

            /*
			 * In die Datenbank schreiben
			 */
            AutoTransaction(tempHash, null);
        }

        #endregion

        #region Dispose Pattern

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// </summary>
        ~ObjectMapper()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disconnecting the database
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (!BorrowedTransaction && !transactionContext.IsTransactionOpen)
                    transactionContext.Dispose();

                objectFactory = null;
                transactionContext = null;
            }

            // free unmanaged resources
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Private save method that stores a value object to database.
        /// </summary>
        /// <param name="vo">Value Object</param>
        /// <param name="hash">Temporary object cache is used to handle circular dependencies.</param>
        /// <param name="hierarchyLevel">The maximum save deep parameter
        /// defines until which maximum hierarchie level the data has to be stored in the database   ///</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <param name="recursionTest">Test if the object has been saved before</param>
        /// <param name="secondStepUpdate">if set to <c>true</c> [second step update].</param>
        /// <returns>
        /// This method returns a persistent object which contains the property mapping for the database.
        /// </returns>
        internal PersistentObject PrivateSave(IValueObject vo, ObjectHash hash, int hierarchyLevel,
                                              IDictionary globalParameter, ConstraintSaveList recursionTest,
                                              ref bool secondStepUpdate)
        {
            PersistentObject po;

            /*
			 * Ist das VO null, dann muss es auch nicht gespeichert werden.
			 */
            if (vo == null)
                return null;

            var projection = ReflectionHelper.GetProjection(vo.GetType(), MirroredLinqProjectionCache);

            /*
             * If the object has been saved, than return immediatly
             */
            if (recursionTest.Contains(vo))
            {
                po = hash.Get(vo);
                if (po == null) 
                    secondStepUpdate = true;

                /*
                 * Create stub, If nothing could be found
                 */
                if ((po == null) && (HierarchyLevel.StoreOnlyLinks(hierarchyLevel)))
                {
                    po = new PersistentObject(projection, vo.Id);
                    po.IsNew = po.IsModified = false;
                }

                return po;
            }
            recursionTest.Add(vo);

            /*
			 * Wurde schon eine ID vergeben?
			 */
            if (vo.IsNew)
            {
                AddToRollbackList(vo);
                vo.IsNew = false;
                po = new PersistentObject(vo, this, hash, hierarchyLevel, recursionTest, ref secondStepUpdate);
            }
            else
                /*
				 * Existiert noch kein neues Persistenzobjekt im Hash ?
				 */ 
                if (!hash.Contains(vo))
                {
                    /*
                     * If we are only interessted in updating links, we can create an empty dummy object
                     */
                    if (HierarchyLevel.StoreOnlyLinks(hierarchyLevel))
                    {
                        po = new PersistentObject(projection, vo.Id);
                        po.IsNew = po.IsModified = false;

                        po.Update(vo, this, hash, hierarchyLevel, recursionTest, ref secondStepUpdate);
                    }
                    else
                    /*
					 * Check if the object does exist and load it if necessary.
					 */
                    {
                        object loaded = PrivateLoad(projection, vo.Id, hash, hierarchyLevel, globalParameter);
                        if (loaded != null)
                        {
                            /*
                             * Das bestehende Persistenzobjekt updaten
                             * Dazu wird eine Kopie des geladenen Persistenzobjekts angelegt.
                             * Dies wird nur im Statefull Modus benötigt, da sonst das Objekt direkt im Primary Hash
                             * geupdated werden würde.
                             */
                            po = hash.Get(vo);
                            if (po == null)
                                throw new InvalidOperationException("Could not load object of type " + vo.GetType());

                            po.Update(vo, this, hash, hierarchyLevel, recursionTest, ref secondStepUpdate);
                        }
                        else
                        {
                            po = new PersistentObject(vo, this, hash, hierarchyLevel, recursionTest, ref secondStepUpdate);
                        }
                    }
                }
                else
                {
                    /*
					 * Das bestehende Persistenzobjekt updaten
					 * Dazu wird eine Kopie des geladenen Persistenzobjekts angelegt.
					 * Dies wird nur im Statefull Modus benötigt, da sonst das Objekt direkt im Primary Hash
					 * geupdated werden würde.
					 */
                    po = hash.Get(vo);
                    if (po == null)
                        throw new InvalidOperationException("Could not load object of type " + vo.GetType());

                    po.Update(vo, this, hash, hierarchyLevel, recursionTest, ref secondStepUpdate);
                }

            /*
			* Das eigentliche Objekt speichern
			*/
            po.IsDeleted = false;
            hash.Add(po, vo);

            /*
             * Now Update One-To-Many Associations, because they require that the object has been stored previously
             */
            po.UpdateOneToMany(vo, this, hash, hierarchyLevel, recursionTest, ref secondStepUpdate);

            return po;
        }

        /// <summary>
        /// Executes a private paging selection
        /// </summary>
        /// <param name="type">Queried object type</param>
        /// <param name="whereClause">Where clause</param>
        /// <param name="orderBy">Order clause</param>
        /// <param name="minLine">Minimal paging row</param>
        /// <param name="maxLine">Maximal paging row</param>
        /// <param name="hash">Temporarily used hash</param>
        /// <param name="hierarchyLevel">Hirarchie Level</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <param name="distinct">Select only distinct values</param>
        /// <returns>List that contains persistent objects</returns>
        internal IList PrivatePaging(Type type, ICondition whereClause, OrderBy orderBy, int minLine, int maxLine,
                                     ObjectHash hash, int hierarchyLevel, IDictionary globalParameter, bool distinct)
        {
            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
            
            List<PersistentProperties> persistFields =
                Persister.PageSelect(projection, whereClause, orderBy, minLine, maxLine,
                                     projection.GetFieldTemplates(HierarchyLevel.IsFlatLoaded(hierarchyLevel)),
                                     globalParameter, distinct);
            return PrivateCreatePOs(projection, persistFields, hash, hierarchyLevel, globalParameter);
        }

        /// <summary>
        /// Executes a private select and returns a list with persistent objects
        /// </summary>
        /// <param name="type">Queried object type</param>
        /// <param name="conditions">Condition collection</param>
        /// <param name="orderBy">Order clause</param>
        /// <param name="hash">Temporarily used hash</param>
        /// <param name="hierarchyLevel">Hirarchie Level</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <param name="distinct">Select only distinct values</param>
        /// <returns>List that contains persistent objects</returns>
        internal List<PersistentObject> PrivateSelect(Type type, ICondition conditions, OrderBy orderBy, ObjectHash hash,
                                     int hierarchyLevel, IDictionary globalParameter, bool distinct)
        {
            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
            
            List<PersistentProperties> persistFields =
                Persister.Select(projection, conditions, orderBy,
                                 projection.GetFieldTemplates(HierarchyLevel.IsFlatLoaded(hierarchyLevel)),
                                 globalParameter, distinct);
            return PrivateCreatePOs(projection, persistFields, hash, hierarchyLevel, globalParameter);
        }

        /// <summary>
        /// Executes a private select and returns a list with persistent objects
        /// </summary>
        /// <param name="type">Queried object type</param>
        /// <param name="selectSql">Complete select string which can be executed directly.</param>
        /// <param name="selectParameter">Parameter used for the placeholders within the select string. 
        /// A placeholder always begins with an @ followed by a defined key.</param>
        /// <param name="hash">Temporarily used hash</param>
        /// <param name="hierarchyLevel">Hirarchie Level</param>
        internal List<PersistentObject> PrivateSelect(Type type, string selectSql, SortedList selectParameter, ObjectHash hash,
                                     int hierarchyLevel)
        {
            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);

            List<PersistentProperties> persistFields =
                Persister.Select(Table.GetTableInstance(type).DefaultName, selectSql, selectParameter,
                                 projection.GetFieldTemplates(HierarchyLevel.IsFlatLoaded(hierarchyLevel)));
            return PrivateCreatePOs(projection, persistFields, hash, hierarchyLevel, null);
        }

        /// <summary>
        /// Privates the select.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="command">The command.</param>
        /// <param name="hash">The hash.</param>
        /// <param name="hierarchyLevel">The hierarchy level.</param>
        /// <returns></returns>
        internal List<PersistentObject> PrivateSelect(Type type, IDbCommand command, ObjectHash hash, int hierarchyLevel)
        {
            var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);

            List<PersistentProperties> persistFields =
                Persister.Select(Table.GetTableInstance(type).DefaultName, command,
                                 projection.GetFieldTemplates(HierarchyLevel.IsFlatLoaded(hierarchyLevel)));
            return PrivateCreatePOs(projection, persistFields, hash, hierarchyLevel, null);
        }

        /// <summary>
        /// Takes a field list and creates persistent objects
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="persistFields">List with persistent fields, loaded by the persister object</param>
        /// <param name="hash">Temporarily used hash</param>
        /// <param name="hierarchyLevel">Hirarchie Level</param>
        /// <param name="globalParameter">Load Parameter for virtual links</param>
        /// <returns>List that contains persistent objects</returns>
        private List<PersistentObject> PrivateCreatePOs(ProjectionClass projection, IEnumerable<PersistentProperties> persistFields, ObjectHash hash, int hierarchyLevel,
                                       IDictionary globalParameter)
        {
            var type = projection.ProjectedType;
            var resultPOs = new List<PersistentObject>();
            string primaryKey = null;
            try
            {
                if (!type.IsReadOnlyType())
                    primaryKey = projection.GetPrimaryKeyDescription().Name;
            }
            catch (NoPrimaryKeyFoundException)
            {
                // No primary Key available
            }

            /*
			 * Neue POs erzeugen
			 */
            foreach (var entry in persistFields)
            {
                object id = null;
                if (primaryKey != null)
                {
                    IModification primaryKeyField;
                    if (entry.FieldProperties.TryGetValue(primaryKey, out primaryKeyField))
                        id = ((Field)primaryKeyField).Value;

                    if (id == null)     // if no primary id could be found, than remove the primary key flag
                        primaryKey = null;
                }

                PersistentObject po;
                if (primaryKey == null)
                {
                    po = new PersistentObject(projection, HierarchyLevel.IsFlatLoaded(hierarchyLevel), entry, null);
                    po.CreateVO(this, objectFactory, hash, hierarchyLevel, globalParameter);
                }
                else if (hash.Contains(type, id))
                {
                    po = hash.Get(type, id);
                    if (hash.GetVO(type, id) == null)
                    {
                        po.CreateVO(this, objectFactory, hash, hierarchyLevel, globalParameter);
                        po.IsModified = true;
                    }

                    /*
					 * Add unmatched fields
					 */
                    var fieldEnum = entry.FieldProperties.GetEnumerator();
                    while (fieldEnum.MoveNext())
                    {
                        var unmatched = fieldEnum.Current.Value as UnmatchedField;
                        if (unmatched != null)
                        {
                            if (po.Properties.FieldProperties.Contains(fieldEnum.Current.Key))
                                po.Properties.FieldProperties.Remove(fieldEnum.Current.Key);

                            po.Properties.FieldProperties = po.Properties.FieldProperties.Add(fieldEnum.Current.Key, unmatched);
                        }
                    }
                }
                else
                {
                    po = new PersistentObject(projection, HierarchyLevel.IsFlatLoaded(hierarchyLevel), entry, ((Field)entry.FieldProperties.Get(primaryKey)).Value);
                    po.CreateVO(this, objectFactory, hash, hierarchyLevel, globalParameter);
                }

                resultPOs.Add(po);
            }


            return resultPOs;
        }

        /// <summary>
        /// Loads an object and returns the value object.
        /// </summary>
        internal object PrivateLoad(ProjectionClass projection, object id, ObjectHash hash, int hierarchyLevel,
                                          IDictionary globalParameter)
        {
            PersistentObject po = null;
            object load = null;

            if (id == null)
                return null;

            Type type = projection.ProjectedType;

            /*
			 * Ist das Objekt bereits im Temporären Hash ?
			 */
            if (hash.Contains(type, id))
            {
                object resultVO = hash.GetVO(type, id);
                if (resultVO != null)
                    return resultVO;

                po = hash.Get(type, id);
            }

            /*
			 * Ist das PO schon durch den Ladehash gesetzt ?
			 */
            if (po == null)
            {
//                var projection = ReflectionHelper.GetProjection(type, MirroredLinqProjectionCache);
                PersistentProperties fields =
                    Persister.Load(projection, id,
                                   projection.GetFieldTemplates(HierarchyLevel.IsFlatLoaded(hierarchyLevel)),
                                   globalParameter);
                po = fields != null ? new PersistentObject(projection, HierarchyLevel.IsFlatLoaded(hierarchyLevel), fields, id) : null;
            }

            /*
			 * Daten in das VO setzen und in den temporären Hash
			 */
            if (po != null)
            {
                load = po.CreateVO(this, objectFactory, hash, hierarchyLevel, globalParameter);
                hash.AddLoad(po, load);
            }

            return load;
        }

        /// <summary>
        /// Internal delete method.
        /// </summary>
        /// <param name="po">Persistent Object that has to be deleted.</param>
        /// <param name="hierarchyLevel">The hierarchy level.</param>
        /// <param name="tempHash">Temporary object cache is used to handle circular dependencies.</param>
        internal void PrivateDelete(PersistentObject po, int hierarchyLevel, ObjectHash tempHash)
        {
            po.IsDeleted = true;
            
            po.DeleteAllLinks(this, hierarchyLevel, tempHash, false);
            tempHash.Add(po);
            po.DeleteAllLinks(this, hierarchyLevel, tempHash, true);
        }

        #endregion

        #region Interface Transaction Members

        /// <summary>
        /// Creates a temporary object hash.
        /// </summary>
        /// <returns>Object Hash</returns>
        public ObjectHash UpdateHash()
        {
            return transactionContext.UpdateHash();
        }

        /// <summary>
        /// Creates a temporary object hash.
        /// </summary>
        /// <returns>Object Hash</returns>
        public ObjectHash SecondUpdateHash()
        {
            return transactionContext.SecondUpdateHash();
        }

        /// <summary>
        /// Checks if a transaction is open. 
        /// If not, a mapper exception will be thrown.
        /// </summary>
        public void CheckOpenTransaction()
        {
            transactionContext.CheckOpenTransaction();
        }

        /// <summary>
        /// Get or sets the current version of the dependent database object model.
        /// </summary>
        public double DatabaseVersion
        {
            get { return transactionContext.DatabaseVersion; }
            set { transactionContext.DatabaseVersion = value; }
        }

        /// <summary>
        /// Get or sets the current version of the dependent database object model.
        /// </summary>
        /// <value>The database version.</value>
        public string DatabaseMajorVersion
        {
            set { transactionContext.DatabaseMajorVersion = int.Parse(value); }
            get { return transactionContext.DatabaseMajorVersion.ToString(); }
        }

        /// <summary>
        /// Gets the database major version integer.
        /// </summary>
        /// <value>The database major version integer.</value>
        int ITransactionContext.DatabaseMajorVersion
        {
            get { return transactionContext.DatabaseMajorVersion; }
            set { transactionContext.DatabaseMajorVersion = value; }
        }

        /// <summary>
        /// Get or sets the current version of the dependent database object model.
        /// </summary>
        /// <value>The database version.</value>
        public string DatabaseMinorVersion
        {
            set
            {
                string version = value;
                if (version.Length == 1)
                    version += "0";

                transactionContext.DatabaseMinorVersion = int.Parse(version);
            }
            get { return transactionContext.DatabaseMinorVersion.ToString("00"); }
        }

        /// <summary>
        /// Gets the database major version integer.
        /// </summary>
        /// <value>The database major version integer.</value>
        int ITransactionContext.DatabaseMinorVersion
        {
            get { return transactionContext.DatabaseMinorVersion; }
            set { transactionContext.DatabaseMinorVersion = value; }
        }

        /// <summary>
        /// Gets the database major version integer.
        /// </summary>
        /// <value>The database major version integer.</value>
        internal int DatabaseMinorVersionInteger
        {
            get { return transactionContext.DatabaseMinorVersion; }
        }

        /// <summary>
        /// Returns the persister class in order to interact with the core database engine.
        /// </summary>
        public IPersister Persister
        {
            get { return transactionContext.Persister; }
            set { transactionContext.Persister = value; }
        }

        /// <summary>
        /// Gets the Schema Manager
        /// </summary>
        public ISchemaWriter Schema
        {
            get { return Persister.Schema; }
        }

        /// <summary>
        /// Gets the Repository
        /// </summary>
        /// <value>The repository.</value>
        public IRepository Repository
        {
            get { return Persister.Repository; }
        }

        /// <summary>
        /// Gets the Integrity Checker
        /// </summary>
        public IIntegrity Integrity
        {
            get { return Persister.Integrity; }
        }

        /// <summary>
        /// Returns true, if a transaction is open
        /// </summary>
        public bool IsTransactionOpen
        {
            get { return transactionContext.IsTransactionOpen; }
        }

        /// <summary>
        /// Returns true, if a transaction is open
        /// </summary>
        public Transactions TransactionSetting
        {
            get { return transactionContext.TransactionSetting; }
            set { transactionContext.TransactionSetting = value; }
        }

        /// <summary>
        /// Returns the transaction context for the database mapper
        /// </summary>
        public ITransactionContext TransactionContext
        {
            get { return transactionContext; }
        }

        /// <summary>
        /// Start eine Transaktion
        /// </summary>
        public void BeginTransaction()
        {
            transactionContext.BeginTransaction();
        }

        /// <summary>
        /// Führt ein Rollback aus
        /// </summary>
        public void Rollback()
        {
            transactionContext.Rollback();
        }

        /// <summary>
        /// Opens a transaction, stores the data and closes the transaction.
        /// </summary>
        /// <param name="tempHash">Object cache that holds the objcets which shall be stored in database.</param>
        /// <param name="secondStepHash">The second step hash.</param>
        public void AutoTransaction(ObjectHash tempHash, ObjectHash secondStepHash)
        {
            transactionContext.AutoTransaction(tempHash, secondStepHash);
        }

        /// <summary>
        /// Merges the update Hash with the primary Hash
        /// </summary>
        public void MergeHash(ObjectHash tempHash)
        {
            transactionContext.MergeHash(tempHash);
        }

        /// <summary>
        /// Adds a new object to the rollbacklist in order to set the property isNew to false, if a rollback takes place.
        /// </summary>
        /// <param name="vo"></param>
        public void AddToRollbackList(IValueObject vo)
        {
            transactionContext.AddToRollbackList(vo);
        }

        /// <summary>
        /// Executes an commit
        /// </summary>
        public void Commit()
        {
            transactionContext.Commit();
        }

        /// <summary>
        /// Flushes the content of the object Hash to database.
        /// </summary>
        public void Flush()
        {
            transactionContext.Flush();
        }

        /// <summary>
        /// Returns true, if the values within the update Hash have been changed
        /// </summary>
        public bool IsModified
        {
            get { return transactionContext.IsModified; }
        }

        /// <summary>
        /// Returns true, if the curren transaction context is valid and can be used.
        /// </summary>
        public bool IsValid
        {
            get { return (transactionContext != null) && (transactionContext.IsValid); }
        }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        public string ApplicationName
        {
            get { return transactionContext.ApplicationName; }
            set { transactionContext.ApplicationName = value; }
        }

        /// <summary>
        /// Defines, if the ObjectMapper .NET uses a borrowed transaction.
        /// If true, the ObjectMapper .NET won't dispose the transaction context
        /// because it may be used within another ObjectMapper
        /// </summary>
        public bool BorrowedTransaction
        {
            get { return borrowedTransaction; }
        }

        /// <summary>
        /// Dynamic cache set by the Linq Methods
        /// </summary>
        /// <value>The mirrored linq projection cache.</value>
        public Cache<Type, ProjectionClass> MirroredLinqProjectionCache { get; set; }

        #endregion

        #region Repository Members

        /// <summary>
        /// Gets the version info.
        /// </summary>
        /// <returns></returns>
        public virtual VersionInfo GetVersionInfo()
        {
            ICondition versionSelect = new ConditionList(
                new AndCondition(typeof (VersionInfo), "MajorVersion", int.Parse(DatabaseMajorVersion)),
                new AndCondition(typeof (VersionInfo), "MinorVersion", int.Parse(DatabaseMinorVersion)),
                new AndCondition(typeof (VersionInfo), "Application", ApplicationName)
                );

            var result = FlatLoad(typeof (VersionInfo), versionSelect) as VersionInfo;

            if (result == null)
            {
                result = new VersionInfo
                             {
                                 IsActive = true,
                                 Application = ApplicationName,
                                 MajorVersion = TransactionContext.DatabaseMajorVersion,
                                 MinorVersion = TransactionContext.DatabaseMinorVersion
                             };

                BeginTransaction();
                FlatSave(result);
                Commit();
            }

            return result;
        }

        #endregion

        #region LINQ Members for VS2008
#if VS2008

        /// <summary>
        /// Creates a new query object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IQueryable<T> Query<T>()
        {
            return new Query<T>(this);
        }

        /// <summary>
        /// Queries the specified table name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public IQueryable<T> Query<T>(string tableName)
        {
            return new Query<T>(this, tableName);
        }
#endif
        #endregion
    }
}