using Microsoft.AspNetCore.SignalR;
using WTA.Infrastructure.EventBus;

namespace WTA.Infrastructure.SignalR;

public class PageHub : Hub
{
    private readonly ILogger<PageHub> _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly object balanceLock = new object();
    public static long Count { get; private set; }

    public PageHub(ILogger<PageHub> logger, IEventPublisher eventPublisher)
    {
        this._logger = logger;
        this._eventPublisher = eventPublisher;
    }

    public override Task OnConnectedAsync()
    {
        var userName = Context.GetHttpContext()?.User.Identity?.Name;
        this._logger.LogInformation($"{Context.ConnectionId} 已连接 {userName}");
        this.Groups.AddToGroupAsync(Context.ConnectionId, Context.ConnectionId);
        if (!string.IsNullOrEmpty(userName))
        {
            this.Groups.AddToGroupAsync(Context.ConnectionId, userName);
        }
        this.Clients.Group(Context.ConnectionId).SendAsync("Connected", Context.ConnectionId);
        lock (balanceLock)
        {
            Count++;
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        this._logger.LogInformation($"{Context.ConnectionId} has disconnected: {exception}");
        lock (balanceLock)
        {
            Count--;
        }
        return base.OnDisconnectedAsync(exception);
    }

    public async Task ClientToServer(string command, string data, string? to = null, string? from = null)
    {
        await _eventPublisher.Publish(new SignalREvent
        {
            Command = command,
            Data = data,
            To = to,
            From = from
        }).ConfigureAwait(false);
    }
}