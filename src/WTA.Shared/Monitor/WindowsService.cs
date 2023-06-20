using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Attributes;
using WTA.Shared.DependencyInjection;
using WTA.Shared.Extensions;

namespace WTA.Shared.Monitor;

[Implement<IMonitorService>(ServiceLifetime.Singleton, PlatformType.Windows)]
[SupportedOSPlatform("windows")]
public class WindowsService : BaseService, IMonitorService
{
    private readonly PerformanceCounter CPUCounter = new("Processor", "% Processor Time", "_Total");
    private readonly PerformanceCounter ThreadCounter = new("Process", "Thread Count", "_Total");
    private readonly PerformanceCounter ProcessDistReadCounter;
    private readonly PerformanceCounter ProcessDistWriteCounter;
    private readonly PerformanceCounter ProcessCPUCounter;
    private readonly PerformanceCounter MemoryCounter = new("Memory", "% Committed Bytes In Use");
    private readonly PerformanceCounterCategory NetworkInterfaceCategory = new("Network Interface");
    private readonly PerformanceCounter PhysicalDiskReadCounter = new("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
    private readonly PerformanceCounter PhysicalDiskWriteCounter = new("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
    private readonly List<PerformanceCounter> ReceivedCounters = new();
    private readonly List<PerformanceCounter> SentCounters = new();
    private string[] Names = Array.Empty<string>();

    public WindowsService()
    {
        this.ProcessDistReadCounter = new PerformanceCounter("Process", "IO Read Bytes/sec", base.CurrentProcess.ProcessName);
        this.ProcessDistWriteCounter = new PerformanceCounter("Process", "IO Write Bytes/sec", base.CurrentProcess.ProcessName);
        this.ProcessCPUCounter = new PerformanceCounter("Process", "% Processor Time", base.CurrentProcess.ProcessName);
        UpdateNetWorkCounters();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        this.CPUCounter.Dispose();
        this.ThreadCounter.Dispose();
        this.ProcessDistReadCounter.Dispose();
        this.ProcessDistWriteCounter.Dispose();
        this.ProcessCPUCounter.Dispose();
        this.MemoryCounter.Dispose();
        this.PhysicalDiskReadCounter.Dispose();
        this.PhysicalDiskWriteCounter.Dispose();
        this.ReceivedCounters.ForEach(o => o.Dispose());
        this.SentCounters.ForEach(o => o.Dispose());
    }

    public MonitorModel GetStatus()
    {
        var model = base.CreateModel();
        model.CpuUsage = this.CPUCounter.NextValue() / 100;
        model.ProcessCpuUsage = this.ProcessCPUCounter.NextValue() / 100 / Environment.ProcessorCount;
        model.MemoryUsage = this.MemoryCounter.NextValue() / 100;
        this.UpdateNetWorkCounters();
        model.NetReceived = this.ReceivedCounters.Sum(o => o.NextValue());
        model.NetSent = this.SentCounters.Sum(o => o.NextValue());
        model.DiskRead = this.PhysicalDiskReadCounter.NextValue();
        model.DiskWrite = this.PhysicalDiskWriteCounter.NextValue();
        model.ProcessDiskRead = this.ProcessDistReadCounter.NextValue();
        model.ProcessDiskWrite = this.ProcessDistWriteCounter.NextValue();
        model.ThreadCount = (int)this.ThreadCounter.NextValue();
        using var mc = new ManagementClass("Win32_PhysicalMemory");
        foreach (var item in mc.GetInstances().Cast<ManagementObject>())
        {
            model.TotalMemory += item.Properties["Capacity"].Value.ToString()!.ToLong();
            item.Dispose();
        }
        return model;
    }

    private void UpdateNetWorkCounters()
    {
        var names = this.NetworkInterfaceCategory.GetInstanceNames();
        if (!Enumerable.SequenceEqual(this.Names, names))
        {
            this.Names = names;
            this.ReceivedCounters.ForEach(o => o.Dispose());
            this.ReceivedCounters.Clear();
            this.SentCounters.ForEach(o => o.Dispose());
            this.SentCounters.Clear();
            names.ForEach(name =>
            {
                this.ReceivedCounters.Add(new PerformanceCounter("Network Interface", "Bytes Received/sec", name));
                this.SentCounters.Add(new PerformanceCounter("Network Interface", "Bytes Sent/sec", name));
            });
        }
    }
}
