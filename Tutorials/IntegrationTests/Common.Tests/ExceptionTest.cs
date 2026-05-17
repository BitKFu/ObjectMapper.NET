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
                Action action = () => OBM.Commit(mapper, nested);
                Assert.Throws<DirtyObjectException>(action);
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
                Action action = () => ObjectDumper.Write(mapper.Load(typeof (MissingSetter), missingSetter.Id));
                Assert.Throws<MissingSetterException>(action);
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
                Action action = () => mapper.Save(buying);
                Assert.Throws<NoOpenTransactionException>(action);
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
                Action action = () => mapper.Select(typeof(Buying), new Join(noPrimaryKey, "Buying", typeof(Buying)));
                Assert.Throws<NoPrimaryKeyFoundException>(action);
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
            Action action = () => repository = XmlTest.XmlMapper.Repository;
            Assert.Throws<PersisterDoesNotSupportRepositoryException>(action);
        }

        /// <summary>
        /// Tests the transaction already open exception.
        /// </summary>
        [Test]
        public void TestTransactionAlreadyOpenException()
        {
            Action action = () =>
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
            };

            Assert.Throws<TransactionAlreadyOpenException>(action);
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
                Action action = () =>
                {
                    wrongType = mapper.Load(typeof(WrongType), wrongType.Id) as WrongType;
                };

                Assert.Throws<WrongTypeException>(action);

                ObjectDumper.Write(wrongType);
            }
        }


    }
}
