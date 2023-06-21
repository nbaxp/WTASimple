using System.ComponentModel.DataAnnotations;

namespace WTA.Shared.Monitor;

public class MonitorModel
{
    public double CpuUsage { get; set; }

    public float DiskRead { get; set; }

    public float DiskWrite { get; set; }

    public long FinalizationPendingCount { get; set; }

    public string FrameworkDescription { get; set; } = null!;

    public long GCTotalMemory { get; set; }

    public int HandleCount { get; set; }
    public long HeapSizeBytes { get; set; }

    [Display]
    public string HostAddresses { get; set; } = null!;

    public string HostName { get; set; } = null!;

    public double MemoryUsage { get; set; }

    public float NetReceived { get; set; }
    public float NetSent { get; set; }
    public long OnlineUsers { get; set; }
    public string OSArchitecture { get; set; } = null!;

    public string OSDescription { get; set; } = null!;

    public string ProcessArchitecture { get; set; } = null!;

    public string ProcessArguments { get; set; } = null!;

    public int ProcessCount { get; set; }

    public double? ProcessCpuUsage { get; set; }

    public float ProcessDiskRead { get; set; }

    public float ProcessDiskWrite { get; set; }

    [Display]
    public string ProcessFileName { get; set; } = null!;

    public int ProcessId { get; set; }

    public float ProcessMemory { get; set; }

    public string ProcessName { get; set; } = null!;

    public int ProcessorCount { get; set; }

    public TimeSpan ProcessRunTime { get; set; }

    public DateTime ProcessStartTime { get; set; }

    public int ProcessThreadCount { get; set; }

    public DateTimeOffset ServerTime { get; set; }

    public string ServicePack { get; set; } = null!;
    public int ThreadCount { get; set; }

    public long TotalMemory { get; set; }

    public double TotalSeconds { get; set; }

    public string UserName { get; set; } = null!;
}
