using System.Linq;
using System.Text;
using CKC.Core;
using CKC.Models;
using CKC.Services;

namespace CKC.Commands;

public static class NetworkCommands
{
    public static string ExecuteNetstat(string[] args)
    {
        try
        {
            var connections = NetworkManager.GetAllConnections();
            if (connections.Count == 0)
                return "No network connections found.";

            string? filterProtocol = null;
            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "-p" || args[i] == "--protocol") && i + 1 < args.Length)
                {
                    filterProtocol = args[i + 1].ToUpperInvariant();
                    break;
                }
            }

            if (filterProtocol != null)
            {
                connections = connections.Where(c =>
                    c.Protocol.Equals(filterProtocol, StringComparison.OrdinalIgnoreCase)).ToList();
                if (connections.Count == 0)
                    return $"No {filterProtocol} connections found.";
            }

            var sb = new StringBuilder();
            sb.AppendLine("Network Connections");
            sb.AppendLine(new string('-', 100));
            sb.AppendFormat("{0,-6} {1,-24} {2,-24} {3,-14} {4,-7} {5,-20}\n", "Proto", "Local Address", "Remote Address", "State", "PID", "Process");
            sb.AppendLine(new string('-', 100));

            var ordered = connections.OrderBy(c => c.ProcessName).ThenBy(c => c.LocalPort).ToList();

            foreach (var c in ordered)
            {
                string local = $"{c.LocalAddress}:{c.LocalPort}";
                string remote = c.Protocol == "UDP" ? "*:*" : $"{c.RemoteAddress}:{c.RemotePort}";
                sb.AppendLine($"{c.Protocol,-6} {local,-24} {remote,-24} {c.State,-14} {c.ProcessId,-7} {c.ProcessName,-20}");
            }

            sb.AppendLine();
            var protoCount = ordered.GroupBy(c => c.Protocol).Select(g => $"{g.Key}: {g.Count()}");
            sb.AppendLine($"Total connections: {ordered.Count} ({string.Join(", ", protoCount)})");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing network connections: {ex.Message}";
        }
    }

    public static string ExecuteConnections(string[] args)
    {
        if (args.Length == 0 || !uint.TryParse(args[0], out uint pid))
            return "Usage: connections <pid>";

        try
        {
            var connections = NetworkManager.GetProcessConnections(pid);
            if (connections.Count == 0)
                return $"No network connections found for PID {pid}.";

            var sb = new StringBuilder();
            sb.AppendLine($"Network Connections for PID {pid} ({connections[0].ProcessName})");
            sb.AppendLine(new string('-', 90));
            sb.AppendFormat("{0,-6} {1,-24} {2,-24} {3,-14}\n", "Proto", "Local Address", "Remote Address", "State");
            sb.AppendLine(new string('-', 90));

            foreach (var c in connections)
            {
                string local = $"{c.LocalAddress}:{c.LocalPort}";
                string remote = c.Protocol == "UDP" ? "*:*" : $"{c.RemoteAddress}:{c.RemotePort}";
                sb.AppendLine($"{c.Protocol,-6} {local,-24} {remote,-24} {c.State,-14}");
            }

            sb.AppendLine();
            sb.AppendLine($"Total connections: {connections.Count}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving connections for PID {pid}: {ex.Message}";
        }
    }
}
