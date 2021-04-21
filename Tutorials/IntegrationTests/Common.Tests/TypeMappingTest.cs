using System;
using System.Collections.Generic;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class TypeMappingTest :  ObjectMapperTest
    {
        /// <summary>
        /// Tests the type mapping.
        /// </summary>
        [Test]
        public void TestTypeMapping()
        {
            TypeMapping reference = TypeMapping.ReferenceClass;

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(reference);
                OBM.Commit(mapper, nested);

                var loaded = (TypeMapping)mapper.Load(typeof(TypeMapping), reference.Id);

                Assert.AreEqual(reference.TypeBoolean  ,loaded.TypeBoolean ,"Mismatch: Boolean");
                Assert.AreEqual(reference.TypeByte     ,loaded.TypeByte    ,"Mismatch: Byte    ");
                Assert.AreEqual(reference.TypeDateTime ,loaded.TypeDateTime,"Mismatch: DateTime");
                Assert.AreEqual(reference.TypeDecimal  ,loaded.TypeDecimal ,"Mismatch: Decimal ");
                
                // Doubles have to be rounded for some databases, e.g. Oracle. 
                Assert.AreEqual(Math.Round(reference.TypeDouble,10)   ,Math.Round(loaded.TypeDouble,10)  ,"Mismatch: Double  ");

                Assert.AreEqual(reference.TypeGuid     ,loaded.TypeGuid    ,"Mismatch: Guid    ");
                Assert.AreEqual(reference.TypeInt16    ,loaded.TypeInt16   ,"Mismatch: Int16   ");
                Assert.AreEqual(reference.TypeInt32    ,loaded.TypeInt32   ,"Mismatch: Int32   ");
                Assert.AreEqual(reference.TypeInt64    ,loaded.TypeInt64   ,"Mismatch: Int64   ");
                Assert.AreEqual(reference.TypeSingle   ,loaded.TypeSingle  ,"Mismatch: Single  ");
                Assert.AreEqual(reference.TypeString   ,loaded.TypeString  ,"Mismatch: String  ");
                Assert.AreEqual(reference.TypeTimespan ,loaded.TypeTimespan,"Mismatch: Timespan");
                Assert.AreEqual(reference.TypeEnum     ,loaded.TypeEnum    ,"Mismatch: Enum    ");
                Assert.AreEqual(reference.TypeChar     ,loaded.TypeChar    ,"Mismatch: Char    ");
            }
        }

        [Test]
        public void CheckDatabaseModel()
        {
            var integrityCollection = new List<IntegrityInfo>();
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
                integrityCollection.AddRange(mapper.Integrity.CheckIntegrity(GetTypes(), mapper));

            if (integrityCollection.Exists(info => !info.IsValid))
            {
                foreach (IntegrityInfo info in integrityCollection)
                    if (info.IsValid == false)
                        foreach (FieldIntegrity fi in info.MismatchedFields)
                        {
                            Console.WriteLine("Failed to validate " + info.TableName + "." + fi.Name);
                        }
            }

            Assert.IsFalse(integrityCollection.Exists(info => !info.IsValid)
                , "Failed to validate database schema");
        }
    }
}
