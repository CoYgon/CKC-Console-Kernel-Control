namespace CKC.Models;

public class ProcessInfo
{
    public uint ProcessId { get; set; }
    public string ProcessName { get; set; } = "N/A";
    public string ExecutablePath { get; set; } = "N/A";
    public uint ParentProcessId { get; set; }
    public int BasePriority { get; set; }
    public uint ThreadCount { get; set; }
    public uint HandleCount { get; set; }
    public ulong WorkingSetMB { get; set; }
    public ulong PeakWorkingSetMB { get; set; }
    public ulong PrivateMemoryMB { get; set; }
    public ulong VirtualMemoryMB { get; set; }
    public ulong PagedMemoryMB { get; set; }
    public ulong PeakPagedMemoryMB { get; set; }
    public ulong PageFaults { get; set; }
    public long UserTime { get; set; }
    public long KernelTime { get; set; }
    public DateTime StartTime { get; set; }
    public string CpuTime { get; set; } = "N/A";
    public string State { get; set; } = "N/A";
    public string SessionName { get; set; } = "N/A";
    public uint SessionId { get; set; }
    public string PriorityClass { get; set; } = "N/A";
    public bool Is64Bit { get; set; }
    public string CommandLine { get; set; } = "N/A";
    public string UserName { get; set; } = "N/A";
    public string IntegrityLevel { get; set; } = "N/A";

    public override string ToString()
    {
        return $"[{ProcessId}] {ProcessName} - {State} - {WorkingSetMB} MB";
    }

    public string ToDetailedString()
    {
        return $"╔══════════════════════════════════════╗\n" +
               $"║      PROCESS INFORMATION             ║\n" +
               $"╚══════════════════════════════════════╝\n" +
               $"Process ID      : {ProcessId}\n" +
               $"Name            : {ProcessName}\n" +
               $"Executable      : {ExecutablePath}\n" +
               $"Parent PID      : {ParentProcessId}\n" +
               $"Priority        : {BasePriority} ({PriorityClass})\n" +
               $"Threads         : {ThreadCount}\n" +
               $"Handles         : {HandleCount}\n" +
               $"Working Set     : {WorkingSetMB} MB (Peak: {PeakWorkingSetMB} MB)\n" +
               $"Private Memory  : {PrivateMemoryMB} MB\n" +
               $"Virtual Memory  : {VirtualMemoryMB} MB\n" +
               $"Paged Memory    : {PagedMemoryMB} MB (Peak: {PeakPagedMemoryMB} MB)\n" +
               $"Page Faults     : {PageFaults:N0}\n" +
               $"CPU Time        : {CpuTime}\n" +
               $"Start Time      : {StartTime:yyyy-MM-dd HH:mm:ss}\n" +
               $"Session         : {SessionName} ({SessionId})\n" +
               $"State           : {State}\n" +
               $"64-bit          : {Is64Bit}\n" +
               $"Command Line    : {CommandLine}\n" +
               $"User            : {UserName}\n" +
               $"Integrity       : {IntegrityLevel}";
    }
}
