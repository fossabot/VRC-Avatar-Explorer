using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.UI.Utils;

internal static class IconUtils
{
    private static readonly Dictionary<string, Bitmap?> systemIcons = new()
    {
        { "System.Icon.Folder", LoadImage("Assets/FolderIcon.png") }
    };
    internal static bool IsSystemFileIcons(string fileName) => systemIcons.ContainsKey(fileName);

    internal static Bitmap? GetIcon(string fileName, IconType iconType)
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
