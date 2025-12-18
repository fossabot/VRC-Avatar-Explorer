using System.Collections.Generic;
using Avalonia.Media.Imaging;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI.Utils;

internal static class IconUtils
{
    internal static readonly Dictionary<string, Bitmap?> SystemIcons = new()
    {
        { SystemIconKey.FolderIcon, ImageService.Load("Assets/FolderIcon.png") },
        { SystemIconKey.FileIcon, ImageService.Load("Assets/FileIcon.png") },
        { SystemIconKey.EmptyIcon, ImageService.Load("Assets/EmptyIcon.png") },
    };

    internal static bool IsSystemIcon(string fileName)
        => SystemIcons.ContainsKey(fileName);
}
