using System.Runtime.InteropServices;
using System.Text;

namespace CKC.Native;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class MEMORYSTATUSEX
{
    public uint dwLength;
    public uint dwMemoryLoad;
    public ulong ullTotalPhys;
    public ulong ullAvailPhys;
    public ulong ullTotalPageFile;
    public ulong ullAvailPageFile;
    public ulong ullTotalVirtual;
    public ulong ullAvailVirtual;
    public ulong ullAvailExtendedVirtual;

    public MEMORYSTATUSEX()
    {
        dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct SYSTEM_INFO
{
    public ushort wProcessorArchitecture;
    public ushort wReserved;
    public uint dwPageSize;
    public IntPtr lpMinimumApplicationAddress;
    public IntPtr lpMaximumApplicationAddress;
    public IntPtr dwActiveProcessorMask;
    public uint dwNumberOfProcessors;
    public uint dwProcessorType;
    public uint dwAllocationGranularity;
    public ushort wProcessorLevel;
    public ushort wProcessorRevision;
}

[StructLayout(LayoutKind.Sequential)]
public struct MEMORY_BASIC_INFORMATION
{
    public IntPtr BaseAddress;
    public IntPtr AllocationBase;
    public uint AllocationProtect;
    public UIntPtr RegionSize;
    public uint State;
    public uint Protect;
    public uint Type;
}

[StructLayout(LayoutKind.Sequential)]
public struct PERFORMANCE_INFORMATION
{
    public uint cb;
    public UIntPtr CommitTotal;
    public UIntPtr CommitLimit;
    public UIntPtr CommitPeak;
    public UIntPtr PhysicalTotal;
    public UIntPtr PhysicalAvailable;
    public UIntPtr SystemCache;
    public UIntPtr KernelTotal;
    public UIntPtr KernelPaged;
    public UIntPtr KernelNonpaged;
    public UIntPtr PageSize;
    public uint HandleCount;
    public uint ProcessCount;
    public uint ThreadCount;
}

[StructLayout(LayoutKind.Sequential)]
public struct FILETIME
{
    public uint dwLowDateTime;
    public uint dwHighDateTime;
}

[StructLayout(LayoutKind.Sequential)]
public struct TIME_ZONE_INFORMATION
{
    public int Bias;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string StandardName;
    public FILETIME StandardStart;
    public int StandardBias;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string DaylightName;
    public FILETIME DaylightStart;
    public int DaylightBias;
}

[StructLayout(LayoutKind.Sequential)]
public struct OSVERSIONINFOEX
{
    public uint dwOSVersionInfoSize;
    public uint dwMajorVersion;
    public uint dwMinorVersion;
    public uint dwBuildNumber;
    public uint dwPlatformId;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string szCSDVersion;
    public ushort wServicePackMajor;
    public ushort wServicePackMinor;
    public ushort wSuiteMask;
    public byte wProductType;
    public byte wReserved;
}

public enum PROCESSINFOCLASS
{
    ProcessBasicInformation = 0,
    ProcessQuotaLimits = 1,
    ProcessIoCounters = 2,
    ProcessVmCounters = 3,
    ProcessTimes = 4,
    ProcessBasePriority = 5,
    ProcessRaisePriority = 6,
    ProcessDebugPort = 7,
    ProcessExceptionPort = 8,
    ProcessAccessToken = 9,
    ProcessLdtInformation = 10,
    ProcessLdtSize = 11,
    ProcessDefaultHardErrorMode = 12,
    ProcessIoPortHandlers = 13,
    ProcessPooledUsageAndLimits = 14,
    ProcessWorkingSetWatch = 15,
    ProcessUserModeIOPL = 16,
    ProcessEnableAlignmentFaultFixup = 17,
    ProcessPriorityClass = 18,
    ProcessWx86Information = 19,
    ProcessHandleCount = 20,
    ProcessAffinityMask = 21,
    ProcessPriorityBoost = 22,
    ProcessDeviceMap = 23,
    ProcessSessionInformation = 24,
    ProcessForegroundInformation = 25,
    ProcessWow64Information = 26,
    ProcessImageFileName = 27,
    ProcessLUIDDeviceMapsEnabled = 28,
    ProcessBreakOnTermination = 29,
    ProcessDebugObjectHandle = 30,
    ProcessDebugFlags = 31,
    ProcessHandleTracing = 32,
    ProcessIoPriority = 33,
    ProcessPagePriority = 34,
    ProcessProcessExecuteFlags = 35,
    ProcessTlsInformation = 36,
    ProcessUserReferenceSet = 37,
    ProcessMitigationPolicy = 38,
    ProcessDynamicFunctionTableInformation = 39,
    ProcessHandleInformation = 40,
    ProcessMitigationPolicy2 = 41,
    MaxProcessInfoClass = 42
}

[StructLayout(LayoutKind.Sequential)]
public struct PROCESS_BASIC_INFORMATION
{
    public IntPtr ExitStatus;
    public IntPtr PebBaseAddress;
    public IntPtr AffinityMask;
    public IntPtr BasePriority;
    public UIntPtr UniqueProcessId;
    public UIntPtr InheritedFromUniqueProcessId;
}

[StructLayout(LayoutKind.Sequential)]
public struct PROCESS_MEMORY_COUNTERS
{
    public uint cb;
    public uint PageFaultCount;
    public UIntPtr PeakWorkingSetSize;
    public UIntPtr WorkingSetSize;
    public UIntPtr QuotaPeakPagedPoolUsage;
    public UIntPtr QuotaPagedPoolUsage;
    public UIntPtr QuotaPeakNonPagedPoolUsage;
    public UIntPtr QuotaNonPagedPoolUsage;
    public UIntPtr PagefileUsage;
    public UIntPtr PeakPagefileUsage;
}

[StructLayout(LayoutKind.Sequential)]
public struct IO_COUNTERS
{
    public ulong ReadOperationCount;
    public ulong WriteOperationCount;
    public ulong OtherOperationCount;
    public ulong ReadTransferCount;
    public ulong WriteTransferCount;
    public ulong OtherTransferCount;
}

[Flags]
public enum ProcessAccessFlags : uint
{
    Terminate = 0x0001,
    CreateThread = 0x0002,
    SetSessionId = 0x0004,
    VmOperation = 0x0008,
    VmRead = 0x0010,
    VmWrite = 0x0020,
    DupHandle = 0x0040,
    SetInformation = 0x0080,
    QueryInformation = 0x0400,
    SuspendResume = 0x0800,
    QueryLimitedInformation = 0x1000,
    SetLimitedInformation = 0x2000,
    AllAccess = 0x1FFFFF,
    Synchronize = 0x00100000
}

[Flags]
public enum ThreadAccessFlags : uint
{
    Terminate = 0x0001,
    SuspendResume = 0x0002,
    GetContext = 0x0008,
    SetContext = 0x0010,
    SetInformation = 0x0020,
    QueryInformation = 0x0040,
    SetThreadToken = 0x0080,
    Impersonate = 0x0100,
    DirectImpersonation = 0x0200,
    SetLimitedInformation = 0x0400,
    QueryLimitedInformation = 0x0800,
    AllAccess = 0x1FFFFF
}

[Flags]
public enum SnapshotFlags : uint
{
    Process = 0x00000001,
    Thread = 0x00000002,
    Module = 0x00000008,
    Module32 = 0x00000010,
    All = 0x0000001F,
    Inherit = 0x80000000
}

[StructLayout(LayoutKind.Sequential)]
public struct PROCESSENTRY32
{
    public uint dwSize;
    public uint cntUsage;
    public uint th32ProcessID;
    public IntPtr th32DefaultHeapID;
    public uint th32ModuleID;
    public uint cntThreads;
    public uint th32ParentProcessID;
    public int pcPriClassBase;
    public uint dwFlags;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szExeFile;
}

[StructLayout(LayoutKind.Sequential)]
public struct THREADENTRY32
{
    public uint dwSize;
    public uint cntUsage;
    public uint th32ThreadID;
    public uint th32OwnerProcessID;
    public int tpBasePri;
    public int tpDeltaPri;
    public uint dwFlags;
}

[StructLayout(LayoutKind.Sequential)]
public struct MODULEENTRY32
{
    public uint dwSize;
    public uint th32ModuleID;
    public uint th32ProcessID;
    public uint GlblcntUsage;
    public uint ProccntUsage;
    public IntPtr modBaseAddr;
    public uint modBaseSize;
    public IntPtr hModule;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string szModule;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string szExePath;
}

[StructLayout(LayoutKind.Sequential)]
public struct SYSTEM_TIMEOFDAY_INFORMATION
{
    public long BootTime;
    public long CurrentTime;
    public long TimeZoneBias;
    public long TimeZoneId;
    public long Reserved;
}

public static class Kernel32
{
    public const uint MEM_COMMIT = 0x1000;
    public const uint MEM_RESERVE = 0x2000;
    public const uint MEM_RELEASE = 0x8000;
    public const uint MEM_FREE = 0x10000;
    public const uint PAGE_READWRITE = 0x04;
    public const uint PAGE_READONLY = 0x02;
    public const uint PAGE_EXECUTE_READWRITE = 0x40;
    public const uint PAGE_NOACCESS = 0x01;
    public const uint MEM_PRIVATE = 0x20000;
    public const uint MEM_IMAGE = 0x1000000;
    public const uint MEM_MAPPED = 0x40000;
    public const uint INVALID_HANDLE_VALUE = 0xFFFFFFFF;
    public const uint STILL_ACTIVE = 0x00000103;
    public const uint PROCESS_QUERY_INFORMATION = 0x0400;
    public const uint PROCESS_VM_READ = 0x0010;

    [DllImport("kernel32.dll")]
    public static extern bool GlobalMemoryStatusEx(MEMORYSTATUSEX lpBuffer);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll")]
    public static extern void GetSystemInfo(out SYSTEM_INFO lpSystemInfo);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SuspendThread(IntPtr hThread, out uint dwPreviousSuspendCount);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, uint th32ProcessID);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Thread32First(IntPtr hSnapshot, ref THREADENTRY32 lpte);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Thread32Next(IntPtr hSnapshot, ref THREADENTRY32 lpte);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenThread(ThreadAccessFlags dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint ResumeThread(IntPtr hThread);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetThreadTimes(IntPtr hThread, out FILETIME lpCreationTime, out FILETIME lpExitTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

    [DllImport("kernel32.dll")]
    public static extern void GetSystemTimeAsFileTime(out FILETIME lpSystemTimeAsFileTime);

    [DllImport("kernel32.dll")]
    public static extern uint GetTickCount();

    [DllImport("kernel32.dll")]
    public static extern ulong GetTickCount64();

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetSystemTimes(out FILETIME lpIdleTime, out FILETIME lpKernelTime, out FILETIME lpUserTime);

    [DllImport("kernel32.dll")]
    public static extern bool GetComputerName(StringBuilder lpBuffer, ref uint nSize);

    [DllImport("kernel32.dll")]
    public static extern bool GetUserName(StringBuilder lpBuffer, ref uint nSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetProductInfo(uint dwOSMajorVersion, uint dwOSMinorVersion, uint dwSpMajorVersion, uint dwSpMinorVersion, out uint pdwReturnedProductType);

    [DllImport("kernel32.dll")]
    public static extern uint GetLastError();
}
