using System.IO;
using System.Text.Json;
using Poe2BuildManager.Models;

namespace Poe2BuildManager.Services;

public class SettingsService
{
    private readonly string _settingsPath;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData);

        var settingsFolder = Path.Combine(
            appData,
            "Poe2BuildManager");

        Directory.CreateDirectory(settingsFolder);

        _settingsPath = Path.Combine(
            settingsFolder,
            "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(_settingsPath))
        {
            var folder = GetDefaultBuildFolder();

            Directory.CreateDirectory(folder);

            return new AppSettings
            {
                BuildFolder = folder
            };
        }

        var json = File.ReadAllText(_settingsPath);

        return JsonSerializer.Deserialize<AppSettings>(json)
               ?? new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(
            settings,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_settingsPath, json);
    }

    private string GetDefaultBuildFolder()
    {
        return Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments),
            "My Games",
            "Path of Exile 2",
            "BuildPlanner");
    }
}