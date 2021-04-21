using System;
using System.Collections;
using System.Collections.Generic;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    [TestFixture]
    public class WildcardSearch : ObjectMapperTest
    {
        /// <summary>
        /// Searches the friends.
        /// </summary>
        [Test]
        public void SearchFriends ()
        {
            Friend friend1 = new Friend();
            friend1.LastName = "Mustermann";
            friend1.FirstName = "Hugo";

            Friend friend2 = new Friend();
            friend2.LastName = "Mustermann";
            friend2.FirstName = "Hans";

            Friend friend3 = new Friend();
            friend3.LastName = "Noltenbach";
            friend3.FirstName = "Hans";

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(friend1);
                mapper.Save(friend2);
                mapper.Save(friend3);
                OBM.Commit(mapper, nested);

                IList search =
                    mapper.Select(typeof(Friend),
                                  new AndCondition(typeof (Friend), "LastName", QueryOperator.Like_NoCaseSensitive,
                                                   "Mu*"));

                Assert.AreEqual(2, search.Count, "Not all contacts could be loaded.");

                search = mapper.Select(typeof(Friend),
                                       new AndCondition(typeof (Friend), "LastName", QueryOperator.Like_NoCaseSensitive,
                                                        "*mann"));

                Assert.AreEqual(2, search.Count, "Not all contacts could be loaded.");
            }
        }
    }
}
