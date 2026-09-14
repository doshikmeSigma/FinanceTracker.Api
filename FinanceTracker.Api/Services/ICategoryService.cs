using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Services
{
    public interface ICategoryService
    {
        Task<PaginationResponse<CategoryResponse>> GetAllAsync(int userId, int page, int pageSize);
        Task<Category?> GetByIdAsync(int userId, int id);
        Task<Category> CreateAsync(int userId, CreateCategoryRequest request);
        Task<bool> UpdateAsync(int userId, int id, UpdateCategoryRequest request);
        Task<CategoryDeleteResult> DeleteAsync(int userId, int id);
    }

    public enum CategoryDeleteResult { Deleted, NotFound, HasTransactions }
}
