using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

internal static class ItemDatabaseService
{
    internal static List<Item> Load(string path)
    {
        try
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            return FileSystemService.DeserializeClass<List<Item>>(path) ?? [];
        }
        catch
        {
            return [];
        }

    }

    internal static void Save(List<Item> items)
    {
        try
        {
            FileSystemService.SerializeClass(items, SystemPath.ItemDatabasePath);
        }
        catch
        {
            // Ignored
        }
    }
}
