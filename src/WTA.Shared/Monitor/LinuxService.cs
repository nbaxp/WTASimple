using System.Diagnostics;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using WTA.Shared.Attributes;
using WTA.Shared.DependencyInjection;
using WTA.Shared.Extensions;

namespace WTA.Shared.Monitor;

[Implement<IMonitorService>(ServiceLifetime.Singleton, PlatformType.Linux)]
[SupportedOSPlatform("linux")]
public class LinuxService : BaseService, IMonitorService
{
    private readonly CancellationTokenSource _cts;
    private readonly Stopwatch _stopWatch;
    private MonitorModel _model;
    private LinuxStatusModel? _prevStatus;

    public LinuxService()
    {
        this._model = base.CreateModel();
        this._cts = new CancellationTokenSource();
        this._stopWatch = new Stopwatch();
        Task.Run(async () =>
        {
            while (!this._cts.IsCancellationRequested)
            {
                if (!this._stopWatch.IsRunning)
                {
                    this._stopWatch.Start();
                    this._prevStatus = this.DoWorkInternal();
                }
                else
                {
                    this._stopWatch.Stop();
                    this.DoWork();
                    this._stopWatch.Reset();
                }
                await Task.Delay(1000 * 1).ConfigureAwait(false);
            }
        }, this._cts.Token);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        this._cts.Cancel();
    }

    public MonitorModel GetStatus()
    {
        return this._model;
    }

    private void DoWork()
    {
        if (this._prevStatus != null)
        {
            this._model = base.CreateModel();
            var status = this.DoWorkInternal();
            var microseconds = this._stopWatch.Elapsed.TotalMicroseconds;
            //整体CPU占用率
            this._model.CpuUsage = 1 - 1.0 * (status.CpuIdle - this._prevStatus.CpuIdle) / (status.CpuTotal - this._prevStatus.CpuTotal);
            //进程CPU占用率：计算采用的时间间隔和系统的CPU占用率计算一致
            this._model.ProcessCpuUsage = 1.0 * (status.ProcessCpuUsed - this._prevStatus.ProcessCpuUsed) / (status.CpuTotal - this._prevStatus.CpuTotal);
            //整体网络
            this._model.NetReceived = (float)(1.0f * (status.NetReceived - this._prevStatus.NetReceived) / (microseconds / 1_000_000));
            this._model.NetSent = (float)(1.0f * (status.NetSent - this._prevStatus.NetSent) / (microseconds / 1_000_000));
            //整体磁盘读写
            this._model.DiskRead = (float)(1.0f * (status.DiskRead - this._prevStatus.DiskRead) / (microseconds / 1_000_000));
            this._model.DiskWrite = (float)(1.0f * (status.DiskWrite - this._prevStatus.DiskWrite) / (microseconds / 1_000_000));
            //进程磁盘读写速率
            this._model.ProcessDiskRead = (float)(1.0f * (status.ProcessDiskRead - this._prevStatus.ProcessDiskRead) / (microseconds / 1_000_000));
            this._model.ProcessDiskWrite = (float)(1.0f * (status.ProcessDiskWrite - this._prevStatus.ProcessDiskWrite) / (microseconds / 1_000_000));
            //
            this._prevStatus = status;
        }
    }

    protected virtual LinuxStatusModel DoWorkInternal()
    {
        var status = new LinuxStatusModel();
        var microseconds = this._stopWatch.Elapsed.TotalMicroseconds;
        // cpu
        var procStat = File.ReadAllLines("/proc/stat");
        var statValues = procStat.First().ToValues()
            .Skip(1)
            .Select(o => o.ToInt())
            .ToArray();
        status.CpuTotal = statValues.Sum();
        status.CpuIdle = statValues[3];
        // process cpu
        var processProcStat = File.ReadAllLines($"/proc/{base.CurrentProcess.Id}/stat");
        var processStatValues = processProcStat.First().ToValues()
            .Skip(13)
            .Take(9)
            .Select(o => o.ToInt())
            .ToArray();
        status.ProcessCpuUsed = processStatValues.Take(2).Sum();
        // memory
        var procMeminfo = File.ReadAllLines("/proc/meminfo")
             .Take(2)
             .Select(o => o.ToValues())
             .Where(o => o.Length == 3)
             .Select(o => new KeyValuePair<string, long>(o[0], o[1].ToLong()))
             .ToDictionary(o => o.Key, o => o.Value);
        this._model.TotalMemory = procMeminfo["MemTotal:"] * 1024;
        this._model.MemoryUsage = 1 - 1.0 * procMeminfo["MemFree:"] * 1024 / this._model.TotalMemory;
        // network
        var procNetDev = File.ReadAllLines("/proc/net/dev").Skip(2)
            .Select(o => o.ToValues())
            .Where(o => o[0] != "lo")
            .Select(o => new { Receive = o[1].ToLong(), Transmit = o[9].ToLong() })
            .ToList();
        var inBytes = procNetDev.Sum(o => o.Receive);
        var outBytes = procNetDev.Sum(o => o.Transmit);
        status.NetReceived = inBytes;
        status.NetSent = outBytes;
        // thread
        this._model.ThreadCount = Process.GetProcesses().Select(o => o.Id)
            .ToArray()
            .Select(pid => File.ReadAllText($"/proc/{pid}/stat").ToValues()[19].ToInt())
            .Sum();
        // disk
        // https://www.kernel.org/doc/Documentation/iostats.txt
        // https://www.kernel.org/doc/Documentation/block/stat.txt
        var diskValues = File.ReadLines("/proc/diskstats").Select(o => o.ToValues())
            .Where(o => o.Length >= 3 && o[2].StartsWith("sd"))
            .Select(o => o[2].Trim())
            .ToList()
            .Select(o => File.ReadAllText($"/sys/block/{o}/stat").ToValues())
            .Select(o => new { ReadBytes = o[2].ToLong() * 512, WriteBytes = o[6].ToLong() * 512 })
            .ToList();
        status.DiskRead = diskValues.Sum(o => o.ReadBytes);
        status.DiskWrite = diskValues.Sum(o => o.WriteBytes);
        //
        var procPidIO = File.ReadLines($"/proc/{base.CurrentProcess.Id}/io")
            .Select(o => o.ToValues())
            //.Skip(4)
            .Select(o => new KeyValuePair<string, long>(o[0], o[1].ToLong()))
            .ToDictionary(o => o.Key, o => o.Value);
        status.ProcessDiskRead = procPidIO["rchar:"];
        status.ProcessDiskWrite = procPidIO["wchar:"];
        //
        return status;
    }
}

public class LinuxStatusModel
{
    public int CpuIdle { get; set; }
    public int CpuTotal { get; set; }
    public long NetReceived { get; set; }
    public long NetSent { get; set; }
    public int ProcessCpuUsed { get; set; }
    public long DiskRead { get; set; }
    public long DiskWrite { get; set; }
    public long ProcessDiskRead { get; set; }
    public long ProcessDiskWrite { get; set; }
}
