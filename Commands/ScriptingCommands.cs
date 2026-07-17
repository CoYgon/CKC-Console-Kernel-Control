using System.Linq;
using System.Text;
using CKC.Core;

namespace CKC.Commands;

public static class ScriptingCommands
{
    public static CommandDispatcher? Dispatcher { get; set; }

    public static string ExecuteHistory(string[] args)
    {
        if (Dispatcher == null)
            return "ScriptingCommands is not initialized with a CommandDispatcher reference.";

        try
        {
            if (args.Length > 0)
            {
                string cmd = args[0].ToLowerInvariant();

                if (cmd == "clear")
                {
                    Dispatcher.History.Clear();
                    return "Command history cleared.";
                }

                if (cmd == "save" && args.Length > 1)
                {
                    Dispatcher.History.SaveToFile(args[1]);
                    return $"Command history saved to '{args[1]}'.";
                }

                if (cmd == "load" && args.Length > 1)
                {
                    Dispatcher.History.LoadFromFile(args[1]);
                    return $"Command history loaded from '{args[1]}'.";
                }

                return "Usage: history [clear|save <file>|load <file>]";
            }

            var entries = Dispatcher.History.GetHistory();
            if (entries.Count == 0)
                return "Command history is empty.";

            var sb = new StringBuilder();
            sb.AppendLine("Command History");
            sb.AppendLine(new string('-', 60));

            int start = Math.Max(0, entries.Count - 100);
            for (int i = start; i < entries.Count; i++)
            {
                sb.AppendLine($"  {i + 1,-5} {entries[i]}");
            }

            sb.AppendLine();
            sb.AppendLine($"Total entries: {entries.Count} (showing last {entries.Count - start})");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error managing history: {ex.Message}";
        }
    }

    public static string ExecuteAlias(string[] args)
    {
        if (Dispatcher == null)
            return "ScriptingCommands is not initialized with a CommandDispatcher reference.";

        try
        {
            if (args.Length == 0)
            {
                var aliases = Dispatcher.Aliases;
                if (aliases.Count == 0)
                    return "No aliases defined.";

                var sb = new StringBuilder();
                sb.AppendLine("Defined Aliases");
                sb.AppendLine(new string('-', 60));
                sb.AppendFormat("{0,-20} {1,-40}\n", "Alias", "Command");
                sb.AppendLine(new string('-', 60));

                foreach (var kvp in aliases.OrderBy(a => a.Key))
                {
                    sb.AppendLine($"{kvp.Key,-20} {kvp.Value,-40}");
                }

                sb.AppendLine();
                sb.AppendLine($"Total aliases: {aliases.Count}");
                return sb.ToString();
            }

            string action = args[0].ToLowerInvariant();

            if (action == "set" && args.Length >= 3)
            {
                Dispatcher.AddAlias(args[1], args[2]);
                return $"Alias '{args[1]}' -> '{args[2]}' created.";
            }

            if (action == "del" && args.Length >= 2)
            {
                Dispatcher.RemoveAlias(args[1]);
                return $"Alias '{args[1]}' deleted.";
            }

            if (action == "set")
                return "Usage: alias set <name> <command>";

            if (action == "del")
                return "Usage: alias del <name>";

            return "Usage: alias [set <name> <command>|del <name>]";
        }
        catch (Exception ex)
        {
            return $"Error managing aliases: {ex.Message}";
        }
    }

    public static string ExecuteEcho(string[] args)
    {
        if (args.Length == 0)
            return "";

        return string.Join(" ", args);
    }
}
