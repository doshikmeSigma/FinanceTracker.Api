using FinanceTracker.Api.Data;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Services;
using FinanceTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Tests
{
    public class TransactionServiceTests
    {
        private async Task<User> CreateUser(AppDbContext context, string email, string passwordHash, string passwordSalt)
        {
            var user = new User { Email = email, PasswordHash = passwordHash, PasswordSalt = passwordSalt };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        private async Task<Category> CreateCategory(AppDbContext context, string name, string type, int userId)
        {
            var category = new Category { Name = name, Type = type, UserId = userId };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }

        private async Task<Transaction> CreateTransaction(AppDbContext context, int amount, DateTime date, string note, int userId, int categoryId)
        {
            var transaction = new Transaction { Amount = amount, Date = date, Note = note, UserId = userId, CategoryId = categoryId };
            context.Transactions.Add(transaction);
            await context.SaveChangesAsync();
            return transaction;
        }

        private async Task<(AppDbContext context, TransactionService service, User user1, User user2, Category cat1, Category cat2, Transaction tra1, Transaction tra2)> Arrange()
        {
            var context = TestDbContextFactory.Create();
            var service = new TransactionService(context);
            var user1 = await CreateUser(context, "qwerty123@gmail.com", "1", "1");
            var user2 = await CreateUser(context, "testme@gmail.com", "2", "2");
            var cat1 = await CreateCategory(context, "Магазин", "Расход", user1.Id);
            var cat2 = await CreateCategory(context, "Работа", "Доход", user2.Id);
            var tra1 = await CreateTransaction(context, 10000, DateTime.UtcNow, "эх, магаз", user1.Id, cat1.Id);
            var tra2 = await CreateTransaction(context, 90000, DateTime.UtcNow, "ура, зпшка", user2.Id, cat2.Id);
            return (context, service, user1, user2, cat1, cat2, tra1, tra2);
        }


        [Fact]
        public async Task GetAllAsync_ReturnsOnlyUserTransactions()
        {
            var (context, service, user1, user2, cat1, cat2, tra1, tra2) = await Arrange();

            var transactions1 = await service.GetAllAsync(user1.Id);
            var transactions2 = await service.GetAllAsync(user2.Id);

            Assert.Single(transactions1);
            Assert.Equal("эх, магаз", transactions1[0].Note);
            Assert.Single(transactions2);
            Assert.Equal("ура, зпшка", transactions2[0].Note);
        }

        [Fact]
        public async Task GetByIdAsync_OwnTransaction_ReturnsTransation()
        {
            var (context, service, user1, user2, cat1, cat2, tra1, tra2) = await Arrange();

            var transaction1 = await service.GetByIdAsync(user1.Id, 1);
            var transaction2 = await service.GetByIdAsync(user2.Id, 2);

            Assert.Equal("эх, магаз", transaction1?.Note);
            Assert.Equal("ура, зпшка", transaction2?.Note);
        }

        [Fact]
        public async Task GetByIdAsync_OtherUserTransaction_ReturnsNull()
        {
            var (context, service, user1, user2, cat1, cat2, tra1, tra2) = await Arrange();

            var transaction1 = await service.GetByIdAsync(user1.Id, 2);
            var transaction2 = await service.GetByIdAsync(user2.Id, 1);

            Assert.Null(transaction1);
            Assert.Null(transaction2);
        }

        [Fact]
        public async Task UpdateAsync_OtherUserTransaction_ReturnsNotFound()
        {
            var (context, service, user1, user2, cat1, cat2, tra1, tra2) = await Arrange();

            var transaction1 = await service.UpdateAsync(user1.Id, 2, new UpdateTransactionRequest { Amount = 12345, Date = DateTime.UtcNow, Note = "чужая транзакция", CategoryId = 1 });
            var transaction2 = await service.UpdateAsync(user2.Id, 1, new UpdateTransactionRequest { Amount = 4844, Date = DateTime.UtcNow, Note = "транзакция чужая", CategoryId = 1 });
            var transactions = await context.Transactions.ToListAsync();

            Assert.Equal(TransactionUpdateResult.NotFound, transaction1);
            Assert.Equal("эх, магаз", transactions[0].Note);
            Assert.Equal(TransactionUpdateResult.NotFound, transaction2);
            Assert.Equal("ура, зпшка", transactions[1].Note);
        }

        [Fact]
        public async Task DeleteAsync_OtherUserTransaction_ReturnsFalse()
        {
            var (context, service, user1, user2, cat1, cat2, tra1, tra2) = await Arrange();

            var transaction1 = await service.DeleteAsync(user1.Id, 2);
            var transaction2 = await service.DeleteAsync(user2.Id, 1);
            var transactions = await context.Transactions.ToListAsync();

            Assert.False(transaction1);
            Assert.Equal("эх, магаз", transactions[0].Note);
            Assert.False(transaction2);
            Assert.Equal("ура, зпшка", transactions[1].Note);
        }

        [Fact]
        public async Task CreateAsync_OwnCategory_CreatesTransaction()
        {
            var (context, service, user1, user2, cat1, cat2, _, _) = await Arrange();

            var tra1 = await service.CreateAsync(user1.Id, new CreateTransactionRequest { Amount = 10000, Date = DateTime.UtcNow, Note = "эх, магаз", CategoryId = cat1.Id });
            var tra2 = await service.CreateAsync(user2.Id, new CreateTransactionRequest { Amount = 90000, Date = DateTime.UtcNow, Note = "ура, зпшка", CategoryId = cat2.Id });
            var transactions = await context.Transactions.ToListAsync();

            Assert.Equal(user1.Id, transactions[2].UserId);
            Assert.Equal(user2.Id, transactions[3].UserId);
        }

        [Fact]
        public async Task CreateAsync_OtherUserCategory_ReturnsNull()
        {
            var (context, service, user1, user2, cat1, cat2, _, _) = await Arrange();

            var tra1 = await service.CreateAsync(user1.Id, new CreateTransactionRequest { Amount = 10000, Date = DateTime.UtcNow, Note = "эх, магаз", CategoryId = cat2.Id });
            var tra2 = await service.CreateAsync(user2.Id, new CreateTransactionRequest { Amount = 90000, Date = DateTime.UtcNow, Note = "ура, зпшка", CategoryId = cat1.Id });

            Assert.Null(tra1);
            Assert.Null(tra2);
        }

        [Fact]
        public async Task UpdateAsync_OwnTransactionWithOtherUserCategory_ReturnsCategoryNotFound()
        {
            var (context, service, user1, user2, cat1, cat2, tra1, tra2) = await Arrange();

            var transaction1 = await service.UpdateAsync(user1.Id, 1, new UpdateTransactionRequest { Amount = 666, CategoryId = 2, Date = DateTime.UtcNow, Note = "чужая категория" });
            var transaction2 = await service.UpdateAsync(user2.Id, 2, new UpdateTransactionRequest { Amount = 666, CategoryId = 1, Date = DateTime.UtcNow, Note = "категория чужая" });

            Assert.Equal(TransactionUpdateResult.CategoryNotFound, transaction1);
            Assert.Equal(TransactionUpdateResult.CategoryNotFound, transaction2);
        }

        [Fact]
        public async Task UpdateAsync_ModifiesTransaction()
        {
            var (context, service, user1, user2, cat1, cat2, tra1, tra2) = await Arrange();
            var time = DateTime.UtcNow;

            var transaction1 = await service.UpdateAsync(user1.Id, 1, new UpdateTransactionRequest { Amount = 666, CategoryId = 1, Date = time, Note = "крутое" });
            var transaction2 = await service.UpdateAsync(user2.Id, 2, new UpdateTransactionRequest { Amount = 4847, CategoryId = 2, Date = time, Note = "прекрасное" });
            var transactions = await context.Transactions.ToListAsync();
            var firstTransaction = transactions[0];
            var secondTransaction = transactions[1];

            Assert.Equal(TransactionUpdateResult.Updated, transaction1);
            Assert.Equal((666, 1, time, "крутое"), (firstTransaction.Amount, firstTransaction.CategoryId, firstTransaction.Date, firstTransaction.Note));
            Assert.Equal(TransactionUpdateResult.Updated, transaction2);
            Assert.Equal((4847, 2, time, "прекрасное"), (secondTransaction.Amount, secondTransaction.CategoryId, secondTransaction.Date, secondTransaction.Note));
        }

        [Fact]
        public async Task DeleteAsync_OwnTransaction_DeletesSuccessfully()
        {
            var (context, service, user1, user2, cat1, cat2, tra1, tra2) = await Arrange();

            var firstTransaction = await service.DeleteAsync(1, 1);
            var secondTransaction = await service.DeleteAsync(2, 2);

            Assert.True(firstTransaction);
            Assert.True(secondTransaction);
            Assert.Empty(await context.Transactions.ToListAsync());
        }

        [Fact]
        public async Task GetAllAsync_WithCategoryId_FiltersCorrectly()
        {
            var (context, service, user1, user2, cat1, cat2, tra1, tra2) = await Arrange();
            var cat3 = await CreateCategory(context, "Аптека", "Расход", user1.Id);
            var tra3 = await CreateTransaction(context, 3000, DateTime.UtcNow, "дорого", user1.Id, cat3.Id);
            var cat4 = await CreateCategory(context, "Транспорт", "Расход", user2.Id);
            var tra4 = await CreateTransaction(context, 56400, DateTime.UtcNow, "очень дорого", user2.Id, cat4.Id);
            await context.SaveChangesAsync();

            var firstCategories = await service.GetAllAsync(user1.Id, 3);
            var secondCategories = await service.GetAllAsync(user2.Id, 2);

            Assert.Equal((3000, "дорого"), (firstCategories[0].Amount, firstCategories[0].Note));
            Assert.Equal((90000, "ура, зпшка"), (secondCategories[0].Amount, secondCategories[0].Note));
        }
    }
}
