using System;
using System.Collections.Generic;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Repository;

namespace AdFactum.Data.Oracle
{
    /// <summary>
    /// This oracle persister uses the repository in order to store the value object meta modell.
    /// </summary>
    public class OracleRepositoryPersister : OraclePersister, IPersister
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
        /// <value>The application.</value>
        public string ApplicationName { get; set; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Base Constructor
        /// </summary>
        public OracleRepositoryPersister()
        {
        }

        /// <summary>
        /// Constructor that connects directly to a oracle instance.
        /// </summary>
        /// <param name="user">User name</param>
        /// <param name="password">Password</param>
        /// <param name="dbAlias">Database alias</param>
        public OracleRepositoryPersister(string user, string password, string dbAlias)
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
        public OracleRepositoryPersister(string user, string password, string dbAlias, ISqlTracer tracer)
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
        public OracleRepositoryPersister(string user, string password, string dbAlias,
                                         string additionalConnectionParameters)
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
        public OracleRepositoryPersister(string user, string password, string dbAlias,
                                         string additionalConnectionParameters, ISqlTracer tracer)
        {
            SqlTracer = tracer;
            Connect(user, password, dbAlias, additionalConnectionParameters);
        }

        /// <summary> Returns the Schema Writer </summary>
        public override ISchemaWriter Schema
        {
            get
            {
                return new OracleRepositorySchemaWriter(TypeMapper, DatabaseSchema);
            }
        }

        #endregion

    }
}