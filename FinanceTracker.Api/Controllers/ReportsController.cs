using FinanceTracker.Api.Extensions;
using FinanceTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static FinanceTracker.Api.DTOs.ReportDtos;

namespace FinanceTracker.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ReportsController(IReportService _service) : ControllerBase
    {

        [HttpGet("summary")]
        public async Task<ActionResult<List<CategorySummaryResponse>>> GetSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            return Ok(await _service.GetSummaryAsync(User.GetId(), from, to));
        }

        [HttpGet("total")]
        public async Task<ActionResult<CategoryTotalResponse>> GetTotal([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            return Ok(await _service.GetTotalAsync(User.GetId(), from, to));
        }
    }
}
