using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class ItemDatabaseService
{
    internal static List<Item> Load(string path)
    {
        return FileSystemService.DeserializeClass<List<Item>>(path) ?? [];
    }

    internal static void Save(List<Item> items)
    {
        FileSystemService.SerializeClass(items, SystemPath.ItemDatabasePath);
    }
}
