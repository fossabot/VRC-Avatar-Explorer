using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;

namespace AvatarExplorer.UI.Utils;

internal static class IconUtils
{
    private static readonly Dictionary<string, Bitmap?> systemIcons = new()
    {
        { "System.Icon.Folder", LoadImage("Assets/FolderIcon.png") }
    };
    internal static bool IsSystemFileIcons(string fileName) => systemIcons.ContainsKey(fileName);

    internal static Bitmap? GetIcon(string fileName)
    {
        if (IsSystemFileIcons(fileName)) return systemIcons[fileName];
        return LoadImage(fileName);
    }

    private static Bitmap? LoadImage(string filePath)
    {
        return File.Exists(filePath) ? new Bitmap(filePath) : null;
    }
}
