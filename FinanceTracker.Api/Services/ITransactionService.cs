using FinanceTracker.Api.DTOs;

namespace FinanceTracker.Api.Services
{
    public interface ITransactionService
    {
        Task<List<TransactionResponse>> GetAllAsync(int userId, int? categoryId = null);
        Task<TransactionResponse?> GetByIdAsync(int userId, int id);
        Task<TransactionResponse?> CreateAsync(int userId, CreateTransactionRequest request);
        Task<TransactionUpdateResult> UpdateAsync(int userId, int id, UpdateTransactionRequest request);
        Task<bool> DeleteAsync(int userId, int id);
    }

    public enum TransactionUpdateResult { Updated, NotFound, CategoryNotFound }
}
