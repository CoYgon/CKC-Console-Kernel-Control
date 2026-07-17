using System.Runtime.InteropServices;
using System.Text;

namespace CKC.Native;

[StructLayout(LayoutKind.Sequential)]
public struct TOKEN_USER
{
    public SID_AND_ATTRIBUTES User;
}

[StructLayout(LayoutKind.Sequential)]
public struct SID_AND_ATTRIBUTES
{
    public IntPtr Sid;
    public uint Attributes;
}

[StructLayout(LayoutKind.Sequential)]
public struct TOKEN_ELEVATION
{
    public uint TokenIsElevated;
}

[StructLayout(LayoutKind.Sequential)]
public struct TOKEN_PRIVILEGES
{
    public uint PrivilegeCount;
    public LUID_AND_ATTRIBUTES Privileges;
}

[StructLayout(LayoutKind.Sequential)]
public struct LUID
{
    public uint LowPart;
    public int HighPart;
}

[StructLayout(LayoutKind.Sequential)]
public struct LUID_AND_ATTRIBUTES
{
    public LUID Luid;
    public uint Attributes;
}

[StructLayout(LayoutKind.Sequential)]
public struct SECURITY_DESCRIPTOR
{
    public byte Revision;
    public byte Sbz1;
    public ushort Control;
    public IntPtr Owner;
    public IntPtr Group;
    public IntPtr Sacl;
    public IntPtr Dacl;
}

[Flags]
public enum ServiceControlAccessRights : uint
{
    QueryConfig = 0x0001,
    ChangeConfig = 0x0002,
    QueryStatus = 0x0004,
    EnumerateDependents = 0x0008,
    Start = 0x0010,
    Stop = 0x0020,
    PauseContinue = 0x0040,
    Interrogate = 0x0080,
    UserDefinedControl = 0x0100,
    AllAccess = 0xF01FF
}

[StructLayout(LayoutKind.Sequential)]
public struct SERVICE_STATUS
{
    public uint dwServiceType;
    public uint dwCurrentState;
    public uint dwControlsAccepted;
    public uint dwWin32ExitCode;
    public uint dwServiceSpecificExitCode;
    public uint dwCheckPoint;
    public uint dwWaitHint;
}

[StructLayout(LayoutKind.Sequential)]
public struct SERVICE_STATUS_PROCESS
{
    public uint dwServiceType;
    public uint dwCurrentState;
    public uint dwControlsAccepted;
    public uint dwWin32ExitCode;
    public uint dwServiceSpecificExitCode;
    public uint dwCheckPoint;
    public uint dwWaitHint;
    public uint dwProcessId;
    public uint dwServiceFlags;
}

[StructLayout(LayoutKind.Sequential)]
public struct QUERY_SERVICE_CONFIG
{
    public uint dwServiceType;
    public uint dwStartType;
    public uint dwErrorControl;
    public IntPtr lpBinaryPathName;
    public IntPtr lpLoadOrderGroup;
    public uint dwTagId;
    public IntPtr lpDependencies;
    public IntPtr lpServiceStartName;
    public IntPtr lpDisplayName;
}

[StructLayout(LayoutKind.Sequential)]
public struct ENUM_SERVICE_STATUS_PROCESS
{
    public IntPtr lpServiceName;
    public IntPtr lpDisplayName;
    public SERVICE_STATUS_PROCESS ServiceStatusProcess;
}

public enum TOKEN_INFORMATION_CLASS
{
    TokenUser = 1,
    TokenGroups = 2,
    TokenPrivileges = 3,
    TokenElevation = 20,
    TokenElevationType = 22,
    TokenLinkedToken = 24,
    TokenUIAccess = 26
}

public enum SERVICE_STATE
{
    SERVICE_STOPPED = 0x00000001,
    SERVICE_START_PENDING = 0x00000002,
    SERVICE_STOP_PENDING = 0x00000003,
    SERVICE_RUNNING = 0x00000004,
    SERVICE_CONTINUE_PENDING = 0x00000005,
    SERVICE_PAUSE_PENDING = 0x00000006,
    SERVICE_PAUSED = 0x00000007
}

[Flags]
public enum ServiceManagerAccessRights : uint
{
    Connect = 0x0001,
    CreateService = 0x0002,
    EnumerateService = 0x0004,
    Lock = 0x0008,
    QueryLockStatus = 0x0010,
    ModifyBootConfig = 0x0020,
    AllAccess = 0xF003F
}

public static class AdvApi32
{
    public const uint SE_PRIVILEGE_ENABLED = 0x00000002;
    public const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 0x00000001;
    public const uint SE_PRIVILEGE_REMOVED = 0x00000004;
    public const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
    public const uint TOKEN_QUERY = 0x0008;
    public const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
    public const uint TOKEN_DUPLICATE = 0x0002;

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, uint BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool LookupAccountSid(string lpSystemName, IntPtr Sid, StringBuilder lpName, ref uint cchName, StringBuilder lpReferencedDomainName, ref uint cchReferencedDomainName, out int peUse);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool IsWellKnownSid(IntPtr pSid, int wellKnownSidType);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool ConvertSidToStringSid(IntPtr Sid, out IntPtr StringSid);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, ServiceManagerAccessRights dwDesiredAccess);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, ServiceControlAccessRights dwDesiredAccess);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CloseServiceHandle(IntPtr hSCObject);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool QueryServiceStatusEx(IntPtr hService, int InfoLevel, IntPtr lpBuffer, int cbBufSize, out int pcbBytesNeeded);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool QueryServiceConfig(IntPtr hService, IntPtr lpServiceConfig, int cbBufSize, out int pcbBytesNeeded);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool StartService(IntPtr hService, int dwNumServiceArgs, string[] lpServiceArgVectors);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool ControlService(IntPtr hService, uint dwControl, ref SERVICE_STATUS lpServiceStatus);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool EnumServicesStatusEx(IntPtr hSCManager, int InfoLevel, uint dwServiceType, uint dwServiceState, IntPtr lpServices, int cbBufSize, out int pcbBytesNeeded, out int lpServicesReturned, ref int lpResumeHandle, string pszGroupName);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool GetSecurityInfo(IntPtr handle, int ObjectType, uint SecurityInfo, out IntPtr pSidOwner, out IntPtr pSidGroup, out IntPtr pDacl, out IntPtr pSacl, out IntPtr pSecurityDescriptor);
}
