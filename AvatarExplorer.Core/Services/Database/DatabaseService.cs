using AvatarExplorer.Core.Interfaces.Database;
using AvatarExplorer.Core.Services.IO;

namespace AvatarExplorer.Core.Services.Database;

internal static class DatabaseService<T> where T : IDatabaseItem
{
    internal static IEnumerable<T> Load(string path)
    {
        return FileSystemService.DeserializeClass<IEnumerable<T>>(path) ?? [];
    }

    internal static void Save(IEnumerable<T> commonAvatars, string path)
    {
        FileSystemService.SerializeClass(commonAvatars, path);
    }
}
