using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Interfaces;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;
using ObjectMapper.NUnits.Xml.Tests;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// Tests the exceptions.
    /// </summary>
    [TestFixture]
    public class ExceptionTest : ObjectMapperTest
    {
        /// <summary>
        /// Tests the could not insert data exception.
        /// </summary>
        [Test]
        public void TestDirtyObjectException()
        {
            Buying buying = new Buying(5, "Hotdogs");
            
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                // First save
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(buying);
                OBM.Commit(mapper, nested);

                // Simulate, that an other user has changed the object
                buying.Count = 3; // the object has to be changed
                buying.LastUpdate = buying.LastUpdate.AddHours(-1);

                // Second save, tend to fail
                nested = OBM.BeginTransaction(mapper);
                mapper.Save(buying);
                Assert.Throws<DirtyObjectException>(() => OBM.Commit(mapper, nested));
            }
        }

        /// <summary>
        /// Tests the missing setter exception.
        /// </summary>
        [Test]
        public void TestMissingSetterException()
        {
            MissingSetter missingSetter = new MissingSetter();

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                // First save
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(missingSetter);
                OBM.Commit(mapper, nested);

                // The Load throws the MissingSetterException, because the value can't be set to the object.
                Assert.Throws<MissingSetterException>(() => ObjectDumper.Write(mapper.Load(typeof (MissingSetter), missingSetter.Id)));
            }
        }

        /// <summary>
        /// Tests the no open transaction exception.
        /// </summary>
        [Test]
        public void TestNoOpenTransactionException()
        {
            Buying buying = new Buying(5, "Hotdogs");

            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                // A NoOpenTransactionException will be thrown, because no transaction has been opened.
                Assert.Throws<NoOpenTransactionException>(() =>  mapper.Save(buying));
            }
        }

        /// <summary>
        /// Tests the no primary key found exception.
        /// </summary>
        [Test]
        public void TestNoPrimaryKeyFoundException()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                NoPrimaryKey noPrimaryKey = new NoPrimaryKey();
                noPrimaryKey.Buying = new Buying(2, "Tissues");

                // Select sub object to produce a NoPrimaryKeyException
                Assert.Throws<NoPrimaryKeyFoundException>(() => mapper.Select(typeof(Buying), new Join(noPrimaryKey, "Buying", typeof(Buying))));
            }
        }


        /// <summary>
        /// Tests the persister does not support method exception.
        /// </summary>
        [Test]
        public void TestPersisterDoesNotSupportRepositoryException()
        {
            // Select sub object to produce a NoPrimaryKeyException
            IRepository repository;
            Assert.Throws<PersisterDoesNotSupportRepositoryException>(() => repository = XmlTest.XmlMapper.Repository);
        }

        /// <summary>
        /// Tests the transaction already open exception.
        /// </summary>
        [Test]
        public void TestTransactionAlreadyOpenException()
        {
            Assert.Throws<TransactionAlreadyOpenException>(() =>
            {
                using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
                {
                    try
                    {
                        mapper.BeginTransaction();
                        mapper.BeginTransaction(); // Second try causes the exception
                    }
                    catch (Exception)
                    {
                        mapper.Commit();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Tests the wrong type exception.
        /// </summary>
        [Test]
        public void TestWrongTypeException()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                WrongType wrongType = new WrongType();
                wrongType.Number = 5;
                
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(wrongType);
                OBM.Commit(mapper, nested);

                // The first try is fine, because the Type is not in Cache
                wrongType = mapper.Load(typeof (WrongType), wrongType.Id) as WrongType;

                // The second try throws the exeption, because a shallow of the copy is cached and the method CreateNewObject will be called.
                Assert.Throws<WrongTypeException>(() =>
                {
                    wrongType = mapper.Load(typeof(WrongType), wrongType.Id) as WrongType;
                });

                ObjectDumper.Write(wrongType);
            }
        }


    }
}
