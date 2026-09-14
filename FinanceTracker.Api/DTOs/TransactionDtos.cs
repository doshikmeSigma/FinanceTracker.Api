using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs
{
    public class CreateTransactionRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Значение должно быть больше 0")]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string Note { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "CategoryId должен быть больше 0")]
        public int CategoryId { get; set; }
    }

    public class UpdateTransactionRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Значение должно быть больше 0")]
        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string Note { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "CategoryId должен быть больше 0")]
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
