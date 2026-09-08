using FinanceTracker.Api.Data;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Services;
using FinanceTracker.Tests.Helpers;

namespace FinanceTracker.Tests
{
    public class TransactionServiceTests
    {
        private async Task<(User user1, User user2)> CreateTwoUsers(AppDbContext context)
        {
            var user1 = new User { Email = "user1@test.com", PasswordHash = "h1", PasswordSalt = "s1" };
            var user2 = new User { Email = "user2@test.com", PasswordHash = "h2", PasswordSalt = "s2" };
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();
            return (user1, user2);
        }

        private async Task<Category> CreateCategory(AppDbContext context, string name, string type, int userId)
        {
            var category = new Category { Name = name, Type = type, UserId = userId };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyUserTransactions()
        {
            var context = TestDbContextFactory.Create();
            var service = new TransactionService(context);

            var (user1, user2) = await CreateTwoUsers(context);
            var cat1 = await CreateCategory(context, "Магазин", "Расход", user1.Id);
            var cat2 = await CreateCategory(context, "Работа", "Доход", user2.Id);

            context.Transactions.AddRange
            (
                new Transaction { Amount = 10000, Date = DateTime.UtcNow, Note = "эх, магаз", UserId = user1.Id, CategoryId = cat1.Id },
                new Transaction { Amount = 90000, Date = DateTime.UtcNow, Note = "ура, зпшка", UserId = user2.Id, CategoryId = cat2.Id }
            );
            await context.SaveChangesAsync();

            var transactions1 = await service.GetAllAsync(user1.Id);
            var transactions2 = await service.GetAllAsync(user2.Id);

            Assert.Single(transactions1);
            Assert.Equal("эх, магаз", transactions1[0].Note);

            Assert.Single(transactions2);
            Assert.Equal("ура, зпшка", transactions2[0].Note);
        }
    }
}
