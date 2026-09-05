using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs
{
    public class CreateTransactionRequest
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string Note { get; set; } = string.Empty;

        public int CategoryId { get; set; }
    }

    public class UpdateTransactionRequest
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string Note { get; set; } = string.Empty;

        public int CategoryId { get; set; }
    }

    public class TransactionResponse
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; } = string.Empty;
        public CategoryResponse Category { get; set; } = null!;
    }
}
