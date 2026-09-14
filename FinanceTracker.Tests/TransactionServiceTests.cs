using FinanceTracker.Api.Data;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Services;
using FinanceTracker.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using static FinanceTracker.Tests.Helpers.TestDataFactory;

namespace FinanceTracker.Tests
{
    public class TransactionServiceTests
    {
        private async Task<(AppDbContext context, TransactionService service, User user1, User user2, Category cat1, Category cat2, Transaction tra1, Transaction tra2)> Arrange(bool flag = false)
        {
            var context = TestDbContextFactory.Create();
            var service = new TransactionService(context);
            var user1 = await CreateUser(context, "qwerty123@gmail.com", "1", "1");
            var user2 = await CreateUser(context, "testme@gmail.com", "2", "2");
            var cat1 = await CreateCategory(context, "Магазин", "Расход", user1.Id);
            var cat2 = await CreateCategory(context, "Работа", "Доход", user2.Id);
            var tra1 = await CreateTransaction(context, 10000, DateTime.UtcNow, "эх, магаз", user1.Id, cat1.Id);
            var tra2 = await CreateTransaction(context, 90000, DateTime.UtcNow, "ура, зпшка", user2.Id, cat2.Id);

            if (flag)
            {
                var cat3 = await CreateCategory(context, "Хобби", "Расход", user1.Id);
                var cat4 = await CreateCategory(context, "Аптека", "Расход", user1.Id);
                var cat5 = await CreateCategory(context, "Коммунальные услуги", "Расход", user1.Id);
                var cat6 = await CreateCategory(context, "Одежда", "Расход", user1.Id);
                var cat7 = await CreateCategory(context, "Подарки", "Расход", user1.Id);
                await CreateTransaction(context, 3200, DateTime.UtcNow, "абонемент в бассик", user1.Id, cat3.Id);
                await CreateTransaction(context, 2600, DateTime.UtcNow, "дорогие лекарства", user1.Id, cat4.Id);
                await CreateTransaction(context, 1700, DateTime.UtcNow, "подняли коммуналку", user1.Id, cat5.Id);
                await CreateTransaction(context, 5600, DateTime.UtcNow, "закупился в гуме", user1.Id, cat6.Id);
                await CreateTransaction(context, 2300, DateTime.UtcNow, "девушке на цветы", user1.Id, cat7.Id);
            }

            return (context, service, user1, user2, cat1, cat2, tra1, tra2);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyUserTransactions()
        {
            var (context, service, user1, user2, cat1, cat2, tra1, tra2) = await Arrange();

            var paginationResponse1 = await service.GetAllAsync(user1.Id, 1, 100);
            var paginationResponse2 = await service.GetAllAsync(user2.Id, 1, 100);

            Assert.Single(paginationResponse1.Items);
            Assert.Equal("эх, магаз", paginationResponse1.Items[0].Note);
            Assert.Single(paginationResponse2.Items);
            Assert.Equal("ура, зпшка", paginationResponse2.Items[0].Note);
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

            var paginationResponse1 = await service.GetAllAsync(user1.Id, 1, 100, 3);
            var paginationResponse2 = await service.GetAllAsync(user2.Id, 1, 100, 2);

            Assert.Equal((3000, "дорого"), (paginationResponse1.Items[0].Amount, paginationResponse1.Items[0].Note));
            Assert.Equal((90000, "ура, зпшка"), (paginationResponse2.Items[0].Amount, paginationResponse2.Items[0].Note));
        }

        [Fact]
        public async Task CreateTransaction_NegativeAmount_HandlesGracefully()
        {
            var (_, service, user1, _, cat1, _, _, _) = await Arrange();

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(user1.Id, new CreateTransactionRequest { Amount = -232, CategoryId = cat1.Id, Date = DateTime.UtcNow, Note = "сумма меньше 0, нельзя" }));
        }

        [Fact]
        public async Task CreateTransaction_NullNote_HandlesGracefully()
        {
            var (_, service, user1, _, cat1, _, _, _) = await Arrange();

            var transaction = await service.CreateAsync(user1.Id, new CreateTransactionRequest { Amount = 24000, CategoryId = cat1.Id, Date = DateTime.UtcNow, Note = "" });

            Assert.Equal("", transaction!.Note);
        }

        [Fact]
        public async Task CreateTransaction_FutureDate_HandlesGracefully()
        {
            var (_, service, user1, _, cat1, _, _, _) = await Arrange();

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(user1.Id, new CreateTransactionRequest { Amount = 27000, CategoryId = cat1.Id, Date = DateTime.UtcNow.AddDays(3), Note = "будущая дата, нельзя" }));
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            var (_, service, user1, _, cat1, _, _, _) = await Arrange();

            var transaction = await service.GetByIdAsync(user1.Id, 9999999);

            Assert.Null(transaction);
        }

        [Fact]
        public async Task UpdateAsync_NonExistentId_ReturnsFalse()
        {
            var (_, service, user1, _, cat1, _, _, _) = await Arrange();

            var transaction = await service.UpdateAsync(user1.Id, 9999999, new UpdateTransactionRequest { Amount = 999, CategoryId = cat1.Id, Date = DateTime.UtcNow, Note = "вернет false" });

            Assert.Equal(TransactionUpdateResult.NotFound, transaction);
        }

        [Fact]
        public async Task GetAllAsync_FirstPage_ReturnsCorrectItems()
        {
            var (context, service, user1, _, cat1, _, _, _) = await Arrange(true);

            var paginationResponse = await service.GetAllAsync(1, 1, 2);

            Assert.Equal(2, paginationResponse.Items.Count);
            Assert.Equal(6, paginationResponse.TotalCount);
            Assert.Equal(3, paginationResponse.TotalPages);
            Assert.False(paginationResponse.HasPreviousPage);
            Assert.True(paginationResponse.HasNextPage);
        }

        [Fact]
        public async Task GetAllAsync_LastPage_HasNoNextPage()
        {
            var (context, service, user1, _, cat1, _, _, _) = await Arrange(true);

            var paginationResponse = await service.GetAllAsync(1, 3, 2);

            Assert.Equal(2, paginationResponse.Items.Count);
            Assert.Equal(6, paginationResponse.TotalCount);
            Assert.Equal(3, paginationResponse.TotalPages);
            Assert.True(paginationResponse.HasPreviousPage);
            Assert.False(paginationResponse.HasNextPage);
        }

        [Fact]
        public async Task GetAllAsync_MiddlePage_HasBothButtons()
        {
            var (context, service, user1, _, cat1, _, _, _) = await Arrange(true);

            var paginationResponse = await service.GetAllAsync(1, 2, 2);

            Assert.Equal(2, paginationResponse.Items.Count);
            Assert.Equal(6, paginationResponse.TotalCount);
            Assert.Equal(3, paginationResponse.TotalPages);
            Assert.True(paginationResponse.HasPreviousPage);
            Assert.True(paginationResponse.HasNextPage);
        }

        [Fact]
        public async Task GetAllAsync_InvalidPage_ThrowsException()
        {
            var (context, service, user1, _, cat1, _, _, _) = await Arrange();

            await Assert.ThrowsAsync<ArgumentException>(() => service.GetAllAsync(1, 0, 1));
        }

        [Fact]
        public async Task GetAllAsync_InvalidPageSize_ThrowsException()
        {
            var (context, service, user1, _, cat1, _, _, _) = await Arrange();

            await Assert.ThrowsAsync<ArgumentException>(() => service.GetAllAsync(1, 1, 0));
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetAllAsync(1, 1, 101));
        }

        [Fact]
        public async Task GetAllAsync_WithCategoryIdAndPagination_FiltersCorrectly()
        {
            var (context, service, user1, _, cat1, _, _, _) = await Arrange();
            var cat3 = await CreateCategory(context, "Одежда", "Расходы", user1.Id);
            await CreateTransaction(context, 3200, DateTime.UtcNow.AddSeconds(1), "авокадо", user1.Id, cat1.Id);
            await CreateTransaction(context, 3200, DateTime.UtcNow.AddSeconds(2), "чудо", user1.Id, cat1.Id);
            await CreateTransaction(context, 3200, DateTime.UtcNow.AddSeconds(3), "мокко", user1.Id, cat1.Id);
            await CreateTransaction(context, 3200, DateTime.UtcNow.AddSeconds(4), "шаурма", user1.Id, cat1.Id);
            await CreateTransaction(context, 3200, DateTime.UtcNow.AddSeconds(5), "протеин", user1.Id, cat1.Id);
            await CreateTransaction(context, 3200, DateTime.UtcNow.AddSeconds(6), "вансы", user1.Id, cat3.Id);
            await CreateTransaction(context, 2600, DateTime.UtcNow.AddSeconds(7), "страдивариус", user1.Id, cat3.Id);
            await CreateTransaction(context, 1700, DateTime.UtcNow.AddSeconds(8), "бершка", user1.Id, cat3.Id);

            var paginationResponse = await service.GetAllAsync(user1.Id, 1, 2, 1);

            Assert.Equal(2, paginationResponse.Items.Count);
            Assert.Equal(6, paginationResponse.TotalCount);
            Assert.Equal(3, paginationResponse.TotalPages);
            Assert.False(paginationResponse.HasPreviousPage);
            Assert.True(paginationResponse.HasNextPage);
            Assert.Equal("протеин", paginationResponse.Items[0].Note);
            Assert.Equal("шаурма", paginationResponse.Items[1].Note);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsItemsSortedByDateDescending()
        {
            var (context, service, user1, _, cat1, _, tra1, _) = await Arrange();
            var tra3 = await CreateTransaction(context, 2000, DateTime.UtcNow.AddDays(-1), "бананы", user1.Id, cat1.Id);
            var tra4 = await CreateTransaction(context, 4000, DateTime.UtcNow.AddDays(-2), "сахар", user1.Id, cat1.Id);

            var paginationResponse = await service.GetAllAsync(user1.Id, 1, 10);

            Assert.True(paginationResponse.Items[0].Date > paginationResponse.Items[1].Date);
            Assert.True(paginationResponse.Items[1].Date > paginationResponse.Items[2].Date);
        }
    }
}
