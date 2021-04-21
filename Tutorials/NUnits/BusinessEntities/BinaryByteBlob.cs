using System;
using AdFactum.Data;
using AdFactum.Data.Util;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Defines a blob as a byte array
    /// </summary>
    [Table("Binary_Blob")]
    public class BinaryByteBlob : BaseVO, ICreateObject
    {
        private string name;
        private byte[] blob;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryByteBlob"/> class.
        /// </summary>
        public BinaryByteBlob()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryByteBlob"/> class.
        /// </summary>
        /// <param name="blobData">The BLOB data.</param>
        public BinaryByteBlob(byte[] blobData)
        {
            blob = blobData;    
        }

        /// <summary>
        /// Gets or sets the BLOB.
        /// </summary>
        /// <value>The BLOB.</value>
        [PropertyName("Stream")]
        public byte[] Blob
        {
            get { return blob; }
            set { blob = value; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <returns></returns>
        public IValueObject CreateNewObject()
        {
            return new BinaryByteBlob();
        }
    }
}
