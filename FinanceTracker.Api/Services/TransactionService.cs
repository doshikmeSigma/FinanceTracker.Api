using FinanceTracker.Api.Data;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Extensions;
using FinanceTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Services
{
    public class TransactionService(AppDbContext _context) : ITransactionService
    {
        public async Task<PaginationResponse<TransactionResponse>> GetAllAsync(int userId, int page, int pageSize, int? categoryId = null)
        {
            if (page < 1) throw new ArgumentException("Страница должна быть больше 0");
            if (pageSize < 1 || pageSize > 100) throw new ArgumentException("Лимит размера страницы от 1 до 100");

            IQueryable<Transaction> query;

            if (categoryId != null) query = _context.Transactions.Where(t => t.UserId == userId && t.CategoryId == categoryId);
            else query = _context.Transactions.Where(t => t.UserId == userId);

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToResponse()
                .ToListAsync();

            var totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((float)totalCount / pageSize);

            return new PaginationResponse<TransactionResponse>
            {
                Items = transactions,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public async Task<TransactionResponse?> GetByIdAsync(int userId, int id)
        {
            var transaction = await _context.Transactions
                .Where(t => t.UserId == userId && t.Id == id)
                .ToResponse()
                .FirstOrDefaultAsync();

            return transaction;
        }

        public async Task<TransactionResponse?> CreateAsync(int userId, CreateTransactionRequest request)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.UserId == userId && c.Id == request.CategoryId);
            if (!categoryExists) return null;

            if (request.Amount <= 0) throw new ArgumentException("Сумма должна быть больше 0");
            if (request.Date > DateTime.UtcNow) throw new ArgumentException("Недопустимы будущие даты");

            var transaction = new Transaction
            {
                Amount = request.Amount,
                Date = request.Date,
                Note = request.Note,
                UserId = userId,
                CategoryId = request.CategoryId
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(userId, transaction.Id);
        }

        public async Task<TransactionUpdateResult> UpdateAsync(int userId, int id, UpdateTransactionRequest request)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return TransactionUpdateResult.NotFound;
            else if (transaction.UserId != userId) return TransactionUpdateResult.NotFound;

            if (request.Amount <= 0) throw new ArgumentException("Сумма должна быть больше 0");
            if (request.Date > DateTime.UtcNow) throw new ArgumentException("Недопустимы будущие даты");

            var categoryExists = await _context.Categories.AnyAsync(c => c.UserId == userId && c.Id == request.CategoryId);
            if (!categoryExists) return TransactionUpdateResult.CategoryNotFound;

            transaction.Amount = request.Amount;
            transaction.Date = request.Date;
            transaction.Note = request.Note;
            transaction.CategoryId = request.CategoryId;

            await _context.SaveChangesAsync();
            return TransactionUpdateResult.Updated;
        }

        public async Task<bool> DeleteAsync(int userId, int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;
            if (transaction.UserId != userId) return false;

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
