using System.Text.Json;

namespace CrosshairMarker;

internal sealed class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string configPath;

    public ConfigStore()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Crosslay");
        Directory.CreateDirectory(directory);
        configPath = Path.Combine(directory, "config.json");
    }

    public AppConfig Load()
    {
        if (!File.Exists(configPath))
        {
            var defaults = new AppConfig();
            defaults.Normalize();
            SaveAtomic(defaults);
            return defaults;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            config.Normalize();
            return config;
        }
        catch
        {
            var config = new AppConfig();
            config.Normalize();
            return config;
        }
    }

    public void SaveAtomic(AppConfig config)
    {
        config.Normalize();
        var tempPath = configPath + ".tmp";
        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, configPath, overwrite: true);
    }
}
