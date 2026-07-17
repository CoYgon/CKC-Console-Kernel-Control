using System.Linq;
using System.Text;
using CKC.Services;

namespace CKC.Commands;

public static class FileCommands
{
    public static string ExecuteHexDump(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            return "Usage: hexdump <filepath> [offset] [length]";

        string filePath = args[0].Trim('"');
        int offset = 0;
        int length = 1024;

        if (args.Length > 1 && !int.TryParse(args[1], out offset))
            return "Invalid offset. Must be a number.";
        if (args.Length > 2 && !int.TryParse(args[2], out length))
            return "Invalid length. Must be a number.";

        try
        {
            return FileSystemManager.HexDump(filePath, offset, length);
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }

    public static string ExecuteFileInfo(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            return "Usage: fileinfo <filepath>";

        string filePath = args[0].Trim('"');

        try
        {
            return FileSystemManager.GetFileInfo(filePath);
        }
        catch (Exception ex)
        {
            return $"Error getting file info: {ex.Message}";
        }
    }

    public static string ExecuteDiskInfo(string[] args)
    {
        try
        {
            var sb = new StringBuilder();

            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                string drive = args[0].Trim('"');
                return FileSystemManager.GetDiskInfo(drive);
            }

            sb.AppendLine("Disk/Drive Information");
            sb.AppendLine(new string('-', 90));

            foreach (var di in DriveInfo.GetDrives())
            {
                try
                {
                    string ready = di.IsReady ? "Ready" : "Not Ready";
                    sb.AppendLine($"  {di.Name,-5} {di.DriveType,-10} {"(" + di.DriveFormat + ")",-8} {di.VolumeLabel,-15} {ready,-12}");

                    if (di.IsReady)
                    {
                        ulong total = (ulong)di.TotalSize;
                        ulong free = (ulong)di.AvailableFreeSpace;
                        ulong used = total - free;
                        double percent = total > 0 ? (double)used / total * 100.0 : 0;

                        sb.AppendLine($"         Total: {FileSystemManager.FormatFileSize(total),-12} Used: {FileSystemManager.FormatFileSize(used),-12} Free: {FileSystemManager.FormatFileSize(free),-12} ({percent:F1}% used)");
                    }
                }
                catch { }
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error getting disk information: {ex.Message}";
        }
    }

    public static string ExecuteSearch(string[] args)
    {
        if (args.Length == 0)
            return "Usage: search <pattern> [directory]";

        string pattern = args[0];
        string directory = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();

        try
        {
            var results = FileSystemManager.SearchFiles(pattern, directory);
            if (results.Count == 0)
                return $"No files found matching '{pattern}' in '{directory}'.";

            var sb = new StringBuilder();
            sb.AppendLine($"Search results for '{pattern}' in '{directory}'");
            sb.AppendLine(new string('-', 80));
            sb.AppendFormat("{0,-60} {1,-12}\n", "File", "Size");
            sb.AppendLine(new string('-', 80));

            foreach (var (path, size) in results.OrderBy(r => r.path))
            {
                string name = path.Length > 58 ? "..." + path[^55..] : path;
                sb.AppendLine($"{name,-60} {FileSystemManager.FormatFileSize((ulong)size),-12}");
            }

            sb.AppendLine();
            sb.AppendLine($"Total files found: {results.Count}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error searching files: {ex.Message}";
        }
    }
}
