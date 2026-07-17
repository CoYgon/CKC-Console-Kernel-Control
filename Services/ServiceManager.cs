using System.Runtime.InteropServices;
using System.Text;
using CKC.Native;
using CKC.Models;

namespace CKC.Services;

public static class ServiceManager
{
    public static List<ServiceInfo> GetAllServices()
    {
        var services = new List<ServiceInfo>();

        IntPtr scm = AdvApi32.OpenSCManager(null, null,
            ServiceManagerAccessRights.EnumerateService | ServiceManagerAccessRights.Connect);

        if (scm == IntPtr.Zero) return services;

        try
        {
            int bufferSize = 0x10000;
            int resumeHandle = 0;

            while (true)
            {
                IntPtr buffer = Marshal.AllocHGlobal(bufferSize);
                try
                {
                    bool result = AdvApi32.EnumServicesStatusEx(
                        scm, 0, 0x00000030, 3,
                        buffer, bufferSize,
                        out int bytesNeeded,
                        out int servicesReturned,
                        ref resumeHandle, null);

                    if (!result)
                    {
                        int err = Marshal.GetLastWin32Error();
                        if (err == 122 || err == 234)
                        {
                            Marshal.FreeHGlobal(buffer);
                            bufferSize = bytesNeeded;
                            continue;
                        }
                        break;
                    }

                    IntPtr current = buffer;
                    int structSize = Marshal.SizeOf<ENUM_SERVICE_STATUS_PROCESS>();

                    for (int i = 0; i < servicesReturned; i++)
                    {
                        try
                        {
                            var essp = Marshal.PtrToStructure<ENUM_SERVICE_STATUS_PROCESS>(current);
                            string serviceName = Marshal.PtrToStringUni(essp.lpServiceName) ?? "N/A";
                            string displayName = Marshal.PtrToStringUni(essp.lpDisplayName) ?? "N/A";

                            var info = new ServiceInfo
                            {
                                ServiceName = serviceName,
                                DisplayName = displayName,
                                Status = MapServiceState(essp.ServiceStatusProcess.dwCurrentState),
                                ServiceType = MapServiceType(essp.ServiceStatusProcess.dwServiceType),
                                ProcessId = essp.ServiceStatusProcess.dwProcessId
                            };

                            GetServiceConfig(scm, serviceName, info);

                            services.Add(info);
                        }
                        catch
                        {
                        }

                        current += structSize;
                    }

                    if (resumeHandle == 0) break;
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }
        finally
        {
            AdvApi32.CloseServiceHandle(scm);
        }

        return services;
    }

    public static ServiceInfo? GetServiceStatus(string name)
    {
        return GetAllServices().FirstOrDefault(s =>
            s.ServiceName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
            s.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public static bool StartService(string name)
    {
        IntPtr scm = AdvApi32.OpenSCManager(null, null, ServiceManagerAccessRights.Connect);
        if (scm == IntPtr.Zero) return false;

        try
        {
            IntPtr hService = AdvApi32.OpenService(scm, name, ServiceControlAccessRights.Start);
            if (hService == IntPtr.Zero) return false;

            try
            {
                return AdvApi32.StartService(hService, 0, null);
            }
            finally
            {
                AdvApi32.CloseServiceHandle(hService);
            }
        }
        finally
        {
            AdvApi32.CloseServiceHandle(scm);
        }
    }

    public static bool StopService(string name)
    {
        IntPtr scm = AdvApi32.OpenSCManager(null, null, ServiceManagerAccessRights.Connect);
        if (scm == IntPtr.Zero) return false;

        try
        {
            IntPtr hService = AdvApi32.OpenService(scm, name, ServiceControlAccessRights.Stop);
            if (hService == IntPtr.Zero) return false;

            try
            {
                var status = new SERVICE_STATUS();
                return AdvApi32.ControlService(hService, 1, ref status);
            }
            finally
            {
                AdvApi32.CloseServiceHandle(hService);
            }
        }
        finally
        {
            AdvApi32.CloseServiceHandle(scm);
        }
    }

    public static bool RestartService(string name)
    {
        return StopService(name) && StartService(name);
    }

    public static bool EnableService(string name)
    {
        return ChangeServiceStartType(name, 2);
    }

    public static bool DisableService(string name)
    {
        return ChangeServiceStartType(name, 4);
    }

    private static bool ChangeServiceStartType(string name, uint startType)
    {
        IntPtr scm = AdvApi32.OpenSCManager(null, null,
            ServiceManagerAccessRights.Connect | ServiceManagerAccessRights.AllAccess);
        if (scm == IntPtr.Zero) return false;

        try
        {
            IntPtr hService = AdvApi32.OpenService(scm, name, ServiceControlAccessRights.ChangeConfig);
            if (hService == IntPtr.Zero) return false;

            try
            {
                return ChangeServiceConfig(hService, startType);
            }
            finally
            {
                AdvApi32.CloseServiceHandle(hService);
            }
        }
        finally
        {
            AdvApi32.CloseServiceHandle(scm);
        }
    }

    private static bool ChangeServiceConfig(IntPtr hService, uint startType)
    {
        try
        {
            var changeConfig = typeof(AdvApi32).GetMethod("ChangeServiceConfig",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (changeConfig != null)
                return (bool)changeConfig.Invoke(null, new object[] { hService, 0, startType, 0, null, null, IntPtr.Zero, null, null, null })!;

            var advApi = NativeLibrary.Load("advapi32.dll");
            IntPtr pFunc = NativeLibrary.GetExport(advApi, "ChangeServiceConfigW");
            if (pFunc == IntPtr.Zero) return false;

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static void GetServiceConfig(IntPtr scm, string serviceName, ServiceInfo info)
    {
        IntPtr hService = AdvApi32.OpenService(scm, serviceName,
            ServiceControlAccessRights.QueryConfig | ServiceControlAccessRights.QueryStatus);

        if (hService == IntPtr.Zero) return;

        try
        {
            int bufferSize = 4096;
            IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

            try
            {
                if (AdvApi32.QueryServiceConfig(hService, buffer, bufferSize, out int bytesNeeded))
                {
                    var config = Marshal.PtrToStructure<QUERY_SERVICE_CONFIG>(buffer);
                    info.BinaryPath = Marshal.PtrToStringUni(config.lpBinaryPathName) ?? "N/A";
                    info.AccountName = Marshal.PtrToStringUni(config.lpServiceStartName) ?? "N/A";
                    info.StartType = MapStartType(config.dwStartType);
                    info.ErrorControl = config.dwErrorControl switch
                    {
                        0 => "Ignore",
                        1 => "Normal",
                        2 => "Severe",
                        3 => "Critical",
                        _ => "Unknown"
                    };
                    info.LoadOrderGroup = Marshal.PtrToStringUni(config.lpLoadOrderGroup) ?? "N/A";

                    if (config.lpDependencies != IntPtr.Zero)
                    {
                        var deps = new StringBuilder();
                        IntPtr depPtr = config.lpDependencies;
                        while (true)
                        {
                            string dep = Marshal.PtrToStringUni(depPtr);
                            if (string.IsNullOrEmpty(dep)) break;
                            if (deps.Length > 0) deps.Append(", ");
                            deps.Append(dep);
                            depPtr += (dep.Length + 1) * 2;
                        }
                        info.Dependencies = deps.Length > 0 ? deps.ToString() : "None";
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        finally
        {
            AdvApi32.CloseServiceHandle(hService);
        }
    }

    public static string MapServiceState(uint state)
    {
        return state switch
        {
            1 => "Stopped",
            2 => "Start Pending",
            3 => "Stop Pending",
            4 => "Running",
            5 => "Continue Pending",
            6 => "Pause Pending",
            7 => "Paused",
            _ => $"Unknown ({state})"
        };
    }

    public static string MapStartType(uint startType)
    {
        return startType switch
        {
            0 => "Boot",
            1 => "System",
            2 => "Automatic",
            3 => "Manual",
            4 => "Disabled",
            _ => $"Unknown ({startType})"
        };
    }

    private static string MapServiceType(uint type)
    {
        var types = new List<string>();
        if ((type & 0x00000001) != 0) types.Add("Kernel Driver");
        if ((type & 0x00000002) != 0) types.Add("File System Driver");
        if ((type & 0x00000004) != 0) types.Add("Adapter");
        if ((type & 0x00000008) != 0) types.Add("Recognizer Driver");
        if ((type & 0x00000010) != 0) types.Add("Win32 Own Process");
        if ((type & 0x00000020) != 0) types.Add("Win32 Share Process");
        if ((type & 0x00000100) != 0) types.Add("Interactive");
        return types.Count > 0 ? string.Join(", ", types) : $"Type 0x{type:X}";
    }
}
