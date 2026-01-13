namespace MiniStrava.Models.Requests
{
    public class FinishSyncSessionRequest
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
