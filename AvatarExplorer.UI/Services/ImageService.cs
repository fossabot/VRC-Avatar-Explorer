using System.IO;
using Avalonia.Media.Imaging;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Utils;

namespace AvatarExplorer.UI.Services;

internal static class ImageService
{
    internal static Bitmap? Get(string fileName, IconType iconType = IconType.None)
    {
        if (IconUtils.IsSystemIcon(fileName)) return IconUtils.SystemIcons[fileName];

        return iconType switch
        {
            IconType.Item => Load(Path.Join(SystemPath.ItemThumbnailsPath, fileName)),
            IconType.Author => Load(Path.Join(SystemPath.AuthorThumbnailsPath, fileName)),
            _ => Load(fileName),
        };
    }

    internal static Bitmap? Load(string filePath)
        => File.Exists(filePath) ? new Bitmap(filePath) : null;
}
