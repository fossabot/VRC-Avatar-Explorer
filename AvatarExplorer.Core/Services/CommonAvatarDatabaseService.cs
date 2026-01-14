using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class CommonAvatarDatabaseService
{
    internal static List<CommonAvatar> Load(string path)
    {
        try
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            return FileSystemService.DeserializeClass<List<CommonAvatar>>(path) ?? [];
        }
        catch
        {
            return [];
        }

    }

    internal static void Save(List<CommonAvatar> commonAvatars)
    {
        try
        {
            FileSystemService.SerializeClass(commonAvatars, SystemPath.CommonAvatarDatabasePath);
        }
        catch
        {
            // Ignored
        }
    }
}
