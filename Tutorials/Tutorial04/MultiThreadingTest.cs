using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AdFactum.Data;
using AdFactum.Data.Util;
using BusinessEntities;
using NUnit.Framework;

namespace Tutorial04
{
    /// <summary>
    /// Tests the multi threading functionality of the OBM (ObjectMapper Manager)
    /// </summary>
    [TestFixture]
    public class MultiThreadingTest : BaseTest
    {
        /// <summary>
        /// Creates the user thread.
        /// </summary>
        private void CreateUserThread ()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            for (int counter = 0; counter < 3; counter++)
            {
                User user = new User();
                user.Logon = user.Name = string.Concat("User-", threadId, "-", counter);

                Console.WriteLine("Store " + user.Name);
                UserTest.StoreUser(user);
            }
        }

        /// <summary>
        /// Multis the thread.
        /// </summary>
        [Test]
        public void MultiThread()
        {
            /*
             * Delete all existing contacts
             */
            using (ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Delete(typeof (User));
                OBM.Commit(mapper, nested);
            }

            /*
             * Create threads
             */
            List<Thread> threads = new List<Thread>();
            for (int x=0; x<10; x++)
                threads.Add(new Thread(new ThreadStart(CreateUserThread)));

            /*
             * Start threads
             */
            foreach (Thread thread in threads)
                thread.Start();

            /*
             * Wait for ending
             */
            foreach (Thread thread in threads)
                thread.Join();
        }
    }
}
