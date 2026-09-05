using FinanceTracker.Api.DTOs;

namespace FinanceTracker.Api.Services
{
    public interface IAuthService
    {
        Task<RegisterResult> RegisterAsync(RegisterRequest registerRequest);
        Task<AuthResponse?> LoginAsync(LoginRequest loginRequest);
    }

    public enum RegisterResult { Created, EmailTaken }
}
