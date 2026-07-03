namespace CrosshairMarker;

internal static class AppRuntimeLog
{
    private static readonly object Sync = new();
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Crosslay",
        "crosslay.log");

    public static void Info(string message) => Write("INFO", message);

    public static void Error(string message, Exception? exception = null)
    {
        Write("ERROR", exception is null ? message : $"{message}: {exception}");
    }

    private static void Write(string level, string message)
    {
        try
        {
            var directory = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            lock (Sync)
            {
                File.AppendAllText(
                    LogPath,
                    $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [{level}] {message}{Environment.NewLine}");
                TrimIfNeeded();
            }
        }
        catch
        {
            // Logging must never break the overlay.
        }
    }

    private static void TrimIfNeeded()
    {
        var info = new FileInfo(LogPath);
        if (!info.Exists || info.Length <= 512 * 1024)
        {
            return;
        }

        var lines = File.ReadLines(LogPath).TakeLast(2000).ToArray();
        File.WriteAllLines(LogPath, lines);
    }
}
