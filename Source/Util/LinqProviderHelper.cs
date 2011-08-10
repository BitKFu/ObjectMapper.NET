using System;
using System.Collections.Generic;
using System.IO;
using AdFactum.Data.Fields;
using AdFactum.Data.Internal;

namespace AdFactum.Data.Util
{
    /// <summary>
    /// The Linq Provider Helper class is used to create an Linq Provider class out of the given types
    /// </summary>
    public static class LinqProviderHelper
    {
        /// <summary>
        /// Creates an Linq Provider Helper file
        /// </summary>
        public static void CreateLinqProvider(string outputFileCs, string lqNamespace, IEnumerable<Type> types)
        {
            // Create the Linq Provider output file
            var providerFile = File.CreateText(outputFileCs);
            
            // Create the Linq Provider 
            CreateLinqProvider(providerFile, lqNamespace, types);
            
            // Close the file
            providerFile.Close();
        }

        /// <summary>
        /// Creates an Linq Provider Helper file
        /// </summary>
        public static void CreateLinqProvider(TextWriter outputStream, string lqNamespace, IEnumerable<Type> types)
        {
            var names = new List<string>();
            WriteHeader(outputStream, lqNamespace);

            using (var mapper = new ObjectMapper(null))
                foreach (var type in types)
                {
                    if (type.IsValueObjectType())
                        AppendTypeAccess(outputStream, mapper, type, names);
                }

            WriteFooter(outputStream);
            outputStream.Flush();
        }

        private static void WriteHeader(TextWriter writer, string lqNamespace)
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Linq;");
            writer.WriteLine("using AdFactum.Data;");
            writer.WriteLine("using AdFactum.Data.Util;");
            writer.WriteLine("");
            writer.WriteLine("namespace " + lqNamespace);
            writer.WriteLine("{");
            writer.WriteLine("    public partial class LinqProvider : IDisposable");
            writer.WriteLine("    {");
            writer.WriteLine("        protected ObjectMapper mapper;");
            writer.WriteLine("");
            writer.WriteLine("        /// <summary>");
            writer.WriteLine("        /// Default Constructor");
            writer.WriteLine("        /// </summary>");
            writer.WriteLine("        public LinqProvider()");
            writer.WriteLine("        {");
            writer.WriteLine("        }");
            writer.WriteLine("");
            writer.WriteLine("        /// <summary>");
            writer.WriteLine("        /// Default Constructor with Database Connection");
            writer.WriteLine("        /// </summary>");
            writer.WriteLine("        public LinqProvider(DatabaseConnection connection)");
            writer.WriteLine("        {");
            writer.WriteLine("            Connection = connection;");
            writer.WriteLine("        }");
            writer.WriteLine("");
            writer.WriteLine("        /// <summary>");
            writer.WriteLine("        /// Creates an instance of the ObjectMapper .NET");
            writer.WriteLine("        /// </summary>");
            writer.WriteLine("        public virtual ObjectMapper Mapper");
            writer.WriteLine("        {");
            writer.WriteLine("            get { return mapper ?? (mapper = OBM.CreateMapper(Connection)); }");
            writer.WriteLine("        }");
            writer.WriteLine("");
            writer.WriteLine("        /// <summary>");
            writer.WriteLine("        /// Destructor");
            writer.WriteLine("        /// </summary>");
            writer.WriteLine("        ~LinqProvider()");
            writer.WriteLine("        {");
            writer.WriteLine("            Dispose(false);");
            writer.WriteLine("        }");
            writer.WriteLine("");
            writer.WriteLine("        /// <summary>");
            writer.WriteLine("        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.");
            writer.WriteLine("        /// </summary>");
            writer.WriteLine("        public void Dispose()");
            writer.WriteLine("        {");
            writer.WriteLine("            Dispose(true);");
            writer.WriteLine("            GC.SuppressFinalize(this);");
            writer.WriteLine("        }");
            writer.WriteLine("");
            writer.WriteLine("        protected virtual void Dispose(bool b)");
            writer.WriteLine("        {");
            writer.WriteLine("            if (b)");
            writer.WriteLine("            {");
            writer.WriteLine("                // Dispose unmanaged code");
            writer.WriteLine("            }");
            writer.WriteLine("");
            writer.WriteLine("            if (mapper == null) return;");
            writer.WriteLine("            ");
            writer.WriteLine("            mapper.Dispose();");
            writer.WriteLine("            mapper = null;");
            writer.WriteLine("        }");
            writer.WriteLine("");
            writer.WriteLine("        /// <summary>");
            writer.WriteLine("        /// Used to store the connection");
            writer.WriteLine("        /// </summary>");
            writer.WriteLine("        public DatabaseConnection Connection { get; protected set; }");
            writer.WriteLine("");
        }

        private static void WriteFooter(TextWriter writer)
        {
            writer.WriteLine("    }");
            writer.WriteLine("}");
        }

        private static void AppendTypeAccess(TextWriter writer, ObjectMapper mapper, Type type, ICollection<string> names)
        {
            var plural = GetPlural(type.Name);
            var counter = 0;
            while (names.Contains(plural))
                plural = GetPlural(type.Name) + (++counter);
            names.Add(plural);

            writer.WriteLine("        ///<summary>Gain access to " + type.FullName + "</summary>");
            writer.WriteLine("        public IQueryable<" + type.FullName + "> " + plural + " { get { return Mapper.Query<" + type.FullName + ">(); } }");
            writer.WriteLine("");

            var projection = ReflectionHelper.GetProjection(type, mapper.MirroredLinqProjectionCache);
            var fields = projection.GetFieldTemplates(false);
            foreach (var field in fields)
            {
                if (field.Value.FieldType != typeof (ListLink)) continue;
                if (field.Value.CustomProperty.MetaInfo.IsGeneralLinked) continue;

                var linkTarget = field.Value.CustomProperty.MetaInfo.LinkTarget;
                var tableName = Table.GetTableInstance(type).DefaultName;

                plural = type.Name + field.Value.PropertyName;
                counter = 0;
                while (names.Contains(plural))
                    plural = GetPlural(type.Name) + (++counter);
                names.Add(plural);

                writer.WriteLine("        ///<summary>Gain access to " + type.FullName + " -> " + linkTarget + "</summary>");
                writer.WriteLine("        public IQueryable<LinkBridge<" + type.FullName + "," + linkTarget.FullName + ">> " + plural + " { get { return Mapper.Query<LinkBridge<" + type.FullName + "," + linkTarget.FullName + ">>(\"" + tableName + "_" + field.Value.Name + "\"); } }");
                writer.WriteLine("");
            }
        }

        private static string GetPlural(string name)
        {
            if (name.EndsWith("y"))
                return name.Substring(0, name.Length - 1) + "ies";

            if (name.EndsWith("s"))
                return name;

            return name + "s";
        }
    }
}