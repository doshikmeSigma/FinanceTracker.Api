namespace FinanceTracker.Api.DTOs
{
    public class ReportDtos
    {
        public class CategorySummaryResponse
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = string.Empty;
            public string CategoryType { get; set; } = string.Empty;
            public int TransactionsCount { get; set; }
            public decimal TotalAmount { get; set; }
        }

        public class CategoryTotalResponse
        {
            public decimal Income { get; set; }
            public decimal Expense { get; set; }
            public decimal Balance => Income - Expense;
        }
    }
}
