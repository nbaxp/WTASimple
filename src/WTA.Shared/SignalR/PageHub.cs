using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
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
        var httpContext = this.Context.GetHttpContext();
        var userName = httpContext?.User.Identity?.Name;
        if (!string.IsNullOrEmpty(userName))
        {
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
            this._eventPublisher.Publish(new SignalRConnectedEvent
            {
                ConnectionId = this.Context.ConnectionId,
                UserName = userName,
                Login = DateTime.UtcNow,
                UserAgent = httpContext?.Request.Headers["User-Agent"]
            });
            this.Context.Features.Get<IConnectionHeartbeatFeature>()?.OnHeartbeat(o =>
            {
                if (DateTime.Now.Second % 15 == 0)
                {
                    using var scope = WebApp.Current.Services.CreateScope();
                    scope.ServiceProvider.GetService<IEventPublisher>()?.Publish(new SignalRHeartbeatEvent { ConnectionId = o?.ToString()! });
                }
            }, this.Context.ConnectionId);
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
        this._eventPublisher.Publish(new SignalRDisconnectedEvent
        {
            ConnectionId = this.Context.ConnectionId,
            Logout = DateTime.UtcNow,
        });
        return base.OnDisconnectedAsync(exception);
    }

    public async Task ClientToServer(string command, string data, string? to = null, string? from = null)
    {
        await this._eventPublisher.Publish(new SignalCommandREvent
        {
            Command = command,
            Data = data,
            To = to,
            From = from
        }).ConfigureAwait(false);
    }
}
