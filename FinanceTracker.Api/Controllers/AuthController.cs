using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracker.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/auth")]
    public class AuthController(IAuthService _service) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            return await _service.RegisterAsync(registerRequest) switch
            {
                RegisterResult.EmailTaken => Conflict("Почта занята, введите другую"),
                _ => Created()
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var response = await _service.LoginAsync(loginRequest);
            if (response == null) return Unauthorized("Неверная почта или пароль");
            return Ok(response);
        }
    }
}
