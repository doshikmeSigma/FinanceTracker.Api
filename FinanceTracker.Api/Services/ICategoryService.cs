using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync(int userId);
        Task<Category?> GetByIdAsync(int userId, int id);
        Task<Category> CreateAsync(int userId, CreateCategoryRequest request);
        Task<bool> UpdateAsync(int userId, int id, UpdateCategoryRequest request);
        Task<CategoryDeleteResult> DeleteAsync(int userId, int id);
    }

    public enum CategoryDeleteResult { Deleted, NotFound, HasTransactions }
}
