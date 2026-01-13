using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace MiniStrava.Services
{
    public interface ICurrentUserService
    {
        string GetEmail();
        Task<Guid> GetUserIdAsync();
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;
        private readonly AppDbContext _ctx;

        public CurrentUserService(IHttpContextAccessor http, AppDbContext ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        public string GetEmail()
        {
            var email = _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
            if (string.IsNullOrWhiteSpace(email))
                throw new UnauthorizedAccessException("Missing user identity (email claim).");
            return email;
        }

        public async Task<Guid> GetUserIdAsync()
        {
            var email = GetEmail();
            var userId = await _ctx.Users
                .Where(u => u.Email == email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not found.");
            return userId;
        }
    }
}
