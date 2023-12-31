using System;
using System.IO;
using System.Security.Cryptography;

namespace Utils
{
	/// <summary>
	/// Used to create a hash code which can be stored in database
	/// </summary>
	public class Md5CryptHelper
	{
		/// <summary>
		/// Computes the md5 hash.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string ComputeHash(string value)
		{
			/*
			 * Create the md5 crypt service provider
			 */
			MD5 crypt = new MD5CryptoServiceProvider();
			StreamWriter pwd = new StreamWriter(new MemoryStream());
			pwd.Write(value);
			pwd.Flush();

			/*
			 * Compute the hash code
			 */
			pwd.BaseStream.Seek(0, SeekOrigin.Begin);
			byte[] cryptHash = crypt.ComputeHash(pwd.BaseStream);

			/*
			 * Convert the result to hex 
			 */
			string result = "";

			int nLen = cryptHash.Length;
			for (int nPos = 0; nPos < nLen; nPos++)
			{
				byte cBuff = cryptHash[nPos];
				result += Convert((long) cBuff, 16);
			}

			return result;
		}

		/// <summary>
		/// Checks the value against hash.
		/// </summary>
		/// <param name="md5Hash">The MD5 hash.</param>
		/// <param name="inputValue">The input value.</param>
		/// <returns></returns>
		public static bool CheckValueAgainstHash(string md5Hash, string inputValue)
		{
			return ComputeHash(inputValue).Equals(md5Hash);
		}

		/// <summary>
		/// Converts a number into different bases
		/// </summary>
		/// <param name="dblCount">Value to convert</param>
		/// <param name="intBaseformat">Numberbase e.x. 16 as Hex</param>
		/// <returns>converted number as a string</returns>
		private static string Convert(long dblCount, int intBaseformat)
		{
			string result = "";
			int potenz = 1;
			long temp;
			long pow;

			while (dblCount/(long) Math.Pow(intBaseformat, potenz) >= intBaseformat) potenz++;

			while (potenz >= 0)
			{
				if (potenz == 0)
				{
					temp = dblCount;
					dblCount = 0;
				}
				else
				{
					pow = (long) Math.Pow(intBaseformat, potenz);
					temp = dblCount/pow;
					dblCount = dblCount - (temp*pow);
				}

				if (temp < 10)
					result += temp.ToString();
				else
					result += (char) (long) ('A' + (temp - 10));

				potenz--;
			}

			if (result == "") result = "0";
			return result;
		}
	}
}