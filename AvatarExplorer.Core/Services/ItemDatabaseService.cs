using System.Text.Json;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.V1;

namespace AvatarExplorer.Core.Services;

internal static class ItemDatabaseService
{
    internal static List<Item> LoadItemsData(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();

        string json = File.ReadAllText(path);
        List<Item> items = JsonSerializer.Deserialize<List<Item>>(json) ?? [];

        return items;
    }
    
    internal static List<Item> LoadItemsDataFromV1(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException();

        string json = File.ReadAllText(path);
        List<ItemV1> items = JsonSerializer.Deserialize<List<ItemV1>>(json) ?? [];

        return MigrateItemsFromV1(items);
    }
    
    private static List<Item> MigrateItemsFromV1(List<ItemV1> items)
    {
        return items.Select(Item.FromV1).ToList();
    }
}
