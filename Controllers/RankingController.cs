using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStrava.Models.Responses;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("api/rankings")]
    [Authorize]
    public class RankingController : ControllerBase
    {
        private readonly AppDbContext _ctx;

        public RankingController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet("weekly")]
        public async Task<ActionResult<List<RankingEntryResponse>>> Weekly([FromQuery] int top = 10)
        {
            if (top <= 0) top = 10;
            if (top > 100) top = 100;

            var now = DateTimeOffset.UtcNow;
            var startOfWeekUtc = StartOfWeekUtc(now, DayOfWeek.Monday);

            var data = await _ctx.Activities.AsNoTracking()
                .Where(a => a.StartTime >= startOfWeekUtc)
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, Total = g.Sum(x => (decimal?)x.DistanceMeters) ?? 0m })
                .OrderByDescending(x => x.Total)
                .Take(top)
                .ToListAsync();

            var userIds = data.Select(d => d.UserId).ToList();
            var emails = await _ctx.Users.AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            var emailMap = emails.ToDictionary(x => x.Id, x => x.Email);

            return data.Select(d => new RankingEntryResponse
            {
                UserId = d.UserId,
                Email = emailMap.TryGetValue(d.UserId, out var e) ? e : string.Empty,
                TotalDistanceMeters = d.Total
            }).ToList();
        }

        private static DateTimeOffset StartOfWeekUtc(DateTimeOffset dt, DayOfWeek startOfWeek)
        {
            var date = dt.UtcDateTime.Date;
            int diff = (7 + (int)date.DayOfWeek - (int)startOfWeek) % 7;
            var start = date.AddDays(-diff);
            return new DateTimeOffset(start, TimeSpan.Zero);
        }
    }
}
