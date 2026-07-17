namespace CKC.Models;

public class SystemInfo
{
    public string ComputerName { get; set; } = "N/A";
    public string UserName { get; set; } = "N/A";
    public string OSVersion { get; set; } = "N/A";
    public string OSArchitecture { get; set; } = "N/A";
    public uint OSBuildNumber { get; set; }
    public uint OSMajorVersion { get; set; }
    public uint OSMinorVersion { get; set; }
    public string ServicePack { get; set; } = "N/A";
    public uint ProcessorCount { get; set; }
    public string ProcessorArchitecture { get; set; } = "N/A";
    public string ProcessorType { get; set; } = "N/A";
    public ushort ProcessorLevel { get; set; }
    public ushort ProcessorRevision { get; set; }
    public uint PageSize { get; set; }
    public ulong TickCount { get; set; }
    public string SystemUptime { get; set; } = "N/A";
    public long BootTime { get; set; }
    public bool IsElevated { get; set; }
    public string SystemManufacturer { get; set; } = "N/A";
    public string SystemModel { get; set; } = "N/A";
    public string BiosVersion { get; set; } = "N/A";
    public string BiosDate { get; set; } = "N/A";

    public override string ToString()
    {
        return $"Computer: {ComputerName}\n" +
               $"User: {UserName}\n" +
               $"OS: {OSVersion} (Build {OSBuildNumber})\n" +
               $"Architecture: {OSArchitecture}\n" +
               $"Service Pack: {ServicePack}\n" +
               $"Processors: {ProcessorCount}\n" +
               $"Processor Arch: {ProcessorArchitecture}\n" +
               $"Page Size: {PageSize} bytes\n" +
               $"Uptime: {SystemUptime}\n" +
               $"Elevated: {IsElevated}";
    }

    public string ToDetailedString()
    {
        return $"╔══════════════════════════════════════╗\n" +
               $"║        SYSTEM INFORMATION            ║\n" +
               $"╚══════════════════════════════════════╝\n\n" +
               $"Computer Name     : {ComputerName}\n" +
               $"User Name         : {UserName}\n" +
               $"OS Version        : {OSVersion}\n" +
               $"OS Build          : {OSBuildNumber}\n" +
               $"OS Major/Minor    : {OSMajorVersion}.{OSMinorVersion}\n" +
               $"Architecture      : {OSArchitecture}\n" +
               $"Service Pack      : {ServicePack}\n" +
               $"Processor Count   : {ProcessorCount}\n" +
               $"Processor Arch    : {ProcessorArchitecture}\n" +
               $"Processor Level   : {ProcessorLevel}\n" +
               $"Processor Rev     : {ProcessorRevision}\n" +
               $"Page Size         : {PageSize} bytes\n" +
               $"System Uptime     : {SystemUptime}\n" +
               $"Boot Time         : {BootTime}\n" +
               $"Tick Count        : {TickCount} ms\n" +
               $"Elevated Process  : {IsElevated}\n" +
               $"System Model      : {SystemManufacturer} {SystemModel}\n" +
               $"BIOS Version      : {BiosVersion}\n" +
               $"BIOS Date         : {BiosDate}";
    }
}
