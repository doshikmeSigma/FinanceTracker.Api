using FinanceTracker.Api.Data;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Tests.Helpers
{
    public static class TestDataFactory
    {
        public static async Task<User> CreateUser(AppDbContext context, string email, string passwordHash, string passwordSalt)
        {
            var user = new User { Email = email, PasswordHash = passwordHash, PasswordSalt = passwordSalt };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public static async Task<Category> CreateCategory(AppDbContext context, string name, string type, int userId)
        {
            var category = new Category { Name = name, Type = type, UserId = userId };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }

        public static async Task<Transaction> CreateTransaction(AppDbContext context, int amount, DateTime date, string note, int userId, int categoryId)
        {
            var transaction = new Transaction { Amount = amount, Date = date, Note = note, UserId = userId, CategoryId = categoryId };
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();
            return transaction;
        }
    }
}
