using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniStrava.Models.Requests;
using MiniStrava.Services;
using MiniStrava.Models.DBObjects;

namespace MiniStrava.Controllers
{
    [ApiController]
    [Route("api/activities")]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly IActivityService _svc;

        public ActivitiesController(IActivityService svc)
        {
            _svc = svc;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] ActivityType? type = null,
        [FromQuery] decimal? minDistance = null,
        [FromQuery] decimal? maxDistance = null,
        [FromQuery] string? sort = "start_desc")
        {
            var items = await _svc.GetMineAsync();

            IEnumerable<MiniStrava.Models.Responses.ActivityResponse> q = items;

            if (from.HasValue) q = q.Where(a => a.StartTime >= from.Value);
            if (to.HasValue) q = q.Where(a => a.StartTime <= to.Value);
            if (type.HasValue) q = q.Where(a => a.ActivityType == type.Value);
            if (minDistance.HasValue) q = q.Where(a => a.DistanceMeters >= minDistance.Value);
            if (maxDistance.HasValue) q = q.Where(a => a.DistanceMeters <= maxDistance.Value);

            q = (sort ?? "start_desc").ToLowerInvariant() switch
            {
                "start_asc" => q.OrderBy(a => a.StartTime),
                "distance_asc" => q.OrderBy(a => a.DistanceMeters),
                "distance_desc" => q.OrderByDescending(a => a.DistanceMeters),
                "duration_asc" => q.OrderBy(a => a.DurationSeconds ?? int.MaxValue),
                "duration_desc" => q.OrderByDescending(a => a.DurationSeconds ?? 0),
                _ => q.OrderByDescending(a => a.StartTime)
            };

            return Ok(q.ToList());
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool includeTrackPoints = false)
            => Ok(await _svc.GetMineByIdAsync(id, includeTrackPoints));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateActivityRequest req)
        {
            var created = await _svc.CreateAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityRequest req)
            => Ok(await _svc.UpdateAsync(id, req));

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id:guid}/trackpoints")]
        public async Task<IActionResult> AddTrackPoints(Guid id, [FromBody] AddTrackPointsRequest req)
        {
            var added = await _svc.AddTrackPointsAsync(id, req);
            return Ok(new { success = true, added });
        }

        [HttpGet("{id:guid}/export/gpx")]
        [Authorize]
        public async Task<IActionResult> ExportGpx(Guid id)
        {
            var (fileName, bytes) = await _svc.ExportGpxAsync(id);
            return File(bytes, "application/gpx+xml", fileName);
        }
    }
}
