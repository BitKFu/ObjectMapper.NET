using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class DeleteTest : ObjectMapperTest
    {
        /// <summary>
        /// This test verifies that no exception is thrown if you try to delete an entity
        /// which wasn't stored in the db.
        /// </summary>
        [Test]
        public void Delete_TransientObject_NoException()
        {
            // Arrange & Act
            Contact contact = new Contact("Bernd", "Beispiel");

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                OBM.BeginTransaction(mapper);
                mapper.DeleteRecursive(contact, HierarchyLevel.Dependend1stLvl);
                OBM.Commit(mapper, false);
            }
        }
    }
}
