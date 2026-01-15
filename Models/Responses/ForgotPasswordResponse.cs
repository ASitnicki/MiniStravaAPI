namespace MiniStrava.Models.Responses
{
    public class ForgotPasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        //zwracamy token w odpowiedzi, bo nie mamy wysyłki maili
        public string? ResetToken { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
    }
}
