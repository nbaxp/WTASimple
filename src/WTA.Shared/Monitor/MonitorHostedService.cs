using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WTA.Shared.Attributes;
using WTA.Shared.Extensions;
using WTA.Shared.SignalR;

namespace WTA.Shared.Monitor;

[Implement<IHostedService>]
public class MonitorHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public MonitorHostedService(IServiceProvider applicationServices)
    {
        this._serviceProvider = applicationServices;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    //Debug.WriteLine($"{(OperatingSystem.IsWindows() ? "dir" : "ls")}".RunCommand());
                    this.DoWork();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                await Task.Delay(1000 * 1).ConfigureAwait(false);
            }
        }, cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this._cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private void DoWork()
    {
        using var scope = _serviceProvider.CreateScope();
        //if (scope.ServiceProvider.GetRequiredService<IConfiguration>().GetValue("EnableMonitor", false))
        {
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<PageHub>>();
            var monitorService = scope.ServiceProvider.GetRequiredService<IMonitorService>();
            hubContext.Clients.All.SendAsync(nameof(HubExtensions.ServerToClient), "monitor", monitorService.GetStatus());
        }
    }
}
