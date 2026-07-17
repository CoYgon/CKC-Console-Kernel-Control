using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using CKC.Native;

namespace CKC.Core;

public static class ElevationHelper
{
    public static bool IsElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void EnsureElevated()
    {
        if (!IsElevated())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Bu komut yönetici yetkileri gerektiriyor.");
            Console.WriteLine("Lütfen terminali yönetici olarak çalıştırın.");
            Console.ResetColor();
            Environment.Exit(1);
        }
    }

    public static bool RequestElevation()
    {
        if (IsElevated())
            return false;

        try
        {
            var process = Process.GetCurrentProcess();
            var startInfo = new ProcessStartInfo
            {
                FileName = process.MainModule?.FileName ?? "CKC.exe",
                Arguments = string.Join(" ", Environment.GetCommandLineArgs(), 1, Environment.GetCommandLineArgs().Length - 1),
                UseShellExecute = true,
                Verb = "runas"
            };

            Process.Start(startInfo);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string GetIntegrityLevel()
    {
        IntPtr hToken = IntPtr.Zero;
        try
        {
            if (!AdvApi32.OpenProcessToken(Kernel32.GetCurrentProcess(), AdvApi32.TOKEN_QUERY, out hToken))
                return "Unknown";

            uint returnLength = 0;
            AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevation,
                IntPtr.Zero, 0, out returnLength);

            IntPtr elevationPtr = Marshal.AllocHGlobal((int)returnLength);
            try
            {
                if (AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevation,
                    elevationPtr, returnLength, out _))
                {
                    var elevation = Marshal.PtrToStructure<TOKEN_ELEVATION>(elevationPtr);
                    if (elevation.TokenIsElevated != 0)
                        return "High";
                }
            }
            finally
            {
                Marshal.FreeHGlobal(elevationPtr);
            }

            uint evalReturnLength = 0;
            AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevationType,
                IntPtr.Zero, 0, out evalReturnLength);

            IntPtr typePtr = Marshal.AllocHGlobal((int)evalReturnLength);
            try
            {
                if (AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevationType,
                    typePtr, evalReturnLength, out _))
                {
                    int type = Marshal.ReadInt32(typePtr);
                    return type switch
                    {
                        1 => "Default",
                        2 => "Full",
                        3 => "Limited",
                        _ => "Unknown"
                    };
                }
            }
            finally
            {
                Marshal.FreeHGlobal(typePtr);
            }

            return "Medium";
        }
        catch
        {
            return "Unknown";
        }
        finally
        {
            if (hToken != IntPtr.Zero)
                Kernel32.CloseHandle(hToken);
        }
    }

    public static string GetProcessIntegrityLevel(IntPtr hProcess)
    {
        IntPtr hToken = IntPtr.Zero;
        try
        {
            if (!AdvApi32.OpenProcessToken(hProcess, AdvApi32.TOKEN_QUERY | AdvApi32.TOKEN_DUPLICATE, out hToken))
                return "Unknown";

            uint returnLength = 0;
            AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevation,
                IntPtr.Zero, 0, out returnLength);

            IntPtr elevationPtr = Marshal.AllocHGlobal((int)returnLength);
            try
            {
                if (AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevation,
                    elevationPtr, returnLength, out _))
                {
                    var elevation = Marshal.PtrToStructure<TOKEN_ELEVATION>(elevationPtr);
                    if (elevation.TokenIsElevated != 0)
                        return "High";
                }
            }
            finally
            {
                Marshal.FreeHGlobal(elevationPtr);
            }

            try
            {
                ulong returnLengthSid = 0;
                AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenUser,
                    IntPtr.Zero, 0, out _);

                IntPtr userPtr = Marshal.AllocHGlobal((int)returnLengthSid);
                try
                {
                    if (AdvApi32.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenUser,
                        userPtr, (uint)returnLengthSid, out _))
                    {
                        var tokenUser = Marshal.PtrToStructure<TOKEN_USER>(userPtr);
                        int sidType = 0;

                        if (AdvApi32.IsWellKnownSid(tokenUser.User.Sid, 16))
                            return "System";
                        if (AdvApi32.IsWellKnownSid(tokenUser.User.Sid, 17))
                            return "High";
                        if (AdvApi32.IsWellKnownSid(tokenUser.User.Sid, 18))
                            return "Medium";
                        if (AdvApi32.IsWellKnownSid(tokenUser.User.Sid, 19))
                            return "Low";
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(userPtr);
                }
            }
            catch
            {
            }

            return "Medium";
        }
        catch
        {
            return "Unknown";
        }
        finally
        {
            if (hToken != IntPtr.Zero)
                Kernel32.CloseHandle(hToken);
        }
    }
}
