using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Extensions;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Services;
using FinanceTracker.Tests.Helpers;

namespace FinanceTracker.Tests
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task RegisterAsync_NewEmail_CreatesUserWithDefaultCategories()
        {
            var context = TestDbContextFactory.Create();
            var tokenService = new FakeTokenService();
            var authService = new AuthService(context, tokenService);

            var result = await authService.RegisterAsync(new RegisterRequest { Email = "test@gmail.com", Password = "123456" });

            Assert.Equal(RegisterResult.Created, result);
            Assert.Single(context.Users);

            var user = context.Users.First();
            Assert.Equal("test@gmail.com", user.Email);
            Assert.NotEmpty(user.PasswordSalt);
            Assert.NotEmpty(user.PasswordHash);
            Assert.Contains(user.Categories, c => c.Name == "Работа");
            Assert.Contains(user.Categories, c => c.Name == "Развлечения");
            Assert.Contains(user.Categories, c => c.Name == "ЖКХ");
        }

        [Fact]
        public async Task RegisterAsync_ExistingEmail_ReturnsEmailTaken()
        {
            var context = TestDbContextFactory.Create();
            var tokenService = new FakeTokenService();
            var authService = new AuthService(context, tokenService);

            context.Users.Add(new User { Email = "test@gmail.com", PasswordHash = "1", PasswordSalt = "2" });
            await context.SaveChangesAsync();
            var result = await authService.RegisterAsync(new RegisterRequest { Email = "test@gmail.com", Password = "sub3sub5" });

            Assert.Equal(RegisterResult.EmailTaken, result);
            Assert.Single(context.Users);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
        {
            var context = TestDbContextFactory.Create();
            var tokenService = new FakeTokenService();
            var authService = new AuthService(context, tokenService);
            var password = "TestPassword123!";
            var (hash, salt) = PasswordHasher.Hash(password);
            context.Users.Add(new User
            {
                Email = "user@test.com",
                PasswordHash = hash,
                PasswordSalt = salt
            });
            await context.SaveChangesAsync();
            var request = new LoginRequest
            {
                Email = "user@test.com",
                Password = password
            };

            var result = await authService.LoginAsync(request);

            Assert.NotNull(result);
            Assert.Equal("fake-token-for-testing", result.Token);
            Assert.True(result.ExpiresAt > DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ReturnsNull()
        {
            var context = TestDbContextFactory.Create();
            var tokenService = new FakeTokenService();
            var authService = new AuthService(context, tokenService);
            var (hash, salt) = PasswordHasher.Hash("CorrectPassword123!");
            context.Users.Add(new User
            {
                Email = "user@test.com",
                PasswordHash = hash,
                PasswordSalt = salt
            });
            await context.SaveChangesAsync();
            var request = new LoginRequest
            {
                Email = "user@test.com",
                Password = "WrongPassword456!"
            };

            var result = await authService.LoginAsync(request);

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WrongEmail_ReturnsNull()
        {
            var context = TestDbContextFactory.Create();
            var tokenService = new FakeTokenService();
            var authService = new AuthService(context, tokenService);
            var request = new LoginRequest
            {
                Email = "nonexistent@test.com",
                Password = "TestPassword123!"
            };

            var result = await authService.LoginAsync(request);

            Assert.Null(result);
        }
    }
}