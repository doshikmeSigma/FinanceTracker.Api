using FinanceTracker.Api.Data;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Services
{
    public class CategoryService(AppDbContext _context) : ICategoryService
    {
        public async Task<List<Category>> GetAllAsync(int userId)
        {
            return await _context.Categories.Where(c => c.UserId == userId).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int userId, int id)
        {
            var category = await _context.Categories.FindAsync(id);
            return category?.UserId == userId ? category : null;
        }

        public async Task<Category> CreateAsync(int userId, CreateCategoryRequest request)
        {
            var category = new Category
            {
                Name = request.Name,
                Type = request.Type,
                UserId = userId
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<bool> UpdateAsync(int userId, int id, UpdateCategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return false;
            else if (category.UserId != userId) return false;

            category.Name = request.Name;
            category.Type = request.Type;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CategoryDeleteResult> DeleteAsync(int userId, int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return CategoryDeleteResult.NotFound;
            else if (category.UserId != userId) return CategoryDeleteResult.NotFound;

            if (await _context.Transactions.AnyAsync(t => t.CategoryId == id)) return CategoryDeleteResult.HasTransactions;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return CategoryDeleteResult.Deleted;
        }
    }
}
