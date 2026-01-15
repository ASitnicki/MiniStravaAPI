using MiniStrava.Models.DBObjects;
using MiniStrava.Models.Requests;
using MiniStrava.Models.Responses;
using MiniStrava.Repositories;
using MiniStrava.Utils;
using System.Linq;

namespace MiniStrava.Services
{
    public interface IActivityService
    {
        Task<List<ActivityResponse>> GetMineAsync();
        Task<ActivityResponse> GetMineByIdAsync(Guid id, bool includeTrackPoints);

        Task<ActivityResponse> CreateAsync(CreateActivityRequest req);
        Task<ActivityResponse> UpdateAsync(Guid id, UpdateActivityRequest req);
        Task DeleteAsync(Guid id);

        Task<int> AddTrackPointsAsync(Guid activityId, AddTrackPointsRequest req);

        Task<(string fileName, byte[] bytes)> ExportGpxAsync(Guid activityId);
    }

    public class ActivityService : IActivityService
    {
        private readonly ICurrentUserService _me;
        private readonly IActivityRepository _repo;

        public ActivityService(ICurrentUserService me, IActivityRepository repo)
        {
            _me = me;
            _repo = repo;
        }

        public async Task<List<ActivityResponse>> GetMineAsync()
        {
            var userId = await _me.GetUserIdAsync();
            var items = await _repo.GetAllForUserAsync(userId);

            return items.Select(MapActivity).ToList();
        }

        public async Task<ActivityResponse> GetMineByIdAsync(Guid id, bool includeTrackPoints)
        {
            var userId = await _me.GetUserIdAsync();
            var a = await _repo.GetByIdForUserAsync(userId, id, includeTrackPoints);
            if (a == null) throw new KeyNotFoundException("Activity not found.");

            return MapActivity(a, includeTrackPoints);
        }

        public async Task<(string fileName, byte[] bytes)> ExportGpxAsync(Guid activityId)
        {
            var userId = await _me.GetUserIdAsync();
            var a = await _repo.GetByIdForUserAsync(userId, activityId, includeTrackPoints: true);
            if (a == null) throw new KeyNotFoundException("Activity not found.");

            var bytes = GpxBuilder.Build(a);

            var safe = string.IsNullOrWhiteSpace(a.Name) ? "activity" : a.Name;
            safe = string.Concat(safe.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_'));
            var fileName = $"{safe}_{a.StartTime:yyyyMMdd_HHmm}.gpx";

            return (fileName, bytes);
        }

        public async Task<ActivityResponse> CreateAsync(CreateActivityRequest req)
        {
            if (req.StartTime == default)
                throw new ArgumentException("StartTime is required.");

            var userId = await _me.GetUserIdAsync();

            var a = new Activity
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = req.Name,
                ActivityType = req.ActivityType,
                StartTime = req.StartTime,
                EndTime = req.EndTime,
                DurationSeconds = req.DurationSeconds,
                DistanceMeters = req.DistanceMeters,
                AveragePaceSecPerKm = req.AveragePaceSecPerKm,
                AverageSpeedMps = req.AverageSpeedMps,
                Notes = req.Notes,
                PhotoUrl = req.PhotoUrl,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _repo.AddAsync(a);
            return MapActivity(a);
        }

        public async Task<ActivityResponse> UpdateAsync(Guid id, UpdateActivityRequest req)
        {
            var userId = await _me.GetUserIdAsync();
            var a = await _repo.GetByIdForUserAsync(userId, id, includeTrackPoints: false);
            if (a == null) throw new KeyNotFoundException("Activity not found.");

            if (req.Name != null) a.Name = req.Name;
            if (req.ActivityType.HasValue) a.ActivityType = req.ActivityType.Value;
            if (req.StartTime.HasValue) a.StartTime = req.StartTime.Value;
            if (req.EndTime.HasValue) a.EndTime = req.EndTime.Value;
            if (req.DurationSeconds.HasValue) a.DurationSeconds = req.DurationSeconds.Value;
            if (req.DistanceMeters.HasValue) a.DistanceMeters = req.DistanceMeters.Value;
            if (req.AveragePaceSecPerKm.HasValue) a.AveragePaceSecPerKm = req.AveragePaceSecPerKm.Value;
            if (req.AverageSpeedMps.HasValue) a.AverageSpeedMps = req.AverageSpeedMps.Value;
            if (req.Notes != null) a.Notes = req.Notes;
            if (req.PhotoUrl != null) a.PhotoUrl = req.PhotoUrl;

            await _repo.UpdateAsync(a);
            return MapActivity(a);
        }

        public async Task DeleteAsync(Guid id)
        {
            var userId = await _me.GetUserIdAsync();
            var a = await _repo.GetByIdForUserAsync(userId, id, includeTrackPoints: false);
            if (a == null) throw new KeyNotFoundException("Activity not found.");

            await _repo.DeleteAsync(a);
        }

        public async Task<int> AddTrackPointsAsync(Guid activityId, AddTrackPointsRequest req)
        {
            if (req?.Points == null || req.Points.Count == 0)
                return 0;

            var userId = await _me.GetUserIdAsync();
            var a = await _repo.GetByIdForUserAsync(userId, activityId, includeTrackPoints: false);
            if (a == null) throw new KeyNotFoundException("Activity not found.");

            var sequences = req.Points.Select(p => p.Sequence).ToArray();
            var existing = await _repo.GetExistingSequencesAsync(activityId, sequences);

            var toInsert = req.Points
                .Where(p => !existing.Contains(p.Sequence))
                .Select(p => new TrackPoint
                {
                    ActivityId = activityId,
                    Sequence = p.Sequence,
                    Timestamp = p.Timestamp,
                    Latitude = p.Latitude,
                    Longitude = p.Longitude,
                    ElevationMeters = p.ElevationMeters,
                    SpeedMps = p.SpeedMps
                })
                .ToList();

            if (toInsert.Count == 0) return 0;

            await _repo.AddTrackPointsAsync(toInsert);
            return toInsert.Count;
        }

        private static ActivityResponse MapActivity(Activity a)
            => MapActivity(a, includeTrackPoints: false);

        private static ActivityResponse MapActivity(Activity a, bool includeTrackPoints)
        {
            return new ActivityResponse
            {
                Id = a.Id,
                ActivityType = a.ActivityType,
                Name = a.Name,
                Notes = a.Notes,
                PhotoUrl = a.PhotoUrl,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                DurationSeconds = a.DurationSeconds,
                DistanceMeters = a.DistanceMeters,
                AveragePaceSecPerKm = a.AveragePaceSecPerKm,
                AverageSpeedMps = a.AverageSpeedMps,
                CreatedAt = a.CreatedAt,
                TrackPoints = includeTrackPoints
                    ? a.TrackPoints
                        .OrderBy(tp => tp.Sequence)
                        .Select(tp => new TrackPointResponse
                        {
                            Id = tp.Id,
                            Sequence = tp.Sequence,
                            Timestamp = tp.Timestamp,
                            Latitude = tp.Latitude,
                            Longitude = tp.Longitude,
                            ElevationMeters = tp.ElevationMeters,
                            SpeedMps = tp.SpeedMps
                        })
                        .ToList()
                    : null
            };
        }
    }
}
