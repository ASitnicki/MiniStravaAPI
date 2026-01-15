using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStrava.Models.Requests;
using MiniStrava.Models.Responses;
using MiniStrava.Services;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("api/me")]
    [Authorize]
    public class MeController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        private readonly ICurrentUserService _me;

        public MeController(AppDbContext ctx, ICurrentUserService me)
        {
            _ctx = ctx;
            _me = me;
        }

        [HttpGet]
        public async Task<ActionResult<MeResponse>> Get()
        {
            var email = _me.GetEmail();
            var u = await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email);
            if (u == null) return Unauthorized();

            return new MeResponse
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                BirthDate = u.BirthDate,
                Gender = u.Gender,
                HeightCm = u.HeightCm,
                WeightKg = u.WeightKg,
                AvatarUrl = u.AvatarUrl,
                PreferredLanguage = u.PreferredLanguage,
                IsAdmin = u.IsAdmin
            };
        }

        [HttpPut]
        public async Task<ActionResult<MeResponse>> Update([FromBody] UpdateMeRequest req)
        {
            var email = _me.GetEmail();
            var u = await _ctx.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (u == null) return Unauthorized();

            if (req.FirstName != null) u.FirstName = req.FirstName;
            if (req.LastName != null) u.LastName = req.LastName;
            if (req.BirthDate.HasValue) u.BirthDate = req.BirthDate.Value;
            if (req.Gender != null) u.Gender = req.Gender;
            if (req.HeightCm.HasValue) u.HeightCm = req.HeightCm.Value;
            if (req.WeightKg.HasValue) u.WeightKg = req.WeightKg.Value;
            if (req.AvatarUrl != null) u.AvatarUrl = req.AvatarUrl;
            if (req.PreferredLanguage != null) u.PreferredLanguage = req.PreferredLanguage;

            await _ctx.SaveChangesAsync();

            return new MeResponse
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                BirthDate = u.BirthDate,
                Gender = u.Gender,
                HeightCm = u.HeightCm,
                WeightKg = u.WeightKg,
                AvatarUrl = u.AvatarUrl,
                PreferredLanguage = u.PreferredLanguage,
                IsAdmin = u.IsAdmin
            };
        }
    }
}
