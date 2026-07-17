using System.Runtime.InteropServices;
using System.Text;
using CKC.Native;
using CKC.Models;

namespace CKC.Services;

public static class NetworkManager
{
    public static List<NetworkConnection> GetAllConnections()
    {
        var connections = new List<NetworkConnection>();

        connections.AddRange(GetTcpConnections());
        connections.AddRange(GetUdpConnections());

        return connections;
    }

    public static List<NetworkConnection> GetProcessConnections(uint pid)
    {
        return GetAllConnections().Where(c => c.ProcessId == pid).ToList();
    }

    private static List<NetworkConnection> GetTcpConnections()
    {
        var connections = new List<NetworkConnection>();

        try
        {
            int bufferSize = 0;
            uint result = IpHlpApi.GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, false, IpHlpApi.AF_INET, 0, 0);

            if (result != IpHlpApi.NO_ERROR && result != IpHlpApi.ERROR_INSUFFICIENT_BUFFER)
                return connections;

            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
            try
            {
                result = IpHlpApi.GetExtendedTcpTable(buffer, ref bufferSize, false, IpHlpApi.AF_INET, 0, 0);
                if (result != IpHlpApi.NO_ERROR) return connections;

                int numEntries = Marshal.ReadInt32(buffer);
                int rowSize = Marshal.SizeOf<MIB_TCPROW_OWNER_PID>();
                IntPtr rowPtr = buffer + 4;

                for (int i = 0; i < numEntries; i++)
                {
                    try
                    {
                        var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);

                        var conn = new NetworkConnection
                        {
                            Protocol = "TCP",
                            ProcessId = row.dwOwningPid,
                            LocalAddress = ConvertIpAddress(row.dwLocalAddr),
                            LocalPort = (ushort)((row.dwLocalPort >> 8) | (row.dwLocalPort << 8)),
                            RemoteAddress = ConvertIpAddress(row.dwRemoteAddr),
                            RemotePort = (ushort)((row.dwRemotePort >> 8) | (row.dwRemotePort << 8)),
                            State = MapTcpState(row.dwState),
                            StateCode = row.dwState.ToString()
                        };

                        conn.ProcessName = GetProcessName(conn.ProcessId);
                        connections.Add(conn);
                    }
                    catch
                    {
                    }

                    rowPtr += rowSize;
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

        return connections;
    }

    private static List<NetworkConnection> GetUdpConnections()
    {
        var connections = new List<NetworkConnection>();

        try
        {
            int bufferSize = 0;
            uint result = IpHlpApi.GetExtendedUdpTable(IntPtr.Zero, ref bufferSize, false, IpHlpApi.AF_INET, 0, 0);

            if (result != IpHlpApi.NO_ERROR && result != IpHlpApi.ERROR_INSUFFICIENT_BUFFER)
                return connections;

            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
            try
            {
                result = IpHlpApi.GetExtendedUdpTable(buffer, ref bufferSize, false, IpHlpApi.AF_INET, 0, 0);
                if (result != IpHlpApi.NO_ERROR) return connections;

                int numEntries = Marshal.ReadInt32(buffer);
                int rowSize = Marshal.SizeOf<MIB_UDPROW_OWNER_PID>();
                IntPtr rowPtr = buffer + 4;

                for (int i = 0; i < numEntries; i++)
                {
                    try
                    {
                        var row = Marshal.PtrToStructure<MIB_UDPROW_OWNER_PID>(rowPtr);

                        var conn = new NetworkConnection
                        {
                            Protocol = "UDP",
                            ProcessId = row.dwOwningPid,
                            LocalAddress = ConvertIpAddress(row.dwLocalAddr),
                            LocalPort = (ushort)((row.dwLocalPort >> 8) | (row.dwLocalPort << 8)),
                            RemoteAddress = "0.0.0.0",
                            RemotePort = 0,
                            State = "-",
                            StateCode = "0"
                        };

                        conn.ProcessName = GetProcessName(conn.ProcessId);
                        connections.Add(conn);
                    }
                    catch
                    {
                    }

                    rowPtr += rowSize;
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

        return connections;
    }

    public static string MapTcpState(uint state)
    {
        return state switch
        {
            1 => "CLOSED",
            2 => "LISTEN",
            3 => "SYN_SENT",
            4 => "SYN_RCVD",
            5 => "ESTABLISHED",
            6 => "FIN_WAIT1",
            7 => "FIN_WAIT2",
            8 => "CLOSE_WAIT",
            9 => "CLOSING",
            10 => "LAST_ACK",
            11 => "TIME_WAIT",
            12 => "MAX",
            _ => $"UNKNOWN ({state})"
        };
    }

    public static string ConvertIpAddress(uint addr)
    {
        try
        {
            byte[] bytes = BitConverter.GetBytes(addr);
            return $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}";
        }
        catch
        {
            return "0.0.0.0";
        }
    }

    private static string GetProcessName(uint pid)
    {
        try
        {
            var proc = System.Diagnostics.Process.GetProcessById((int)pid);
            return proc.ProcessName;
        }
        catch
        {
            return $"PID:{pid}";
        }
    }
}
