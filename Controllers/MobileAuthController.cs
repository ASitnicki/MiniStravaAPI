using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniStrava.Models.Mobile;
using MiniStrava.Models.Requests;
using MiniStrava.Services;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("auth")]
    [Route("api/auth")]
    public class MobileAuthController : ControllerBase
    {
        private readonly ILoginService _login;

        public MobileAuthController(ILoginService login)
        {
            _login = login;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] MobileLoginRequest req)
        {
            var response = _login.Login(new LoginRequest
            {
                Login = req.Email,
                Password = req.Password
            });

            if (!response.Success || string.IsNullOrWhiteSpace(response.JWTToken))
            {
                return Unauthorized(new { message = string.IsNullOrWhiteSpace(response.Message) ? "Invalid credentials." : response.Message });
            }

            return Ok(new MobileAuthResponse { Token = response.JWTToken });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] MobileRegisterRequest req)
        {
            var (firstName, lastName) = SplitName(req.Name);

            var response = _login.Register(new RegisterRequests
            {
                Email = req.Email,
                Password = req.Password,
                ConfirmPassword = req.Password,

                FirstName = firstName,
                LastName = lastName,
                BirthDate = DateTime.UtcNow.Date,
                Gender = string.Empty,
                HeightCm = 0,
                WeightKg = 0,
                AvatarUrl = string.Empty,
                PreferredLanguage = "pl"
            });

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        private static (string firstName, string lastName) SplitName(string? name)
        {
            var n = (name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(n)) return (string.Empty, string.Empty);

            var parts = n.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return (parts[0], string.Empty);

            var first = parts[0];
            var last = string.Join(' ', parts.Skip(1));
            return (first, last);
        }
    }
}
