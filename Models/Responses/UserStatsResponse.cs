namespace MiniStrava.Models.Responses
{
    public class UserStatsResponse
    {
        public int TrainingsCount { get; set; }
        public decimal TotalDistanceMeters { get; set; }
        public decimal? AverageSpeedMps { get; set; }
    }
}
