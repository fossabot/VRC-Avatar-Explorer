using System.IO;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Services;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI.Services;

internal static class UserPreferencesService
{
    internal static UserPreferences LoadUserPreferences(string path)
    {
        try
        {
            if (!File.Exists(path)) return new();
            return FileSystemService.DeserializeClass<UserPreferences>(path) ?? new();
        }
        catch
        {
            return new();
        }
    }

    internal static void SaveUserPreferences(UserPreferences userPreferences)
    {
        try
        {
            FileSystemService.SerializeClass(userPreferences, SystemPath.UserPreferencesFilePath);
        }
        catch
        {
            // Ignored
        }
    }
}
