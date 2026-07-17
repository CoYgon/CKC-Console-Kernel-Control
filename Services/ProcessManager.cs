using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CKC.Native;
using CKC.Models;

namespace CKC.Services;

public static class ProcessManager
{
    private static readonly Dictionary<uint, string> ProcessNameCache = new();

    public static List<ProcessInfo> GetAllProcesses()
    {
        var processList = new List<ProcessInfo>();
        int arraySize = 1024 * 4;
        var processIds = new int[arraySize];

        if (!PsApi.EnumProcesses(processIds, arraySize, out int bytesReturned))
            return processList;

        int count = bytesReturned / sizeof(int);

        for (int i = 0; i < count; i++)
        {
            uint pid = (uint)processIds[i];
            if (pid == 0) continue;

            try
            {
                var info = GetProcessById(pid);
                if (info != null)
                    processList.Add(info);
            }
            catch
            {
            }
        }

        return processList;
    }

    public static ProcessInfo? GetProcessById(uint pid)
    {
        var info = new ProcessInfo { ProcessId = pid };

        IntPtr hProcess = IntPtr.Zero;
        try
        {
            hProcess = Kernel32.OpenProcess(
                ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VmRead,
                false, pid);

            if (hProcess == IntPtr.Zero)
            {
                hProcess = Kernel32.OpenProcess(
                    ProcessAccessFlags.QueryLimitedInformation,
                    false, pid);
                if (hProcess == IntPtr.Zero)
                    return GetBasicProcessInfo(pid);
            }

            IntPtr pbiPtr = Marshal.AllocHGlobal(Marshal.SizeOf<PROCESS_BASIC_INFORMATION>());
            try
            {
                int ret = NtDll.NtQueryInformationProcess(
                    hProcess, PROCESSINFOCLASS.ProcessBasicInformation,
                    pbiPtr, Marshal.SizeOf<PROCESS_BASIC_INFORMATION>(), out _);

                if (ret == 0)
                {
                    var pbi = Marshal.PtrToStructure<PROCESS_BASIC_INFORMATION>(pbiPtr);
                    info.ParentProcessId = (uint)pbi.InheritedFromUniqueProcessId;
                    info.BasePriority = (int)pbi.BasePriority;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pbiPtr);
            }

            IntPtr pmcPtr = Marshal.AllocHGlobal(Marshal.SizeOf<PROCESS_MEMORY_COUNTERS>());
            try
            {
                if (PsApi.GetProcessMemoryInfo(hProcess, pmcPtr, Marshal.SizeOf<PROCESS_MEMORY_COUNTERS>()))
                {
                    var pmc = Marshal.PtrToStructure<PROCESS_MEMORY_COUNTERS>(pmcPtr);
                    info.PageFaults = pmc.PageFaultCount;
                    info.WorkingSetMB = (ulong)pmc.WorkingSetSize / (1024 * 1024);
                    info.PeakWorkingSetMB = (ulong)pmc.PeakWorkingSetSize / (1024 * 1024);
                    info.PagedMemoryMB = (ulong)pmc.QuotaPagedPoolUsage / (1024 * 1024);
                    info.PeakPagedMemoryMB = (ulong)pmc.QuotaPeakPagedPoolUsage / (1024 * 1024);
                    info.PrivateMemoryMB = (ulong)pmc.PagefileUsage / (1024 * 1024);
                    info.VirtualMemoryMB = (ulong)pmc.PeakPagefileUsage / (1024 * 1024);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pmcPtr);
            }

            var sb = new StringBuilder(260);
            if (PsApi.GetProcessImageFileName(hProcess, sb, sb.Capacity) > 0)
            {
                info.ExecutablePath = sb.ToString();
                info.ProcessName = GetFileNameFromPath(sb.ToString());
            }

            GetTimesFromNtDll(hProcess, info);

            int handleCount = 0;
            IntPtr hCountPtr = Marshal.AllocHGlobal(sizeof(int));
            try
            {
                if (NtDll.NtQueryInformationProcess(hProcess, PROCESSINFOCLASS.ProcessHandleCount,
                    hCountPtr, sizeof(int), out _) == 0)
                {
                    handleCount = Marshal.ReadInt32(hCountPtr);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(hCountPtr);
            }
            info.HandleCount = (uint)handleCount;

            info.ThreadCount = (uint)Process.GetProcessById((int)pid).Threads.Count;
            info.State = "Running";

            try
            {
                var proc = Process.GetProcessById((int)pid);
                info.SessionId = (uint)proc.SessionId;
                info.PriorityClass = proc.PriorityClass.ToString();
                info.ProcessName = proc.ProcessName;
            }
            catch
            {
            }

            try
            {
                var sbName = new StringBuilder(260);
                var sbDomain = new StringBuilder(260);
                uint nameLen = (uint)sbName.Capacity;
                uint domainLen = (uint)sbDomain.Capacity;

                IntPtr tokenHandle;
                if (AdvApi32.OpenProcessToken(hProcess, AdvApi32.TOKEN_QUERY, out tokenHandle))
                {
                    try
                    {
                        uint returnLength;
                        AdvApi32.GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenUser,
                            IntPtr.Zero, 0, out returnLength);

                        IntPtr tokenUserPtr = Marshal.AllocHGlobal((int)returnLength);
                        try
                        {
                            if (AdvApi32.GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenUser,
                                tokenUserPtr, returnLength, out _))
                            {
                                var tokenUser = Marshal.PtrToStructure<TOKEN_USER>(tokenUserPtr);
                                int use = 0;
                                if (AdvApi32.LookupAccountSid(null, tokenUser.User.Sid,
                                    sbName, ref nameLen, sbDomain, ref domainLen, out use))
                                {
                                    info.UserName = $"{sbDomain}\\{sbName}";
                                }
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(tokenUserPtr);
                        }
                    }
                    finally
                    {
                        Kernel32.CloseHandle(tokenHandle);
                    }
                }
            }
            catch
            {
            }

            return info;
        }
        catch
        {
            return GetBasicProcessInfo(pid);
        }
        finally
        {
            if (hProcess != IntPtr.Zero)
                Kernel32.CloseHandle(hProcess);
        }
    }

    private static void GetTimesFromNtDll(IntPtr hProcess, ProcessInfo info)
    {
        try
        {
            int size = Marshal.SizeOf<SYSTEM_PROCESS_INFORMATION>() + 256;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                if (NtDll.NtQuerySystemInformation(
                    SYSTEM_INFORMATION_CLASS.SystemProcessInformation,
                    buffer, size, out _) == 0)
                {
                    IntPtr current = buffer;
                    while (true)
                    {
                        var spi = Marshal.PtrToStructure<SYSTEM_PROCESS_INFORMATION>(current);
                        if ((uint)spi.UniqueProcessId == info.ProcessId)
                        {
                            info.UserTime = spi.UserTime;
                            info.KernelTime = spi.KernelTime;
                            info.CpuTime = FormatCpuTime(spi.UserTime + spi.KernelTime);
                            info.StartTime = DateTime.FromFileTime(spi.CreateTime);

                            if (info.ThreadCount == 0)
                                info.ThreadCount = spi.NumberOfThreads;
                            if (info.HandleCount == 0)
                                info.HandleCount = spi.HandleCount;
                            break;
                        }
                        if (spi.NextEntryOffset == 0) break;
                        current += (int)spi.NextEntryOffset;
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        catch
        {
        }
    }

    private static ProcessInfo? GetBasicProcessInfo(uint pid)
    {
        try
        {
            var proc = Process.GetProcessById((int)pid);
            return new ProcessInfo
            {
                ProcessId = pid,
                ProcessName = proc.ProcessName,
                ThreadCount = (uint)proc.Threads.Count,
                HandleCount = (uint)proc.HandleCount,
                WorkingSetMB = (ulong)proc.WorkingSet64 / (1024 * 1024),
                State = "Running",
                SessionId = (uint)proc.SessionId,
                PriorityClass = proc.PriorityClass.ToString(),
                StartTime = proc.StartTime
            };
        }
        catch
        {
            return null;
        }
    }

    public static bool KillProcess(uint pid)
    {
        IntPtr hProcess = Kernel32.OpenProcess(ProcessAccessFlags.Terminate, false, pid);
        if (hProcess == IntPtr.Zero) return false;

        try
        {
            return Kernel32.TerminateProcess(hProcess, 1);
        }
        finally
        {
            Kernel32.CloseHandle(hProcess);
        }
    }

    public static bool SuspendProcess(uint pid)
    {
        IntPtr hSnapshot = Kernel32.CreateToolhelp32Snapshot(SnapshotFlags.Thread, pid);
        if (hSnapshot == (IntPtr)Kernel32.INVALID_HANDLE_VALUE) return false;

        try
        {
            var te = new THREADENTRY32();
            te.dwSize = (uint)Marshal.SizeOf<THREADENTRY32>();
            bool success = false;

            if (Kernel32.Thread32First(hSnapshot, ref te))
            {
                do
                {
                    if (te.th32OwnerProcessID == pid)
                    {
                        IntPtr hThread = Kernel32.OpenThread(ThreadAccessFlags.SuspendResume, false, te.th32ThreadID);
                        if (hThread != IntPtr.Zero)
                        {
                            Kernel32.SuspendThread(hThread, out _);
                            Kernel32.CloseHandle(hThread);
                            success = true;
                        }
                    }
                } while (Kernel32.Thread32Next(hSnapshot, ref te));
            }

            return success;
        }
        finally
        {
            Kernel32.CloseHandle(hSnapshot);
        }
    }

    public static bool ResumeProcess(uint pid)
    {
        IntPtr hSnapshot = Kernel32.CreateToolhelp32Snapshot(SnapshotFlags.Thread, pid);
        if (hSnapshot == (IntPtr)Kernel32.INVALID_HANDLE_VALUE) return false;

        try
        {
            var te = new THREADENTRY32();
            te.dwSize = (uint)Marshal.SizeOf<THREADENTRY32>();
            bool success = false;

            if (Kernel32.Thread32First(hSnapshot, ref te))
            {
                do
                {
                    if (te.th32OwnerProcessID == pid)
                    {
                        IntPtr hThread = Kernel32.OpenThread(ThreadAccessFlags.SuspendResume, false, te.th32ThreadID);
                        if (hThread != IntPtr.Zero)
                        {
                            Kernel32.ResumeThread(hThread);
                            Kernel32.CloseHandle(hThread);
                            success = true;
                        }
                    }
                } while (Kernel32.Thread32Next(hSnapshot, ref te));
            }

            return success;
        }
        finally
        {
            Kernel32.CloseHandle(hSnapshot);
        }
    }

    public static List<ThreadInfo> GetProcessThreads(uint pid)
    {
        var threads = new List<ThreadInfo>();

        IntPtr hSnapshot = Kernel32.CreateToolhelp32Snapshot(SnapshotFlags.Thread, pid);
        if (hSnapshot == (IntPtr)Kernel32.INVALID_HANDLE_VALUE) return threads;

        try
        {
            var te = new THREADENTRY32();
            te.dwSize = (uint)Marshal.SizeOf<THREADENTRY32>();

            if (Kernel32.Thread32First(hSnapshot, ref te))
            {
                do
                {
                    if (te.th32OwnerProcessID == pid)
                    {
                        threads.Add(new ThreadInfo
                        {
                            ThreadId = te.th32ThreadID,
                            ProcessId = te.th32OwnerProcessID,
                            BasePriority = te.tpBasePri,
                            CurrentPriority = te.tpBasePri + te.tpDeltaPri,
                            State = (uint)(te.dwFlags == 0 ? 0 : 1),
                            StateStr = te.dwFlags == 0 ? "Running" : "Suspended"
                        });
                    }
                } while (Kernel32.Thread32Next(hSnapshot, ref te));
            }
        }
        finally
        {
            Kernel32.CloseHandle(hSnapshot);
        }

        return threads;
    }

    public static List<KernelModule> GetProcessModules(uint pid)
    {
        var modules = new List<KernelModule>();

        IntPtr hSnapshot = Kernel32.CreateToolhelp32Snapshot(SnapshotFlags.Module | SnapshotFlags.Module32, pid);
        if (hSnapshot == (IntPtr)Kernel32.INVALID_HANDLE_VALUE) return modules;

        try
        {
            var me = new MODULEENTRY32();
            me.dwSize = (uint)Marshal.SizeOf<MODULEENTRY32>();

            if (Kernel32.Module32First(hSnapshot, ref me))
            {
                do
                {
                    modules.Add(new KernelModule
                    {
                        Name = me.szModule,
                        FullPath = me.szExePath,
                        ImageBase = (ulong)me.modBaseAddr,
                        ImageSize = me.modBaseSize,
                        LoadCount = (ushort)me.ProccntUsage,
                        LoadCountStr = me.ProccntUsage.ToString()
                    });
                } while (Kernel32.Module32Next(hSnapshot, ref me));
            }
        }
        finally
        {
            Kernel32.CloseHandle(hSnapshot);
        }

        return modules;
    }

    public static List<HandleInfo> GetProcessHandles(uint pid)
    {
        var handles = new List<HandleInfo>();

        try
        {
            int bufferSize = 0x10000;
            IntPtr buffer = IntPtr.Zero;

            while (true)
            {
                buffer = Marshal.AllocHGlobal(bufferSize);
                int ret = NtDll.NtQuerySystemInformation(
                    SYSTEM_INFORMATION_CLASS.SystemHandleInformation,
                    buffer, bufferSize, out int returnLength);

                if (ret == 0) break;

                Marshal.FreeHGlobal(buffer);
                if (ret == unchecked((int)NtDll.STATUS_INFO_LENGTH_MISMATCH) ||
                    ret == unchecked((int)NtDll.STATUS_BUFFER_TOO_SMALL))
                {
                    bufferSize = returnLength;
                    continue;
                }
                return handles;
            }

            try
            {
                ulong handleCount = (ulong)Marshal.ReadIntPtr(buffer);
                int entrySize = Marshal.SizeOf<SYSTEM_HANDLE_TABLE_ENTRY_INFO>();
                IntPtr entryPtr = buffer + IntPtr.Size;

                for (ulong i = 0; i < handleCount; i++)
                {
                    var entry = Marshal.PtrToStructure<SYSTEM_HANDLE_TABLE_ENTRY_INFO>(entryPtr);
                    if (entry.UniqueProcessId == pid)
                    {
                        handles.Add(new HandleInfo
                        {
                            ProcessId = entry.UniqueProcessId,
                            HandleValue = entry.HandleValue,
                            ObjectTypeIndex = entry.ObjectTypeIndex,
                            GrantedAccess = entry.GrantedAccess,
                            Object = entry.Object,
                            ObjectType = MapObjectType(entry.ObjectTypeIndex)
                        });
                    }
                    entryPtr += entrySize;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        catch
        {
        }

        return handles;
    }

    public static PROCESS_MEMORY_COUNTERS? GetProcessMemoryInfo(uint pid)
    {
        IntPtr hProcess = Kernel32.OpenProcess(
            ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VmRead, false, pid);
        if (hProcess == IntPtr.Zero) return null;

        try
        {
            IntPtr pmcPtr = Marshal.AllocHGlobal(Marshal.SizeOf<PROCESS_MEMORY_COUNTERS>());
            try
            {
                if (PsApi.GetProcessMemoryInfo(hProcess, pmcPtr, Marshal.SizeOf<PROCESS_MEMORY_COUNTERS>()))
                    return Marshal.PtrToStructure<PROCESS_MEMORY_COUNTERS>(pmcPtr);
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(pmcPtr);
            }
        }
        finally
        {
            Kernel32.CloseHandle(hProcess);
        }
    }

    public static string FormatCpuTime(long ticks)
    {
        long hundredNs = ticks;
        long totalMs = hundredNs / 10000;

        long days = totalMs / (24 * 60 * 60 * 1000);
        totalMs %= (24 * 60 * 60 * 1000);
        long hours = totalMs / (60 * 60 * 1000);
        totalMs %= (60 * 60 * 1000);
        long minutes = totalMs / (60 * 1000);
        totalMs %= (60 * 1000);
        long seconds = totalMs / 1000;
        long millis = totalMs % 1000;

        if (days > 0)
            return $"{days}d {hours:D2}:{minutes:D2}:{seconds:D2}.{millis:D3}";
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}.{millis:D3}";
    }

    private static string GetFileNameFromPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return "N/A";
        int pos = path.LastIndexOf('\\');
        return pos >= 0 ? path.Substring(pos + 1) : path;
    }

    private static string MapObjectType(byte index)
    {
        return index switch
        {
            1 => "Event",
            2 => "Mutant",
            3 => "Process",
            4 => "Thread",
            5 => "Job",
            6 => "Section",
            7 => "File",
            8 => "Desktop",
            9 => "Key",
            10 => "Token",
            11 => "WindowStation",
            12 => "Timer",
            13 => "IoCompletion",
            14 => "Semaphore",
            15 => "TpWorkerFactory",
            16 => "SymbolicLink",
            17 => "Callback",
            18 => "Port",
            19 => "FilterCommunicationPort",
            20 => "FilterConnectionPort",
            21 => "IoCompletionReserve",
            22 => "DebugObject",
            23 => "RawInputManager",
            24 => "DmaAdapter",
            25 => "DmaDomain",
            26 => "DmaMemory",
            27 => "EnergyTracker",
            28 => "DxgkCurrentDmuTarget",
            _ => $"Type{index}"
        };
    }
}
