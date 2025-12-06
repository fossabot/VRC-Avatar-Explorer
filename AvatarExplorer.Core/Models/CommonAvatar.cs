using AvatarExplorer.Core.Models.V1;

namespace AvatarExplorer.Core.Models;

public class CommonAvatar
{
    /// <summary>
    /// 共通素体グループの名前を取得または設定します。
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 共通素体のアバターのパスを取得または設定します。
    /// </summary>
    public List<string> Avatars { get; set; } = new List<string>();

    public static CommonAvatar FromV1(CommonAvatarV1 commonAvatar)
    {
        var migratedCommonAvatar = new CommonAvatar()
        {
            GroupName = commonAvatar.Name,
            Avatars = new List<string>(commonAvatar.Avatars),
        };

        MigrateItemPaths(migratedCommonAvatar.Avatars);

        return migratedCommonAvatar;
    }

    private static void MigrateItemPaths(List<string> paths)
    {
        for (int i = 0; i < paths.Count; i++)
        {
            paths[i] = MigrateItemPath(paths[i]);
        }
    }

    private static string MigrateItemPath(string path)
        => path.Replace("Datas\\Items\\", "<sys>");
}
