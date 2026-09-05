using FinanceTracker.Api.Data;
using FinanceTracker.Api.Extensions;
using Microsoft.EntityFrameworkCore;
using static FinanceTracker.Api.DTOs.ReportDtos;

namespace FinanceTracker.Api.Services
{
    public class ReportService(AppDbContext _context) : IReportService
    {
        public async Task<List<CategorySummaryResponse>> GetSummaryAsync(int userId, DateTime? from, DateTime? to)
        {
            var summary = await _context.Transactions
                .ProcessByDate(userId, from, to)
                .GroupBy(t => new { t.CategoryId, t.Category.Name, t.Category.Type })
                .Select(g => new CategorySummaryResponse
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.Name,
                    CategoryType = g.Key.Type,
                    TransactionsCount = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount)
                })
                .OrderByDescending(s => s.TotalAmount)
                .ToListAsync();

            return summary;
        }

        public async Task<CategoryTotalResponse> GetTotalAsync(int userId, DateTime? from, DateTime? to)
        {
            var income = await _context.Transactions
                .ProcessByDate(userId, from, to)
                .Where(t => t.Category.Type == "Доход")
                .SumAsync(t => t.Amount);

            var expense = await _context.Transactions
                .ProcessByDate(userId, from, to)
                .Where(t => t.Category.Type == "Расход")
                .SumAsync(t => t.Amount);

            return new CategoryTotalResponse
            {
                Income = income,
                Expense = expense
            };
        }
    }
}
