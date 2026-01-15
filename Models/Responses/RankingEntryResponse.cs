namespace MiniStrava.Models.Responses
{
    public class RankingEntryResponse
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public decimal TotalDistanceMeters { get; set; }
    }
}
