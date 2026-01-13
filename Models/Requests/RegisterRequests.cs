namespace MiniStrava.Models.Requests
{
    public class RegisterRequests
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public int HeightCm { get; set; }
        public double WeightKg { get; set; }
        public string AvatarUrl { get; set; }
        public string PreferredLanguage { get; set; }
    }
}