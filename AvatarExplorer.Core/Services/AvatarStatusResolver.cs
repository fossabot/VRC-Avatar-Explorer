using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class AvatarStatusResolver
{
    internal static AvatarStatus Resolve(string? avatarPath, Item item, List<CommonAvatar> commonAvatars)
    {
        AvatarStatus avatarStatus = new();
        if (string.IsNullOrEmpty(avatarPath)) return avatarStatus;
        
        if (item.SupportedAvatars.Count == 0 || item.SupportedAvatars.Contains(avatarPath))
            avatarStatus.IsSupported = true;

        if (item.Type != ItemType.Clothing) return avatarStatus;
        
        // アイテムの対応アバターが共通素体グループで登録されていた時用の処理
        foreach (string supportedAvatar in item.SupportedAvatars)
        {
            if (!supportedAvatar.StartsWith(CommonAvatar.InternalPathPrefix)) continue;

            CommonAvatar? group = commonAvatars.FirstOrDefault(g => g.GroupName == CommonAvatar.GetGroupName(supportedAvatar));
            if (group != null && group.Avatars.Contains(avatarPath))
            {
                avatarStatus.IsCommon = true;
                avatarStatus.CommonAvatarName = group.GroupName;
                return avatarStatus;
            }
        }

        CommonAvatar[] groupsForPath = commonAvatars
            .Where(x => x.Avatars.Contains(avatarPath))
            .ToArray();

        if (groupsForPath.Length == 0) return avatarStatus;

        foreach (string supportedAvatar in item.SupportedAvatars)
        {
            CommonAvatar? group = groupsForPath.FirstOrDefault(g => g.Avatars.Contains(supportedAvatar));
            if (group != null)
            {
                avatarStatus.IsCommon = true;
                avatarStatus.CommonAvatarName = group.GroupName;
                break;
            }
        }

        return avatarStatus;
    }
}
