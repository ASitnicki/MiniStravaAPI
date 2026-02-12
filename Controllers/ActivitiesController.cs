using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniStrava.Models.Requests;
using MiniStrava.Services;
using MiniStrava.Models.DBObjects;
using MiniStrava.Models.Mobile;
using System.Globalization;

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
        [Authorize]
        public async Task<ActionResult<List<MobileActivityDto>>> GetMobile()
        {
            var list = await _svc.GetMineAsync();

            // Mobile expects Track[] for TrackCount, so we fetch details with trackpoints.
            var result = new List<MobileActivityDto>(list.Count);
            foreach (var a in list)
            {
                var full = await _svc.GetMineByIdAsync(a.Id, includeTrackPoints: true);
                result.Add(MapToMobile(full));
            }

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<MobileActivityDto>> CreateMobile([FromBody] MobileCreateActivityRequest req)
        {
            var distanceKm = req.DistanceKm ?? 0.0;
            var distanceMeters = (decimal)(distanceKm * 1000.0);

            int? durationSeconds = null;
            if (req.EndTime.HasValue)
            {
                var d = (req.EndTime.Value - req.StartTime).TotalSeconds;
                if (d > 0) durationSeconds = (int)Math.Round(d);
            }

            var activityType = ParseMobileType(req.Type);

            var create = new CreateActivityRequest
            {
                Name = req.Name,
                ActivityType = activityType,
                StartTime = req.StartTime,
                EndTime = req.EndTime,
                DurationSeconds = durationSeconds,
                DistanceMeters = distanceMeters,
                Notes = req.Note
            };

            // Best-effort: compute avg speed/pace if possible.
            if (durationSeconds.HasValue && durationSeconds.Value > 0 && distanceKm > 0.0001)
            {
                create.AverageSpeedMps = (decimal)((distanceKm * 1000.0) / durationSeconds.Value);
                create.AveragePaceSecPerKm = (int)Math.Round(durationSeconds.Value / distanceKm);
            }

            var created = await _svc.CreateAsync(create);

            // Persist trackpoints if provided by mobile
            if (req.Track != null && req.Track.Count > 0)
            {
                var tp = new AddTrackPointsRequest();
                for (var i = 0; i < req.Track.Count; i++)
                {
                    var p = req.Track[i];
                    tp.Points.Add(new TrackPointDto
                    {
                        Sequence = i,
                        Timestamp = req.StartTime.AddSeconds(i),
                        Latitude = (decimal)p.Latitude,
                        Longitude = (decimal)p.Longitude
                    });
                }

                await _svc.AddTrackPointsAsync(created.Id, tp);
                created = await _svc.GetMineByIdAsync(created.Id, includeTrackPoints: true);
            }

            return Ok(MapToMobile(created));
        }

        

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool includeTrackPoints = false)
            => Ok(await _svc.GetMineByIdAsync(id, includeTrackPoints));

        

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

        // =======================
        // Mobile compatibility endpoints
        // Expected routes (from mobile): 
        //   POST   /auth/login, /auth/register, /auth/reset-password
        //   GET    /activities
        //   POST   /activities
        //   GET    /leaderboard/weekly
        // =======================


        [HttpGet("web")]
        [Authorize]
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

        [HttpPost("web")]
        public async Task<IActionResult> Create([FromBody] CreateActivityRequest req)
        {
            var created = await _svc.CreateAsync(req);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        private static ActivityType ParseMobileType(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return ActivityType.running;

            var t = raw.Trim().ToLowerInvariant();

            // Polish + English keywords
            if (t.Contains("bieg") || t.Contains("run")) return ActivityType.running;
            if (t.Contains("rower") || t.Contains("cycle") || t.Contains("bike")) return ActivityType.cycling;
            if (t.Contains("spacer") || t.Contains("walk")) return ActivityType.walking;
            if (t.Contains("wędr") || t.Contains("wedr") || t.Contains("hike") || t.Contains("trek")) return ActivityType.hike;
            if (t.Contains("trening") || t.Contains("workout") || t.Contains("gym")) return ActivityType.workout;

            // Fall back to enum parse
            if (Enum.TryParse<ActivityType>(t, ignoreCase: true, out var parsed))
                return parsed;

            return ActivityType.running;
        }

        private static MobileActivityDto MapToMobile(MiniStrava.Models.Responses.ActivityResponse a)
        {
            var distanceKm = (double)(a.DistanceMeters / 1000m);

            var durationSeconds =
                a.DurationSeconds
                ?? (a.EndTime.HasValue ? (int?)Math.Round((a.EndTime.Value - a.StartTime).TotalSeconds) : null);

            var paceText = FormatPaceText(a.AveragePaceSecPerKm, durationSeconds, distanceKm);
            var speedText = FormatSpeedText(a.AverageSpeedMps, durationSeconds, distanceKm);

            var dto = new MobileActivityDto
            {
                Id = a.Id.ToString(),
                Name = a.Name,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                DistanceKm = distanceKm,
                Note = a.Notes,
                Type = a.ActivityType.ToString(),
                PaceText = paceText,
                SpeedText = speedText,
                PhotoBase64 = null,
                Track = a.TrackPoints?.OrderBy(tp => tp.Sequence)
                    .Select(tp => new MobileGpsPoint
                    {
                        Latitude = (double)tp.Latitude,
                        Longitude = (double)tp.Longitude
                    })
                    .ToList() ?? new List<MobileGpsPoint>()
            };

            return dto;
        }

        private static string? FormatPaceText(int? avgPaceSecPerKm, int? durationSeconds, double distanceKm)
        {
            int? paceSec = avgPaceSecPerKm;
            if (!paceSec.HasValue && durationSeconds.HasValue && distanceKm > 0.0001)
                paceSec = (int)Math.Round(durationSeconds.Value / distanceKm);

            if (!paceSec.HasValue || paceSec.Value <= 0) return "--";

            var ts = TimeSpan.FromSeconds(paceSec.Value);
            // mm:ss /km
            return $"{(int)ts.TotalMinutes:00}:{ts.Seconds:00} /km";
        }

        private static string? FormatSpeedText(decimal? avgSpeedMps, int? durationSeconds, double distanceKm)
        {
            double? kmh = null;

            if (avgSpeedMps.HasValue && avgSpeedMps.Value > 0)
                kmh = (double)avgSpeedMps.Value * 3.6;

            if (!kmh.HasValue && durationSeconds.HasValue && durationSeconds.Value > 0 && distanceKm > 0.0001)
                kmh = distanceKm / (durationSeconds.Value / 3600.0);

            if (!kmh.HasValue || kmh.Value <= 0) return "--";

            return $"{kmh.Value.ToString("0.0", CultureInfo.InvariantCulture)} km/h";
        }

    }
}
