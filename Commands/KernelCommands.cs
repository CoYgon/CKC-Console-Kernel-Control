using System.Linq;
using System.Text;
using CKC.Core;
using CKC.Models;
using CKC.Services;

namespace CKC.Commands;

public static class KernelCommands
{
    public static string ExecuteKernelModules(string[] args)
    {
        try
        {
            var modules = KernelManager.GetKernelModules();
            if (modules.Count == 0)
                return "No kernel modules found. Try running as administrator.";

            var sb = new StringBuilder();
            sb.AppendLine("Loaded Kernel Modules");
            sb.AppendLine(new string('-', 100));
            sb.AppendFormat("{0,-35} {1,-18} {2,-12} {3,-12} {4,-12}\n", "Name", "Base Address", "Size", "Load Order", "Load Count");
            sb.AppendLine(new string('-', 100));

            foreach (var m in modules.OrderBy(m => m.LoadOrderIndex))
            {
                sb.AppendLine($"{Truncate(m.Name, 34),-35} 0x{m.ImageBase:X16} {ConsoleFormatter.FormatBytes(m.ImageSize),-12} {m.LoadOrderIndex,-12} {m.LoadCountStr,-12}");
            }

            sb.AppendLine();
            sb.AppendLine($"Total modules: {modules.Count}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing kernel modules: {ex.Message}";
        }
    }

    public static string ExecuteDriverInfo(string[] args)
    {
        try
        {
            var modules = KernelManager.GetKernelModules();
            if (modules.Count == 0)
                return "No driver information available. Try running as administrator.";

            var sb = new StringBuilder();

            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                var driver = modules.FirstOrDefault(m =>
                    m.Name.Contains(args[0], StringComparison.OrdinalIgnoreCase));
                if (driver == null)
                    return $"Driver '{args[0]}' not found.";

                sb.AppendLine($"Driver Information: {driver.Name}");
                sb.AppendLine(new string('-', 60));
                sb.AppendLine($"  Name         : {driver.Name}");
                sb.AppendLine($"  Full Path    : {driver.FullPath}");
                sb.AppendLine($"  Image Base   : 0x{driver.ImageBase:X16}");
                sb.AppendLine($"  Image Size   : {ConsoleFormatter.FormatBytes(driver.ImageSize)}");
                sb.AppendLine($"  Load Order   : {driver.LoadOrderIndex}");
                sb.AppendLine($"  Init Order   : {driver.InitOrderIndex}");
                sb.AppendLine($"  Load Count   : {driver.LoadCountStr}");
            }
            else
            {
                sb.AppendLine("Loaded Drivers");
                sb.AppendLine(new string('-', 100));
                sb.AppendFormat("{0,-35} {1,-12} {2,-12}\n", "Name", "Size", "Load Order");
                sb.AppendLine(new string('-', 100));

                foreach (var m in modules.OrderBy(m => m.LoadOrderIndex))
                {
                    sb.AppendLine($"{Truncate(m.Name, 34),-35} {ConsoleFormatter.FormatBytes(m.ImageSize),-12} {m.LoadOrderIndex,-12}");
                }

                sb.AppendLine();
                sb.AppendLine($"Total drivers: {modules.Count}");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving driver information: {ex.Message}";
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }
}
