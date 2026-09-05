using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Extensions
{
    public static class CategoryExtensions
    {
        public static CategoryResponse ToCategoryResponse(this Category category) => new CategoryResponse
            {
                Id = category.Id, 
                Name = category.Name,
                Type = category.Type
            };
    }
}
