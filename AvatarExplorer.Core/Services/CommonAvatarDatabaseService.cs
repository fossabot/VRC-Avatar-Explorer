using System.Text.Json;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.V1;

namespace AvatarExplorer.Core.Services;

internal static class CommonAvatarDatabaseService
{
    internal static List<CommonAvatar> LoadCommonAvatarsData(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();

        string json = File.ReadAllText(path);
        List<CommonAvatar> commonAvatars = JsonSerializer.Deserialize<List<CommonAvatar>>(json) ?? [];

        return commonAvatars;
    }

    internal static List<CommonAvatar> LoadCommonAvatarsDataFromV1(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();

        string json = File.ReadAllText(path);
        List<CommonAvatarV1> commonAvatars = JsonSerializer.Deserialize<List<CommonAvatarV1>>(json) ?? [];

        return MigrateCommonAvatarsFromV1(commonAvatars);
    }

    private static List<CommonAvatar> MigrateCommonAvatarsFromV1(List<CommonAvatarV1> commonAvatars)
    {
        return commonAvatars.Select(CommonAvatar.FromV1).ToList();
    }
}
