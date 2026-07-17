using System.Text;

namespace CKC.Core;

public class CommandHistory
{
    private const int DefaultCapacity = 1000;
    private readonly List<string> _entries;
    private int _currentIndex;

    public CommandHistory(int maxCapacity = DefaultCapacity)
    {
        MaxCapacity = maxCapacity;
        _entries = new List<string>(MaxCapacity);
        _currentIndex = -1;
    }

    public int CurrentIndex => _currentIndex;

    public int Count => _entries.Count;

    public int MaxCapacity { get; }

    public void Add(string command)
    {
        if (string.IsNullOrEmpty(command))
            return;

        if (_entries.Count > 0 && _entries[^1] == command)
            return;

        if (_entries.Count >= MaxCapacity)
            _entries.RemoveAt(0);

        _entries.Add(command);
        _currentIndex = _entries.Count;
    }

    public string? GetPrevious()
    {
        if (_entries.Count == 0)
            return null;

        if (_currentIndex > 0)
            _currentIndex--;

        return _entries[_currentIndex];
    }

    public string? GetNext()
    {
        if (_entries.Count == 0)
            return null;

        if (_currentIndex < _entries.Count - 1)
            _currentIndex++;
        else
        {
            _currentIndex = _entries.Count;
            return null;
        }

        return _entries[_currentIndex];
    }

    public List<string> GetHistory()
    {
        return new List<string>(_entries);
    }

    public void Clear()
    {
        _entries.Clear();
        _currentIndex = -1;
    }

    public void SaveToFile(string path)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(path, _entries, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Komut geçmişi kaydedilemedi: {ex.Message}");
            Console.ResetColor();
        }
    }

    public void LoadFromFile(string path)
    {
        try
        {
            if (!File.Exists(path))
                return;

            var lines = File.ReadAllLines(path, Encoding.UTF8);
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                    Add(line);
            }

            _currentIndex = _entries.Count;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Komut geçmişi yüklenemedi: {ex.Message}");
            Console.ResetColor();
        }
    }
}
