using System.IO;
using Avalonia.Media.Imaging;
using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Utils;

namespace AvatarExplorer.UI.Services;

internal static class ImageService
{
    internal static Bitmap? GetImage(string fileName, IconType iconType = IconType.None)
    {
        if (IconUtils.IsSystemFileIcon(fileName)) return IconUtils.SystemIcons[fileName];

        return iconType switch
        {
            IconType.Item => LoadImage(Path.Join(SystemPath.ItemThumbnailsPath, fileName)),
            IconType.Author => LoadImage(Path.Join(SystemPath.AuthorThumbnailsPath, fileName)),
            _ => LoadImage(fileName),
        };
    }

    internal static Bitmap? LoadImage(string filePath)
        => File.Exists(filePath) ? new Bitmap(filePath) : null;
}
