using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AdFactum.Data;
using AdFactum.Data.Internal;
using AdFactum.Data.Linq;
using AdFactum.Data.Util;
using LINQPad.Extensibility.DataContext;

namespace ObjectMapper2LinqPad
{
    /// <summary>
    /// This is a static context driver for the LinqPAD support
    /// </summary>
    public class ObjectMapperStaticDriver : StaticDataContextDriver
    {
        /// <summary>
        /// Gets the connection description.
        /// </summary>
        public override string GetConnectionDescription(IConnectionInfo cxInfo)
        {
            var connection = new LinqPadConnection(cxInfo);
            switch (connection.DatabaseType)
            {
                case DatabaseType.Oracle:
                    return connection.UserName + "@" + connection.DbAlias;

                case DatabaseType.SqlServer2000:
                case DatabaseType.SqlServer:
                    return connection.UserName + "@" + connection.DatabaseName + " (" + connection.ServerName + ")";

                case DatabaseType.Postgres:
                    return connection.UserName + "@" + connection.DatabaseName + " (" + connection.ServerName + ")";

                case DatabaseType.Access:
                    return connection.DatabaseFile;

                // For static drivers, we can use the description of the custom type & its assembly:
                default:
                    return cxInfo.CustomTypeInfo.GetCustomTypeDescription();
            }

        }

        /// <summary>
        /// Shows the connection dialog.
        /// </summary>
        public override bool ShowConnectionDialog(IConnectionInfo cxInfo, bool isNewConnection)
        {
            // Prompt the user for a custom assembly and type name:
            WindowsFormsSynchronizationContext.AutoInstall = true;

            using (var cd = new ConnectionDialog(cxInfo))
            {
                try
                {
                    Application.Run(cd);
                }
                catch(InvalidOperationException)
                {
                    cd.ShowDialog();
                }

                Application.Exit();
                return cd.DialogResult == DialogResult.OK;
            }
        }

        /// <summary>
        /// Initializes the context.
        /// </summary>
        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            var sqlTracer = new LinqPadWriter(executionManager.SqlTranslationWriter);
            OBM.CurrentSqlTracer = sqlTracer;

            // If there's already a transaction, than set the SQL Tracer into that transaction
            var transaction = OBM.CurrentTransaction;
            var tcontext = transaction != null ? transaction.TransactionContext : null;
            var persister = tcontext != null ? tcontext.Persister : null;

            if (persister != null)
                persister.SqlTracer = sqlTracer;
        }

        public override IEnumerable<string> GetNamespacesToAdd()
        {
            return new[] {"AdFactum.Data.Linq"};
        }

        public override IEnumerable<string> GetAssembliesToAdd()
        {
            return new[] {"ObjectMapper.dll"};
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get { return "ObjectMapper .NET LinqPAD Driver"; }
        }

        /// <summary>
        /// Gets the author.
        /// </summary>
        /// <value>The author.</value>
        public override string Author
        {
            get { return "Gerhard Stephan"; }
        }

        /// <summary>
        /// Gets the schema.
        /// </summary>
        public override List<ExplorerItem> GetSchema(IConnectionInfo cxInfo, Type customType)
        {
            var valueTypes = customType.GetProperties().Where(
                property => property.PropertyType.IsQueryable()
                ).OrderBy(queryable => queryable.Name)
                .Select(queryable => new { queryable.Name, Selector = queryable.PropertyType.GetGenericArguments().First() });

            var result = new List<ExplorerItem>();
            foreach (var valueType in valueTypes)
            {
                ExplorerIcon icon = valueType.Selector.GetCustomAttributes(typeof (ViewAttribute), false).Length > 0
                                        ? ExplorerIcon.View
                                        : valueType.Selector.IsValueObjectType() || 
                                        ( valueType.Selector.IsGenericType && valueType.Selector.GetGenericTypeDefinition() == typeof(LinkBridge<,>) )
                                              ? ExplorerIcon.Table
                                              : ExplorerIcon.View;

                var valueTypeItem = new ExplorerItem(valueType.Name, ExplorerItemKind.QueryableObject, icon);

                valueTypeItem.Children = EvaluateValueObject(valueType.Selector, new HashSet<Type>());
                result.Add(valueTypeItem);
            }
            return result;
        }

        /// <summary>
        /// Evaluates the value object.
        /// </summary>
        /// <param name="customType">Type of the custom.</param>
        /// <returns></returns>
        private List<ExplorerItem> EvaluateValueObject(Type customType, HashSet<Type> evaluatedTypes)
        {
            evaluatedTypes.Add(customType);

            var properties = new List<ExplorerItem>();
            foreach (var pmi in ReflectionHelper.GetPropertyMetaInfos(customType)
                .Where(pmi => !pmi.IsIgnore && !pmi.IsVirtualLink).OrderBy(pmi=>pmi.PropertyName))
            {
                ExplorerIcon icon = ExplorerIcon.Column;

                
                if (pmi.IsPrimaryKey) icon = ExplorerIcon.Key;
                if (pmi.IsForeignKey) icon = ExplorerIcon.Key;
                switch (pmi.Association)
                {
                    case AssociationType.OneToOne:
                        icon = ExplorerIcon.OneToOne;
                        break;
                    case AssociationType.OneToMany:
                        icon = ExplorerIcon.OneToMany;
                        break;
                    case AssociationType.ManyToMany:
                        icon = ExplorerIcon.ManyToMany;
                        break;
                }

                var explorerItem = new ExplorerItem(pmi.PropertyName, ExplorerItemKind.Property, icon);
                properties.Add(explorerItem);

                // Perhaps we can go more deeper
                if (pmi.Association == AssociationType.OneToOne && !evaluatedTypes.Contains(pmi.LinkTarget))
                    explorerItem.Children = EvaluateValueObject(pmi.LinkTarget, evaluatedTypes);

            }
            evaluatedTypes.Remove(customType);
            return properties;
        }

        public override object[] GetContextConstructorArguments(IConnectionInfo cxInfo)
        {
            var connection = new LinqPadConnection(cxInfo);
            return new object[]{connection};
        }

        public override ParameterDescriptor[] GetContextConstructorParameters(IConnectionInfo cxInfo)
        {
            return new[]{new ParameterDescriptor("connection", typeof(DatabaseConnection).FullName), };
        }
    }
}
