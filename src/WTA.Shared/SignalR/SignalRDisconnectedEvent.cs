namespace WTA.Shared.SignalR;

public class SignalRDisconnectedEvent
{
    public string ConnectionId { get; set; } = null!;
    public DateTime Logout { get; set; }
}
