namespace CKC.Models;

public class ServiceInfo
{
    public string ServiceName { get; set; } = "N/A";
    public string DisplayName { get; set; } = "N/A";
    public string Description { get; set; } = "N/A";
    public string Status { get; set; } = "N/A";
    public string StartType { get; set; } = "N/A";
    public string ServiceType { get; set; } = "N/A";
    public string BinaryPath { get; set; } = "N/A";
    public string AccountName { get; set; } = "N/A";
    public uint ProcessId { get; set; }
    public string Dependencies { get; set; } = "N/A";
    public string ErrorControl { get; set; } = "N/A";
    public string LoadOrderGroup { get; set; } = "N/A";
    public bool IsRunning => Status == "Running";

    public override string ToString()
    {
        var icon = IsRunning ? "●" : "○";
        return $"{icon} [{ServiceName}] {DisplayName} - {Status}";
    }

    public string ToDetailedString()
    {
        return $"Service Name    : {ServiceName}\n" +
               $"Display Name    : {DisplayName}\n" +
               $"Description     : {Description}\n" +
               $"Status          : {Status}\n" +
               $"Start Type      : {StartType}\n" +
               $"Service Type    : {ServiceType}\n" +
               $"Binary Path     : {BinaryPath}\n" +
               $"Account         : {AccountName}\n" +
               $"Process ID      : {ProcessId}\n" +
               $"Dependencies    : {Dependencies}\n" +
               $"Error Control   : {ErrorControl}\n" +
               $"Load Group      : {LoadOrderGroup}";
    }
}
