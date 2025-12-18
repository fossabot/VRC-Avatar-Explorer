using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.V1;

namespace AvatarExplorer.Core.Services;

internal static class ItemDatabaseService
{
    internal static List<Item> Load(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();
        return FileSystemService.DeserializeClass<List<Item>>(path) ?? [];
    }
    
    internal static List<Item> LoadFromV1(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();
        return MigrateItemsFromV1(FileSystemService.DeserializeClass<List<ItemV1>>(path) ?? []);
    }
    
    private static List<Item> MigrateItemsFromV1(List<ItemV1> items)
    {
        return items.Select(Item.FromV1).ToList();
    }
}
