using System.Runtime.InteropServices;

namespace CKC.Native;

[StructLayout(LayoutKind.Sequential)]
public struct SYSTEM_PROCESS_INFORMATION
{
    public uint NextEntryOffset;
    public uint NumberOfThreads;
    public long WorkingSetPrivateSize;
    public uint HardFaultCount;
    public uint NumberOfThreadsHighWatermark;
    public ulong CycleTime;
    public long CreateTime;
    public long UserTime;
    public long KernelTime;
    public UNICODE_STRING ImageName;
    public int BasePriority;
    public IntPtr UniqueProcessId;
    public IntPtr InheritedFromUniqueProcessId;
    public uint HandleCount;
    public uint SessionId;
    public UIntPtr UniqueProcessKey;
    public UIntPtr PeakVirtualSize;
    public UIntPtr VirtualSize;
    public uint PageFaultCount;
    public UIntPtr PeakWorkingSetSize;
    public UIntPtr WorkingSetSize;
    public UIntPtr QuotaPeakPagedPoolUsage;
    public UIntPtr QuotaPagedPoolUsage;
    public UIntPtr QuotaPeakNonPagedPoolUsage;
    public UIntPtr QuotaNonPagedPoolUsage;
    public UIntPtr PagefileUsage;
    public UIntPtr PeakPagefileUsage;
    public UIntPtr PrivatePageCount;
    public long ReadOperationCount;
    public long WriteOperationCount;
    public long OtherOperationCount;
    public long ReadTransferCount;
    public long WriteTransferCount;
    public long OtherTransferCount;
}

[StructLayout(LayoutKind.Sequential)]
public struct SYSTEM_HANDLE_TABLE_ENTRY_INFO
{
    public ushort UniqueProcessId;
    public ushort CreatorBackTraceIndex;
    public byte ObjectTypeIndex;
    public byte HandleAttributes;
    public ushort HandleValue;
    public IntPtr Object;
    public uint GrantedAccess;
}

[StructLayout(LayoutKind.Sequential)]
public struct SYSTEM_HANDLE_INFORMATION
{
    public IntPtr NumberOfHandles;
    public SYSTEM_HANDLE_TABLE_ENTRY_INFO Handles;
}

[StructLayout(LayoutKind.Sequential)]
public struct UNICODE_STRING
{
    public ushort Length;
    public ushort MaximumLength;
    public IntPtr Buffer;
}

[StructLayout(LayoutKind.Sequential)]
public struct OBJECT_ATTRIBUTES
{
    public uint Length;
    public IntPtr RootDirectory;
    public IntPtr ObjectName;
    public uint Attributes;
    public IntPtr SecurityDescriptor;
    public IntPtr SecurityQualityOfService;
}

public enum SYSTEM_INFORMATION_CLASS
{
    SystemBasicInformation = 0,
    SystemPerformanceInformation = 2,
    SystemTimeOfDayInformation = 3,
    SystemProcessInformation = 5,
    SystemProcessorPerformanceInformation = 8,
    SystemInterruptInformation = 23,
    SystemExceptionInformation = 33,
    SystemRegistryQuotaInformation = 37,
    SystemLookasideInformation = 45,
    SystemPerformanceTraceInformation = 64,
    SystemHandleInformation = 16,
    SystemKernelDebuggerInformation = 35,
    SystemModuleInformation = 11,
    SystemLocksInformation = 12,
    SystemObjectInformation = 17,
    SystemPageFileInformation = 18,
    SystemCodeIntegrityInformation = 103,
    SystemSecureBootInformation = 144,
}

[StructLayout(LayoutKind.Sequential)]
public struct SYSTEM_MODULE
{
    public IntPtr Reserved1;
    public IntPtr Reserved2;
    public IntPtr ImageBase;
    public uint ImageSize;
    public uint Flags;
    public ushort LoadOrderIndex;
    public ushort InitOrderIndex;
    public ushort LoadCount;
    public ushort ModuleNameOffset;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
    public byte[] ImageName;
    public string GetImageName() =>
        System.Text.Encoding.ASCII.GetString(ImageName).TrimEnd('\0');
}

[StructLayout(LayoutKind.Sequential)]
public struct RTL_OSVERSIONINFOEX
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

public static class NtDll
{
    public const uint STATUS_SUCCESS = 0x00000000;
    public const uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;
    public const uint STATUS_BUFFER_TOO_SMALL = 0xC0000023;
    public const uint STATUS_ACCESS_DENIED = 0xC0000022;

    [DllImport("ntdll.dll")]
    public static extern int NtQuerySystemInformation(
        SYSTEM_INFORMATION_CLASS SystemInformationClass,
        IntPtr SystemInformation,
        int SystemInformationLength,
        out int ReturnLength);

    [DllImport("ntdll.dll")]
    public static extern int NtQueryInformationProcess(
        IntPtr ProcessHandle,
        PROCESSINFOCLASS ProcessInformationClass,
        IntPtr ProcessInformation,
        int ProcessInformationLength,
        out int ReturnLength);

    [DllImport("ntdll.dll")]
    public static extern int NtQueryInformationThread(
        IntPtr ThreadHandle,
        int ThreadInformationClass,
        IntPtr ThreadInformation,
        int ThreadInformationLength,
        out int ReturnLength);

    [DllImport("ntdll.dll")]
    public static extern int NtSuspendProcess(IntPtr ProcessHandle);

    [DllImport("ntdll.dll")]
    public static extern int NtResumeProcess(IntPtr ProcessHandle);

    [DllImport("ntdll.dll")]
    public static extern int RtlGetVersion(ref RTL_OSVERSIONINFOEX lpVersionInformation);

    [DllImport("ntdll.dll")]
    public static extern int NtQuerySystemTime(out long lpSystemTime);
}
