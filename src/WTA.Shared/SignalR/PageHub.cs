using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using WTA.Shared.EventBus;

namespace WTA.Shared.SignalR;

public class PageHub : Hub
{
    private readonly ILogger<PageHub> _logger;
    private readonly IEventPublisher _eventPublisher;
    private readonly object balanceLock = new();
    public static long Count { get; private set; }

    public PageHub(ILogger<PageHub> logger, IEventPublisher eventPublisher)
    {
        this._logger = logger;
        this._eventPublisher = eventPublisher;
    }

    public override Task OnConnectedAsync()
    {
        var userName = this.Context.GetHttpContext()?.User.Identity?.Name;
        this._logger.LogInformation($"{this.Context.ConnectionId} 已连接 {userName}");
        this.Groups.AddToGroupAsync(this.Context.ConnectionId, this.Context.ConnectionId);
        if (!string.IsNullOrEmpty(userName))
        {
            this.Groups.AddToGroupAsync(this.Context.ConnectionId, userName);
        }
        this.Clients.Group(this.Context.ConnectionId).SendAsync("Connected", this.Context.ConnectionId);
        lock (this.balanceLock)
        {
            Count++;
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        this._logger.LogInformation($"{this.Context.ConnectionId} has disconnected: {exception}");
        lock (this.balanceLock)
        {
            Count--;
        }
        return base.OnDisconnectedAsync(exception);
    }

    public async Task ClientToServer(string command, string data, string? to = null, string? from = null)
    {
        await this._eventPublisher.Publish(new SignalREvent
        {
            Command = command,
            Data = data,
            To = to,
            From = from
        }).ConfigureAwait(false);
    }
}
