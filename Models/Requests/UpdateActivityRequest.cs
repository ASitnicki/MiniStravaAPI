using MiniStrava.Models.DBObjects;

namespace MiniStrava.Models.Requests
{
    public class UpdateActivityRequest
    {
        public string? Name { get; set; }
        public ActivityType? ActivityType { get; set; }

        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }

        public int? DurationSeconds { get; set; }
        public decimal? DistanceMeters { get; set; }

        public int? AveragePaceSecPerKm { get; set; }
        public decimal? AverageSpeedMps { get; set; }

        public string? Notes { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
