using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using CKC.Commands;
using CKC.Core;

namespace CKC;

class Program
{
    static CommandDispatcher _dispatcher = new();

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        SetupCommands();

        if (args.Length > 0)
        {
            ExecuteCommandLine(string.Join(" ", args));
            return;
        }

        RunRepl();
    }

    static void SetupCommands()
    {
        _dispatcher.RegisterCommand("help", "Kullanilabilir tum komutlari listeler", HelpExecute);
        _dispatcher.RegisterCommand("sysinfo", "Sistem bilgilerini gosterir (OS, CPU, bellek, uptime)", SystemCommands.ExecuteSysInfo);
        _dispatcher.RegisterCommand("cpuinfo", "CPU mimari ve ozelliklerini gosterir", SystemCommands.ExecuteCpuInfo);
        _dispatcher.RegisterCommand("uptime", "Sistem calisma suresini gosterir", SystemCommands.ExecuteUptime);
        _dispatcher.RegisterCommand("biosinfo", "BIOS/UEFI bilgilerini gosterir", SystemCommands.ExecuteBiosInfo);
        _dispatcher.RegisterCommand("osver", "Isletim sistemi surumunu gosterir", SystemCommands.ExecuteOsVersion);
        _dispatcher.RegisterCommand("whoami", "Mevcut kullanici bilgilerini gosterir", SecurityCommands.ExecuteWhoAmI);
        _dispatcher.RegisterCommand("elevated", "Yonetici yetkisi kontrolu yapar", SystemCommands.ExecuteElevated);
        _dispatcher.RegisterCommand("privileges", "Mevcut process yetkilerini listeler", SecurityCommands.ExecutePrivileges);
        _dispatcher.RegisterCommand("elevate", "Process'i yonetici olarak yeniden baslatmayi dener", SecurityCommands.ExecuteElevate);

        _dispatcher.RegisterCommand("ps", "Calisan processleri listeler (ps tree, ps <pid>)", ProcessCommands.ExecutePs);
        _dispatcher.RegisterCommand("kill", "Process sonlandirir: kill <pid>", ProcessCommands.ExecuteKill);
        _dispatcher.RegisterCommand("suspend", "Process'i dondurur: suspend <pid>", ProcessCommands.ExecuteSuspend);
        _dispatcher.RegisterCommand("resume", "Dondurulmus process'i devam ettirir: resume <pid>", ProcessCommands.ExecuteResume);
        _dispatcher.RegisterCommand("procinfo", "Process detayli bilgi: procinfo <pid>", ProcessCommands.ExecuteProcInfo);
        _dispatcher.RegisterCommand("threads", "Process thread'lerini listeler: threads <pid>", ProcessCommands.ExecuteThreads);
        _dispatcher.RegisterCommand("modules", "Process modullerini listeler: modules <pid>", ProcessCommands.ExecuteModules);
        _dispatcher.RegisterCommand("handles", "Process handle'larini gosterir: handles <pid>", ProcessCommands.ExecuteHandles);

        _dispatcher.RegisterCommand("sysmem", "Fiziksel ve sanal bellek durumu", MemoryCommands.ExecuteSysMem);
        _dispatcher.RegisterCommand("vmmap", "Sanal bellek haritasini gosterir", MemoryCommands.ExecuteVMMap);
        _dispatcher.RegisterCommand("pool", "Kernel pool bellek bilgisi", MemoryCommands.ExecutePool);

        _dispatcher.RegisterCommand("netstat", "Aktif ag baglantilarini listeler (netstat -p tcp/udp)", NetworkCommands.ExecuteNetstat);
        _dispatcher.RegisterCommand("connections", "PID'e gore baglantilari gosterir: connections <pid>", NetworkCommands.ExecuteConnections);

        _dispatcher.RegisterCommand("hexdump", "Dosya hex dump gosterimi: hexdump <dosya> [offset] [length]", FileCommands.ExecuteHexDump);
        _dispatcher.RegisterCommand("fileinfo", "Dosya detayli bilgi: fileinfo <dosya>", FileCommands.ExecuteFileInfo);
        _dispatcher.RegisterCommand("diskinfo", "Disk/Drive bilgisi: diskinfo [drive]", FileCommands.ExecuteDiskInfo);
        _dispatcher.RegisterCommand("search", "Dosya ara: search <pattern> [dizin]", FileCommands.ExecuteSearch);

        _dispatcher.RegisterCommand("service", "Servisleri listeler: service (running/stopped)", ServiceCommands.ExecuteServiceList);
        _dispatcher.RegisterCommand("serviceinfo", "Servis detay: serviceinfo <servis_adi>", ServiceCommands.ExecuteServiceInfo);
        _dispatcher.RegisterCommand("servicestart", "Servis baslatir: servicestart <servis_adi>", ServiceCommands.ExecuteServiceStart);
        _dispatcher.RegisterCommand("servicestop", "Servis durdurur: servicestop <servis_adi>", ServiceCommands.ExecuteServiceStop);
        _dispatcher.RegisterCommand("servicerestart", "Servisi yeniden baslatir: servicerestart <servis_adi>", ServiceCommands.ExecuteServiceRestart);

        _dispatcher.RegisterCommand("regquery", "Registry anahtari sorgular: regquery <path>", RegistryCommands.ExecuteRegQuery);
        _dispatcher.RegisterCommand("regenum", "Registry alt anahtarlarini listeler: regenum <path>", RegistryCommands.ExecuteRegEnum);

        _dispatcher.RegisterCommand("kernelmodules", "Yuklu kernel modullerini/driverlarini listeler", KernelCommands.ExecuteKernelModules);
        _dispatcher.RegisterCommand("driverinfo", "Driver/modul bilgisi: driverinfo [isim]", KernelCommands.ExecuteDriverInfo);

        _dispatcher.RegisterCommand("windows", "Pencere listesini gosterir", WindowCommands.ExecuteWindows);
        _dispatcher.RegisterCommand("wininfo", "Pencere detay: wininfo <handle_hex>", WindowCommands.ExecuteWindowInfo);
        _dispatcher.RegisterCommand("foreground", "Aktif on plandaki pencereyi gosterir", WindowCommands.ExecuteForeground);

        _dispatcher.RegisterCommand("history", "Komut gecmisini gosterir (clear/save/load)", ScriptingCommands.ExecuteHistory);
        _dispatcher.RegisterCommand("alias", "Alias yonetimi: alias [set <a> <c>] [del <a>]", ScriptingCommands.ExecuteAlias);
        _dispatcher.RegisterCommand("echo", "Metni ekrana yazdirir: echo <metin>", ScriptingCommands.ExecuteEcho);

        _dispatcher.RegisterCommand("shell", "Yerel shell komutu calistirir: shell <komut>", ShellExecute);
        _dispatcher.RegisterCommand("clear", "Ekrani temizler", ClearExecute);
        _dispatcher.RegisterCommand("cls", "Ekrani temizler", ClearExecute);
        _dispatcher.RegisterCommand("exit", "Terminali kapatir", ExitExecute);
        _dispatcher.RegisterCommand("quit", "Terminali kapatir", ExitExecute);

        _dispatcher.AddAlias("services", "service");
        _dispatcher.AddAlias("list", "ps");
        _dispatcher.AddAlias("mem", "sysmem");
        _dispatcher.AddAlias("ver", "osver");
        _dispatcher.AddAlias("drivers", "kernelmodules");

        ScriptingCommands.Dispatcher = _dispatcher;
    }

    static void RunRepl()
    {
        Console.Clear();
        PrintHeader();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("ckc@kernel:~$ ");
            Console.ResetColor();

            string? input = Console.ReadLine();
            if (input is null) break;

            input = input.Trim();
            if (string.IsNullOrEmpty(input)) continue;

            try
            {
                string output = _dispatcher.Dispatch(input);
                if (!string.IsNullOrEmpty(output))
                {
                    Console.WriteLine(output);
                }
            }
            catch (Exception ex)
            {
                ConsoleFormatter.WriteError($"Beklenmeyen hata: {ex.Message}");
                if (ex.InnerException != null)
                    ConsoleFormatter.WriteError($"  Ic hata: {ex.InnerException.Message}");
            }

            Console.WriteLine();
        }
    }

    static void ExecuteCommandLine(string input)
    {
        try
        {
            string output = _dispatcher.Dispatch(input);
            if (!string.IsNullOrEmpty(output))
                Console.WriteLine(output);
        }
        catch (Exception ex)
        {
            ConsoleFormatter.WriteError($"Hata: {ex.Message}");
        }
    }

    static string HelpExecute(string[] args)
    {
        if (args.Length > 0)
            return "";

        var sb = new StringBuilder();
        var commands = _dispatcher.GetRegisteredCommands();
        sb.AppendLine(ConsoleFormatter.ApplyColor("╔══════════════════════════════════════════════╗", ConsoleColor.DarkCyan));
        sb.AppendLine(ConsoleFormatter.ApplyColor("║          CKC KOMUTLARI                      ║", ConsoleColor.DarkCyan));
        sb.AppendLine(ConsoleFormatter.ApplyColor("╚══════════════════════════════════════════════╝", ConsoleColor.DarkCyan));
        sb.AppendLine();

        var categories = new Dictionary<string, List<(string, string)>>
        {
            ["SISTEM"] = new(),
            ["PROCESS"] = new(),
            ["BELLEK"] = new(),
            ["AG"] = new(),
            ["DOSYA"] = new(),
            ["SERVIS"] = new(),
            ["REGISTRY"] = new(),
            ["KERNEL"] = new(),
            ["PENCERE"] = new(),
            ["BETIK"] = new(),
            ["DIGER"] = new()
        };

        var catMap = new Dictionary<string, string>
        {
            ["sysinfo"] = "SISTEM", ["cpuinfo"] = "SISTEM", ["uptime"] = "SISTEM", ["biosinfo"] = "SISTEM",
            ["osver"] = "SISTEM", ["elevated"] = "SISTEM", ["whoami"] = "SISTEM", ["privileges"] = "SISTEM", ["elevate"] = "SISTEM",
            ["ps"] = "PROCESS", ["kill"] = "PROCESS", ["suspend"] = "PROCESS", ["resume"] = "PROCESS",
            ["procinfo"] = "PROCESS", ["threads"] = "PROCESS", ["modules"] = "PROCESS", ["handles"] = "PROCESS",
            ["sysmem"] = "BELLEK", ["vmmap"] = "BELLEK", ["pool"] = "BELLEK",
            ["netstat"] = "AG", ["connections"] = "AG",
            ["hexdump"] = "DOSYA", ["fileinfo"] = "DOSYA", ["diskinfo"] = "DOSYA", ["search"] = "DOSYA",
            ["service"] = "SERVIS", ["serviceinfo"] = "SERVIS", ["servicestart"] = "SERVIS", ["servicestop"] = "SERVIS", ["servicerestart"] = "SERVIS",
            ["regquery"] = "REGISTRY", ["regenum"] = "REGISTRY",
            ["kernelmodules"] = "KERNEL", ["driverinfo"] = "KERNEL",
            ["windows"] = "PENCERE", ["wininfo"] = "PENCERE", ["foreground"] = "PENCERE",
            ["history"] = "BETIK", ["alias"] = "BETIK", ["echo"] = "BETIK",
            ["shell"] = "DIGER", ["clear"] = "DIGER", ["exit"] = "DIGER", ["help"] = "DIGER"
        };

        foreach (var kvp in commands)
        {
            var cat = catMap.GetValueOrDefault(kvp.Key, "DIGER");
            if (categories.ContainsKey(cat))
                categories[cat].Add((kvp.Key, kvp.Value.description));
        }

        var headerColor = ConsoleFormatter.ApplyColor;
        foreach (var cat in categories)
        {
            if (cat.Value.Count == 0) continue;
            sb.AppendLine(headerColor($"  ┌─ {cat.Key} ──────────────────────", ConsoleColor.Yellow));
            foreach (var (cmd, desc) in cat.Value.OrderBy(x => x.Item1))
            {
                sb.AppendLine($"  │ {cmd,-16} {desc}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("Detayli bilgi icin: help <komut_adi>");
        return sb.ToString();
    }

    static string ClearExecute(string[] args)
    {
        Console.Clear();
        PrintHeader();
        return "";
    }

    static string ExitExecute(string[] args)
    {
        ConsoleFormatter.WriteWarning("Terminal kapatiliyor. Gorusmek uzere!");
        Environment.Exit(0);
        return "";
    }

    static string ShellExecute(string[] args)
    {
        if (args.Length == 0)
            return ConsoleFormatter.ApplyColor("Hata: Calistirilacak komut girin. Orn: shell ipconfig /all", ConsoleColor.Red);

        string command = string.Join(" ", args);
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        string shell = isWindows ? "cmd.exe" : "/bin/bash";
        string arguments = isWindows ? $"/c {command}" : $"-c \"{command}\"";

        var psi = new ProcessStartInfo
        {
            FileName = shell,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = Process.Start(psi);
            if (process == null)
                return "Process baslatilamadi.";
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit(30000);

            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(output))
                sb.AppendLine(output.TrimEnd());
            if (!string.IsNullOrEmpty(error))
                sb.AppendLine(ConsoleFormatter.ApplyColor($"[Hata]: {error.TrimEnd()}", ConsoleColor.Red));

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return ConsoleFormatter.ApplyColor($"Komut calistirilirken hata: {ex.Message}", ConsoleColor.Red);
        }
    }

    static void PrintHeader()
    {
        var c = ConsoleFormatter.ApplyColor;
        Console.WriteLine(c("╔════════════════════════════════════════════════════════════╗", ConsoleColor.Red));
        Console.WriteLine(c("║           CKC | CONSOLE KERNEL CONTROL v3.0              ║", ConsoleColor.Red));
        Console.WriteLine(c("║        Gelistirici: CoYgon | Kernel Seviyesinde Komut    ║", ConsoleColor.Red));
        Console.WriteLine(c("╚════════════════════════════════════════════════════════════╝", ConsoleColor.Red));
        Console.WriteLine();
        Console.WriteLine(c("  'help' yazarak tum komutlari goruntuleyin | 'exit' cikis", ConsoleColor.DarkGray));
        Console.WriteLine();
    }
}
