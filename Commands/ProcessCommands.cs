using System.Linq;
using System.Text;
using CKC.Core;
using CKC.Models;
using CKC.Services;

namespace CKC.Commands;

public static class ProcessCommands
{
    public static string ExecutePs(string[] args)
    {
        try
        {
            var processes = ProcessManager.GetAllProcesses();
            if (processes.Count == 0)
                return "No processes found.";

            bool treeMode = args.Any(a => a.Equals("tree", StringComparison.OrdinalIgnoreCase));

            if (uint.TryParse(args.FirstOrDefault() ?? "", out uint pid))
            {
                var proc = processes.FirstOrDefault(p => p.ProcessId == pid);
                if (proc == null)
                    return $"Process with PID {pid} not found.";
                return proc.ToDetailedString();
            }

            string sortBy = "pid";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("sort", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    sortBy = args[i + 1].ToLowerInvariant();
                    break;
                }
            }

            var sorted = sortBy switch
            {
                "cpu" => processes.OrderByDescending(p => p.CpuTime).ToList(),
                "memory" or "mem" => processes.OrderByDescending(p => p.WorkingSetMB).ToList(),
                "name" => processes.OrderBy(p => p.ProcessName).ToList(),
                _ => processes.OrderBy(p => p.ProcessId).ToList()
            };

            var sb = new StringBuilder();

            if (treeMode)
            {
                sb.AppendLine("Process Tree");
                sb.AppendLine(new string('-', 80));
                var roots = sorted.Where(p => p.ParentProcessId == 0 || !sorted.Any(sp => sp.ProcessId == p.ParentProcessId)).ToList();
                foreach (var root in roots)
                {
                    PrintProcessTree(sb, root, sorted, "", true);
                }
            }
            else
            {
                sb.AppendFormat("{0,-7} {1,-25} {2,-8} {3,-8} {4,-12} {5,-14} {6,-10}\n", "PID", "Name", "Threads", "Handles", "Working Set", "CPU Time", "State");
                sb.AppendLine(new string('-', 84));

                foreach (var p in sorted)
                {
                    sb.AppendLine($"{p.ProcessId,-7} {Truncate(p.ProcessName, 24),-25} {p.ThreadCount,-8} {p.HandleCount,-8} {ConsoleFormatter.FormatBytes(p.WorkingSetMB * 1024 * 1024),-12} {p.CpuTime,-14} {p.State,-10}");
                }

                sb.AppendLine();
                sb.AppendLine($"Total processes: {sorted.Count}");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing processes: {ex.Message}";
        }
    }

    private static void PrintProcessTree(StringBuilder sb, ProcessInfo proc, List<ProcessInfo> all, string indent, bool last)
    {
        string marker = last ? "└── " : "├── ";
        string ws = proc.WorkingSetMB >= 1024 ? $"{proc.WorkingSetMB / 1024.0:F1} GB" : $"{proc.WorkingSetMB} MB";
        sb.AppendLine($"{indent}{marker}[{proc.ProcessId}] {proc.ProcessName} ({ws})");

        string childIndent = indent + (last ? "    " : "│   ");
        var children = all.Where(p => p.ParentProcessId == proc.ProcessId).ToList();
        for (int i = 0; i < children.Count; i++)
        {
            PrintProcessTree(sb, children[i], all, childIndent, i == children.Count - 1);
        }
    }

    public static string ExecuteKill(string[] args)
    {
        if (args.Length == 0 || !uint.TryParse(args[0], out uint pid))
            return "Usage: kill <pid>";

        try
        {
            if (ProcessManager.KillProcess(pid))
                return $"Process {pid} terminated successfully.";
            return $"Failed to terminate process {pid}. Access denied or process not found.";
        }
        catch (Exception ex)
        {
            return $"Error killing process {pid}: {ex.Message}";
        }
    }

    public static string ExecuteSuspend(string[] args)
    {
        if (args.Length == 0 || !uint.TryParse(args[0], out uint pid))
            return "Usage: suspend <pid>";

        try
        {
            if (ProcessManager.SuspendProcess(pid))
                return $"Process {pid} suspended successfully.";
            return $"Failed to suspend process {pid}. Access denied or process not found.";
        }
        catch (Exception ex)
        {
            return $"Error suspending process {pid}: {ex.Message}";
        }
    }

    public static string ExecuteResume(string[] args)
    {
        if (args.Length == 0 || !uint.TryParse(args[0], out uint pid))
            return "Usage: resume <pid>";

        try
        {
            if (ProcessManager.ResumeProcess(pid))
                return $"Process {pid} resumed successfully.";
            return $"Failed to resume process {pid}. Access denied or process not found.";
        }
        catch (Exception ex)
        {
            return $"Error resuming process {pid}: {ex.Message}";
        }
    }

    public static string ExecuteProcInfo(string[] args)
    {
        if (args.Length == 0)
            return "Usage: procinfo <pid|name>";

        try
        {
            ProcessInfo? proc = null;

            if (uint.TryParse(args[0], out uint pid))
            {
                proc = ProcessManager.GetProcessById(pid);
            }
            else
            {
                var processes = ProcessManager.GetAllProcesses();
                proc = processes.FirstOrDefault(p =>
                    p.ProcessName.Equals(args[0], StringComparison.OrdinalIgnoreCase) ||
                    (p.ExecutablePath != null && p.ExecutablePath.IndexOf(args[0], StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (proc == null)
                return $"Process '{args[0]}' not found.";

            return proc.ToDetailedString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving process info: {ex.Message}";
        }
    }

    public static string ExecuteThreads(string[] args)
    {
        if (args.Length == 0 || !uint.TryParse(args[0], out uint pid))
            return "Usage: threads <pid>";

        try
        {
            var threads = ProcessManager.GetProcessThreads(pid);
            if (threads.Count == 0)
                return $"No threads found for process {pid}.";

            var sb = new StringBuilder();
            sb.AppendLine($"Threads for PID {pid}");
            sb.AppendLine(new string('-', 70));
            sb.AppendFormat("{0,-8} {1,-9} {2,-9} {3,-12} {4,-18}\n", "TID", "Base Pri", "Curr Pri", "State", "Start Address");
            sb.AppendLine(new string('-', 70));

            foreach (var t in threads)
            {
                sb.AppendLine($"{t.ThreadId,-8} {t.BasePriority,-9} {t.CurrentPriority,-9} {t.StateStr,-12} 0x{t.StartAddress:X16}");
            }

            sb.AppendLine();
            sb.AppendLine($"Total threads: {threads.Count}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving threads: {ex.Message}";
        }
    }

    public static string ExecuteModules(string[] args)
    {
        if (args.Length == 0 || !uint.TryParse(args[0], out uint pid))
            return "Usage: modules <pid>";

        try
        {
            var modules = ProcessManager.GetProcessModules(pid);
            if (modules.Count == 0)
                return $"No modules found for process {pid}.";

            var sb = new StringBuilder();
            sb.AppendLine($"Modules for PID {pid}");
            sb.AppendLine(new string('-', 100));
            sb.AppendFormat("{0,-30} {1,-18} {2,-12} {3,-12}\n", "Module", "Base Address", "Size", "Load Count");
            sb.AppendLine(new string('-', 100));

            foreach (var m in modules.OrderBy(m => m.ImageBase))
            {
                sb.AppendLine($"{Truncate(m.Name, 29),-30} 0x{m.ImageBase:X16} {ConsoleFormatter.FormatBytes(m.ImageSize),-12} {m.LoadCountStr,-12}");
            }

            sb.AppendLine();
            sb.AppendLine($"Total modules: {modules.Count}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving modules: {ex.Message}";
        }
    }

    public static string ExecuteHandles(string[] args)
    {
        if (args.Length == 0 || !uint.TryParse(args[0], out uint pid))
            return "Usage: handles <pid>";

        try
        {
            var handles = ProcessManager.GetProcessHandles(pid);
            if (handles.Count == 0)
                return $"No handles found for process {pid}.";

            var grouped = handles.GroupBy(h => h.ObjectType)
                .OrderByDescending(g => g.Count())
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"Handles for PID {pid}");
            sb.AppendLine(new string('-', 70));
            sb.AppendFormat("{0,-10} {1,-16} {2,-12} {3}\n", "Handle", "Type", "Access", "Count");
            sb.AppendLine(new string('-', 70));

            foreach (var g in grouped)
            {
                foreach (var h in g.Take(10))
                {
                    sb.AppendLine($"0x{h.HandleValue:X4}    {h.ObjectType,-16} 0x{h.GrantedAccess:X8}");
                }
                if (g.Count() > 10)
                {
                    sb.AppendLine($"  ... and {g.Count() - 10} more handles of type {g.Key}");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"Total handles: {handles.Count} ({grouped.Count} types)");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving handles: {ex.Message}";
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }
}
