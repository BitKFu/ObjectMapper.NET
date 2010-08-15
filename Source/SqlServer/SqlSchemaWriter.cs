using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data.SqlServer
{
    public class SqlSchemaWriter : BaseSchemaWriter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeMapper"></param>
        /// <param name="databaseSchema"></param>
        public SqlSchemaWriter(ITypeMapper typeMapper, string databaseSchema) 
            : base(typeMapper, databaseSchema)
        {
        }

        /// <summary>
        /// Overwrite the Drop Table Statements
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="tables"></param>
        /// <param name="persistentTypes"></param>
        protected override void PrivateWriteDropTable(StringBuilder sql, List<string> tables, IEnumerable<Type> persistentTypes)
        {
            sql.Append("--------------------------------------------------------------------------\n");
            sql.Append("--- DROP TABLE STATEMENTS\n");
            sql.Append("--------------------------------------------------------------------------\n");
            IEnumerator enumerator = persistentTypes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var type = (Type)enumerator.Current;
                ProjectionClass projection = ReflectionHelper.GetProjection(type, null);

                string tablename = Table.GetTableInstance(type).DefaultName;
                tables.Add(tablename);

                Dictionary<string, FieldDescription> fields = projection.GetFieldTemplates(false);
                Dictionary<string, FieldDescription>.Enumerator fieldEnumerator = fields.GetEnumerator();
                while (fieldEnumerator.MoveNext())
                {
                    FieldDescription field = fieldEnumerator.Current.Value;

                    if ((field != null) && (field.FieldType.Equals(typeof(ListLink))))
                    {
                        var linkTableName = CreateChildTableName(tablename, field.Name);
                        sql.Append("if object_id('" + TypeMapper.DoCasing(TypeMapper.Quote(linkTableName)) + "', 'U') is not null \n");
                        sql.Append(string.Concat("DROP TABLE ", linkTableName, ";\n"));
                    }
                }
                sql.Append("if object_id('" + TypeMapper.DoCasing(TypeMapper.Quote(tablename)) + "', 'U') is not null \n");
                sql.Append(string.Concat("DROP TABLE ", TypeMapper.Quote(tablename), ";\n"));
            }
            sql.Append("\n");
        }
    }
}