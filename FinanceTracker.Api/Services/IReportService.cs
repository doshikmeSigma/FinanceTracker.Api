using static FinanceTracker.Api.DTOs.ReportDtos;

namespace FinanceTracker.Api.Services
{
    public interface IReportService
    {
        Task<List<CategorySummaryResponse>> GetSummaryAsync(int userId, DateTime? from, DateTime? to);
        Task<CategoryTotalResponse> GetTotalAsync(int userId, DateTime? from, DateTime? to);
    }
}
