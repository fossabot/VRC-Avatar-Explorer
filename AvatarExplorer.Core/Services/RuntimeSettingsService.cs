using System.Text.Json;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class RuntimeSettingsService
{
    internal static RuntimeSettings LoadRuntimeSettings(string path)
    {
        try
        {
            if (!File.Exists(path)) return new();

            string json = File.ReadAllText(path);
            var runtimeSettings = JsonSerializer.Deserialize<RuntimeSettings>(json) ?? new();

            return runtimeSettings;
        }
        catch
        {
            return new();
        }
    }

    public static void Save(RuntimeSettings settings)
    {
        try
        {
            FileSystemService.SerializeClass(settings, SystemPath.RuntimeSettingsFilePath);
        }
        catch
        {
            // Ignored
        }
    }

    public static bool TrySetDataRootDirectory(RuntimeSettings settings, string path)
    {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            settings.SetDataRootDirectory(SystemPath.DefaultItemsFolderPath);
            return false;
        }

        settings.SetDataRootDirectory(Path.GetFullPath(path));
        return true;
    }
}
