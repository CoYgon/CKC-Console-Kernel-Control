namespace CKC.Models;

public class NetworkConnection
{
    public uint ProcessId { get; set; }
    public string ProcessName { get; set; } = "N/A";
    public string Protocol { get; set; } = "N/A";
    public string LocalAddress { get; set; } = "N/A";
    public int LocalPort { get; set; }
    public string RemoteAddress { get; set; } = "N/A";
    public int RemotePort { get; set; }
    public string State { get; set; } = "N/A";
    public string StateCode { get; set; } = "N/A";

    public string ToShortString()
    {
        if (Protocol == "TCP")
        {
            var local = $"{LocalAddress}:{LocalPort}";
            var remote = $"{RemoteAddress}:{RemotePort}";
            return $"{ProcessName,-20} {Protocol,-4} {local,-22} {remote,-22} {State,-12}";
        }
        else
        {
            var local = $"{LocalAddress}:{LocalPort}";
            return $"{ProcessName,-20} {Protocol,-4} {local,-22} {"*:*",-22} {"-",-12}";
        }
    }

    public override string ToString()
    {
        return $"[{ProcessId}] {ProcessName} - {Protocol} {LocalAddress}:{LocalPort} -> {RemoteAddress}:{RemotePort} ({State})";
    }
}
