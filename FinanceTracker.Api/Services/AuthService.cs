using FinanceTracker.Api.Data;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Extensions;
using FinanceTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Services
{
    public class AuthService(AppDbContext _context, ITokenService _tokenService) : IAuthService
    {
        private static readonly (string Name, string Type)[] DefaultCategories =
        [
            ("Работа", "Доход"),
            ("Развлечения", "Расход"),
            ("ЖКХ", "Расход")
        ];

        public async Task<RegisterResult> RegisterAsync(RegisterRequest registerRequest)
        {
            var isEmailTaken = await _context.Users.AnyAsync(u => u.Email == registerRequest.Email);
            if (isEmailTaken) return RegisterResult.EmailTaken;

            (string hash, string salt) = PasswordHasher.Hash(registerRequest.Password);

            _context.Users.Add(new User
            {
                Email = registerRequest.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                Categories = [.. DefaultCategories.Select(c => new Category
                {
                    Name = c.Name,
                    Type = c.Type
                })]
            });

            await _context.SaveChangesAsync();

            return RegisterResult.Created;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest loginRequest)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginRequest.Email);
            if (user == null) return null;

            if (PasswordHasher.Verify(loginRequest.Password, user.PasswordHash, user.PasswordSalt))
            {
                var (token, expiresAt) = _tokenService.GenerateToken(user);
                return new AuthResponse
                {
                    Token = token,
                    ExpiresAt = expiresAt
                };
            }
            else return null;
        }
    }
}
