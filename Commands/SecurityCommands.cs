using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using CKC.Core;
using CKC.Native;
using CKC.Services;

namespace CKC.Commands;

public static class SecurityCommands
{
    public static string ExecuteWhoAmI(string[] args)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Current User Information");
            sb.AppendLine(new string('-', 60));

            using var identity = WindowsIdentity.GetCurrent();
            sb.AppendLine($"  {"User Name",-22}: {identity.Name}");
            sb.AppendLine($"  {"Authentication",-22}: {identity.AuthenticationType}");
            sb.AppendLine($"  {"Is Authenticated",-22}: {identity.IsAuthenticated}");
            sb.AppendLine($"  {"Is System",-22}: {identity.IsSystem}");
            sb.AppendLine($"  {"Is Guest",-22}: {identity.IsGuest}");

            try
            {
                string sidStr = identity.User?.Value ?? "N/A";
                sb.AppendLine($"  {"SID",-22}: {sidStr}");
            }
            catch { }

            bool elevated = ElevationHelper.IsElevated();
            string integrity = ElevationHelper.GetIntegrityLevel();
            sb.AppendLine($"  {"Elevated",-22}: {elevated}");
            sb.AppendLine($"  {"Integrity Level",-22}: {integrity}");

            try
            {
                var groups = identity.Groups;
                sb.AppendLine();
                sb.AppendLine("  Group Memberships:");
                foreach (var group in groups)
                {
                    try
                    {
                        var account = group.Translate(typeof(NTAccount));
                        sb.AppendLine($"    {account}");
                    }
                    catch
                    {
                        sb.AppendLine($"    {group.Value}");
                    }
                }
            }
            catch { }

            try
            {
                string? sidVal = identity.User?.Value;
                if (!string.IsNullOrEmpty(sidVal))
                    sb.AppendLine($"  {"SID (String)",-22}: {sidVal}");
            }
            catch { }

            sb.AppendLine();
            sb.AppendLine($"  {"Domain",-22}: {Environment.UserDomainName}");
            sb.AppendLine($"  {"Machine",-22}: {Environment.MachineName}");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving user information: {ex.Message}";
        }
    }

    public static string ExecutePrivileges(string[] args)
    {
        try
        {
            return KernelManager.GetAllPrivileges();
        }
        catch (Exception ex)
        {
            return $"Error listing privileges: {ex.Message}";
        }
    }

    public static string ExecuteElevate(string[] args)
    {
        try
        {
            if (ElevationHelper.IsElevated())
                return "Process is already elevated.";

            bool result = ElevationHelper.RequestElevation();
            if (result)
                return "Elevation requested. A new process will be started with administrator privileges.";
            return "Elevation failed. Try running this terminal as administrator manually.";
        }
        catch (Exception ex)
        {
            return $"Error during elevation: {ex.Message}";
        }
    }
}
