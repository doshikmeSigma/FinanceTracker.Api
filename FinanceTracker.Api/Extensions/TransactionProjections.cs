using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Extensions
{
    public static class TransactionProjections
    {
        public static IQueryable<TransactionResponse> ToResponse(this IQueryable<Transaction> query)
        {
            return query.Select(t => new TransactionResponse
            {
                Id = t.Id,
                Amount = t.Amount,
                Date = t.Date,
                Note = t.Note,
                Category = new CategoryResponse
                {
                    Id = t.Category.Id,
                    Name = t.Category.Name,
                    Type = t.Category.Type
                }
            });
        }

        public static IQueryable<Transaction> ProcessByDate(this IQueryable<Transaction> query, int userId, DateTime? from, DateTime? to)
        {
            if (from != null) query = query.Where(t => t.UserId == userId && t.Date >= from);
            if (to != null) query = query.Where(t => t.UserId == userId && t.Date < to.Value.AddDays(1));

            return query;
        }
    }
}
