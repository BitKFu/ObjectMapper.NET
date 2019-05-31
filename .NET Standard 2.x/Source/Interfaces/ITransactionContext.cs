using System;
using AdFactum.Data.Internal;

namespace AdFactum.Data
{
	/// <summary>
	/// This interface is used for the transaction handling
	/// </summary>
	public interface ITransactionContext : IDisposable
	{
		/// <summary>
		/// Gets or sets the database version.
		/// </summary>
		/// <value>The database version.</value>
		double DatabaseVersion { get; set; }

		/// <summary>
		/// Get or sets the current version of the dependent database object model.
		/// </summary>
		/// <value>The database version.</value>
		int DatabaseMajorVersion { get; set; }

		/// <summary>
		/// Get or sets the current version of the dependent database object model.
		/// </summary>
		/// <value>The database version.</value>
		int DatabaseMinorVersion { get; set; }

		/// <summary>
		/// Returns the persister class in order to interact with the core database engine.
		/// </summary>
		/// <value>The persister.</value>
		IPersister Persister { get; set; }

		/// <summary>
		/// Returns true, if a transaction is open
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is transaction open; otherwise, <c>false</c>.
		/// </value>
		bool IsTransactionOpen { get; }

		/// <summary>
		/// Returns true, if a transaction is open
		/// </summary>
		/// <value>The transaction setting.</value>
        Transactions TransactionSetting { get; set; }

		/// <summary>
		/// Begins a transaction
		/// </summary>
		void BeginTransaction();

		/// <summary>
		/// Returns true, if the values within the update Hash have been changed
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is modified; otherwise, <c>false</c>.
		/// </value>
		bool IsModified { get; }

		/// <summary>
		/// Returns true, if the curren transaction context is valid and can be used.
		/// </summary>
		/// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
		bool IsValid { get; }

		/// <summary>
		/// Checks if a transaction is open.
		/// If not, a mapper exception will be thrown.
		/// </summary>
		void CheckOpenTransaction();

		/// <summary>
		/// Executes a commit
		/// </summary>
		void Commit();

		/// <summary>
		/// Flushes the content of the object hash to database.
		/// This method does not close the transaction.
		/// </summary>
		void Flush();

		/// <summary>
		/// Executes a Rollback
		/// </summary>
		void Rollback();

		/// <summary>
		/// Creates a temporary object hash.
		/// </summary>
		/// <returns>Object Hash</returns>
		ObjectHash UpdateHash();

		/// <summary>
		/// Creates the second step hash.
		/// </summary>
		/// <returns></returns>
		ObjectHash SecondUpdateHash ();

		/// <summary>
		/// Opens a transaction, stores the data and closes the transaction.
		/// </summary>
		/// <param name="updateHash">Object cache that holds the objects which shall be stored in database.</param>
		/// <param name="secondUpdateHash">The second step hash.</param>
		void AutoTransaction(ObjectHash updateHash, ObjectHash secondUpdateHash);

		/// <summary>
		/// Merges the update Hash with the primary Hash
		/// </summary>
		/// <param name="tempHash">The temp hash.</param>
		void MergeHash(ObjectHash tempHash);

		/// <summary>
		/// Adds a new object to the rollbacklist in order to set the property isNew to false, if a rollback takes place.
		/// </summary>
		/// <param name="vo"></param>
		void AddToRollbackList (IValueObject vo);

		/// <summary>
		/// Gets or sets the name of the application.
		/// </summary>
		/// <value>The name of the application.</value>
		string ApplicationName { get; set; }
	}
}