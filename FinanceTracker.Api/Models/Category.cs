namespace FinanceTracker.Api.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public List<Transaction> Transactions { get; set; } = [];
    }
}
