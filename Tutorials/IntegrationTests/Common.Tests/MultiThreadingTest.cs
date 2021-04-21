using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AdFactum.Data.Internal;
using AdFactum.Data.Queries;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// Test the multithreading capibitlity
    /// </summary>
    [TestFixture]
    public class MultiThreadingTest : ObjectMapperTest
    {
        private readonly TimeSpan testTime = new TimeSpan(0, 0, 10);       // 1Minute
        private string[] files;
        private int rnd;
        private DateTime startTime;

        /// <summary>
        /// Tests the multi threading.
        /// </summary>
        [Test]
        [Category("ExcludeForAccess")]
        [Category("ExcludeForSqlServerCE")]
        public void TestMultiThreading()
        {
            files = Directory.GetFiles(".");
            startTime = DateTime.Now;

            Thread writeThread = new Thread(WriteThread);
            Thread deleteThread = new Thread(DeleteThread);
            Thread selectThread = new Thread(SelectThread);

            writeThread.Name = "Write Thread";
            deleteThread.Name = "Delete Thread";
            selectThread.Name = "Select Thread";

            writeThread.Start();
            selectThread.Start();
            deleteThread.Start();

            writeThread.Join();
            deleteThread.Join();
            selectThread.Join();
        }

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <value>The file.</value>
        private string RandomFile
        {
            get
            {
                int next = ++rnd%files.Length;
                return files[next];
            }
        }

        /// <summary>
        /// Gets a value indicating whether [end threads].
        /// </summary>
        /// <value><c>true</c> if [end threads]; otherwise, <c>false</c>.</value>
        private bool EndThreads
        {
            get
            {
                return DateTime.Now - startTime > testTime;
            }
        }

        /// <summary>
        /// Test write access
        /// </summary>
        public void WriteThread()
        {
            do
            {
                using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
                {
                    //BaseCache.ClearAllCaches();     // To clear the cache is not necessary, but it makes it more error-prone. ;)
                    string fileName = RandomFile;
                    int count = mapper.Count(typeof (FileStore), new AndCondition(typeof (FileStore), "FileName", fileName));
                    if (count == 1)
                        continue;

//                    Console.WriteLine("Save " + fileName);
                    using (FileStream stream = File.OpenRead(fileName))
                    {
                        FileStore fileStore = new FileStore();
                        fileStore.FileName = fileName;
                        fileStore.FileSize = (int)stream.Length;
                        fileStore.CreationDate = File.GetCreationTime(fileName);
                        fileStore.ModificationDate = File.GetLastWriteTime(fileName);

                        bool nested = OBM.BeginTransaction(mapper);
                        mapper.Save(fileStore);
                        OBM.Commit(mapper, nested);
                    }

                }
            } while (!EndThreads);
        }

        /// <summary>
        /// Test delete access
        /// </summary>
        public void DeleteThread()
        {
            do
            {
                using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
                {
                    //BaseCache.ClearAllCaches();     // To clear the cache is not necessary, but it makes it more error-prone. ;)
                    string fileName = RandomFile;
                    int count = mapper.Count(typeof(FileStore), new AndCondition(typeof(FileStore), "FileName", fileName));
                    if (count == 0)
                        continue;

//                    Console.WriteLine("Delete " + fileName);
                    bool nested = OBM.BeginTransaction(mapper);
                    mapper.Delete(typeof(FileStore), new AndCondition(typeof(FileStore), "FileName", fileName));
                    OBM.Commit(mapper, nested);
                }

            } while (!EndThreads);
        }

        /// <summary>
        /// Test select access
        /// </summary>
        public void SelectThread ()
        {
            do
            {
                using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
                {
                    //BaseCache.ClearAllCaches();     // To clear the cache is not necessary, but it makes it more error-prone. ;)
                    string fileName = RandomFile;
                    FileStore fileStore =
                        (FileStore)
                        mapper.Load(typeof (FileStore), new AndCondition(typeof (FileStore), "FileName", fileName));
                    if (fileStore == null)
                        continue;

//                    Console.WriteLine("Select " + fileName);
                }
            } while (!EndThreads);
        }
    }
}
