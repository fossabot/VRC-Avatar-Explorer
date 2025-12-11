using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.V1;
using System.Text.Json;

namespace AvatarExplorer.Core.Utils;

public static class DatabaseUtils
{
    public static string GetSoftwareFolderPath(string path)
        => Path.Combine(path, "Avatar Explorer V2");

    public static string GetDataFolderPath(string softwarePath)
        => Path.Combine(softwarePath, "database");

    public static string GetBackupFolderPath(string softwarePath)
        => Path.Combine(softwarePath, "backups");

    public static string GetImagesFolderPath(string softwarePath)
        => Path.Combine(softwarePath, "images");

    public static string GetItemsFolderPath(string softwarePath)
        => Path.Combine(softwarePath, "items");

    public static string GetSettingsFolderPath(string softwarePath)
        => Path.Combine(softwarePath, "settings");
        
    public static string GetItemThumbnailsFolderPath(string softwarePath)
        => Path.Combine(GetImagesFolderPath(softwarePath), "item_thumbnails");

    public static string GetAuthorThumbnailsFolderPath(string softwarePath)
        => Path.Combine(GetImagesFolderPath(softwarePath), "author_thumbnails");
    
    internal static List<Item> LoadItemsData(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();

        string json = File.ReadAllText(path);
        List<Item> items = JsonSerializer.Deserialize<List<Item>>(json) ?? [];

        return items;
    }

    internal static List<CommonAvatar> LoadCommonAvatarsData(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();

        string json = File.ReadAllText(path);
        List<CommonAvatar> commonAvatars = JsonSerializer.Deserialize<List<CommonAvatar>>(json) ?? [];

        return commonAvatars;
    }

    internal static List<Item> LoadItemsDataFromV1(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();

        string json = File.ReadAllText(path);
        List<ItemV1> items = JsonSerializer.Deserialize<List<ItemV1>>(json) ?? [];

        return MigrateItemsFromV1(items);
    }

    internal static List<CommonAvatar> LoadCommonAvatarsDataFromV1(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();

        string json = File.ReadAllText(path);
        List<CommonAvatarV1> commonAvatars = JsonSerializer.Deserialize<List<CommonAvatarV1>>(json) ?? [];

        return MigrateCommonAvatarsFromV1(commonAvatars);
    }

    private static List<Item> MigrateItemsFromV1(List<ItemV1> items)
    {
        return items.Select(Item.FromV1).ToList();
    }
    private static List<CommonAvatar> MigrateCommonAvatarsFromV1(List<CommonAvatarV1> commonAvatars)
    {
        return commonAvatars.Select(CommonAvatar.FromV1).ToList();
    }

    internal static Dictionary<string, string> GetAvatarNameMaps(List<Item> items)
    {
        return items
            .Where(i => i.Type == ItemType.Avatar)
            .ToDictionary(
                i => i.ItemPath,
                i => ItemUtils.GetAvatarNameFromPath(items, i.ItemPath)
            );
    }
}
