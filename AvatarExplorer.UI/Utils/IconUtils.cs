using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.UI.Utils;

internal static class IconUtils
{
    private static readonly Dictionary<string, Bitmap?> systemIcons = new()
    {
        { SystemIcon.FolderIcon, LoadImage("Assets/FolderIcon.png") },
        { SystemIcon.FileIcon, LoadImage("Assets/FileIcon.png") },
        { SystemIcon.NothingIcon, LoadImage("Assets/NothingIcon.png") },
    };
    internal static bool IsSystemFileIcons(string fileName) => systemIcons.ContainsKey(fileName);

    internal static Bitmap? GetIcon(string fileName, IconType iconType = IconType.None)
    {
        if (IsSystemFileIcons(fileName)) return systemIcons[fileName];

        return iconType switch
        {
            IconType.Item => LoadImage(Path.Join(SystemPath.ItemThumbnailsPath, fileName)),
            IconType.Author => LoadImage(Path.Join(SystemPath.AuthorThumbnailsPath, fileName)),
            _ => LoadImage(fileName),
        };
    }

    private static Bitmap? LoadImage(string filePath)
    {
        return File.Exists(filePath) ? new Bitmap(filePath) : null;
    }
}
