namespace MiniStrava.Models.Responses
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? JWTToken { get; set; }
        public bool MustChangePassword { get; set; }

    }
}
