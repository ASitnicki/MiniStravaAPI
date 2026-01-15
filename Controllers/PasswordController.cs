using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStrava.Models.Requests;
using MiniStrava.Models.Responses;
using MiniStrava.Utils;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("api/password")]
    public class PasswordController : ControllerBase
    {
        private readonly AppDbContext _ctx;

        public PasswordController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpPost("forgot")]
        public async Task<ActionResult<ForgotPasswordResponse>> Forgot([FromBody] ForgotPasswordRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest(new ForgotPasswordResponse { Success = false, Message = "Email is required." });

            var u = await _ctx.Users.FirstOrDefaultAsync(x => x.Email == req.Email);
            if (u == null)
            {
                return Ok(new ForgotPasswordResponse { Success = true, Message = "If the account exists, reset token was generated." });
            }

            var token = Guid.NewGuid().ToString("N");
            var expires = DateTimeOffset.UtcNow.AddHours(1);

            u.PasswordResetToken = token;
            u.PasswordResetTokenExpiresAt = expires;

            await _ctx.SaveChangesAsync();

            return Ok(new ForgotPasswordResponse
            {
                Success = true,
                Message = "Reset token generated.",
                ResetToken = token,
                ExpiresAt = expires
            });
        }

        [HttpPost("reset")]
        public async Task<ActionResult<RegisterResponse>> Reset([FromBody] ResetPasswordRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Token) || string.IsNullOrWhiteSpace(req.NewPassword))
                return BadRequest(new RegisterResponse { Success = false, Message = "Email, Token and NewPassword are required." });

            if (!IsPasswordOk(req.NewPassword))
                return BadRequest(new RegisterResponse { Success = false, Message = "Password is too weak." });

            var u = await _ctx.Users.FirstOrDefaultAsync(x => x.Email == req.Email);
            if (u == null)
                return BadRequest(new RegisterResponse { Success = false, Message = "Invalid token." });

            if (u.PasswordResetToken == null || u.PasswordResetTokenExpiresAt == null)
                return BadRequest(new RegisterResponse { Success = false, Message = "Invalid token." });

            if (!string.Equals(u.PasswordResetToken, req.Token, StringComparison.Ordinal))
                return BadRequest(new RegisterResponse { Success = false, Message = "Invalid token." });

            if (u.PasswordResetTokenExpiresAt.Value < DateTimeOffset.UtcNow)
                return BadRequest(new RegisterResponse { Success = false, Message = "Token expired." });

            var salt = PasswordTools.GenerateSalt();
            var hash = PasswordTools.GenerateHash(req.NewPassword, salt);
            u.PasswordSalt = Encoding.UTF8.GetBytes(salt);
            u.PasswordHash = Encoding.UTF8.GetBytes(hash);

            u.PasswordResetToken = null;
            u.PasswordResetTokenExpiresAt = null;
            u.MustChangePassword = false;

            await _ctx.SaveChangesAsync();

            return Ok(new RegisterResponse { Success = true, Message = "" });
        }

        private static bool IsPasswordOk(string password)
        {
            if (password.Length < 7) return false;
            return Regex.IsMatch(password, "^(?=.*[^A-Za-z0-9]).+$");
        }
    }
}
