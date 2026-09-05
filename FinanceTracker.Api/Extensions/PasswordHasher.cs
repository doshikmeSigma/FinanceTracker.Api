using System.Security.Cryptography;

namespace FinanceTracker.Api.Extensions
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int NumberOfIterations = 100000;
        private const int OutputLength = 32;

        public static (string hash, string salt) Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, NumberOfIterations, HashAlgorithmName.SHA256, OutputLength);
            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public static bool Verify(string password, string hash, string salt)
        {
            var storedHash = Convert.FromBase64String(hash);
            var storedSalt = Convert.FromBase64String(salt);
            var computedHash = Rfc2898DeriveBytes.Pbkdf2(password, storedSalt, NumberOfIterations, HashAlgorithmName.SHA256, OutputLength);

            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
    }
}
