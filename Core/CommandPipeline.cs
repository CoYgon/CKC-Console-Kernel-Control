using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CKC.Core;

public static class CommandPipeline
{
    public static bool HasPipeline(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        bool inQuotes = false;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '"')
                inQuotes = !inQuotes;
            else if (input[i] == '|' && !inQuotes)
                return true;
        }

        return false;
    }

    public static List<string> ParsePipeline(string input)
    {
        var segments = new List<string>();
        if (string.IsNullOrEmpty(input))
            return segments;

        bool inQuotes = false;
        var current = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
            }
            else if (c == '|' && !inQuotes)
            {
                var segment = current.ToString().Trim();
                if (segment.Length > 0)
                    segments.Add(segment);
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        var lastSegment = current.ToString().Trim();
        if (lastSegment.Length > 0)
            segments.Add(lastSegment);

        return segments;
    }

    public static string ExecutePipeline(string input)
    {
        if (!HasPipeline(input))
            return input;

        var segments = ParsePipeline(input);
        if (segments.Count == 0)
            return string.Empty;

        if (segments.Count == 1)
            return segments[0];

        string previousOutput = string.Empty;

        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            string commandWithArgs;

            if (i > 0 && !string.IsNullOrEmpty(previousOutput))
            {
                commandWithArgs = $"{segment} \"{previousOutput.TrimEnd()}\"";
            }
            else
            {
                commandWithArgs = segment;
            }

            if (i < segments.Count - 1)
            {
                previousOutput = ExecuteNativeCommandAndCapture(commandWithArgs);
            }
            else
            {
                return commandWithArgs;
            }
        }

        return previousOutput;
    }

    private static string ExecuteNativeCommandAndCapture(string command)
    {
        try
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            string shell = isWindows ? "cmd.exe" : "/bin/bash";
            string args = isWindows ? $"/c {command}" : $"-c \"{command.Replace("\"", "\\\"")}\"";

            var psi = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(psi);
            if (process == null)
                return string.Empty;

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(error);
                Console.ResetColor();
            }

            return output.TrimEnd();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Pipeline hatası: {ex.Message}");
            Console.ResetColor();
            return string.Empty;
        }
    }
}
