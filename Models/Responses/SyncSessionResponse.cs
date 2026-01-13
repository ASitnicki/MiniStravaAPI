using MiniStrava.Models.DBObjects;

namespace MiniStrava.Models.Responses
{
    public class SyncSessionResponse
    {
        public Guid Id { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public SyncStatus Status { get; set; }
    }
}
