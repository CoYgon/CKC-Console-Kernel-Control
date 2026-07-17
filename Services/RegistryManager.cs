using Microsoft.Win32;
using System.Text;

namespace CKC.Services;

public static class RegistryManager
{
    public static List<string> EnumerateKey(string path)
    {
        var subKeys = new List<string>();

        try
        {
            var (hive, subPath) = ParseRootKey(path);
            using var rootKey = OpenRootKey(hive);
            if (rootKey == null) return subKeys;

            using var key = string.IsNullOrEmpty(subPath) ? rootKey : rootKey.OpenSubKey(subPath);
            if (key == null) return subKeys;

            foreach (var name in key.GetSubKeyNames())
                subKeys.Add(name);
        }
        catch
        {
        }

        return subKeys;
    }

    public static Dictionary<string, object> GetKeyValues(string path)
    {
        var values = new Dictionary<string, object>();

        try
        {
            var (hive, subPath) = ParseRootKey(path);
            using var rootKey = OpenRootKey(hive);
            if (rootKey == null) return values;

            using var key = string.IsNullOrEmpty(subPath) ? rootKey : rootKey.OpenSubKey(subPath);
            if (key == null) return values;

            foreach (var name in key.GetValueNames())
            {
                try
                {
                    object? val = key.GetValue(name);
                    if (val is string[] strArray)
                        values[name] = string.Join(", ", strArray);
                    else if (val is byte[] byteArray)
                        values[name] = BitConverter.ToString(byteArray).Replace("-", " ");
                    else if (val is int intVal)
                        values[name] = $"0x{intVal:X8} ({intVal})";
                    else if (val is long longVal)
                        values[name] = $"0x{longVal:X16} ({longVal})";
                    else
                        values[name] = val?.ToString() ?? "(null)";
                }
                catch
                {
                    values[name] = "(error reading value)";
                }
            }
        }
        catch
        {
        }

        return values;
    }

    public static (RegistryHive hive, string subPath) ParseRootKey(string path)
    {
        if (string.IsNullOrEmpty(path))
            return (RegistryHive.LocalMachine, "");

        path = path.Trim();

        if (path.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("HKEY_LOCAL_MACHINE\\", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("HKLM", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("HKEY_LOCAL_MACHINE", StringComparison.OrdinalIgnoreCase))
        {
            int idx = path.IndexOf('\\');
            return (RegistryHive.LocalMachine, idx >= 0 ? path.Substring(idx + 1) : "");
        }

        if (path.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("HKEY_CURRENT_USER\\", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("HKCU", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase))
        {
            int idx = path.IndexOf('\\');
            return (RegistryHive.CurrentUser, idx >= 0 ? path.Substring(idx + 1) : "");
        }

        if (path.StartsWith("HKCR\\", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("HKEY_CLASSES_ROOT\\", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("HKCR", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("HKEY_CLASSES_ROOT", StringComparison.OrdinalIgnoreCase))
        {
            int idx = path.IndexOf('\\');
            return (RegistryHive.ClassesRoot, idx >= 0 ? path.Substring(idx + 1) : "");
        }

        if (path.StartsWith("HKU\\", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("HKEY_USERS\\", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("HKU", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("HKEY_USERS", StringComparison.OrdinalIgnoreCase))
        {
            int idx = path.IndexOf('\\');
            return (RegistryHive.Users, idx >= 0 ? path.Substring(idx + 1) : "");
        }

        if (path.StartsWith("HKCC\\", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("HKEY_CURRENT_CONFIG\\", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("HKCC", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("HKEY_CURRENT_CONFIG", StringComparison.OrdinalIgnoreCase))
        {
            int idx = path.IndexOf('\\');
            return (RegistryHive.CurrentConfig, idx >= 0 ? path.Substring(idx + 1) : "");
        }

        if (path.StartsWith("HKPD\\", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("HKEY_PERFORMANCE_DATA\\", StringComparison.OrdinalIgnoreCase))
        {
            int idx = path.IndexOf('\\');
            return (RegistryHive.PerformanceData, idx >= 0 ? path.Substring(idx + 1) : "");
        }

        return (RegistryHive.LocalMachine, path);
    }

    private static RegistryKey? OpenRootKey(RegistryHive hive)
    {
        try
        {
            return hive switch
            {
                RegistryHive.ClassesRoot => Registry.ClassesRoot,
                RegistryHive.CurrentUser => Registry.CurrentUser,
                RegistryHive.LocalMachine => Registry.LocalMachine,
                RegistryHive.Users => Registry.Users,
                RegistryHive.PerformanceData => Registry.PerformanceData,
                RegistryHive.CurrentConfig => Registry.CurrentConfig,
                _ => Registry.LocalMachine
            };
        }
        catch
        {
            return null;
        }
    }
}
