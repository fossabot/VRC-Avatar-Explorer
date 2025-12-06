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

    internal static AvatarStatus GetAvatarStatus(Item item, List<CommonAvatar> commonAvatars, string? path)
    {
        var avatarStatus = new AvatarStatus();

        if (string.IsNullOrEmpty(path)) return avatarStatus;
        if (item.SupportedAvatars.Contains(path))
        {
            avatarStatus.IsSupported = true;
            return avatarStatus;
        }

        if (item.Type != ItemType.Clothing) return avatarStatus;
        var commonAvatarsArray = commonAvatars.Where(x => x.Avatars.Contains(path));
        var isCommonAvatar = item.SupportedAvatars.Any(supportedAvatar => commonAvatarsArray.Any(x => x.Avatars.Contains(supportedAvatar)));

        if (!isCommonAvatar) return new AvatarStatus();

        var commonAvatar = item.SupportedAvatars
            .Select(avatar => commonAvatarsArray.FirstOrDefault(x => x.Avatars.Contains(avatar)))
            .FirstOrDefault(x => x != null);

        avatarStatus.IsCommon = true;
        avatarStatus.CommonAvatarName = commonAvatar?.Name ?? string.Empty;

        return avatarStatus;
    }

    public static string GetItemPath(string itemPath)
    {
        // <sys>で始まっていないものはフルパスと認識する
        return itemPath.StartsWith("<sys>") ? Path.Join(SystemPath.ItemsFolderPath, itemPath.Replace("<sys>", "")) : itemPath;
    }
}
