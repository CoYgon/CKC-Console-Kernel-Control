using System.Text;
using CKC.Core;
using CKC.Models;
using CKC.Services;

namespace CKC.Commands;

public static class MemoryCommands
{
    public static string ExecuteSysMem(string[] args)
    {
        try
        {
            var mem = MemoryManager.GetMemoryStatus();
            var sb = new StringBuilder();

            sb.AppendLine("╔══════════════════════════════════════════════════╗");
            sb.AppendLine("║            MEMORY STATUS                        ║");
            sb.AppendLine("╚══════════════════════════════════════════════════╝");
            sb.AppendLine();

            sb.Append("  Physical Memory: ");
            sb.AppendLine(mem.ToProgressBar(40));
            sb.AppendLine();

            sb.AppendLine($"  {"Memory Load",-25}: {mem.MemoryLoad}%");
            sb.AppendLine($"  {"Total Physical",-25}: {ConsoleFormatter.FormatBytes(mem.TotalPhysicalMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Available Physical",-25}: {ConsoleFormatter.FormatBytes(mem.AvailablePhysicalMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Used Physical",-25}: {ConsoleFormatter.FormatBytes(mem.UsedPhysicalMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Usage",-25}: {mem.PhysicalUsagePercent:F1}%");
            sb.AppendLine();

            sb.AppendLine($"  {"Total Page File",-25}: {ConsoleFormatter.FormatBytes(mem.TotalPageFileMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Available Page File",-25}: {ConsoleFormatter.FormatBytes(mem.AvailablePageFileMB * 1024 * 1024)}");
            sb.AppendLine();

            sb.AppendLine($"  {"Total Virtual",-25}: {ConsoleFormatter.FormatBytes(mem.TotalVirtualMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Available Virtual",-25}: {ConsoleFormatter.FormatBytes(mem.AvailableVirtualMB * 1024 * 1024)}");
            sb.AppendLine();

            sb.AppendLine($"  {"System Cache",-25}: {ConsoleFormatter.FormatBytes(mem.SystemCacheMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Paged Pool",-25}: {ConsoleFormatter.FormatBytes(mem.TotalPagedPoolMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Non-Paged Pool",-25}: {ConsoleFormatter.FormatBytes(mem.TotalNonPagedPoolMB * 1024 * 1024)}");
            sb.AppendLine();

            sb.AppendLine($"  {"Commit Total",-25}: {ConsoleFormatter.FormatBytes(mem.CommitTotalMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Commit Limit",-25}: {ConsoleFormatter.FormatBytes(mem.CommitLimitMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Commit Peak",-25}: {ConsoleFormatter.FormatBytes(mem.CommitPeakMB * 1024 * 1024)}");
            sb.AppendLine();

            sb.AppendLine($"  {"Handle Count",-25}: {mem.HandleCount:N0}");
            sb.AppendLine($"  {"Process Count",-25}: {mem.ProcessCount}");
            sb.AppendLine($"  {"Thread Count",-25}: {mem.ThreadCount}");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving memory status: {ex.Message}";
        }
    }

    public static string ExecuteVMMap(string[] args)
    {
        try
        {
            var layout = MemoryManager.GetVirtualMemoryLayout();
            return layout;
        }
        catch (Exception ex)
        {
            return $"Error retrieving virtual memory layout: {ex.Message}";
        }
    }

    public static string ExecutePool(string[] args)
    {
        try
        {
            var mem = MemoryManager.GetMemoryStatus();
            var sb = new StringBuilder();

            sb.AppendLine("Kernel Pool Memory Information");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"  {"Paged Pool (Total)",-25}: {ConsoleFormatter.FormatBytes(mem.TotalPagedPoolMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Non-Paged Pool (Total)",-25}: {ConsoleFormatter.FormatBytes(mem.TotalNonPagedPoolMB * 1024 * 1024)}");
            sb.AppendLine($"  {"System Cache",-25}: {ConsoleFormatter.FormatBytes(mem.SystemCacheMB * 1024 * 1024)}");
            sb.AppendLine();
            sb.AppendLine($"  {"Commit Total",-25}: {ConsoleFormatter.FormatBytes(mem.CommitTotalMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Commit Limit",-25}: {ConsoleFormatter.FormatBytes(mem.CommitLimitMB * 1024 * 1024)}");
            sb.AppendLine($"  {"Commit Peak",-25}: {ConsoleFormatter.FormatBytes(mem.CommitPeakMB * 1024 * 1024)}");
            sb.AppendLine();
            sb.AppendLine($"  {"Handles",-25}: {mem.HandleCount:N0}");
            sb.AppendLine($"  {"Processes",-25}: {mem.ProcessCount}");
            sb.AppendLine($"  {"Threads",-25}: {mem.ThreadCount}");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving pool information: {ex.Message}";
        }
    }
}
