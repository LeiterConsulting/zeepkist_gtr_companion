using System;
using System.IO;
using System.Text.Json;

namespace LeiterConsulting.Zeepkist.GtrCompanion.Hub;

internal sealed class HubSettings
{
    public bool LocalCompanionEnabled { get; set; }
}

internal sealed class HubSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsPath;

    public HubSettingsStore()
    {
        var dataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Zeepkist GTR Companion");
        _settingsPath = Path.Combine(dataDirectory, "settings.json");
    }

    public bool Exists => File.Exists(_settingsPath);

    public HubSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new HubSettings();
            }

            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<HubSettings>(json, SerializerOptions)
                ?? new HubSettings();
        }
        catch (JsonException)
        {
            return new HubSettings();
        }
        catch (IOException)
        {
            return new HubSettings();
        }
    }

    public void Save(HubSettings settings)
    {
        var directory = Path.GetDirectoryName(_settingsPath)
            ?? throw new InvalidOperationException("The settings directory could not be determined.");
        Directory.CreateDirectory(directory);

        var temporaryPath = _settingsPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, SerializerOptions));
        File.Move(temporaryPath, _settingsPath, true);
    }
}
