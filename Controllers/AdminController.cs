using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniStrava.Models.DBObjects;
using MiniStrava.Models.Responses;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _ctx;

        public AdminController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        [HttpGet("users")]
        public async Task<ActionResult<List<AdminUserResponse>>> Users()
        {
            var users = await _ctx.Users.AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new AdminUserResponse
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsAdmin = u.IsAdmin,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();

            return users;
        }

        [HttpGet("activities")]
        public async Task<ActionResult<List<AdminActivityResponse>>> Activities(
            [FromQuery] Guid? userId = null,
            [FromQuery] string? userEmail = null,
            [FromQuery] DateTimeOffset? from = null,
            [FromQuery] DateTimeOffset? to = null,
            [FromQuery] ActivityType? type = null,
            [FromQuery] decimal? minDistance = null,
            [FromQuery] decimal? maxDistance = null,
            [FromQuery] string? search = null)
        {
            IQueryable<Activity> q = _ctx.Activities.AsNoTracking();

            if (userId.HasValue)
                q = q.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                var uid = await _ctx.Users.AsNoTracking()
                    .Where(u => u.Email == userEmail)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();

                if (uid != Guid.Empty)
                    q = q.Where(a => a.UserId == uid);
                else
                    return new List<AdminActivityResponse>();
            }

            if (from.HasValue) q = q.Where(a => a.StartTime >= from.Value);
            if (to.HasValue) q = q.Where(a => a.StartTime <= to.Value);
            if (type.HasValue) q = q.Where(a => a.ActivityType == type.Value);
            if (minDistance.HasValue) q = q.Where(a => a.DistanceMeters >= minDistance.Value);
            if (maxDistance.HasValue) q = q.Where(a => a.DistanceMeters <= maxDistance.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(a => (a.Name != null && a.Name.Contains(s)) || (a.Notes != null && a.Notes.Contains(s)));
            }

            var data = await (from a in q
                              join u in _ctx.Users.AsNoTracking() on a.UserId equals u.Id
                              orderby a.StartTime descending
                              select new AdminActivityResponse
                              {
                                  Id = a.Id,
                                  UserId = a.UserId,
                                  UserEmail = u.Email,
                                  ActivityType = a.ActivityType,
                                  Name = a.Name,
                                  StartTime = a.StartTime,
                                  EndTime = a.EndTime,
                                  DurationSeconds = a.DurationSeconds,
                                  DistanceMeters = a.DistanceMeters,
                                  Notes = a.Notes,
                                  PhotoUrl = a.PhotoUrl
                              }).ToListAsync();

            return data;
        }

        [HttpDelete("activities/{id:guid}")]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            var a = await _ctx.Activities.FirstOrDefaultAsync(x => x.Id == id);
            if (a == null) return NotFound();

            _ctx.Activities.Remove(a);
            await _ctx.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("stats")]
        public async Task<ActionResult<AdminStatsResponse>> Stats()
        {
            var usersCount = await _ctx.Users.AsNoTracking().CountAsync();
            var activitiesCount = await _ctx.Activities.AsNoTracking().CountAsync();
            var totalDistance = await _ctx.Activities.AsNoTracking().SumAsync(a => (decimal?)a.DistanceMeters) ?? 0m;

            return new AdminStatsResponse
            {
                UsersCount = usersCount,
                ActivitiesCount = activitiesCount,
                TotalDistanceMeters = totalDistance
            };
        }
    }
}
