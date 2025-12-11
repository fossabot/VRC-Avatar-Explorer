using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AvatarExplorer.UI.Utils;

internal static class AvaloniaLauncherUtils
{
    internal static async Task OpenFile(Visual visual, string filePath)
    {
        ILauncher? launcher = GetLauncher(visual);
        if (launcher == null) return;

        FileInfo fileInfo = new(filePath);

        await launcher.LaunchFileInfoAsync(fileInfo);
    }

    internal static async Task OpenFolder(Visual visual, string folderPath)
    {
        ILauncher? launcher = GetLauncher(visual);
        if (launcher == null) return;

        DirectoryInfo folderInfo = new(folderPath);

        await launcher.LaunchDirectoryInfoAsync(folderInfo);
    }

    internal static async Task OpenLink(Visual visual, string itemLink)
    {
        ILauncher? launcher = GetLauncher(visual);
        if (launcher == null) return;

        Uri itemLinkUri = new(itemLink);

        await launcher.LaunchUriAsync(itemLinkUri);
    }

    private static ILauncher? GetLauncher(Visual visual)
        => TopLevel.GetTopLevel(visual)?.Launcher;
}
