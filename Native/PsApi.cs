using System.Runtime.InteropServices;

namespace CKC.Native;

public static class PsApi
{
    public const uint PROCESS_MEMORY_COUNTERS_SIZE = 72;

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool GetPerformanceInfo(out PERFORMANCE_INFORMATION pPerformanceInformation, int cb);

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool EnumProcesses([Out] int[] processIds, int arraySize, out int bytesReturned);

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern int GetProcessImageFileName(IntPtr hProcess, System.Text.StringBuilder lpImageFileName, int nSize);

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool GetProcessMemoryInfo(IntPtr hProcess, IntPtr ppsmemCounters, int cb);

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, System.Text.StringBuilder lpBaseName, uint nSize);

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool EnumProcessModules(IntPtr hProcess, [Out] IntPtr[] lphModule, int cb, out int lpcbNeeded);
}
