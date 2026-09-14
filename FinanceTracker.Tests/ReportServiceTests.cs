using FinanceTracker.Api.Data;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Services;
using FinanceTracker.Tests.Helpers;
using static FinanceTracker.Tests.Helpers.TestDataFactory;

namespace FinanceTracker.Tests
{
    public class ReportServiceTests
    {
        private async Task<(AppDbContext context, ReportService service, User user1, User user2, Category cat1, Category cat2, Transaction tra1, Transaction tra2)> Arrange()
        {
            var context = TestDbContextFactory.Create();
            var service = new ReportService(context);
            var user1 = await CreateUser(context, "qwerty123@gmail.com", "1", "1");
            var user2 = await CreateUser(context, "testme@gmail.com", "2", "2");
            var cat1 = await CreateCategory(context, "Магазин", "Расход", user1.Id);
            var cat2 = await CreateCategory(context, "Работа", "Доход", user2.Id);
            var tra1 = await CreateTransaction(context, 300, DateTime.UtcNow.AddDays(1), "эх, магаз", user1.Id, cat1.Id);
            var tra2 = await CreateTransaction(context, 5000, DateTime.UtcNow.AddDays(2), "ура, зпшка", user2.Id, cat2.Id);

            return (context, service, user1, user2, cat1, cat2, tra1, tra2);
        }

        [Fact]
        public async Task GetSummaryAsync_ReturnsOnlyUserTransactions()
        {
            var (_, service, _, user2, _, _, _, _) = await Arrange();

            var transactions = await service.GetSummaryAsync(user2.Id, null, null);

            Assert.Single(transactions);
            Assert.Equal("Работа", transactions[0].CategoryName);
        }

        [Fact]
        public async Task GetSummaryAsync_WithDateFilters_FiltersCorrectly()
        {
            var (_, service, _, user2, _, _, _, _) = await Arrange();

            var transactions = await service.GetSummaryAsync(user2.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(3));

            Assert.Single(transactions);
            Assert.Equal("Работа", transactions[0].CategoryName);
        }

        [Fact]
        public async Task GetTotalAsync_ReturnsOnlyUserTransactions()
        {
            var (context, service, user1, user2, _, _, _, _) = await Arrange();
            var cat3 = await CreateCategory(context, "Инвестиции", "Доход", user1.Id);
            await CreateTransaction(context, 1000, DateTime.UtcNow.AddDays(1), "брокер, спс", user1.Id, cat3.Id);

            var total = await service.GetTotalAsync(user1.Id, null, null);

            Assert.Equal(1000, total.Income);
            Assert.Equal(300, total.Expense);
        }

        [Fact]
        public async Task GetTotalAsync_WithDateFilters_FiltersCorrectly()
        {
            var (context, service, user1, user2, _, _, _, _) = await Arrange();
            var cat3 = await CreateCategory(context, "Инвестиции", "Доход", user1.Id);
            await CreateTransaction(context, 1000, DateTime.UtcNow.AddDays(1), "брокер, спс", user1.Id, cat3.Id);

            var total = await service.GetTotalAsync(user1.Id, DateTime.UtcNow, DateTime.UtcNow.AddDays(3));

            Assert.Equal(1000, total.Income);
            Assert.Equal(300, total.Expense);
        }
    }
}
