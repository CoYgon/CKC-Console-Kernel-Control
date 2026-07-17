using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CKC.Native;
using CKC.Models;

namespace CKC.Services;

public static class KernelManager
{
    public static List<KernelModule> GetKernelModules()
    {
        var modules = new List<KernelModule>();

        try
        {
            int bufferSize = 0x10000;

            while (true)
            {
                IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
                try
                {
                    int ret = NtDll.NtQuerySystemInformation(
                        SYSTEM_INFORMATION_CLASS.SystemModuleInformation,
                        buffer, bufferSize, out int returnLength);

                    if (ret == 0)
                    {
                        int moduleCount = Marshal.ReadInt32(buffer);
                        int moduleSize = Marshal.SizeOf<SYSTEM_MODULE>();
                        IntPtr modulePtr = buffer + IntPtr.Size;

                        for (int i = 0; i < moduleCount; i++)
                        {
                            try
                            {
                                var sm = Marshal.PtrToStructure<SYSTEM_MODULE>(modulePtr);
                                string imageName = sm.GetImageName();

                                int nameOffset = sm.ModuleNameOffset;
                                string name = "";
                                if (nameOffset < imageName.Length)
                                    name = imageName.Substring(nameOffset).TrimEnd('\0');
                                else
                                    name = System.IO.Path.GetFileName(imageName);

                                modules.Add(new KernelModule
                                {
                                    Name = name,
                                    FullPath = imageName.TrimEnd('\0'),
                                    ImageBase = (ulong)sm.ImageBase,
                                    ImageSize = sm.ImageSize,
                                    LoadOrderIndex = sm.LoadOrderIndex,
                                    InitOrderIndex = sm.InitOrderIndex,
                                    LoadCount = sm.LoadCount,
                                    LoadCountStr = sm.LoadCount == 0xFFFF ? "N/A" : sm.LoadCount.ToString()
                                });
                            }
                            catch
                            {
                            }

                            modulePtr += moduleSize;
                        }

                        break;
                    }

                    if (ret == unchecked((int)NtDll.STATUS_INFO_LENGTH_MISMATCH) ||
                        ret == unchecked((int)NtDll.STATUS_BUFFER_TOO_SMALL))
                    {
                        Marshal.FreeHGlobal(buffer);
                        bufferSize = returnLength;
                        continue;
                    }

                    break;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }
        catch
        {
        }

        return modules;
    }

    public static TimeSpan GetSystemUptime()
    {
        try
        {
            ulong tickCount = Kernel32.GetTickCount64();
            return TimeSpan.FromMilliseconds(tickCount);
        }
        catch
        {
            try
            {
                uint tickCount32 = Kernel32.GetTickCount();
                return TimeSpan.FromMilliseconds(tickCount32);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
    }

    public static (string idle, string kernel, string user) GetCpuTimes()
    {
        try
        {
            if (Kernel32.GetSystemTimes(out FILETIME idleTime, out FILETIME kernelTime, out FILETIME userTime))
            {
                long idle = ((long)idleTime.dwHighDateTime << 32) | (uint)idleTime.dwLowDateTime;
                long kernel = ((long)kernelTime.dwHighDateTime << 32) | (uint)kernelTime.dwLowDateTime;
                long user = ((long)userTime.dwHighDateTime << 32) | (uint)userTime.dwLowDateTime;

                return (
                    FormatDuration(idle),
                    FormatDuration(kernel),
                    FormatDuration(user)
                );
            }
        }
        catch
        {
        }

        return ("N/A", "N/A", "N/A");
    }

    public static SystemInfo GetSystemInformation()
    {
        var info = new SystemInfo();

        try
        {
            var sysInfo = new SYSTEM_INFO();
            Kernel32.GetSystemInfo(out sysInfo);

            info.PageSize = sysInfo.dwPageSize;
            info.ProcessorCount = sysInfo.dwNumberOfProcessors;
            info.ProcessorArchitecture = sysInfo.wProcessorArchitecture switch
            {
                0 => "x86",
                5 => "ARM",
                6 => "IA64",
                9 => "x64",
                12 => "ARM64",
                _ => $"Unknown (0x{sysInfo.wProcessorArchitecture:X})"
            };
            info.ProcessorType = sysInfo.dwProcessorType.ToString();
            info.ProcessorLevel = sysInfo.wProcessorLevel;
            info.ProcessorRevision = sysInfo.wProcessorRevision;
        }
        catch
        {
        }

        try
        {
            info.SystemUptime = GetSystemUptime().ToString(@"d\.hh\:mm\:ss");
            var bootTime = DateTime.Now - GetSystemUptime();
            info.BootTime = new DateTimeOffset(bootTime).ToUnixTimeSeconds();
            info.TickCount = Kernel32.GetTickCount64();
        }
        catch
        {
        }

        try
        {
            info.OSVersion = GetOsVersion();
        }
        catch
        {
        }

        try
        {
            var sb = new StringBuilder(260);
            uint size = (uint)sb.Capacity;
            if (Kernel32.GetComputerName(sb, ref size))
                info.ComputerName = sb.ToString();
        }
        catch
        {
        }

        try
        {
            var sb = new StringBuilder(260);
            uint size = (uint)sb.Capacity;
            if (Kernel32.GetUserName(sb, ref size))
                info.UserName = sb.ToString();
        }
        catch
        {
        }

        info.IsElevated = IsProcessElevated();

        return info;
    }

    public static string GetOsVersion()
    {
        try
        {
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
                return $"{edition} (Build {osvi.dwBuildNumber}){sp}";
            }
        }
        catch
        {
        }

        try
        {
            return Environment.OSVersion.ToString();
        }
        catch
        {
            return "N/A";
        }
    }

    public static bool IsProcessElevated()
    {
        try
        {
            IntPtr tokenHandle;
            if (!AdvApi32.OpenProcessToken(Kernel32.GetCurrentProcess(), AdvApi32.TOKEN_QUERY, out tokenHandle))
                return false;

            try
            {
                uint returnLength;
                AdvApi32.GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevation,
                    IntPtr.Zero, 0, out returnLength);

                IntPtr elevationPtr = Marshal.AllocHGlobal((int)returnLength);
                try
                {
                    if (AdvApi32.GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevation,
                        elevationPtr, returnLength, out _))
                    {
                        var elevation = Marshal.PtrToStructure<TOKEN_ELEVATION>(elevationPtr);
                        return elevation.TokenIsElevated != 0;
                    }
                    return false;
                }
                finally
                {
                    Marshal.FreeHGlobal(elevationPtr);
                }
            }
            finally
            {
                Kernel32.CloseHandle(tokenHandle);
            }
        }
        catch
        {
            return false;
        }
    }

    public static bool EnablePrivilege(string privilege)
    {
        try
        {
            IntPtr hToken;
            if (!AdvApi32.OpenProcessToken(Kernel32.GetCurrentProcess(),
                AdvApi32.TOKEN_QUERY | AdvApi32.TOKEN_ADJUST_PRIVILEGES, out hToken))
                return false;

            try
            {
                if (!AdvApi32.LookupPrivilegeValue(null, privilege, out LUID luid))
                    return false;

                var tp = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Privileges = new LUID_AND_ATTRIBUTES
                    {
                        Luid = luid,
                        Attributes = AdvApi32.SE_PRIVILEGE_ENABLED
                    }
                };

                return AdvApi32.AdjustTokenPrivileges(hToken, false, ref tp,
                    (uint)Marshal.SizeOf<TOKEN_PRIVILEGES>(), IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                Kernel32.CloseHandle(hToken);
            }
        }
        catch
        {
            return false;
        }
    }

    public static string GetAllPrivileges()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Mevcut Ayrıcalıklar:");
        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"{"Ayrıcalık Adı",-50} {"Durum",-10}");
        sb.AppendLine(new string('-', 80));

        var knownPrivileges = new (string name, string constant)[]
        {
            ("SeAssignPrimaryTokenPrivilege", "Birincil belirteç ata"),
            ("SeAuditPrivilege", "Güvenlik denetimi oluştur"),
            ("SeBackupPrivilege", "Dosya ve dizinleri yedekle"),
            ("SeChangeNotifyPrivilege", "Bildirimleri atla"),
            ("SeCreateGlobalPrivilege", "Global nesneler oluştur"),
            ("SeCreatePagefilePrivilege", "Sayfa dosyası oluştur"),
            ("SeCreatePermanentPrivilege", "Kalıcı nesneler oluştur"),
            ("SeCreateSymbolicLinkPrivilege", "Sembolik bağlantı oluştur"),
            ("SeCreateTokenPrivilege", "Belirteç oluştur"),
            ("SeDebugPrivilege", "Program hata ayıkla"),
            ("SeEnableDelegationPrivilege", "Temsil et"),
            ("SeImpersonatePrivilege", "İstemciyi taklit et"),
            ("SeIncreaseBasePriorityPrivilege", "Öncelik artır"),
            ("SeIncreaseQuotaPrivilege", "Kota artır"),
            ("SeIncreaseWorkingSetPrivilege", "Çalışma seti artır"),
            ("SeLoadDriverPrivilege", "Sürücü yükle/kaldır"),
            ("SeLockMemoryPrivilege", "Belleği kilitle"),
            ("SeMachineAccountPrivilege", "Makine hesabı ekle"),
            ("SeManageVolumePrivilege", "Birimi yönet"),
            ("SeProfileSingleProcessPrivilege", "Tek işlemi profille"),
            ("SeRelabelPrivilege", "Nesne etiketini değiştir"),
            ("SeRemoteShutdownPrivilege", "Uzaktan kapat"),
            ("SeRestorePrivilege", "Dosya ve dizinleri geri yükle"),
            ("SeSecurityPrivilege", "Güvenlik ayarlarını yönet"),
            ("SeShutdownPrivilege", "Sistemi kapat"),
            ("SeSyncAgentPrivilege", "Senkronizasyon aracısı"),
            ("SeSystemEnvironmentPrivilege", "Sistem ortamını değiştir"),
            ("SeSystemProfilePrivilege", "Sistem performansını profille"),
            ("SeSystemtimePrivilege", "Sistem saatini değiştir"),
            ("SeTakeOwnershipPrivilege", "Sahipliği al"),
            ("SeTcbPrivilege", "Güvenilir bilgi işlem tabanı"),
            ("SeTimeZonePrivilege", "Saat dilimini değiştir"),
            ("SeTrustedCredManAccessPrivilege", "Kimlik bilgisi yöneticisine eriş"),
            ("SeUndockPrivilege", "Bilgisayarı çıkar"),
            ("SeUnsolicitedInputPrivilege", "Bilgisayardan girdi al"),
        };

        try
        {
            IntPtr hToken;
            if (!AdvApi32.OpenProcessToken(Kernel32.GetCurrentProcess(), AdvApi32.TOKEN_QUERY, out hToken))
                return "Token alınamadı.";

            try
            {
                foreach (var (name, display) in knownPrivileges)
                {
                    string status = "Kontrol Ediliyor...";

                    try
                    {
                        if (AdvApi32.LookupPrivilegeValue(null, name, out LUID luid))
                        {
                            uint returnLength;
                            AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenPrivileges,
                                IntPtr.Zero, 0, out returnLength);

                            IntPtr privPtr = Marshal.AllocHGlobal((int)returnLength);
                            try
                            {
                                if (AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenPrivileges,
                                    privPtr, returnLength, out _))
                                {
                                    var privs = Marshal.PtrToStructure<TOKEN_PRIVILEGES>(privPtr);

                                    bool found = false;
                                    for (int i = 0; i < privs.PrivilegeCount; i++)
                                    {
                                        IntPtr entryPtr = privPtr + Marshal.SizeOf<TOKEN_PRIVILEGES>() +
                                            i * Marshal.SizeOf<LUID_AND_ATTRIBUTES>();

                                        var entry = Marshal.PtrToStructure<LUID_AND_ATTRIBUTES>(entryPtr);
                                        if (entry.Luid.LowPart == luid.LowPart && entry.Luid.HighPart == luid.HighPart)
                                        {
                                            status = (entry.Attributes & AdvApi32.SE_PRIVILEGE_ENABLED) != 0
                                                ? "Etkin" : "Devre Dışı";
                                            found = true;
                                            break;
                                        }
                                    }

                                    if (!found)
                                        status = "Yok";
                                }
                            }
                            finally
                            {
                                Marshal.FreeHGlobal(privPtr);
                            }
                        }
                        else
                        {
                            status = "Bilinmiyor";
                        }
                    }
                    catch
                    {
                        status = "Hata";
                    }

                    sb.AppendLine($"{name,-50} {status,-10}");
                }
            }
            finally
            {
                Kernel32.CloseHandle(hToken);
            }
        }
        catch
        {
            sb.AppendLine("Ayrıcalıklar sorgulanamadı.");
        }

        return sb.ToString();
    }

    private static string FormatDuration(long ticks)
    {
        long totalSeconds = ticks / 10000000;
        long days = totalSeconds / 86400;
        long hours = (totalSeconds % 86400) / 3600;
        long minutes = (totalSeconds % 3600) / 60;
        long secs = totalSeconds % 60;

        if (days > 0)
            return $"{days}d {hours:D2}:{minutes:D2}:{secs:D2}";
        return $"{hours:D2}:{minutes:D2}:{secs:D2}";
    }
}
