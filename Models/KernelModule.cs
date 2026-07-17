namespace CKC.Models;

public class KernelModule
{
    public string Name { get; set; } = "N/A";
    public string FullPath { get; set; } = "N/A";
    public ulong ImageBase { get; set; }
    public uint ImageSize { get; set; }
    public ushort LoadOrderIndex { get; set; }
    public ushort InitOrderIndex { get; set; }
    public ushort LoadCount { get; set; }
    public string LoadCountStr { get; set; } = "N/A";

    public override string ToString()
    {
        return $"{Name,-30} 0x{ImageBase:X16} {ImageSize,8} KB";
    }
}

public class ThreadInfo
{
    public uint ThreadId { get; set; }
    public uint ProcessId { get; set; }
    public string ProcessName { get; set; } = "N/A";
    public int BasePriority { get; set; }
    public int CurrentPriority { get; set; }
    public uint State { get; set; }
    public string StateStr { get; set; } = "N/A";
    public ulong StartAddress { get; set; }

    public override string ToString()
    {
        return $"[{ThreadId,6}] PID:{ProcessId,-6} Pri:{BasePriority,-3} State:{StateStr,-10} 0x{StartAddress:X16}";
    }
}

public class HandleInfo
{
    public ushort ProcessId { get; set; }
    public ushort HandleValue { get; set; }
    public byte ObjectTypeIndex { get; set; }
    public string ObjectType { get; set; } = "N/A";
    public uint GrantedAccess { get; set; }
    public IntPtr Object { get; set; }

    public override string ToString()
    {
        return $"[{ProcessId,6}] 0x{HandleValue:X4} Type:{ObjectType,-12} Access:0x{GrantedAccess:X8}";
    }
}

public class WindowInfo
{
    public IntPtr Handle { get; set; }
    public string Title { get; set; } = "N/A";
    public string ClassName { get; set; } = "N/A";
    public uint ProcessId { get; set; }
    public string ProcessName { get; set; } = "N/A";
    public bool Visible { get; set; }
    public RECT Position { get; set; }

    public struct RECT { public int Left, Top, Right, Bottom; }

    public override string ToString()
    {
        var visible = Visible ? "V" : "H";
        return $"0x{Handle.ToInt64():X8} [{visible}] {ProcessName,-20} \"{Title}\" ({Position.Left},{Position.Top})-({Position.Right},{Position.Bottom})";
    }
}
