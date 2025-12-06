using AvatarExplorer.Core.Models.V1;
using AvatarExplorer.Core.Utils;

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

        MigrateUtils.MigrateItemPaths(migratedCommonAvatar.Avatars);

        return migratedCommonAvatar;
    }
}
