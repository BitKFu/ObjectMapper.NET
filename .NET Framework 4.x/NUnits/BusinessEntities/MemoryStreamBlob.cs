using System;
using System.IO;
using AdFactum.Data;
using AdFactum.Data.Util;
using ObjectMapper.NUnits.BusinessEntities.Core;

namespace ObjectMapper.NUnits.BusinessEntities
{
    /// <summary>
    /// Defines a blob as a stream
    /// </summary>
    [Table("Stream_Blob")]
    public class MemoryStreamBlob : BaseVO, ICreateObject 
    {
        private string name;
        private Stream stream = new MemoryStream();

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryByteBlob"/> class.
        /// </summary>
        public MemoryStreamBlob()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryByteBlob"/> class.
        /// </summary>
        /// <param name="streamData">The stream data.</param>
        public MemoryStreamBlob(Stream streamData)
        {
            stream = streamData;    
        }

        /// <summary>
        /// Gets or sets the BLOB.
        /// </summary>
        /// <value>The BLOB.</value>
        [PropertyName("Stream")]
        public Stream Stream
        {
            get { return stream; }
            set { stream = value; }
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
            return new MemoryStreamBlob();
        }
    }
}
