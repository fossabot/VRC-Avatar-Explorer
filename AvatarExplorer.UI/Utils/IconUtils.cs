using System.Collections.Generic;
using Avalonia.Media.Imaging;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI.Utils;

internal static class IconUtils
{
    internal static readonly Dictionary<string, Bitmap?> SystemIcons = new()
    {
        { SystemIconKey.FolderIcon, ImageService.LoadImage("Assets/FolderIcon.png") },
        { SystemIconKey.FileIcon, ImageService.LoadImage("Assets/FileIcon.png") },
        { SystemIconKey.NothingIcon, ImageService.LoadImage("Assets/NothingIcon.png") },
    };

    internal static bool IsSystemFileIcon(string fileName)
        => SystemIcons.ContainsKey(fileName);
}
