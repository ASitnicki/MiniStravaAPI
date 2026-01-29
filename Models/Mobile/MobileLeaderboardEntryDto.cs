namespace MiniStrava.Models.Mobile
{
    public class MobileLeaderboardEntryDto
    {
        public int Position { get; set; }
        public string User { get; set; } = string.Empty;
        public double DistanceKm { get; set; }
    }
}

