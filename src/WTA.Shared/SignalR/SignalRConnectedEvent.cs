namespace WTA.Shared.SignalR;

public class SignalRConnectedEvent
{
    public string ConnectionId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public DateTime Login { get; set; }
    public string? UserAgent { get; set; }
}
