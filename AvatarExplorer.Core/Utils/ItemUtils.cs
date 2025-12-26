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
        return FileNameUtils.GetSafeTitle(itemTitle);
    }
    
    internal static Dictionary<string, string> GetAvatarNameMaps(List<Item> items)
    {
        return items
            .Where(i => i.Type == ItemType.Avatar)
            .ToDictionary(
                i => i.ItemPath,
                i => GetAvatarNameFromPath(items, i.ItemPath)
            );
    }
}
