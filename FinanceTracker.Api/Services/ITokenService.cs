using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Services
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) GenerateToken(User user);
    }
}
