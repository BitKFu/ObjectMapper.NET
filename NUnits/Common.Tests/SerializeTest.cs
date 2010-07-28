using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using AdFactum.Data;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.BusinessEntities.Core;
using IgnoreAttribute=AdFactum.Data.IgnoreAttribute;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class SerializeTest
    {
        /// <summary>
        /// Serializes the attributes.
        /// </summary>
        [Test]
        public void SerializeAttributes()
        {
            foreach (KeyValuePair<Type, object> type in AttributeTypes)
            {
                Console.WriteLine("Serialize " + type.Key);
                var serializer = new XmlSerializer(type.Key);

                var output = new MemoryStream();
                serializer.Serialize(output, type.Value);
                output.Seek(0, SeekOrigin.Begin);

                Assert.IsNotNull(serializer.Deserialize(output), "Object could de-serialized.");
            }
        }

        /// <summary>
        /// Gets the meta informations.
        /// </summary>
        [Test]
        public void GetMetaInformations()
        {
            PropertyMetaInfo[] metaInfos = ReflectionHelper.GetPropertyMetaInfos(typeof(Employee));    
            
            foreach (PropertyMetaInfo info in metaInfos)
                Console.WriteLine(info.PropertyName);
        }

        /// <summary>
        /// Gets the attribute types.
        /// </summary>
        /// <value>The attribute types.</value>
        /// <returns></returns>
        private static Dictionary<Type, object> AttributeTypes 
        { 
            get
            {
                var result = new Dictionary<Type, object>();
                result.Add(typeof (BackLinkAttribute), new BackLinkAttribute(typeof(Company_With_Employees), "LegalName", "Employees", typeof(BackLinkedEmployee)));
                result.Add(typeof (BindPropertyToAttribute), new BindPropertyToAttribute(typeof(Contact)));
                result.Add(typeof (ForeignKeyAttribute), new ForeignKeyAttribute(typeof(Company_With_Employees), "LegalName"));
                result.Add(typeof (GeneralLinkAttribute), new GeneralLinkAttribute());
                result.Add(typeof (IgnoreAttribute), new IgnoreAttribute());
                result.Add(typeof (OneToManyAttribute), new OneToManyAttribute(typeof(Employee), "CompanyId"));
                result.Add(typeof (PropertyLengthAttribute), new PropertyLengthAttribute(50));
                result.Add(typeof (PropertyNameAttribute), new PropertyNameAttribute("Test"));
                result.Add(typeof (RequiredAttribute), new RequiredAttribute());
                result.Add(typeof (PrimaryKeyAttribute), new PrimaryKeyAttribute());
                result.Add(typeof (StaticDataAttribute), new StaticDataAttribute());
                result.Add(typeof (TableAttribute), new TableAttribute("TestTable"));
                result.Add(typeof (UniqueAttribute), new UniqueAttribute());
                //result.Add(typeof (ValidSinceAttribute), new ValidSinceAttribute(2.0));
                //result.Add(typeof (ValidUntilAttribute), new ValidUntilAttribute(2.0));
                result.Add(typeof (VirtualLinkAttribute), new VirtualLinkAttribute(typeof(Product), "ProductName", "ProductKey", "ProductKey", "ValidUntil", "@VALIDATION_DATE"));
                result.Add(typeof (WeakReferencedAttribute), new WeakReferencedAttribute());
                result.Add(typeof (PropertyMetaInfo), new Property(typeof(BackLinkedEmployee).GetPropertyInfo("CompanyName")).MetaInfo);
                result.Add(typeof (Table), new Table());
                
                return result;
            }
        }
    }
}
