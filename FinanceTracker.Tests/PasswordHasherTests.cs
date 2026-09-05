using FinanceTracker.Api.Extensions;

namespace FinanceTracker.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            var password = "TestPassword123!";
            var (hash, salt) = PasswordHasher.Hash(password);

            var result = PasswordHasher.Verify(password, hash, salt);

            Assert.True(result);
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            var password = "TestPassword123!";
            var wrongPassword = "WrongPassword456!";
            var (hash, salt) = PasswordHasher.Hash(password);

            var result = PasswordHasher.Verify(wrongPassword, hash, salt);

            Assert.False(result);
        }

        [Fact]
        public void Hash_SamePasswordTwice_ProducesDifferentHashes()
        {
            var password = "TestPassword123!";

            var (hash1, salt1) = PasswordHasher.Hash(password);
            var (hash2, salt2) = PasswordHasher.Hash(password);

            Assert.NotEqual(salt1, salt2);
            Assert.NotEqual(hash1, hash2);
        }
    }
}
