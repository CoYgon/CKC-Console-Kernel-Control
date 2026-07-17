using System.Security.AccessControl;
using System.Text;

namespace CKC.Services;

public static class FileSystemManager
{
    public static string HexDump(string filePath, int offset, int length)
    {
        if (!File.Exists(filePath))
            return $"Dosya bulunamadı: {filePath}";

        var sb = new StringBuilder();
        int bytesPerLine = 16;
        long fileLength = new FileInfo(filePath).Length;

        if (offset >= fileLength)
            return "Offset dosya boyutunun ötesinde.";

        if (offset + length > fileLength)
            length = (int)(fileLength - offset);

        int totalLines = (int)Math.Ceiling(length / (double)bytesPerLine);
        int lineNumberWidth = (offset + length).ToString("X").Length;
        if (lineNumberWidth < 8) lineNumberWidth = 8;

        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            fs.Seek(offset, SeekOrigin.Begin);

            byte[] buffer = new byte[bytesPerLine];
            int bytesRead;
            int totalRead = 0;
            int currentOffset = offset;

            while ((bytesRead = fs.Read(buffer, 0, bytesPerLine)) > 0 && totalRead < length)
            {
                if (totalRead + bytesRead > length)
                    bytesRead = length - totalRead;

                string addr = currentOffset.ToString($"X{lineNumberWidth}");
                sb.Append(addr).Append("  ");

                for (int i = 0; i < bytesPerLine; i++)
                {
                    if (i < bytesRead)
                        sb.Append($"{buffer[i]:X2} ");
                    else
                        sb.Append("   ");

                    if (i == 7) sb.Append(" ");
                }

                sb.Append(" |");
                for (int i = 0; i < bytesPerLine; i++)
                {
                    if (i < bytesRead)
                    {
                        char c = (char)buffer[i];
                        sb.Append(char.IsControl(c) ? '.' : c);
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }
                sb.AppendLine("|");

                currentOffset += bytesRead;
                totalRead += bytesRead;
            }
        }
        catch (Exception ex)
        {
            return $"Hata: {ex.Message}";
        }

        return sb.ToString();
    }

    public static string GetFileInfo(string path)
    {
        try
        {
            var fi = new FileInfo(path);
            if (!fi.Exists) return $"Dosya bulunamadı: {path}";

            var sb = new StringBuilder();
            sb.AppendLine($"Dosya Adı       : {fi.Name}");
            sb.AppendLine($"Tam Yol         : {fi.FullName}");
            sb.AppendLine($"Boyut           : {FormatFileSize((ulong)fi.Length)} ({fi.Length:N0} bayt)");
            sb.AppendLine($"Oluşturulma     : {fi.CreationTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Değiştirilme    : {fi.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Son Erişim      : {fi.LastAccessTime:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Öznitelikler    : {fi.Attributes}");

            try
            {
                var security = fi.GetAccessControl();
                var owner = security.GetOwner(typeof(System.Security.Principal.NTAccount));
                sb.AppendLine($"Sahibi          : {owner}");
            }
            catch
            {
                sb.AppendLine($"Sahibi          : Erişilemedi");
            }

            try
            {
                sb.AppendLine($"Dizin           : {fi.DirectoryName}");
                sb.AppendLine($"Uzantı          : {fi.Extension}");
                sb.AppendLine($"Salt Okunur     : {fi.IsReadOnly}");
            }
            catch
            {
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Hata: {ex.Message}";
        }
    }

    public static string GetDiskInfo(string driveLetter)
    {
        try
        {
            if (string.IsNullOrEmpty(driveLetter))
                return "Sürücü harfi belirtilmedi.";

            if (driveLetter.Length == 1)
                driveLetter = driveLetter + ":\\";
            else if (driveLetter.Length == 2 && driveLetter[1] == ':')
                driveLetter = driveLetter + "\\";

            var di = new DriveInfo(driveLetter[0].ToString());
            if (!di.IsReady) return $"{di.Name} ({di.VolumeLabel}) - Hazır değil";

            var sb = new StringBuilder();
            sb.AppendLine($"Sürücü          : {di.Name}");
            sb.AppendLine($"Etiket          : {di.VolumeLabel}");
            sb.AppendLine($"Dosya Sistemi   : {di.DriveFormat}");
            sb.AppendLine($"Tip             : {di.DriveType}");
            sb.AppendLine($"Toplam Boyut    : {FormatFileSize((ulong)di.TotalSize)} ({di.TotalSize:N0} bayt)");
            sb.AppendLine($"Boş Alan        : {FormatFileSize((ulong)di.AvailableFreeSpace)} ({di.AvailableFreeSpace:N0} bayt)");
            sb.AppendLine($"Kullanılan      : {FormatFileSize((ulong)(di.TotalSize - di.AvailableFreeSpace))} ({(di.TotalSize - di.AvailableFreeSpace):N0} bayt)");
            sb.AppendLine($"Kullanım Oranı  : %{100.0 - (di.AvailableFreeSpace / (double)di.TotalSize * 100.0):F1}");

            try
            {
                var volumeInfo = GetVolumeInformation(driveLetter);
                if (!string.IsNullOrEmpty(volumeInfo))
                    sb.Append(volumeInfo);
            }
            catch
            {
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Hata: {ex.Message}";
        }
    }

    public static List<(string path, long size)> SearchFiles(string pattern, string directory)
    {
        var results = new List<(string, long)>();

        try
        {
            var dir = new DirectoryInfo(directory);
            if (!dir.Exists) return results;

            foreach (var file in dir.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly))
            {
                try
                {
                    results.Add((file.FullName, file.Length));
                }
                catch
                {
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch
        {
        }

        return results;
    }

    public static string FormatFileSize(ulong bytes)
    {
        if (bytes >= (1024L * 1024 * 1024 * 1024))
            return $"{bytes / (1024.0 * 1024 * 1024 * 1024):F2} TB";
        if (bytes >= (1024 * 1024 * 1024))
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        if (bytes >= (1024 * 1024))
            return $"{bytes / (1024.0 * 1024):F2} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024.0:F2} KB";
        return $"{bytes} B";
    }

    private static string GetVolumeInformation(string drivePath)
    {
        try
        {
            var sb = new StringBuilder();
            var fsb = new StringBuilder();
            uint serial = 0;
            uint maxLen = 0;
            uint flags = 0;

            if (GetVolumeInformation(drivePath, sb, (uint)sb.Capacity, out serial, out maxLen, out flags, fsb, (uint)fsb.Capacity))
            {
                return $"Seri No         : {serial:X8}\n" +
                       $"Maksimum Yol Uz. : {maxLen}\n" +
                       $"FS Bayrakları    : 0x{flags:X8}\n";
            }

            return "";
        }
        catch
        {
            return "";
        }
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
    private static extern bool GetVolumeInformation(
        string lpRootPathName,
        StringBuilder lpVolumeNameBuffer,
        uint nVolumeNameSize,
        out uint lpVolumeSerialNumber,
        out uint lpMaximumComponentLength,
        out uint lpFileSystemFlags,
        StringBuilder lpFileSystemNameBuffer,
        uint nFileSystemNameSize);
}
