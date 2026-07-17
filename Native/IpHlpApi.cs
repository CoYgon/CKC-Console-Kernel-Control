using System.Runtime.InteropServices;

namespace CKC.Native;

[StructLayout(LayoutKind.Sequential)]
public struct MIB_TCPROW_OWNER_PID
{
    public uint dwState;
    public uint dwLocalAddr;
    public uint dwLocalPort;
    public uint dwRemoteAddr;
    public uint dwRemotePort;
    public uint dwOwningPid;
}

[StructLayout(LayoutKind.Sequential)]
public struct MIB_TCPTABLE_OWNER_PID
{
    public uint dwNumEntries;
    public MIB_TCPROW_OWNER_PID table;
}

[StructLayout(LayoutKind.Sequential)]
public struct MIB_UDPROW_OWNER_PID
{
    public uint dwLocalAddr;
    public uint dwLocalPort;
    public uint dwOwningPid;
}

[StructLayout(LayoutKind.Sequential)]
public struct MIB_UDPTABLE_OWNER_PID
{
    public uint dwNumEntries;
    public MIB_UDPROW_OWNER_PID table;
}

public static class IpHlpApi
{
    public const uint AF_INET = 2;
    public const uint NO_ERROR = 0;
    public const int ERROR_INSUFFICIENT_BUFFER = 122;

    public const uint MIB_TCP_STATE_CLOSED = 1;
    public const uint MIB_TCP_STATE_LISTEN = 2;
    public const uint MIB_TCP_STATE_SYN_SENT = 3;
    public const uint MIB_TCP_STATE_SYN_RCVD = 4;
    public const uint MIB_TCP_STATE_ESTAB = 5;
    public const uint MIB_TCP_STATE_FIN_WAIT1 = 6;
    public const uint MIB_TCP_STATE_FIN_WAIT2 = 7;
    public const uint MIB_TCP_STATE_CLOSE_WAIT = 8;
    public const uint MIB_TCP_STATE_CLOSING = 9;
    public const uint MIB_TCP_STATE_LAST_ACK = 10;
    public const uint MIB_TCP_STATE_TIME_WAIT = 11;
    public const uint MIB_TCP_STATE_MAX = 12;

    [DllImport("iphlpapi.dll", SetLastError = true)]
    public static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwSize, bool bOrder, uint ulAf, uint tableClass, uint reserved);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    public static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize, bool bOrder, uint ulAf, uint tableClass, uint reserved);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    public static extern uint GetIfEntry(IntPtr pIfRow);
}
