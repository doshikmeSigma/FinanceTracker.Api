using FinanceTracker.Api.Models;
using FinanceTracker.Api.Services;

namespace FinanceTracker.Tests.Helpers
{
    public class FakeTokenService : ITokenService
    {
        public (string token, DateTime expiresAt) GenerateToken(User user)
        {
            return ("fake-token-for-testing", DateTime.UtcNow.AddHours(1));
        }
    }
}
