using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

public static class ItemUtils
{
    internal static string GetAvatarNameFromPath(IReadOnlyList<Item> items, string? path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        return items.Where(i => i.Type == ItemType.Avatar).FirstOrDefault(x => x.ItemPath == path)?.Title ?? string.Empty;
    }

    internal static string GetAvatarNameFromDictionary(Dictionary<string, string> avatarNamesDictionary, string avatarPath)
    {
        avatarNamesDictionary.TryGetValue(avatarPath, out var avatarName);
        return avatarName ?? "";
    }

    internal static AvatarStatus GetAvatarStatus(string? avatarPath, Item item, List<CommonAvatar> commonAvatars)
    {
        var avatarStatus = new AvatarStatus();
        if (string.IsNullOrEmpty(avatarPath)) return avatarStatus;
        
        if (item.SupportedAvatars.Count == 0 || item.SupportedAvatars.Contains(avatarPath))
            avatarStatus.IsSupported = true;

        if (item.Type != ItemType.Clothing) return avatarStatus;

        var groupsForPath = commonAvatars
            .Where(x => x.Avatars.Contains(avatarPath))
            .ToArray();

        if (groupsForPath.Length == 0) return avatarStatus;

        foreach (var supportedAvatar in item.SupportedAvatars)
        {
            var group = groupsForPath.FirstOrDefault(g => g.Avatars.Contains(supportedAvatar));
            if (group != null)
            {
                avatarStatus.IsCommon = true;
                avatarStatus.CommonAvatarName = group.GroupName;
                break;
            }
        }

        return avatarStatus;
    }

    public static string GetItemPath(string parentFolder, string itemPath)
    {
        // <sys>で始まっていないものはフルパスと認識する
        return itemPath.StartsWith("<sys>") ? Path.Join(parentFolder, itemPath.Replace("<sys>", "")) : itemPath;
    }
}
