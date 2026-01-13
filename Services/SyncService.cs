using MiniStrava.Models.DBObjects;
using MiniStrava.Models.Requests;
using MiniStrava.Models.Responses;
using MiniStrava.Repositories;

namespace MiniStrava.Services
{
    public interface ISyncService
    {
        Task<List<SyncSessionResponse>> GetMineAsync();
        Task<SyncSessionResponse> StartAsync(StartSyncSessionRequest req);
        Task<SyncSessionResponse> FinishAsync(Guid sessionId, FinishSyncSessionRequest req);
    }

    public class SyncService : ISyncService
    {
        private readonly ICurrentUserService _me;
        private readonly ISyncSessionRepository _repo;

        public SyncService(ICurrentUserService me, ISyncSessionRepository repo)
        {
            _me = me;
            _repo = repo;
        }

        public async Task<List<SyncSessionResponse>> GetMineAsync()
        {
            var userId = await _me.GetUserIdAsync();
            var sessions = await _repo.GetAllForUserAsync(userId);
            return sessions.Select(Map).ToList();
        }

        public async Task<SyncSessionResponse> StartAsync(StartSyncSessionRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.DeviceId))
                throw new ArgumentException("DeviceId is required.");
            if (string.IsNullOrWhiteSpace(req.ClientSessionId))
                throw new ArgumentException("ClientSessionId is required.");

            var userId = await _me.GetUserIdAsync();

            var s = new SyncSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartedAt = DateTimeOffset.UtcNow,
                Status = SyncStatus.running
            };

            await _repo.AddAsync(s);
            return Map(s);
        }

        public async Task<SyncSessionResponse> FinishAsync(Guid sessionId, FinishSyncSessionRequest req)
        {
            var userId = await _me.GetUserIdAsync();
            var s = await _repo.GetByIdForUserAsync(userId, sessionId);
            if (s == null) throw new KeyNotFoundException("Sync session not found.");

            s.CompletedAt = DateTimeOffset.UtcNow;
            s.Status = req.Success ? SyncStatus.completed : SyncStatus.failed;

            await _repo.UpdateAsync(s);
            return Map(s);
        }

        private static SyncSessionResponse Map(SyncSession s)
            => new SyncSessionResponse
            {
                Id = s.Id,
                StartedAt = s.StartedAt,
                CompletedAt = s.CompletedAt,
                Status = s.Status
            };
    }
}
