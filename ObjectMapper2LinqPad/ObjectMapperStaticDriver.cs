using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AdFactum.Data.Internal;
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
            // For static drivers, we can use the description of the custom type & its assembly:
            return cxInfo.CustomTypeInfo.GetCustomTypeDescription();
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
                Application.Run(cd);
                return cd.DialogResult == DialogResult.OK;
            }
        }

        /// <summary>
        /// Initializes the context.
        /// </summary>
        public override void InitializeContext(IConnectionInfo cxInfo, object context, QueryExecutionManager executionManager)
        {
            OBM.CurrentSqlTracer = new LinqPadWriter(executionManager.SqlTranslationWriter);
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
                ).Select(queryable => new { queryable.Name, Selector = queryable.PropertyType.GetGenericArguments().First() });

            var result = new List<ExplorerItem>();
            foreach (var valueType in valueTypes)
            {
                var valueTypeItem = new ExplorerItem(valueType.Name, ExplorerItemKind.QueryableObject, ExplorerIcon.Table);
                valueTypeItem.Children = EvaluateValueObject(valueType.Selector);
                result.Add(valueTypeItem);
            }
            return result;
        }

        /// <summary>
        /// Evaluates the value object.
        /// </summary>
        /// <param name="customType">Type of the custom.</param>
        /// <returns></returns>
        private List<ExplorerItem> EvaluateValueObject(Type customType)
        {
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
                if (pmi.Association == AssociationType.OneToOne)
                    explorerItem.Children = EvaluateValueObject(pmi.LinkTarget);

            }

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
