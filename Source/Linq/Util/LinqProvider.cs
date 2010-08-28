    /// <summary>
    /// The LinqProvider offers methods to access the value objects as a Linq Resource
    /// </summary>
    public class LinqProvider : IDisposable
    {
        /// <summary> ObjectMapper  </summary>
        public ObjectMapper Mapper { get; private set;}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mapper"></param>
        public LinqProvider(ObjectMapper mapper)
        {
            Mapper = mapper;    
        }

        /// <summary> Desctructor </summary>
        ~LinqProvider()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        /// <param name="dispose"></param>
        private void Dispose(bool dispose)
        {
            if (dispose)
            {
                Mapper.Dispose();
                Mapper = null;
            }
        }        

 