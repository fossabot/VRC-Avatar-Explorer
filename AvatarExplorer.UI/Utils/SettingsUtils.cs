using System.IO;
using System.Text.Json;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI.Utils;

internal static class SettingsUtils
{
    internal static UserUiPreferences LoadUserPreferences(string path)
    {
        try
        {
            if (!File.Exists(path)) return new();

            string json = File.ReadAllText(path);
            UserUiPreferences userUiPreferences = JsonSerializer.Deserialize<UserUiPreferences>(json) ?? new();

            return userUiPreferences;
        }
        catch
        {
            return new();
        }
    }
}
