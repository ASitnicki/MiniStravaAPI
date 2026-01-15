namespace MiniStrava.Models.Responses
{
    public class AdminUserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsAdmin { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
    }
}
