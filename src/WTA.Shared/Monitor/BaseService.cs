using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using WTA.Shared.SignalR;

namespace WTA.Shared.Monitor;

public class BaseService
{
    public Process CurrentProcess = Process.GetCurrentProcess();

    public BaseService()
    {
    }

    public MonitorModel CreateModel()
    {
        var addresses = Dns.GetHostAddresses(Dns.GetHostName())
            .Where(o => o.AddressFamily == AddressFamily.InterNetwork)
            .Select(o => o.ToString())
            .Where(o => !o.StartsWith("127."))
            .ToArray();
        var gcMemoryInfo = GC.GetGCMemoryInfo();
        var drive = DriveInfo.GetDrives().FirstOrDefault(o => o.RootDirectory.FullName == Directory.GetDirectoryRoot(Path.GetPathRoot(Environment.ProcessPath!)!))!;
        var model = new MonitorModel
        {
            ServerTime = DateTimeOffset.UtcNow,
            UserName = Environment.UserName,
            OSArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            OSDescription = RuntimeInformation.OSDescription,
            ProcessCount = Process.GetProcesses().Length,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessorCount = Environment.ProcessorCount,
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            ProcessName = this.CurrentProcess.ProcessName,
            ProcessId = this.CurrentProcess.Id,
            ProcessFileName = Environment.ProcessPath!,
            HostName = Dns.GetHostName(),
            HostAddresses = string.Join(',', addresses),
            ProcessThreadCount = this.CurrentProcess.Threads.Count,
            ProcessStartTime = this.CurrentProcess.StartTime,
            ProcessArguments = Environment.CommandLine,
            GCTotalMemory = GC.GetTotalMemory(false),
            FinalizationPendingCount = gcMemoryInfo.FinalizationPendingCount,
            HeapSizeBytes = gcMemoryInfo.HeapSizeBytes,
            ProcessMemory = this.CurrentProcess.WorkingSet64,
            OnlineUsers = PageHub.Count,
            HandleCount = this.CurrentProcess.HandleCount,
            DriveName = drive.Name,
            DrivieTotalSize = drive.TotalSize,
            DriveAvailableFreeSpace = drive.AvailableFreeSpace
        };
        return model;
    }
}
