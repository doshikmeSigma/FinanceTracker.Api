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
    public class TransactionsController(ITransactionService _service) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<TransactionResponse>>> GetAll([FromQuery] int? categoryId = null)
        {
            return Ok(await _service.GetAllAsync(User.GetId(), categoryId));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionResponse>> GetById(int id)
        {
            var transaction = await _service.GetByIdAsync(User.GetId(), id);
            return transaction == null ? NotFound() : Ok(transaction);
        }

        [HttpPost]
        public async Task<ActionResult<TransactionResponse>> Create([FromBody] CreateTransactionRequest request)
        {
            var transaction = await _service.CreateAsync(User.GetId(), request);

            return transaction == null ? NotFound("Данная категория не существует") : CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateTransactionRequest request)
        {
            return await _service.UpdateAsync(User.GetId(), id, request) switch
            {
                TransactionUpdateResult.NotFound => NotFound(),
                TransactionUpdateResult.CategoryNotFound => NotFound("Данная категория не существует"),
                _ => NoContent()
            };
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            return await _service.DeleteAsync(User.GetId(), id) switch
            {
                false => NotFound(),
                true => NoContent()
            };
        }
    }
}
