using FinanceTracker.Api.Data;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Services;
using FinanceTracker.Tests.Helpers;
using static FinanceTracker.Tests.Helpers.TestDataFactory;

namespace FinanceTracker.Tests
{
    public class CategoryServiceTests
    {
        private async Task<(AppDbContext context, CategoryService service, User user1, User user2, Category cat1, Category cat2)> Arrange()
        {
            var context = TestDbContextFactory.Create();
            var service = new CategoryService(context);
            var user1 = await CreateUser(context, "qwerty123@gmail.com", "1", "1");
            var user2 = await CreateUser(context, "testme@gmail.com", "2", "2");
            var cat1 = await CreateCategory(context, "Магазин", "Расход", user1.Id);
            var cat2 = await CreateCategory(context, "Работа", "Доход", user2.Id);
            return (context, service, user1, user2, cat1, cat2);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyUserCategories()
        {
            var (_, service, user1, user2, _, _) = await Arrange();

            var categoriesFirst = await service.GetAllAsync(user1.Id);
            var categoriesSecond = await service.GetAllAsync(user2.Id);

            Assert.Single(categoriesFirst);
            Assert.Equal(user1.Id, categoriesFirst[0].Id);
            Assert.Single(categoriesSecond);
            Assert.Equal(user2.Id, categoriesSecond[0].Id);
        }

        [Fact]
        public async Task GetByIdAsync_OwnCategory_ReturnsCategory()
        {
            var (_, service, user1, user2, _, _) = await Arrange();

            var categoryFirst = await service.GetByIdAsync(user1.Id, 1);
            var categorySecond = await service.GetByIdAsync(user2.Id, 2);

            Assert.Equal(user1.Id, categoryFirst!.UserId);
            Assert.Equal(user2.Id, categorySecond!.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_OtherUserCategory_ReturnsNull()
        {
            var (_, service, user1, user2, _, _) = await Arrange();

            var categoryFirst = await service.GetByIdAsync(user1.Id, 2);
            var categorySecond = await service.GetByIdAsync(user2.Id, 1);

            Assert.Null(categoryFirst);
            Assert.Null(categorySecond);
        }

        [Fact]
        public async Task UpdateAsync_OtherUserCategory_ReturnsFalse()
        {
            var (_, service, user1, _, _, _) = await Arrange();

            var isUpdated = await service.UpdateAsync(user1.Id, 2, new UpdateCategoryRequest { Name = "Чужая категория", Type = "Расход" });

            Assert.False(isUpdated);
        }

        [Fact]
        public async Task DeleteAsync_OtherUserCategory_ReturnsNotFound()
        {
            var (_, service, user1, _, _, _) = await Arrange();

            var deleteResult = await service.DeleteAsync(user1.Id, 2);

            Assert.Equal(CategoryDeleteResult.NotFound, deleteResult);
        }

        [Fact]
        public async Task CreateAsync_SetsUserId()
        {
            var (_, service, user1, _, _, _) = await Arrange();

            var category = await service.CreateAsync(user1.Id, new CreateCategoryRequest { Name = "Отдых", Type = "Расход" });

            Assert.Equal(user1.Id, category.UserId);
        }

        [Fact]
        public async Task UpdateAsync_OwnCategory_UpdatesSuccessfully()
        {
            var (_, service, user1, _, _, _) = await Arrange();

            var isUpdated = await service.UpdateAsync(user1.Id, 1, new UpdateCategoryRequest { Name = "Курорт", Type = "Расход" });

            Assert.True(isUpdated);
        }

        [Fact]
        public async Task DeleteAsync_OwnCategory_DeletesSuccessfully()
        {
            var (_, service, user1, _, _, _) = await Arrange();

            var deleteResult = await service.DeleteAsync(user1.Id, 1);

            Assert.Equal(CategoryDeleteResult.Deleted, deleteResult);
        }

        [Fact]
        public async Task DeleteAsync_CategoryWithTransactions_ReturnsHasTransactions()
        {
            var (context, service, user1, _, cat1, _) = await Arrange();
            await CreateTransaction(context, 10000, DateTime.UtcNow, "эх, магаз", user1.Id, cat1.Id);

            var deleteResult = await service.DeleteAsync(user1.Id, 1);

            Assert.Equal(CategoryDeleteResult.HasTransactions, deleteResult);
        }

        [Fact]
        public async Task CreateCategory_EmptyName_HandlesGracefully()
        {
            var (_, service, user1, _, _, _) = await Arrange();

            var category = await service.CreateAsync(user1.Id, new CreateCategoryRequest { Name = "", Type = "Расход" });

            Assert.Equal("", category.Name);
        }
    }
}
