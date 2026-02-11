using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Interfaces.Database;
using AvatarExplorer.Core.Services.IO;
using AvatarExplorer.Core.Services.System;
using ErrorOr;

namespace AvatarExplorer.Core.Services.Database;

internal static class DatabaseService<T> where T : IDatabaseItem
{
    internal static IEnumerable<T> Load(string path)
    {
        ErrorOr<IEnumerable<T>> deserializedResult = FileSystemService.DeserializeClass<IEnumerable<T>>(path);
        if (deserializedResult.IsError)
        {
            ErrorManager.Instance.PostInternalError(string.Format("Failed to load database: '{0}'.", path), tag: deserializedResult.Errors.ToErrorString());
            return [];
        }
        
        return deserializedResult.Value;
    }

    internal static void Save(IEnumerable<T> items, string path)
    {
        ErrorOr<Success> serializeResult = FileSystemService.SerializeClass(items, path);
        if (serializeResult.IsError)
        {
            ErrorManager.Instance.PostInternalError(string.Format("Failed to save database: '{0}'.", path), tag: serializeResult.Errors.ToErrorString());
        }
    }
}
