namespace WTA.Shared.SignalR;

public class SignalCommandREvent
{
    public string Command { get; set; } = null!;
    public object? Data { get; set; }
    public string? To { get; set; }
    public string? From { get; set; }
}
