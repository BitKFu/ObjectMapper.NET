using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using AdFactum.Data.Exceptions;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace AdFactum.Data
{
	/// <summary>
	/// This class is used for transaction handling
	/// </summary>
	public class TransactionContext : ITransactionContext, IDisposable
	{
		/// <summary>
		/// Update object cache. This object cache handles circular object dependencies and 
		/// prevents the database mapper for stepping in a trap.
		/// </summary>
		private ObjectHash updateHash;

		/// <summary>
		/// Second Step update hash used for objects that references to their parents.
		/// </summary>
		private ObjectHash secondStepUpdateHash;

		/// <summary>
		/// Points to a object that implements the persister interface in order to interact with the core database engine.
		/// </summary>
		private IPersister persister;

		/// <summary>
		/// Transaction Mutex handles the single write access to the database
		/// </summary>
		private Mutex transactionMutex;

		/// <summary>
		/// True, if a transaction is open.
		/// </summary>
		private bool isTransaction;

		/// <summary>
		/// List that holds all New Objects which have to be reseted if a rollback takes place.
		/// </summary>
		private IList rollbackList;

		/// <summary>
		/// Defines the transaction settings for the database mapper.
		/// </summary>
		private Transactions transactionSetting;

		/// <summary>
		/// Defines the database major version of the dependent object model.
		/// </summary>
		private int majorVersion;

		/// <summary>
		/// Defines the database minor version of the dependent object model.
		/// </summary>
		private int minorVersion;

		/// <summary>
		/// Defines the name of the application
		/// </summary>
		private string applicationName;

		/// <summary>
		/// Get or sets the current version of the dependent database object model.
		/// </summary>
		public double DatabaseVersion
		{
			get
			{
				string versionstring = string.Concat(DatabaseMajorVersion + "." + DatabaseMinorVersion);
				double version = double.Parse(versionstring, CultureInfo.InvariantCulture.NumberFormat);
				return version;
			}
			set
			{
				int dbMajorVersion = (int) Math.Floor(value);							
				int dbMinorVersion = (int) ((value - dbMajorVersion)*10000+50)/100;		

				DatabaseMajorVersion = dbMajorVersion;
				DatabaseMinorVersion = dbMinorVersion;
			}
		}

		/// <summary>
		/// Get or sets the current version of the dependent database object model.
		/// </summary>
		/// <value>The database version.</value>
		public int DatabaseMajorVersion
		{
			get { return majorVersion; }
			set	{ majorVersion = value;	}
		}

		/// <summary>
		/// Get or sets the current version of the dependent database object model.
		/// </summary>
		/// <value>The database version.</value>
		public int DatabaseMinorVersion
		{
			get { return minorVersion; }
			set { minorVersion = value; }
		}

		/// <summary>
		/// Returns the persister class in order to interact with the core database engine.
		/// </summary>
		public IPersister Persister
		{
			get { return persister; }
		    set { persister = value; }
		}

		/// <summary>
		/// Returns true, if a transaction is open
		/// </summary>
		public bool IsTransactionOpen
		{
			get { return isTransaction; }
		}

		/// <summary>
		/// Returns true, if a transaction is open
		/// </summary>
		public Transactions TransactionSetting
		{
			get { return transactionSetting; }
		    set { transactionSetting = value;}
		}


		/// <summary>
		/// Constructor for a transaction
		/// </summary>
		/// <param name="_persister">The _persister.</param>
		/// <param name="_transactions">The _transactions.</param>
		public TransactionContext(
			IPersister _persister,
			Transactions _transactions
			)
		{
			transactionMutex = new Mutex();
			updateHash = new ObjectHash();
			secondStepUpdateHash = new ObjectHash();
			rollbackList = new ArrayList();

			persister = _persister;
			transactionSetting = _transactions;
		}

		/// <summary>
		/// Begins a transaction
		/// </summary>
		public void BeginTransaction()
		{
			transactionMutex.WaitOne();
			if (isTransaction)
				throw new TransactionAlreadyOpenException();

			persister.BeginTransaction();
			updateHash = new ObjectHash();
			secondStepUpdateHash = new ObjectHash();
			rollbackList = new ArrayList();
			isTransaction = true;
		}

		/// <summary>
		/// Returns true, if the values within the update Hash have been changed
		/// </summary>
		public bool IsModified
		{
			get
			{
				if (updateHash == null)
					return false;

				return updateHash.IsModified;
			}
		}

		/// <summary>
		/// Returns true, if the curren transaction context is valid and can be used.
		/// </summary>
		public bool IsValid
		{
			get { return persister != null; }
		}

		/// <summary>
		/// Checks if a transaction is open. 
		/// If not, a mapper exception will be thrown.
		/// </summary>
		public void CheckOpenTransaction()
		{
			/*
			 * Only check when the transaction Setting is manual
			 */
			if (TransactionSetting == Transactions.Automatic)
				return;

			if (!isTransaction)
				throw new NoOpenTransactionException();
		}

		/// <summary>
		/// Executes a commit
		/// </summary>
		public void Commit()
		{
			CheckOpenTransaction();

			updateHash.Persist(persister, this);
            if (secondStepUpdateHash != null)
            {
                secondStepUpdateHash.UpdateAutoincrementedIds();
                secondStepUpdateHash.Persist(persister, this);
            }

		    persister.Commit();
			updateHash = new ObjectHash();
			secondStepUpdateHash = new ObjectHash();
			rollbackList = new ArrayList();

			transactionMutex.ReleaseMutex();
			isTransaction = false;
		}

		/// <summary>
		/// Flushes the content of the object hash to database.
		/// This method does not close the transaction.
		/// </summary>
		public void Flush()
		{
			CheckOpenTransaction();

			updateHash.Persist(persister, this);
			secondStepUpdateHash.Persist(persister, this);

			updateHash = new ObjectHash();
			secondStepUpdateHash = new ObjectHash();
			rollbackList = new ArrayList();
		}

		/// <summary>
		/// Executes a Rollback
		/// </summary>
		public void Rollback()
		{
			if (!isTransaction)
				throw new NoOpenTransactionException();

			persister.Rollback();

			/*
			 * Set all objects formerly knwon as NEW, back to state new
			 */
			IEnumerator newVo = rollbackList.GetEnumerator();
			while (newVo.MoveNext())
				(newVo.Current as IValueObject).IsNew = true;

			updateHash = new ObjectHash();
			secondStepUpdateHash = new ObjectHash();
			rollbackList = new ArrayList();

			/*
			 * Close the transaction
			 */
			isTransaction = false;
			transactionMutex.ReleaseMutex();
		}

		/// <summary>
		/// Creates a temporary object hash.
		/// </summary>
		/// <returns>Object Hash</returns>
		public ObjectHash UpdateHash()
		{
			if ((transactionSetting == Transactions.Manual) && (isTransaction))
				return updateHash;

			return new ObjectHash();
		}

		/// <summary>
		/// Creates the second step hash.
		/// </summary>
		/// <returns></returns>
		public ObjectHash SecondUpdateHash ()
		{
			if ((transactionSetting == Transactions.Manual) && (isTransaction))
				return secondStepUpdateHash;

			return new ObjectHash();
		}

		/// <summary>
		/// Diese Version wird intern verwendet, um den temporären Hash direkt
		/// zu persistieren
		/// </summary>
		/// <param name="pUpdateHash">The temp hash.</param>
		/// <param name="pSecondUpdateHash">The second step hash.</param>
		private void privateCommit(ObjectHash pUpdateHash, ObjectHash pSecondUpdateHash)
		{
			CheckOpenTransaction();

			pUpdateHash.Persist(Persister, this);
			if (pSecondUpdateHash != null)
				pSecondUpdateHash.Persist(Persister, this);
			Persister.Commit();

			isTransaction = false;
			transactionMutex.ReleaseMutex();
		}

		/// <summary>
		/// Opens a transaction, stores the data and closes the transaction.
		/// </summary>
		/// <param name="tempHash">Object cache that holds the objcets which shall be stored in database.</param>
		/// <param name="secondStepHash">The second step hash.</param>
		public void AutoTransaction(ObjectHash tempHash, ObjectHash secondStepHash)
		{
			if (TransactionSetting == Transactions.Manual)
				return;

			BeginTransaction();

			try
			{
				privateCommit(tempHash, secondStepHash);
			}
			catch (Exception)
			{
				/*
				 * Eigene Exceptions direkt weiterleiten
				 */
				Rollback();
				throw;
			}
		}

		/// <summary>
		/// Merges the update Hash with the primary Hash
		/// Because there is no primary hash, the method does only a clean up
		/// </summary>
		public void MergeHash(ObjectHash tempHash)
		{
			tempHash.CleanFlatLoaded();
		}

		/// <summary>
		/// Adds a new object to the rollbacklist in order to set the property isNew to false, if a rollback takes place.
		/// </summary>
		/// <param name="vo"></param>
		public void AddToRollbackList(IValueObject vo)
		{
			if (vo.IsNew == false)
				throw new NotSupportedException("Only new value objects are supported to add to the rollback list.");

			rollbackList.Add(vo);
		}

		/// <summary>
		/// Gets or sets the name of the application.
		/// </summary>
		/// <value>The name of the application.</value>
		public string ApplicationName
		{
			get { return applicationName; }
			set { applicationName = value; }
		}

        #region Dispose Pattern

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="T:AdFactum.Data.XmlPersister.XmlPersister"/> is reclaimed by garbage collection.
        /// </summary>
        ~TransactionContext()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disconnecting the database
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                updateHash = null;
                secondStepUpdateHash = null;
                rollbackList = null;

                if (persister != null)
                    persister.Dispose();

                persister = null;
            }

            // free unmanaged resources
        }

        #endregion

	}
}