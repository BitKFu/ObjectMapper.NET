    /// <summary>
    /// This class is used to create the LinqProvider. It outputs a classfile that can be used as a partial class.
    /// </summary>
    public class CreateLinqProvider
    {
        /// <summary>
        /// All used types 
        /// </summary>
        protected IEnumerable<Type> Types { get; private set;}

        /// <summary>
        /// Namespace used for the Linq Provider
        /// </summary>
        protected string NamespaceName { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CreateLinqProvider(IEnumerable<Type> types, string namespaceName)
        {
            Types = types;
            NamespaceName = namespaceName;
        }

        /// <summary>
        /// Creates the class file.
        /// </summary>
        /// <param name="outputFile">The output file.</param>
        public virtual void CreateClassFile(string outputFile)
        {
            Console.WriteLine("Create Class File ...");

            var providerFile = File.CreateText(outputFile);
            List<string> names = new List<string>();
            WriteHeader(providerFile);

            using (ObjectMapper mapper = FactoryProvider.CreateInstance("Repository.Mapper", typeof(ObjectMapper)) as ObjectMapper)
                foreach (var type in Types)
                {
                    if (type.IsValueObjectType())
                        AppendTypeAccess(providerFile, mapper, type, names);
                }

            WriteFooter(providerFile);

            providerFile.Flush();
            providerFile.Close();
        }


        protected virtual void WriteHeader(StreamWriter writer)
        {
            writer.WriteLine("using AdFactum.Data.Linq;");
            writer.WriteLine("using System.Linq;");
            writer.WriteLine("");
            writer.WriteLine("namespace " + NamespaceName);
            writer.WriteLine("{");
            writer.WriteLine("    public partial class LinqProvider");
            writer.WriteLine("    {");
        }

        protected virtual void WriteFooter(StreamWriter writer)
        {
            writer.WriteLine("    }");
            writer.WriteLine("}");
        }

        protected virtual void AppendTypeAccess(StreamWriter writer, ObjectMapper mapper, Type type, List<string> names)
        {
            string plural = GetPlural(type.Name);
            int counter = 0;
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

                var linkTarget = field.Value.CustomProperty.MetaInfo.LinkTarget;
                var tableName = Table.GetTableInstance(type).GetName(DatabaseType.Oracle);

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

        protected virtual string GetPlural(string name)
        {
            if (name.EndsWith("y"))
                return name.Substring(0, name.Length - 1) + "ies";

            if (name.EndsWith("s"))
                return name;

            return name + "s";
        }

    }
