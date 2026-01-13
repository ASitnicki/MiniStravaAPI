using Microsoft.EntityFrameworkCore;
using MiniStrava.Models.DBObjects;

namespace MiniStrava.Repositories
{
    public interface ISyncSessionRepository
    {
        Task<List<SyncSession>> GetAllForUserAsync(Guid userId);
        Task<SyncSession?> GetByIdForUserAsync(Guid userId, Guid sessionId);
        Task AddAsync(SyncSession session);
        Task UpdateAsync(SyncSession session);
    }

    public class SyncSessionRepository : ISyncSessionRepository
    {
        private readonly AppDbContext _ctx;

        public SyncSessionRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public Task<List<SyncSession>> GetAllForUserAsync(Guid userId)
            => _ctx.SyncSessions
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.StartedAt)
                .ToListAsync();

        public Task<SyncSession?> GetByIdForUserAsync(Guid userId, Guid sessionId)
            => _ctx.SyncSessions.FirstOrDefaultAsync(s => s.UserId == userId && s.Id == sessionId);

        public async Task AddAsync(SyncSession session)
        {
            _ctx.SyncSessions.Add(session);
            await _ctx.SaveChangesAsync();
        }

        public async Task UpdateAsync(SyncSession session)
        {
            _ctx.SyncSessions.Update(session);
            await _ctx.SaveChangesAsync();
        }
    }
}
