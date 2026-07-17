using System.Runtime.InteropServices;
using System.Text;
using CKC.Native;
using CKC.Core;
using CKC.Models;
using CKC.Services;

namespace CKC.Commands;

public static class SystemCommands
{
    public static string ExecuteSysInfo(string[] args)
    {
        try
        {
            var info = KernelManager.GetSystemInformation();
            var sb = new StringBuilder();
            sb.AppendLine("╔══════════════════════════════════════════════════╗");
            sb.AppendLine("║             SYSTEM INFORMATION                  ║");
            sb.AppendLine("╚══════════════════════════════════════════════════╝");
            sb.AppendLine();
            sb.AppendLine($"  {"Computer Name",-20}: {info.ComputerName}");
            sb.AppendLine($"  {"User Name",-20}: {info.UserName}");
            sb.AppendLine($"  {"OS Version",-20}: {info.OSVersion}");
            sb.AppendLine($"  {"OS Build",-20}: {info.OSBuildNumber}");
            sb.AppendLine($"  {"OS Architecture",-20}: {info.OSArchitecture}");
            sb.AppendLine($"  {"Service Pack",-20}: {info.ServicePack}");
            sb.AppendLine($"  {"Processors",-20}: {info.ProcessorCount}");
            sb.AppendLine($"  {"Processor Arch",-20}: {info.ProcessorArchitecture}");
            sb.AppendLine($"  {"Processor Level",-20}: {info.ProcessorLevel}");
            sb.AppendLine($"  {"Processor Revision",-20}: {info.ProcessorRevision}");
            sb.AppendLine($"  {"Page Size",-20}: {info.PageSize} bytes");
            sb.AppendLine($"  {"System Uptime",-20}: {info.SystemUptime}");
            sb.AppendLine($"  {"Boot Time",-20}: {DateTimeOffset.FromUnixTimeSeconds(info.BootTime).LocalDateTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"  {"Elevated",-20}: {info.IsElevated}");
            sb.AppendLine($"  {"System Model",-20}: {info.SystemManufacturer} {info.SystemModel}");
            sb.AppendLine($"  {"BIOS Version",-20}: {info.BiosVersion}");
            sb.AppendLine($"  {"BIOS Date",-20}: {info.BiosDate}");

            var (idle, kernel, user) = KernelManager.GetCpuTimes();
            sb.AppendLine($"  {"CPU Idle Time",-20}: {idle}");
            sb.AppendLine($"  {"CPU Kernel Time",-20}: {kernel}");
            sb.AppendLine($"  {"CPU User Time",-20}: {user}");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving system information: {ex.Message}";
        }
    }

    public static string ExecuteCpuInfo(string[] args)
    {
        try
        {
            var sysInfo = new SYSTEM_INFO();
            Kernel32.GetSystemInfo(out sysInfo);

            var sb = new StringBuilder();
            sb.AppendLine("CPU Information");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"  Number of Processors : {sysInfo.dwNumberOfProcessors}");
            sb.AppendLine($"  Processor Architecture: {sysInfo.wProcessorArchitecture switch
            {
                0 => "x86",
                5 => "ARM",
                6 => "IA64",
                9 => "x64",
                12 => "ARM64",
                _ => $"Unknown (0x{sysInfo.wProcessorArchitecture:X})"
            }}");
            sb.AppendLine($"  Processor Type       : {sysInfo.dwProcessorType}");
            sb.AppendLine($"  Processor Level      : {sysInfo.wProcessorLevel}");
            sb.AppendLine($"  Processor Revision   : 0x{sysInfo.wProcessorRevision:X4}");
            sb.AppendLine($"  Page Size            : {sysInfo.dwPageSize} bytes");
            sb.AppendLine($"  Min App Address      : 0x{(ulong)sysInfo.lpMinimumApplicationAddress:X16}");
            sb.AppendLine($"  Max App Address      : 0x{(ulong)sysInfo.lpMaximumApplicationAddress:X16}");
            sb.AppendLine($"  Allocation Granularity: {sysInfo.dwAllocationGranularity} bytes");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving CPU information: {ex.Message}";
        }
    }

    public static string ExecuteUptime(string[] args)
    {
        try
        {
            ulong tickMs = Kernel32.GetTickCount64();
            var uptime = TimeSpan.FromMilliseconds(tickMs);

            var sb = new StringBuilder();
            sb.AppendLine("System Uptime");
            sb.AppendLine(new string('-', 50));
            sb.AppendLine($"  Uptime    : {uptime.Days}d {uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}");
            sb.AppendLine($"  Total Days: {uptime.TotalDays:F2}");
            sb.AppendLine($"  Total Hours: {uptime.TotalHours:F2}");
            sb.AppendLine($"  Total Minutes: {uptime.TotalMinutes:F0}");
            sb.AppendLine($"  Total Seconds: {uptime.TotalSeconds:F0}");
            sb.AppendLine($"  Tick Count: {tickMs:N0} ms");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving uptime: {ex.Message}";
        }
    }

    public static string ExecuteBiosInfo(string[] args)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("BIOS Information");
            sb.AppendLine(new string('-', 50));

            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"HARDWARE\DESCRIPTION\System\BIOS");
                if (key != null)
                {
                    foreach (var name in key.GetValueNames())
                    {
                        var val = key.GetValue(name);
                        sb.AppendLine($"  {name,-30}: {val}");
                    }
                }
                else
                {
                    sb.AppendLine("  BIOS information not available.");
                }
            }
            catch
            {
                sb.AppendLine("  BIOS information not available (access denied or unsupported).");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving BIOS information: {ex.Message}";
        }
    }

    public static string ExecuteOsVersion(string[] args)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("OS Version Information");
            sb.AppendLine(new string('-', 50));

            var osvi = new RTL_OSVERSIONINFOEX();
            osvi.dwOSVersionInfoSize = (uint)Marshal.SizeOf<RTL_OSVERSIONINFOEX>();

            int ret = NtDll.RtlGetVersion(ref osvi);
            if (ret == 0)
            {
                string edition = osvi.dwMajorVersion switch
                {
                    10 when osvi.dwBuildNumber >= 22000 => "Windows 11",
                    10 => "Windows 10",
                    6 when osvi.dwMajorVersion == 6 && osvi.dwMinorVersion == 3 => "Windows 8.1",
                    6 when osvi.dwMajorVersion == 6 && osvi.dwMinorVersion == 2 => "Windows 8",
                    6 when osvi.dwMajorVersion == 6 && osvi.dwMinorVersion == 1 => "Windows 7",
                    6 when osvi.dwMajorVersion == 6 && osvi.dwMinorVersion == 0 => "Windows Vista",
                    5 when osvi.dwMinorVersion == 1 => "Windows XP",
                    _ => $"Windows {osvi.dwMajorVersion}.{osvi.dwMinorVersion}"
                };

                string sp = !string.IsNullOrEmpty(osvi.szCSDVersion) ? $" {osvi.szCSDVersion}" : "";
                sb.AppendLine($"  Edition        : {edition}{sp}");
                sb.AppendLine($"  Major Version  : {osvi.dwMajorVersion}");
                sb.AppendLine($"  Minor Version  : {osvi.dwMinorVersion}");
                sb.AppendLine($"  Build Number   : {osvi.dwBuildNumber}");
                sb.AppendLine($"  Platform ID    : {osvi.dwPlatformId}");
                sb.AppendLine($"  Service Pack   : {(string.IsNullOrEmpty(osvi.szCSDVersion) ? "None" : osvi.szCSDVersion)}");
                sb.AppendLine($"  Service Pack Maj: {osvi.wServicePackMajor}");
                sb.AppendLine($"  Service Pack Min: {osvi.wServicePackMinor}");
                sb.AppendLine($"  Suite Mask     : 0x{osvi.wSuiteMask:X4}");
                sb.AppendLine($"  Product Type   : {osvi.wProductType}");
            }
            else
            {
                sb.AppendLine($"  RtlGetVersion failed with status: 0x{ret:X8}");
                sb.AppendLine($"  Fallback: {Environment.OSVersion}");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error retrieving OS version: {ex.Message}";
        }
    }

    public static string ExecuteElevated(string[] args)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Elevation Status");
            sb.AppendLine(new string('-', 50));

            bool isElevated = ElevationHelper.IsElevated();
            string integrity = ElevationHelper.GetIntegrityLevel();

            sb.AppendLine($"  Elevated       : {isElevated}");
            sb.AppendLine($"  Integrity Level: {integrity}");

            IntPtr hToken = IntPtr.Zero;
            try
            {
                if (AdvApi32.OpenProcessToken(Kernel32.GetCurrentProcess(),
                    AdvApi32.TOKEN_QUERY, out hToken))
                {
                    uint returnLength = 0;
                    AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevationType,
                        IntPtr.Zero, 0, out returnLength);

                    IntPtr typePtr = Marshal.AllocHGlobal((int)returnLength);
                    try
                    {
                        if (AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevationType,
                            typePtr, returnLength, out _))
                        {
                            int type = Marshal.ReadInt32(typePtr);
                            string typeStr = type switch
                            {
                                1 => "Default",
                                2 => "Full",
                                3 => "Limited",
                                _ => $"Unknown ({type})"
                            };
                            sb.AppendLine($"  Elevation Type : {typeStr}");
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(typePtr);
                    }
                }
            }
            catch { }
            finally
            {
                if (hToken != IntPtr.Zero)
                    Kernel32.CloseHandle(hToken);
            }

            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                sb.AppendLine($"  User Name      : {identity.Name}");
                sb.AppendLine($"  Authentication : {identity.AuthenticationType}");
                sb.AppendLine($"  Is System      : {identity.IsSystem}");
                sb.AppendLine($"  Is Guest       : {identity.IsGuest}");
            }
            catch { }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error checking elevation status: {ex.Message}";
        }
    }
}
