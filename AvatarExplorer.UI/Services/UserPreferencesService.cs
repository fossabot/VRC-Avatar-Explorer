using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Services;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI.Services;

internal static class UserPreferencesService
{
    internal static UserPreferences Load(string path)
    {
        return FileSystemService.DeserializeClass<UserPreferences>(path) ?? new();
    }

    internal static void Save(UserPreferences userPreferences)
    {
        FileSystemService.SerializeClass(userPreferences, SystemPath.UserPreferencesFilePath);
    }
}
