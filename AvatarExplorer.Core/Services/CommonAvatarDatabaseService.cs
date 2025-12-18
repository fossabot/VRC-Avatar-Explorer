using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.V1;

namespace AvatarExplorer.Core.Services;

internal static class CommonAvatarDatabaseService
{
    internal static List<CommonAvatar> Load(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();
        return FileSystemService.DeserializeClass<List<CommonAvatar>>(path) ?? [];
    }

    internal static List<CommonAvatar> LoadFromV1(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();
        return MigrateCommonAvatarsFromV1(FileSystemService.DeserializeClass<List<CommonAvatarV1>>(path) ?? []);
    }

    private static List<CommonAvatar> MigrateCommonAvatarsFromV1(List<CommonAvatarV1> commonAvatars)
    {
        return commonAvatars.Select(CommonAvatar.FromV1).ToList();
    }
}
