using System.Linq;
using System.Text;
using CKC.Models;
using CKC.Services;

namespace CKC.Commands;

public static class ServiceCommands
{
    public static string ExecuteServiceList(string[] args)
    {
        try
        {
            var services = ServiceManager.GetAllServices();
            if (services.Count == 0)
                return "No services found.";

            string? filter = null;
            if (args.Length > 0)
            {
                string arg = args[0].ToLowerInvariant();
                if (arg == "running")
                    filter = "running";
                else if (arg == "stopped")
                    filter = "stopped";
            }

            var filtered = filter switch
            {
                "running" => services.Where(s => s.IsRunning).ToList(),
                "stopped" => services.Where(s => !s.IsRunning).ToList(),
                _ => services
            };

            var sb = new StringBuilder();
            sb.AppendLine("Windows Services");
            sb.AppendLine(new string('-', 90));
            sb.AppendFormat("{0,-10} {1,-30} {2,-35} {3,-7} {4,-12}\n", "Status", "Name", "Display Name", "PID", "Start Type");
            sb.AppendLine(new string('-', 90));

            foreach (var s in filtered.OrderBy(s => s.ServiceName))
            {
                string status = s.IsRunning ? "● Running" : "○ Stopped";
                sb.AppendLine($"{status,-10} {Truncate(s.ServiceName, 29),-30} {Truncate(s.DisplayName, 34),-35} {s.ProcessId,-7} {s.StartType,-12}");
            }

            sb.AppendLine();
            int running = filtered.Count(s => s.IsRunning);
            int stopped = filtered.Count(s => !s.IsRunning);
            sb.AppendLine($"Total: {filtered.Count} (Running: {running}, Stopped: {stopped})");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing services: {ex.Message}";
        }
    }

    public static string ExecuteServiceInfo(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            return "Usage: serviceinfo <servicename>";

        try
        {
            var service = ServiceManager.GetServiceStatus(args[0]);
            if (service == null)
                return $"Service '{args[0]}' not found.";

            var sb = new StringBuilder();
            sb.AppendLine($"Service Information: {service.ServiceName}");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"  Service Name  : {service.ServiceName}");
            sb.AppendLine($"  Display Name  : {service.DisplayName}");
            sb.AppendLine($"  Description   : {service.Description}");
            sb.AppendLine($"  Status        : {service.Status}");
            sb.AppendLine($"  Start Type    : {service.StartType}");
            sb.AppendLine($"  Service Type  : {service.ServiceType}");
            sb.AppendLine($"  Binary Path   : {service.BinaryPath}");
            sb.AppendLine($"  Account       : {service.AccountName}");
            sb.AppendLine($"  Process ID    : {service.ProcessId}");
            sb.AppendLine($"  Dependencies  : {service.Dependencies}");
            sb.AppendLine($"  Error Control : {service.ErrorControl}");
            sb.AppendLine($"  Load Group    : {service.LoadOrderGroup}");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving service info: {ex.Message}";
        }
    }

    public static string ExecuteServiceStart(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            return "Usage: servicestart <servicename>";

        try
        {
            if (ServiceManager.StartService(args[0]))
                return $"Service '{args[0]}' started successfully.";
            return $"Failed to start service '{args[0]}'. Check permissions or service state.";
        }
        catch (Exception ex)
        {
            return $"Error starting service: {ex.Message}";
        }
    }

    public static string ExecuteServiceStop(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            return "Usage: servicestop <servicename>";

        try
        {
            if (ServiceManager.StopService(args[0]))
                return $"Service '{args[0]}' stopped successfully.";
            return $"Failed to stop service '{args[0]}'. Check permissions or service state.";
        }
        catch (Exception ex)
        {
            return $"Error stopping service: {ex.Message}";
        }
    }

    public static string ExecuteServiceRestart(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            return "Usage: servicerestart <servicename>";

        try
        {
            if (ServiceManager.RestartService(args[0]))
                return $"Service '{args[0]}' restarted successfully.";
            return $"Failed to restart service '{args[0]}'.";
        }
        catch (Exception ex)
        {
            return $"Error restarting service: {ex.Message}";
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }
}
