using System.Text.RegularExpressions;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Utils;

public static partial class ItemUtils
{
    [GeneratedRegex(@"\u3010[^\u3011]+\u3011")]
    private static partial Regex TextBracketsRegex();

    internal static string GetAvatarNameFromId(IReadOnlyList<Item> items, string? id)
    {
        if (string.IsNullOrEmpty(id)) return string.Empty;
        return items.Where(i => i.Type == ItemType.Avatar).FirstOrDefault(x => x.Id == id)?.Title ?? string.Empty;
    }

    internal static string GetAvatarNameFromDictionary(Dictionary<string, string> avatarNamesDictionary, string avatarPath)
    {
        avatarNamesDictionary.TryGetValue(avatarPath, out string? avatarName);
        return avatarName ?? string.Empty;
    }

    public static string GetItemPath(string parentFolder, string itemPath)
    {
        // <sys>で始まっていないものはフルパスと認識する
        return itemPath.StartsWith("<sys>") ? Path.Join(parentFolder, itemPath.Replace("<sys>", string.Empty)) : itemPath;
    }

    public static string RemoveBrackets(string value) => TextBracketsRegex().Replace(value, string.Empty);
    
    public static string? GetSafeTitle(string itemTitle)
    {
        // パスに使用しても大丈夫な文字だけ残す
        return FileNameUtils.GetSafeTitle(itemTitle);
    }
    
    internal static Dictionary<string, string> GetAvatarNameMaps(IReadOnlyList<Item> items)
    {
        return items
            .Where(i => i.Type == ItemType.Avatar)
            .Select(i => i.Id)
            .Distinct()
            .ToDictionary(
                i => i,
                i => GetAvatarNameFromId(items, i)
            );
    }
}
