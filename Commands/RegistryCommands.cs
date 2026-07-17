using System.Linq;
using System.Text;
using CKC.Services;

namespace CKC.Commands;

public static class RegistryCommands
{
    public static string ExecuteRegQuery(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            return "Usage: regquery <registrypath>\nExample: regquery HKLM\\SOFTWARE\\Microsoft";

        string path = args[0];

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Registry Query: {path}");
            sb.AppendLine(new string('-', 80));

            var subKeys = RegistryManager.EnumerateKey(path);
            var values = RegistryManager.GetKeyValues(path);

            if (values.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Values:");
                sb.AppendLine(new string('-', 80));
                sb.AppendFormat("{0,-40} {1,-40}\n", "Name", "Data");
                sb.AppendLine(new string('-', 80));

                foreach (var kvp in values)
                {
                    string name = string.IsNullOrEmpty(kvp.Key) ? "(Default)" : kvp.Key;
                    string data = kvp.Value?.ToString() ?? "(null)";
                    if (data.Length > 60) data = data[..57] + "...";
                    sb.AppendLine($"{Truncate(name, 39),-40} {data,-40}");
                }
            }

            if (subKeys.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Subkeys:");
                sb.AppendLine(new string('-', 80));

                foreach (var key in subKeys.OrderBy(k => k))
                {
                    sb.AppendLine($"  {key}");
                }
            }

            if (values.Count == 0 && subKeys.Count == 0)
            {
                sb.AppendLine("  (empty)");
            }

            sb.AppendLine();
            sb.AppendLine($"Subkeys: {subKeys.Count}, Values: {values.Count}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error querying registry: {ex.Message}";
        }
    }

    public static string ExecuteRegEnum(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            return "Usage: regenum <registrypath>\nExample: regenum HKLM\\SOFTWARE";

        string path = args[0];

        try
        {
            var subKeys = RegistryManager.EnumerateKey(path);
            if (subKeys.Count == 0)
                return $"No subkeys found under '{path}'.";

            var sb = new StringBuilder();
            sb.AppendLine($"Subkeys of '{path}'");
            sb.AppendLine(new string('-', 60));

            foreach (var key in subKeys.OrderBy(k => k))
            {
                sb.AppendLine($"  {key}");
            }

            sb.AppendLine();
            sb.AppendLine($"Total subkeys: {subKeys.Count}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error enumerating registry: {ex.Message}";
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }
}
