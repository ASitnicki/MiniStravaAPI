namespace MiniStrava.Models.Requests
{
    public class StartSyncSessionRequest
    {
        public string DeviceId { get; set; } = null!;
        public string ClientSessionId { get; set; } = null!;
    }
}