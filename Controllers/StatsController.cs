using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStrava.Models.Responses;
using MiniStrava.Services;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("api/stats")]
    [Authorize]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _ctx;
        private readonly ICurrentUserService _me;

        public StatsController(AppDbContext ctx, ICurrentUserService me)
        {
            _ctx = ctx;
            _me = me;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserStatsResponse>> GetMyStats([FromQuery] DateTimeOffset? from = null, [FromQuery] DateTimeOffset? to = null)
        {
            var userId = await _me.GetUserIdAsync();

            var q = _ctx.Activities.AsNoTracking().Where(a => a.UserId == userId);
            if (from.HasValue) q = q.Where(a => a.StartTime >= from.Value);
            if (to.HasValue) q = q.Where(a => a.StartTime <= to.Value);

            var trainingsCount = await q.CountAsync();
            var totalDistance = await q.SumAsync(a => (decimal?)a.DistanceMeters) ?? 0m;

            var avgSpeed = await q.Where(a => a.AverageSpeedMps != null)
                .AverageAsync(a => (decimal?)a.AverageSpeedMps);

            return new UserStatsResponse
            {
                TrainingsCount = trainingsCount,
                TotalDistanceMeters = totalDistance,
                AverageSpeedMps = avgSpeed
            };
        }
    }
}
