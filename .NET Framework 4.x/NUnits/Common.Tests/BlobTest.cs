using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdFactum.Data.Util;
using NUnit.Framework;
using ObjectMapper.NUnits.BusinessEntities;
using ObjectMapper.NUnits.Core;

namespace ObjectMapper.NUnits.Common.Tests
{
    /// <summary>
    /// Test the 3 ways of creating blobs
    /// </summary>
    [TestFixture]
    public class BlobTest : ObjectMapperTest
    {

        /// <summary>
        /// Creates the test file.
        /// </summary>
        /// <returns></returns>
        private static string CreateTestFile ()
        {
            string tempFile = Path.GetTempFileName();
            StreamWriter writer = File.CreateText(tempFile);
            writer.WriteLine("That's a binary test file.");
            writer.Flush();
            writer.Close();

            return tempFile;
        }

        /// <summary>
        /// Toes the byte array.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        private static byte[] ToByteArray(Stream stream)
        {
            var length = (int)stream.Length;
            stream.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }

        /// <summary>
        /// Tests the binary byte BLOB.
        /// </summary>
        [Test]
        public void TestBinaryByteBlob()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var blob = new BinaryByteBlob();
                blob.Blob = ToByteArray(File.OpenRead(CreateTestFile()));

                /*
                 * Store
                 */
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(blob);
                OBM.Commit(mapper, nested);

                /*
                 * Load
                 */
                var loaded = (BinaryByteBlob)mapper.Load(typeof(BinaryByteBlob), blob.Id);
                Assert.AreEqual(blob.Blob.Length, loaded.Blob.Length, "Could not load blob.");

                var binaries = mapper.Query<BinaryByteBlob>();
                var binarySql = from binary in binaries where binary.Id == blob.Id select binary;
                loaded = binarySql.First();
                Assert.AreEqual(blob.Blob.Length, loaded.Blob.Length, "Could not load blob.");
            }
        }

        /// <summary>
        /// Tests the memory stream BLOB.
        /// </summary>
        [Test]
        public void TestMemoryStreamBlob()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var blob = new MemoryStreamBlob();
                blob.Stream = File.OpenRead(CreateTestFile());

                /*
                 * Store
                 */
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(blob);
                OBM.Commit(mapper, nested);

                /*
                 * Load
                 */
                var loaded = (MemoryStreamBlob)mapper.Load(typeof(MemoryStreamBlob), blob.Id);
                Assert.AreEqual(blob.Stream.Length, loaded.Stream.Length, "Could not load blob.");
            }
        }


        /// <summary>
        /// Tests the memory stream BLOB.
        /// </summary>
        [Test]
        public void TestCharacterBlob()
        {
            using (AdFactum.Data.ObjectMapper mapper = OBM.CreateMapper(Connection))
            {
                var blob = new CharacterBlob();
                blob.Content = File.ReadAllText(CreateTestFile());

                /*
                 * Store
                 */
                bool nested = OBM.BeginTransaction(mapper);
                mapper.Save(blob);
                OBM.Commit(mapper, nested);

                /*
                 * Load
                 */
                var loaded = (CharacterBlob)mapper.Load(typeof(CharacterBlob), blob.Id);
                Assert.AreEqual(blob.Content, loaded.Content, "Could not load blob.");

                var characterBlob = mapper.Query<CharacterBlob>();
                var loaded2 = (from dbBlob in characterBlob where dbBlob.Id == blob.Id select dbBlob).Single();

                Assert.AreEqual(loaded.Id, loaded2.Id);
            }
        }

    }
}