using System;
using System.Collections.Generic;
using AdFactum.Data.Repository;
using AdFactum.Data.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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

                ClassicAssert.AreEqual(reference.TypeBoolean  ,loaded.TypeBoolean ,"Mismatch: Boolean");
                ClassicAssert.AreEqual(reference.TypeByte     ,loaded.TypeByte    ,"Mismatch: Byte    ");
                ClassicAssert.AreEqual(reference.TypeDateTime ,loaded.TypeDateTime,"Mismatch: DateTime");
                ClassicAssert.AreEqual(reference.TypeDecimal  ,loaded.TypeDecimal ,"Mismatch: Decimal ");
                
                // Doubles have to be rounded for some databases, e.g. Oracle. 
                ClassicAssert.AreEqual(Math.Round(reference.TypeDouble,10)   ,Math.Round(loaded.TypeDouble,10)  ,"Mismatch: Double  ");

                ClassicAssert.AreEqual(reference.TypeGuid     ,loaded.TypeGuid    ,"Mismatch: Guid    ");
                ClassicAssert.AreEqual(reference.TypeInt16    ,loaded.TypeInt16   ,"Mismatch: Int16   ");
                ClassicAssert.AreEqual(reference.TypeInt32    ,loaded.TypeInt32   ,"Mismatch: Int32   ");
                ClassicAssert.AreEqual(reference.TypeInt64    ,loaded.TypeInt64   ,"Mismatch: Int64   ");
                ClassicAssert.AreEqual(reference.TypeSingle   ,loaded.TypeSingle  ,"Mismatch: Single  ");
                ClassicAssert.AreEqual(reference.TypeString   ,loaded.TypeString  ,"Mismatch: String  ");
                ClassicAssert.AreEqual(reference.TypeTimespan ,loaded.TypeTimespan,"Mismatch: Timespan");
                ClassicAssert.AreEqual(reference.TypeEnum     ,loaded.TypeEnum    ,"Mismatch: Enum    ");
                ClassicAssert.AreEqual(reference.TypeChar     ,loaded.TypeChar    ,"Mismatch: Char    ");
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

            ClassicAssert.IsFalse(integrityCollection.Exists(info => !info.IsValid)
                , "Failed to validate database schema");
        }
    }
}
