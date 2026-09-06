using FinanceTracker.Api.DTOs;
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

            await authService.RegisterAsync(new RegisterRequest { Email = "mog@gmail.com", Password = "youhavebeenmogged" });
            var result = await authService.LoginAsync(new LoginRequest { Email = "mog@gmail.com", Password = "youhavebeenmogged" });

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

            await authService.RegisterAsync(new RegisterRequest { Email = "mog@gmail.com", Password = "youhavebeenmogged" });
            var result = await authService.LoginAsync(new LoginRequest { Email = "mog@gmail.com", Password = "lie" });

            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WrongEmail_ReturnsNull()
        {
            var context = TestDbContextFactory.Create();
            var tokenService = new FakeTokenService();
            var authService = new AuthService(context, tokenService);

            await authService.RegisterAsync(new RegisterRequest { Email = "mog@gmail.com", Password = "youhavebeenmogged" });
            var result = await authService.LoginAsync(new LoginRequest { Email = "lie@gmail.com", Password = "youhavebeenmogged" });

            Assert.Null(result);
        }
    }
}