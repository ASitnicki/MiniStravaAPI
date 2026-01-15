using MiniStrava.Models.DBObjects;

namespace MiniStrava.Models.Responses
{
    public class AdminActivityResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;

        public ActivityType ActivityType { get; set; }
        public string? Name { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public int? DurationSeconds { get; set; }
        public decimal DistanceMeters { get; set; }
        public string? Notes { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
