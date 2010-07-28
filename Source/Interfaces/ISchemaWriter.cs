using System;
using System.Collections.Generic;
using System.IO;
using AdFactum.Data.Repository;

namespace AdFactum.Data.Interfaces
{
    /// <summary>
    /// This interface is used to cover all methods in order to write a database schema
    /// </summary>
    public interface ISchemaWriter
    {
        /// <summary>
        /// Export a database schema file.
        /// </summary>
        /// <param name="schemaFile">File name for the schema export</param>
        /// <param name="persistentTypes">Array with persistent object types that shall be exported.</param>
        void WriteSchema(string schemaFile, IEnumerable<Type> persistentTypes);

        /// <summary>
        /// Export a database schema file.
        /// </summary>
        /// <param name="ouputStream">Output Stream</param>
        /// <param name="persistentTypes">Array with persistent object types that shall be exported.</param>
        void WriteSchema(TextWriter ouputStream, IEnumerable<Type> persistentTypes);

        /// <summary>
        /// Writes the schema dif file in order to update a database to the needed sql schema.
        /// </summary>
        /// <param name="schemaFile">The schema file.</param>
        /// <param name="persistentTypes">The persistent types.</param>
        /// <param name="integrityInfos">Integrity Info List</param>
        void WriteSchemaDif(string schemaFile, IEnumerable<Type> persistentTypes, IEnumerable<IntegrityInfo> integrityInfos);

        /// <summary>
        /// Writes the schema dif file in order to update a database to the needed sql schema.
        /// </summary>
        /// <param name="ouputStream">Output Stream</param>
        /// <param name="persistentTypes">The persistent types.</param>
        /// <param name="integrityInfos">Integrity Info List</param>
        void WriteSchemaDif(TextWriter ouputStream, IEnumerable<Type> persistentTypes, IEnumerable<IntegrityInfo> integrityInfos);
    }
}
