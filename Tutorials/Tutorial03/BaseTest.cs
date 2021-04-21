using System.IO;
using AdFactum.Data;
using AdFactum.Data.Access;
using BusinessEntities;
using Utils;

namespace Tutorial02
{
	/// <summary>
	/// Base class for all testing szenarios
	/// </summary>
	public class BaseTest
	{
		/// <summary>
		/// Factory that creates the business entities
		/// </summary>
		private IObjectFactory objectFactory = new ObjectFactory();

		/// <summary>
		/// Defines if the configuration has already been initialized
		/// </summary>
		private static bool isInitialized = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseTest"/> class.
		/// </summary>
		protected BaseTest ()
		{
			if (!isInitialized)
			{
				/*
				* Log4Net
				*/
				string log4NetConfig = Path.GetDirectoryName("..\\..\\")+"\\logging.config";
				log4net.Config.XmlConfigurator.Configure(new FileInfo(log4NetConfig));

				isInitialized = true;
			}
		}

		/// <summary>
		/// Factory that creates the business entities
		/// </summary>
		protected IObjectFactory ObjectFactory
		{
			get { return objectFactory; }
		}

		/// <summary>
		/// Creates the mapper.
		/// </summary>
		/// <param name="persister">The persister.</param>
		/// <returns></returns>
		protected ObjectMapper CreateMapper(IPersister persister)
		{
			return new ObjectMapper(
				ObjectFactory,
				persister,
				Transactions.Manual);
		}

		/// <summary>
		/// Gets the access persister.
		/// </summary>
		/// <returns></returns>
		protected AccessPersister GetAccessPersister ()
		{
			return new AccessPersister("accessDb.mdb", string.Empty, new SqlTracer());
		}
	}
}