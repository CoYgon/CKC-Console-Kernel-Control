using System.Runtime.InteropServices;
using System.Text;
using CKC.Native;
using CKC.Models;

namespace CKC.Services;

public static class MemoryManager
{
    public static MemoryStatus GetMemoryStatus()
    {
        var status = new MemoryStatus();

        try
        {
            var memStatus = new MEMORYSTATUSEX();
            if (Kernel32.GlobalMemoryStatusEx(memStatus))
            {
                status.MemoryLoad = memStatus.dwMemoryLoad;
                status.TotalPhysicalMB = memStatus.ullTotalPhys / (1024 * 1024);
                status.AvailablePhysicalMB = memStatus.ullAvailPhys / (1024 * 1024);
                status.TotalPageFileMB = memStatus.ullTotalPageFile / (1024 * 1024);
                status.AvailablePageFileMB = memStatus.ullAvailPageFile / (1024 * 1024);
                status.TotalVirtualMB = memStatus.ullTotalVirtual / (1024 * 1024);
                status.AvailableVirtualMB = memStatus.ullAvailVirtual / (1024 * 1024);
            }
        }
        catch
        {
        }

        try
        {
            var perfInfo = new PERFORMANCE_INFORMATION();
            perfInfo.cb = (uint)Marshal.SizeOf<PERFORMANCE_INFORMATION>();

            if (PsApi.GetPerformanceInfo(out perfInfo, Marshal.SizeOf<PERFORMANCE_INFORMATION>()))
            {
                ulong pageSize = (ulong)perfInfo.PageSize;

                status.UsedPhysicalMB = ((ulong)perfInfo.PhysicalTotal - (ulong)perfInfo.PhysicalAvailable) * pageSize / (1024 * 1024);
                status.SystemCacheMB = (ulong)perfInfo.SystemCache * pageSize / (1024 * 1024);
                status.TotalPagedPoolMB = (ulong)perfInfo.KernelPaged * pageSize / (1024 * 1024);
                status.TotalNonPagedPoolMB = (ulong)perfInfo.KernelNonpaged * pageSize / (1024 * 1024);
                status.CommitTotalMB = (ulong)perfInfo.CommitTotal * pageSize / (1024 * 1024);
                status.CommitLimitMB = (ulong)perfInfo.CommitLimit * pageSize / (1024 * 1024);
                status.CommitPeakMB = (ulong)perfInfo.CommitPeak * pageSize / (1024 * 1024);
                status.HandleCount = perfInfo.HandleCount;
                status.ProcessCount = perfInfo.ProcessCount;
                status.ThreadCount = perfInfo.ThreadCount;
            }
        }
        catch
        {
        }

        if (status.TotalPhysicalMB > 0)
            status.PhysicalUsagePercent = (double)(status.TotalPhysicalMB - status.AvailablePhysicalMB) / status.TotalPhysicalMB * 100.0;

        return status;
    }

    public static string GetVirtualMemoryLayout()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Virtual Memory Layout:");
        sb.AppendLine(new string('-', 120));
        sb.AppendLine($"{"Base Address",-20} {"Size",-12} {"State",-10} {"Protect",-20} {"Type",-12}");
        sb.AppendLine(new string('-', 120));

        IntPtr hProcess = Kernel32.GetCurrentProcess();
        IntPtr address = IntPtr.Zero;

        try
        {
            var sysInfo = new SYSTEM_INFO();
            Kernel32.GetSystemInfo(out sysInfo);

            while (true)
            {
                if (!Kernel32.VirtualQueryEx(hProcess, address, out MEMORY_BASIC_INFORMATION mbi,
                    Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()))
                    break;

                if (mbi.RegionSize == UIntPtr.Zero)
                    break;

                string state = mbi.State switch
                {
                    Kernel32.MEM_COMMIT => "Commit",
                    Kernel32.MEM_RESERVE => "Reserve",
                    Kernel32.MEM_FREE => "Free  ",
                    _ => $"{mbi.State:X}"
                };

                string protect = mbi.State == Kernel32.MEM_FREE ? "-" : ProtectToString(mbi.Protect);

                string type = mbi.Type switch
                {
                    Kernel32.MEM_PRIVATE => "Private",
                    Kernel32.MEM_IMAGE => "Image  ",
                    Kernel32.MEM_MAPPED => "Mapped ",
                    _ => $"{mbi.Type:X}"
                };

                ulong size = (ulong)mbi.RegionSize;
                sb.AppendLine($"0x{(ulong)mbi.BaseAddress:X16} {FormatBytes(size),-12} {state,-10} {protect,-20} {type,-12}");

                address = (IntPtr)((ulong)mbi.BaseAddress + (ulong)mbi.RegionSize);
            }
        }
        catch
        {
        }

        return sb.ToString();
    }

    public static string GetMemoryInfo(IntPtr hProcess, IntPtr address)
    {
        if (!Kernel32.VirtualQueryEx(hProcess, address, out MEMORY_BASIC_INFORMATION mbi,
            Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()))
            return "Failed to query memory information.";

        var sb = new StringBuilder();
        sb.AppendLine($"Base Address      : 0x{(ulong)mbi.BaseAddress:X16}");
        sb.AppendLine($"Allocation Base   : 0x{(ulong)mbi.AllocationBase:X16}");
        sb.AppendLine($"Allocation Protect: {ProtectToString(mbi.AllocationProtect)}");
        sb.AppendLine($"Region Size       : {FormatBytes((ulong)mbi.RegionSize)}");
        sb.AppendLine($"State             : {mbi.State switch { Kernel32.MEM_COMMIT => "Committed", Kernel32.MEM_RESERVE => "Reserved", Kernel32.MEM_FREE => "Free", _ => "Unknown" }}");
        sb.AppendLine($"Protect           : {ProtectToString(mbi.Protect)}");
        sb.AppendLine($"Type              : {mbi.Type switch { Kernel32.MEM_PRIVATE => "Private", Kernel32.MEM_IMAGE => "Image", Kernel32.MEM_MAPPED => "Mapped", _ => "Unknown" }}");
        return sb.ToString();
    }

    public static string FormatBytes(ulong bytes)
    {
        if (bytes >= (1024L * 1024 * 1024))
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        if (bytes >= (1024 * 1024))
            return $"{bytes / (1024.0 * 1024):F2} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024.0:F2} KB";
        return $"{bytes} B";
    }

    private static string ProtectToString(uint protect)
    {
        return protect switch
        {
            0x01 => "No Access",
            0x02 => "Read Only",
            0x04 => "Read/Write",
            0x08 => "Write Copy",
            0x10 => "Execute",
            0x20 => "Execute/Read",
            0x40 => "Execute/Read/Write",
            0x80 => "Execute/Write Copy",
            0x100 => "Guard",
            0x200 => "No Cache",
            0x400 => "Write Combine",
            _ => $"0x{protect:X4}"
        };
    }
}
