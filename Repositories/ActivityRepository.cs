using Microsoft.EntityFrameworkCore;
using MiniStrava.Models.DBObjects;

namespace MiniStrava.Repositories
{
    public interface IActivityRepository
    {
        Task<List<Activity>> GetAllForUserAsync(Guid userId);
        Task<Activity?> GetByIdForUserAsync(Guid userId, Guid activityId, bool includeTrackPoints);
        Task AddAsync(Activity activity);
        Task UpdateAsync(Activity activity);
        Task DeleteAsync(Activity activity);

        Task<HashSet<int>> GetExistingSequencesAsync(Guid activityId, IEnumerable<int> sequences);
        Task AddTrackPointsAsync(IEnumerable<TrackPoint> points);
    }

    public class ActivityRepository : IActivityRepository
    {
        private readonly AppDbContext _ctx;

        public ActivityRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public Task<List<Activity>> GetAllForUserAsync(Guid userId)
            => _ctx.Activities
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

        public async Task<Activity?> GetByIdForUserAsync(Guid userId, Guid activityId, bool includeTrackPoints)
        {
            IQueryable<Activity> q = _ctx.Activities.Where(a => a.UserId == userId && a.Id == activityId);

            if (includeTrackPoints)
            {
                q = q.Include(a => a.TrackPoints)
                     .Include(a => a.ActivityPhotos);
            }

            return await q.FirstOrDefaultAsync();
        }

        public async Task AddAsync(Activity activity)
        {
            _ctx.Activities.Add(activity);
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(Activity activity)
        {
            _ctx.Activities.Update(activity);
            await _ctx.SaveChangesAsync();
        }

        public async Task DeleteAsync(Activity activity)
        {
            _ctx.Activities.Remove(activity);
            await _ctx.SaveChangesAsync();
        }

        public async Task<HashSet<int>> GetExistingSequencesAsync(Guid activityId, IEnumerable<int> sequences)
        {
            var list = await _ctx.TrackPoints
                .Where(tp => tp.ActivityId == activityId && sequences.Contains(tp.Sequence))
                .Select(tp => tp.Sequence)
                .ToListAsync();

            return list.ToHashSet();
        }

        public async Task AddTrackPointsAsync(IEnumerable<TrackPoint> points)
        {
            _ctx.TrackPoints.AddRange(points);
            await _ctx.SaveChangesAsync();
        }
    }
}
