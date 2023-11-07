using System.Security.Cryptography;
using System.Text;

namespace RESTAPI_DynamoDB.Utilities
{
    public class PasswordHasher
    {
        /// <summary>
        /// Hashes a given string using the SHA512 hashing algorithm.
        /// </summary>
        /// <param name="password">String containing the password desired to Hash.</param>
        /// <returns>String containing the hashed password.</returns>
        public static string SHA512Hasher(string password)
        {
            using var hashAlgorithm = SHA512.Create();
            var byteValue = Encoding.UTF8.GetBytes(password);
            var byteHash = hashAlgorithm.ComputeHash(byteValue);
            return Convert.ToBase64String(byteHash);
        }
    }
}
