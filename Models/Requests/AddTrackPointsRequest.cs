namespace MiniStrava.Models.Requests
{
    public class AddTrackPointsRequest
    {
        public List<TrackPointDto> Points { get; set; } = new();
    }

    public class TrackPointDto
    {
        public int Sequence { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public decimal? ElevationMeters { get; set; }
        public decimal? SpeedMps { get; set; }
    }
}
