using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Extensions;
using FinanceTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CategoriesController(ICategoryService _service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<CategoryResponse>>> GetAll()
        {
            return (await _service.GetAllAsync(User.GetId()))
                .Select(c => c.ToCategoryResponse())
                .ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponse>> GetById(int id)
        {
            var category = await _service.GetByIdAsync(User.GetId(), id);
            if (category == null) return NotFound();

            var response = category.ToCategoryResponse();
            return Ok(response);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> Create([FromBody] CreateCategoryRequest request)
        {
            var category = await _service.CreateAsync(User.GetId(), request);

            var response = category.ToCategoryResponse();

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateCategoryRequest request)
        {
            var isUpdated = await _service.UpdateAsync(User.GetId(), id, request);
            if (!isUpdated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await _service.DeleteAsync(User.GetId(), id) switch
            {
                CategoryDeleteResult.HasTransactions => Conflict("Нельзя удалить категорию: к ней привязаны транзакции"),
                CategoryDeleteResult.NotFound => NotFound(),
                _ => NoContent()
            };
        }
    }
}