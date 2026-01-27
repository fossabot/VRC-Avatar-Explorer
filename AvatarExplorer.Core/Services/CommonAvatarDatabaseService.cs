using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class CommonAvatarDatabaseService
{
    internal static List<CommonAvatar> Load(string path)
    {
        return FileSystemService.DeserializeClass<List<CommonAvatar>>(path) ?? [];
    }

    internal static void Save(List<CommonAvatar> commonAvatars)
    {
        FileSystemService.SerializeClass(commonAvatars, SystemPath.CommonAvatarDatabasePath);
    }
}
