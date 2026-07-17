using System.Text;

namespace CKC.Core;

public class CommandDispatcher
{
    private readonly Dictionary<string, (string description, Func<string[], string> handler)> _commands = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _aliases = new(StringComparer.OrdinalIgnoreCase);

    public CommandHistory History { get; } = new();

    public Dictionary<string, string> Aliases => _aliases;

    public void RegisterCommand(string name, string description, Func<string[], string> handler)
    {
        _commands[name] = (description, handler);
    }

    public string Dispatch(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        History.Add(input);

        try
        {
            string resolvedInput = ResolvePipeline(input);

            string[] parts = ParseCommandLine(resolvedInput);
            if (parts.Length == 0)
                return string.Empty;

            string commandName = parts[0].ToLowerInvariant();
            string[] args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            if (commandName == "help")
                return GetHelpText(args);

            if (_aliases.TryGetValue(commandName, out string? aliasTarget))
            {
                string[] aliasParts = ParseCommandLine(aliasTarget);
                commandName = aliasParts[0].ToLowerInvariant();
                args = aliasParts.Length > 1 ? aliasParts[1..].Concat(args).ToArray() : args;
            }

            if (_commands.TryGetValue(commandName, out var entry))
            {
                try
                {
                    return entry.handler(args);
                }
                catch (Exception ex)
                {
                    return $"Komut yürütülürken hata oluştu: {ex.Message}";
                }
            }

            return $"Bilinmeyen komut: '{commandName}'. Yardım için 'help' yazın.";
        }
        catch (Exception ex)
        {
            return $"Hata: {ex.Message}";
        }
    }

    public Dictionary<string, (string description, Func<string[], string> handler)> GetRegisteredCommands()
    {
        return new Dictionary<string, (string description, Func<string[], string> handler)>(_commands);
    }

    public void AddAlias(string alias, string command)
    {
        _aliases[alias] = command;
    }

    public void RemoveAlias(string alias)
    {
        _aliases.Remove(alias);
    }

    private string ResolvePipeline(string input)
    {
        return CommandPipeline.ExecutePipeline(input);
    }

    private string GetHelpText(string[] args)
    {
        var sb = new StringBuilder();

        if (args.Length > 0)
        {
            string topic = args[0].ToLowerInvariant();
            if (_commands.TryGetValue(topic, out var entry))
            {
                sb.AppendLine($"Komut: {topic}");
                sb.AppendLine($"Açıklama: {entry.description}");
                return sb.ToString();
            }

            if (_aliases.TryGetValue(topic, out string? aliasTarget))
            {
                sb.AppendLine($"Alias: {topic} -> {aliasTarget}");
                return sb.ToString();
            }

            return $"'{topic}' hakkında yardım bulunamadı.";
        }

        sb.AppendLine("Kullanılabilir komutlar:");
        sb.AppendLine(new string('-', 60));

        foreach (var kvp in _commands.OrderBy(c => c.Key))
        {
            sb.AppendLine($"  {kvp.Key,-20} {kvp.Value.description}");
        }

        if (_aliases.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Aliaslar:");
            sb.AppendLine(new string('-', 60));
            foreach (var alias in _aliases.OrderBy(a => a.Key))
            {
                sb.AppendLine($"  {alias.Key,-20} -> {alias.Value}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("İpuçları:");
        sb.AppendLine("  | ile komutları pipeline yapabilirsiniz (ör: command1 | command2)");
        sb.AppendLine("  ↑/↓ ile komut geçmişinde gezinebilirsiniz");
        sb.AppendLine("  help <komut> ile detaylı yardım alabilirsiniz");

        return sb.ToString();
    }

    private static string[] ParseCommandLine(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Array.Empty<string>();

        var args = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (current.Length > 0)
                {
                    args.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
            args.Add(current.ToString());

        return args.ToArray();
    }
}
