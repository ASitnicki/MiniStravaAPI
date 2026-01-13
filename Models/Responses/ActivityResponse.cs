using MiniStrava.Models.DBObjects;

namespace MiniStrava.Models.Responses
{
    public class ActivityResponse
    {
        public Guid Id { get; set; }
        public ActivityType ActivityType { get; set; }

        public string? Name { get; set; }
        public string? Notes { get; set; }
        public string? PhotoUrl { get; set; }

        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }

        public int? DurationSeconds { get; set; }
        public decimal DistanceMeters { get; set; }
        public int? AveragePaceSecPerKm { get; set; }
        public decimal? AverageSpeedMps { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public List<TrackPointResponse>? TrackPoints { get; set; }
    }

    public class TrackPointResponse
    {
        public long Id { get; set; }
        public int Sequence { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public decimal? ElevationMeters { get; set; }
        public decimal? SpeedMps { get; set; }
    }
}
