namespace CKC.Models;

public class MemoryStatus
{
    public uint MemoryLoad { get; set; }
    public ulong TotalPhysicalMB { get; set; }
    public ulong AvailablePhysicalMB { get; set; }
    public ulong UsedPhysicalMB { get; set; }
    public double PhysicalUsagePercent { get; set; }
    public ulong TotalPageFileMB { get; set; }
    public ulong AvailablePageFileMB { get; set; }
    public ulong TotalVirtualMB { get; set; }
    public ulong AvailableVirtualMB { get; set; }
    public ulong TotalPagedPoolMB { get; set; }
    public ulong TotalNonPagedPoolMB { get; set; }
    public ulong SystemCacheMB { get; set; }
    public ulong CommitTotalMB { get; set; }
    public ulong CommitLimitMB { get; set; }
    public ulong CommitPeakMB { get; set; }
    public uint HandleCount { get; set; }
    public uint ProcessCount { get; set; }
    public uint ThreadCount { get; set; }

    public override string ToString()
    {
        return $"Memory Load: {MemoryLoad}%\n" +
               $"Physical Memory: {UsedPhysicalMB} MB used / {TotalPhysicalMB} MB total ({PhysicalUsagePercent:F1}%)\n" +
               $"Available Physical: {AvailablePhysicalMB} MB\n" +
               $"Page File: {TotalPageFileMB} MB total, {AvailablePageFileMB} MB available\n" +
               $"Virtual Memory: {TotalVirtualMB} MB total, {AvailableVirtualMB} MB available\n" +
               $"System Cache: {SystemCacheMB} MB\n" +
               $"Commit: {CommitTotalMB} MB / {CommitLimitMB} MB (Peak: {CommitPeakMB} MB)\n" +
               $"Paged Pool: {TotalPagedPoolMB} MB\n" +
               $"Non-Paged Pool: {TotalNonPagedPoolMB} MB\n" +
               $"Handles: {HandleCount:N0} | Processes: {ProcessCount} | Threads: {ThreadCount}";
    }

    public string ToProgressBar(int width = 30)
    {
        var filled = (int)(PhysicalUsagePercent / 100.0 * width);
        var empty = width - filled;
        var bar = new string('█', filled) + new string('░', empty);
        return $"[{bar}] {PhysicalUsagePercent:F1}%";
    }
}
