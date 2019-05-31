namespace AdFactum.Data
{
	/// <summary>
	/// This enumerations defines how the AdFactum Object mapper handles transactions.
	/// <br></br>
	/// A transactions is a summary of database actions that are executed in a reliable way, 
	/// independent of other transactions. Therefore it's used for making database changes. 
	/// Every transaction must be confirmed with a commit, or a rollback has to be done, in 
	/// order to get the old database state, if a failure occured.
	/// </summary>
	public enum Transactions
	{
		/// <summary>
		/// Automatic commits when inserting or updating values.
		/// <br></br>
		/// If an automatic transaction handling is wanted, the database mapper must be 
		/// created with the transaction setting "Automatic". It's not recommended to 
		/// use the Automatic transaction setting. Please keep in mind that a logical transaction 
		/// most time is a summary of many single database actions. Because of this it's 
		/// not advisable to make a commit after every single database command.
		/// </summary>
		Automatic,

		/// <summary>
		/// Commit data manual when insertingm, updating or deleting values.
		/// <br></br>
		/// If a manual transaction handling is preffered, the developer 
		/// has to invoke a transaction before storing data to database.
		/// Also the developer needs to implement an exception handling. 
		/// In case of an failure, the rollback method of the object mapper has to be invoked.
		/// </summary>
		Manual
	}
}