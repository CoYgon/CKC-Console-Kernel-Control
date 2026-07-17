using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CKC.Core;
using CKC.Models;
using CKC.Native;

namespace CKC.Commands;

public static class WindowCommands
{
    public static string ExecuteWindows(string[] args)
    {
        try
        {
            var windows = new List<WindowInfo>();
            var callback = new User32.EnumWindowsProc((hWnd, lParam) =>
            {
                var sbTitle = new StringBuilder(256);
                User32.GetWindowText(hWnd, sbTitle, sbTitle.Capacity);
                string title = sbTitle.ToString();

                if (string.IsNullOrEmpty(title))
                    return true;

                User32.GetWindowThreadProcessId(hWnd, out uint pid);
                bool visible = User32.IsWindowVisible(hWnd);

                User32.GetWindowRect(hWnd, out RECT rect);

                string processName = "N/A";
                try
                {
                    using var proc = System.Diagnostics.Process.GetProcessById((int)pid);
                    processName = proc.ProcessName;
                }
                catch { }

                windows.Add(new WindowInfo
                {
                    Handle = hWnd,
                    Title = title,
                    ProcessId = pid,
                    ProcessName = processName,
                    Visible = visible,
                    Position = new WindowInfo.RECT
                    {
                        Left = rect.left,
                        Top = rect.top,
                        Right = rect.right,
                        Bottom = rect.bottom
                    }
                });

                return true;
            });

            User32.EnumWindows(callback, IntPtr.Zero);

            if (windows.Count == 0)
                return "No windows found.";

            var sb = new StringBuilder();
            sb.AppendLine("Top-Level Windows");
            sb.AppendLine(new string('-', 100));
            sb.AppendFormat("{0,-14} {1,-8} {2,-7} {3,-22} {4,-40}\n", "Handle", "Visible", "PID", "Process", "Title");
            sb.AppendLine(new string('-', 100));

            foreach (var w in windows.OrderBy(w => w.ProcessName))
            {
                string vis = w.Visible ? "Yes" : "No";
                string title = w.Title.Length > 38 ? w.Title[..35] + "..." : w.Title;
                sb.AppendLine($"0x{w.Handle.ToInt64():X8}  {vis,-8} {w.ProcessId,-7} {Truncate(w.ProcessName, 21),-22} {title,-40}");
            }

            sb.AppendLine();
            sb.AppendLine($"Total windows: {windows.Count}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error enumerating windows: {ex.Message}";
        }
    }

    public static string ExecuteWindowInfo(string[] args)
    {
        if (args.Length == 0)
            return "Usage: wininfo <handle_hex>";

        if (!long.TryParse(args[0].StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? args[0][2..] : args[0],
            System.Globalization.NumberStyles.HexNumber, null, out long handleRaw))
            return $"Invalid handle: '{args[0]}'. Use hex format, e.g., 0x1234 or 1234.";

        var hWnd = new IntPtr(handleRaw);

        try
        {
            if (!User32.IsWindow(hWnd))
                return $"Handle 0x{hWnd.ToInt64():X8} is not a valid window.";

            var sbTitle = new StringBuilder(256);
            User32.GetWindowText(hWnd, sbTitle, sbTitle.Capacity);
            string title = sbTitle.ToString();

            var sbClass = new StringBuilder(256);
            // GetClassName would be needed but not available in User32 - skip for now

            User32.GetWindowThreadProcessId(hWnd, out uint pid);
            bool visible = User32.IsWindowVisible(hWnd);
            User32.GetWindowRect(hWnd, out RECT rect);

            string processName = "N/A";
            try
            {
                using var proc = System.Diagnostics.Process.GetProcessById((int)pid);
                processName = proc.ProcessName;
            }
            catch { }

            var sb = new StringBuilder();
            sb.AppendLine($"Window Information: 0x{hWnd.ToInt64():X8}");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"  Handle         : 0x{hWnd.ToInt64():X8}");
            sb.AppendLine($"  Title          : {title}");
            sb.AppendLine($"  Visible        : {visible}");
            sb.AppendLine($"  Process ID     : {pid}");
            sb.AppendLine($"  Process Name   : {processName}");
            sb.AppendLine($"  Position       : ({rect.left},{rect.top})-({rect.right},{rect.bottom})");
            sb.AppendLine($"  Width          : {rect.right - rect.left}");
            sb.AppendLine($"  Height         : {rect.bottom - rect.top}");

            try
            {
                var placement = new WINDOWPLACEMENT();
                placement.length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>();
                if (User32.GetWindowPlacement(hWnd, ref placement))
                {
                    string showCmd = placement.showCmd switch
                    {
                        0 => "Hidden",
                        1 => "Normal",
                        2 => "Minimized",
                        3 => "Maximized",
                        _ => $"Unknown ({placement.showCmd})"
                    };
                    sb.AppendLine($"  Show State     : {showCmd}");
                }
            }
            catch { }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error getting window info: {ex.Message}";
        }
    }

    public static string ExecuteForeground(string[] args)
    {
        try
        {
            var hWnd = User32.GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
                return "No foreground window.";

            var sbTitle = new StringBuilder(256);
            User32.GetWindowText(hWnd, sbTitle, sbTitle.Capacity);
            string title = sbTitle.ToString();

            User32.GetWindowThreadProcessId(hWnd, out uint pid);
            bool visible = User32.IsWindowVisible(hWnd);
            User32.GetWindowRect(hWnd, out RECT rect);

            string processName = "N/A";
            try
            {
                using var proc = System.Diagnostics.Process.GetProcessById((int)pid);
                processName = proc.ProcessName;
            }
            catch { }

            var sb = new StringBuilder();
            sb.AppendLine("Foreground Window");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"  Handle         : 0x{hWnd.ToInt64():X8}");
            sb.AppendLine($"  Title          : {title}");
            sb.AppendLine($"  Process ID     : {pid}");
            sb.AppendLine($"  Process Name   : {processName}");
            sb.AppendLine($"  Visible        : {visible}");
            sb.AppendLine($"  Position       : ({rect.left},{rect.top})-({rect.right},{rect.bottom})");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error getting foreground window: {ex.Message}";
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }
}
