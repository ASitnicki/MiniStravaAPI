namespace MiniStrava.Models.Responses
{
    public class MeResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public int? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public string? AvatarUrl { get; set; }

        public string PreferredLanguage { get; set; } = "pl";
        public bool IsAdmin { get; set; }
    }
}
