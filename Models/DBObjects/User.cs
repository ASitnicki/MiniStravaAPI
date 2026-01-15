namespace MiniStrava.Models.DBObjects
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public bool MustChangePassword { get; set; } = false;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public int HeightCm { get; set; }
        public decimal WeightKg { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsAdmin { get; set; }
        public string PreferredLanguage { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTimeOffset? PasswordResetTokenExpiresAt { get; set; }
    }
}
