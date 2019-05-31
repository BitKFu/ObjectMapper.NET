using System;
using System.Collections.Generic;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class Repository : ObjectMapperTest 
    {
        /// <summary>
        /// Writes the repository.
        /// </summary>
        [Test]
        public void WriteRepository ()
        {
            var typesToWriteInRepository = new List<Type>
                                               {
                                                   typeof (Translation),
                                                   typeof (Country),
                                                   typeof (CountryRegion)
                                               };

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
                mapper.Repository.WriteRepository(mapper, typesToWriteInRepository);  
        }
        
    }
}
