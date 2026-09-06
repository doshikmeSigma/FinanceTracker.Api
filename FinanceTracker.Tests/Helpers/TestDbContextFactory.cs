using FinanceTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Tests.Helpers
{
    public static class TestDbContextFactory
    {
        public static AppDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }
    }
}
