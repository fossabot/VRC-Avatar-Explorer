using AvatarExplorer.Core.Models.V1;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Models;

public class CommonAvatar
{
    public string GroupName { get; set; } = string.Empty;
    public List<string> Avatars { get; set; } = new List<string>();

    public static CommonAvatar FromV1(CommonAvatarV1 commonAvatar)
    {
        CommonAvatar migratedCommonAvatar = new()
        {
            GroupName = commonAvatar.Name
        };

        migratedCommonAvatar.Avatars.AddRange(commonAvatar.Avatars);
        MigrateUtils.MigrateItemPaths(migratedCommonAvatar.Avatars);

        return migratedCommonAvatar;
    }
}
