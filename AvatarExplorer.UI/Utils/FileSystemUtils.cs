using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AvatarExplorer.UI.Utils;

internal static class AvaloniaLauncherUtils
{
    internal async static Task OpenFile(Visual visual, string filePath)
    {
        var launcher = GetLauncher(visual);
        if (launcher == null) return;

        var fileInfo = new FileInfo(filePath);

        await launcher.LaunchFileInfoAsync(fileInfo);
    }

    internal async static Task OpenFolder(Visual visual, string folderPath)
    {
        var launcher = GetLauncher(visual);
        if (launcher == null) return;

        var folderInfo = new DirectoryInfo(folderPath);

        await launcher.LaunchDirectoryInfoAsync(folderInfo);
    }

    internal async static Task OpenLink(Visual visual, string itemLink)
    {
        var launcher = GetLauncher(visual);
        if (launcher == null) return;

        var itemLinkUri = new Uri(itemLink);

        await launcher.LaunchUriAsync(itemLinkUri);
    }

    private static ILauncher? GetLauncher(Visual visual)
        => TopLevel.GetTopLevel(visual)?.Launcher;
}
