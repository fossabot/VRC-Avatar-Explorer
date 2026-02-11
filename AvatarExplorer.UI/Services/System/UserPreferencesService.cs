using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Services.IO;
using AvatarExplorer.Core.Services.System;
using AvatarExplorer.UI.Models.Settings;
using ErrorOr;

namespace AvatarExplorer.UI.Services.System;

internal static class UserPreferencesService
{
    internal static UserPreferences Load(string path)
    {
        ErrorOr<UserPreferences> deserializeResult = FileSystemService.DeserializeClass<UserPreferences>(path);
        if (deserializeResult.IsError) ErrorManager.Instance.PostInternalError(deserializeResult.Errors.ToErrorString());

        return deserializeResult.Value;
    }

    internal static void Save(UserPreferences userPreferences)
    {
        FileSystemService.SerializeClass(userPreferences, SystemPath.UserPreferencesFilePath);
    }
}
