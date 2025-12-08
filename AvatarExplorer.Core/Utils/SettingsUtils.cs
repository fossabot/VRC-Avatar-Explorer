using System.Text.Json;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

internal static class SettingsUtils
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
}
