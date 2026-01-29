using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.UI.Data;
using AvatarExplorer.UI.Models.ContextMenu;

namespace AvatarExplorer.UI.Services.Utilities;

internal static class ImageService
{
    internal static readonly Dictionary<string, Bitmap?> SystemIconsDictionary = new()
    {
        { SystemIconKey.FolderIcon, Load("Assets/FolderIcon.png") },
        { SystemIconKey.FileIcon, Load("Assets/FileIcon.png") },
        { SystemIconKey.EmptyIcon, Load("Assets/EmptyIcon.png") },
        { SystemIconKey.GroupIcon, Load("Assets/GroupIcon.png") }
    };

    internal static bool IsSystemIcon(string fileName) => SystemIconsDictionary.ContainsKey(fileName);

    internal static Bitmap? Get(string fileName, IconType iconType = IconType.None)
    {
        if (IsSystemIcon(fileName)) return SystemIconsDictionary[fileName];

        return iconType switch
        {
            IconType.Item => Load(Path.Join(SystemPath.ItemThumbnailsPath, fileName)),
            IconType.Author => Load(Path.Join(SystemPath.AuthorThumbnailsPath, fileName)),
            _ => Load(fileName),
        };
    }

    internal static Bitmap? Load(string filePath) => File.Exists(filePath) ? new Bitmap(filePath) : null;
}
