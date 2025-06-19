using System.Security.Cryptography;
using System.Text;

namespace HexagonalSkeleton.Infrastructure.Auth
{
    /// <summary>
    /// Utility class for password hashing operations.
    /// This is infrastructure concern, not domain logic.
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Computes a hash from password, salt and pepper using SHA256.
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <param name="salt">The salt to add</param>
        /// <param name="pepper">The pepper to add</param>
        /// <returns>Base64 encoded hash</returns>
        public static string? ComputeHash(string password, string salt, string pepper)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            
            if (string.IsNullOrWhiteSpace(salt))
                throw new ArgumentException("Salt cannot be null or empty", nameof(salt));
            
            if (pepper == null)
                throw new ArgumentNullException(nameof(pepper));

            using var sha256 = SHA256.Create();
            var passwordSaltPepper = $"{password}{salt}{pepper}";
            var byteValue = Encoding.UTF8.GetBytes(passwordSaltPepper);
            var byteHash = sha256.ComputeHash(byteValue);
            var hash = Convert.ToBase64String(byteHash);
            return hash;
        }

        /// <summary>
        /// Generates a cryptographically secure random salt.
        /// </summary>
        /// <returns>Base64 encoded salt</returns>
        public static string GenerateSalt()
        {
            using var rng = RandomNumberGenerator.Create();
            var byteSalt = new byte[16];
            rng.GetBytes(byteSalt);
            var salt = Convert.ToBase64String(byteSalt);
            return salt;
        }
    }
}
