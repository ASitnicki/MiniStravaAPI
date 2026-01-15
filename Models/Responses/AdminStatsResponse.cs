namespace MiniStrava.Models.Responses
{
    public class AdminStatsResponse
    {
        public int UsersCount { get; set; }
        public int ActivitiesCount { get; set; }
        public decimal TotalDistanceMeters { get; set; }
    }
}
