namespace WTA.Infrastructure.SignalR;

public class SignalREvent
{
    public string Command { get; set; } = null!;
    public object? Data { get; set; }
    public string? To { get; set; }
    public string? From { get; set; }
}