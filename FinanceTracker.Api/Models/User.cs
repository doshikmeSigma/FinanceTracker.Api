namespace FinanceTracker.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;

        public List<Transaction> Transactions { get; set; } = [];

        public List<Category> Categories { get; set; } = [];
    }
}
