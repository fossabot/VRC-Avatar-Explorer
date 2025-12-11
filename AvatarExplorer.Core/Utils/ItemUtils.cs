using System.Text.RegularExpressions;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

public static partial class ItemUtils
{
    [GeneratedRegex(@"\u3010[^\u3011]+\u3011")]
    private static partial Regex TextBracketsRegex();

    internal static string GetAvatarNameFromPath(IReadOnlyList<Item> items, string? path)
    {
        if (string.IsNullOrEmpty(path)) return string.Empty;
        return items.Where(i => i.Type == ItemType.Avatar).FirstOrDefault(x => x.ItemPath == path)?.Title ?? string.Empty;
    }

    internal static string GetAvatarNameFromDictionary(Dictionary<string, string> avatarNamesDictionary, string avatarPath)
    {
        avatarNamesDictionary.TryGetValue(avatarPath, out string? avatarName);
        return avatarName ?? "";
    }

    internal static AvatarStatus GetAvatarStatus(string? avatarPath, Item item, List<CommonAvatar> commonAvatars)
    {
        AvatarStatus avatarStatus = new();
        if (string.IsNullOrEmpty(avatarPath)) return avatarStatus;
        
        if (item.SupportedAvatars.Count == 0 || item.SupportedAvatars.Contains(avatarPath))
            avatarStatus.IsSupported = true;

        if (item.Type != ItemType.Clothing) return avatarStatus;

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

    public static string GetItemPath(string parentFolder, string itemPath)
    {
        // <sys>で始まっていないものはフルパスと認識する
        return itemPath.StartsWith("<sys>") ? Path.Join(parentFolder, itemPath.Replace("<sys>", "")) : itemPath;
    }

    public static string RemoveBrackets(string value)
        => TextBracketsRegex().Replace(value, "");
    
    public static string? GetSafeTitle(string itemTitle)
    {
        // パスに使用しても大丈夫な文字だけ残す
        string safeTitle = itemTitle;
        foreach (char invalidChar in FileSystemUtils.InvalidChars)
        {
            safeTitle = safeTitle.Replace(invalidChar, '_');
        }

        return string.IsNullOrEmpty(safeTitle) ? null : safeTitle;
    }
}
