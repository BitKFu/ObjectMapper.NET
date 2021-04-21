using AdFactum.Data;
using AdFactum.Data.Queries;
using BusinessEntities;
using NUnit.Framework;
using Utils;

namespace Tutorial02
{
	/// <summary>
	/// Try to create a user
	/// </summary>
	[TestFixture]
	public class UserTest : BaseTest
	{
		/// <summary>
		/// Creates the user.
		/// </summary>
		[Test]
		public void CreateUser()
		{
			using (ObjectMapper mapper = base.CreateMapper(base.GetAccessPersister()))
			{
				/*
				 * Open a transaction
				 */
				mapper.BeginTransaction();

				/*
				 * Store the data
				 */
				User HansDampf = new User();
				HansDampf.Name = "Hans Dampf";
				HansDampf.Logon = "Dampf";
				HansDampf.Md5PasswordKey = Md5CryptHelper.ComputeHash("turboDiesel");
				StoreUser (mapper, HansDampf);

				User DanielThunderbird = new User();
				DanielThunderbird.Name = "Daniel Thunderbird";
				DanielThunderbird.Logon = "Thunder";
				DanielThunderbird.Md5PasswordKey = Md5CryptHelper.ComputeHash("pokemon");
				StoreUser (mapper, DanielThunderbird);

				/*
				 * Commit the transaction
				 */
				mapper.Commit();
			}
		}


		/// <summary>
		/// Creates the user.
		/// </summary>
		/// <param name="mapper">The mapper.</param>
		/// <param name="user">The user.</param>
		/// <returns>True, if the user could be stored</returns>
		private bool StoreUser (ObjectMapper mapper, User user)
		{
			/*
			 * If the user is new, check if the logon name does already exists
			 * If the logon name does exist, return false
			 */
			if (user.IsNew)
			{
				ICondition logonCondition = new AndCondition(
					typeof(User),	
					"Logon", 
					QueryOperator.Like_NoCaseSensitive, 
					user.Logon);
				
				int count = mapper.Count(typeof(User), logonCondition);
				if (count > 0)
					return false;
			}

			/*
			 * if the logon name is not used or it is an existing user, than store the user
			 */
			mapper.Save(user);

			return true;
		}
	}
}