using System.Text;
using CKC.Models;

namespace CKC.Core;

public static class ConsoleFormatter
{
    public static void WriteInfo(string text)
    {
        WriteLineColored(text, ConsoleColor.Cyan);
    }

    public static void WriteSuccess(string text)
    {
        WriteLineColored(text, ConsoleColor.Green);
    }

    public static void WriteWarning(string text)
    {
        WriteLineColored(text, ConsoleColor.Yellow);
    }

    public static void WriteError(string text)
    {
        WriteLineColored(text, ConsoleColor.Red);
    }

    public static void WriteHeader(string text)
    {
        int width = text.Length + 4;
        string border = new string('=', width);
        WriteLineColored(border, ConsoleColor.DarkCyan);
        WriteLineColored($"= {text} =", ConsoleColor.DarkCyan);
        WriteLineColored(border, ConsoleColor.DarkCyan);
    }

    public static void WriteTable<T>(List<T> items, params Func<T, string>[] columns)
    {
        if (items == null || items.Count == 0 || columns.Length == 0)
            return;

        var rows = new List<string[]>();
        foreach (var item in items)
        {
            var row = new string[columns.Length];
            for (int i = 0; i < columns.Length; i++)
                row[i] = columns[i](item) ?? "";
            rows.Add(row);
        }

        var widths = new int[columns.Length];
        for (int i = 0; i < columns.Length; i++)
        {
            widths[i] = rows.Max(r => r[i]?.Length ?? 0);
        }

        var sb = new StringBuilder();
        foreach (var row in rows)
        {
            for (int i = 0; i < row.Length; i++)
            {
                sb.Append(row[i]?.PadRight(widths[i]) ?? new string(' ', widths[i]));
                if (i < row.Length - 1)
                    sb.Append("  ");
            }
            WriteLineColored(sb.ToString(), ConsoleColor.Gray);
            sb.Clear();
        }
    }

    public static void WriteProgressBar(double percent, int width = 30)
    {
        percent = Math.Clamp(percent, 0, 100);
        int filled = (int)(percent / 100.0 * width);
        int empty = width - filled;

        Console.Write("[");
        Console.ForegroundColor = percent >= 80 ? ConsoleColor.Green :
                                  percent >= 50 ? ConsoleColor.Yellow :
                                  ConsoleColor.Red;
        Console.Write(new string('█', filled));
        Console.ResetColor();
        Console.Write(new string('░', empty));
        Console.Write("] ");
        WriteColored($"{percent:F1}%", ConsoleColor.White);
    }

    public static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    public static void WriteLineColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    public static void WriteSeparator(char c = '─', int count = 50)
    {
        WriteLineColored(new string(c, count), ConsoleColor.DarkGray);
    }

    public static void WriteCommandOutput(string label, string value)
    {
        WriteColored($"{label}: ", ConsoleColor.Cyan);
        WriteLineColored(value, ConsoleColor.White);
    }

    public static string FormatBytes(ulong bytes)
    {
        if (bytes >= 1024UL * 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024 * 1024 * 1024):F2} TB";
        if (bytes >= 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        if (bytes >= 1024 * 1024)
            return $"{bytes / (1024.0 * 1024):F2} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024.0:F2} KB";
        return $"{bytes} B";
    }

    public static string FormatTimeSpan(TimeSpan ts)
    {
        if (ts.TotalDays >= 1)
            return $"{(int)ts.TotalDays}d {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    public static void ClearAndWriteHeader(string header)
    {
        Console.Clear();
        WriteHeader(header);
    }

    public static string ApplyColor(string text, ConsoleColor color)
    {
        return $"\u001b[{(int)color + 30}m{text}\u001b[0m";
    }

    public static void ResetColor()
    {
        Console.ResetColor();
    }
}
