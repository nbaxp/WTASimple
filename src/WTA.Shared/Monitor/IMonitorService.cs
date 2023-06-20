namespace WTA.Shared.Monitor;

public interface IMonitorService : IDisposable
{
    MonitorModel GetStatus();
}
