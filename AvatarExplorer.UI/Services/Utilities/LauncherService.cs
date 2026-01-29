using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvatarExplorer.Core.Services.System;

namespace AvatarExplorer.UI.Services.Utilities;

internal static class LauncherService
{
    private static ILauncher? GetLauncher(Visual visual) => TopLevel.GetTopLevel(visual)?.Launcher;

    internal static async Task OpenFile(Visual visual, string filePath)
    {
        ILauncher? launcher = GetLauncher(visual);
        if (launcher == null) return;

        FileInfo fileInfo = new(filePath);

        try
        {
            await launcher.LaunchFileInfoAsync(fileInfo);
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError(string.Format("Failed to open file. '{0}'", filePath), ex);
        }
    }

    internal static async Task OpenFolder(Visual visual, string folderPath)
    {
        ILauncher? launcher = GetLauncher(visual);
        if (launcher == null) return;

        DirectoryInfo folderInfo = new(folderPath);

        try
        {
            await launcher.LaunchDirectoryInfoAsync(folderInfo);
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError(string.Format("Failed to open directory. '{0}'", folderPath), ex);
        }
    }

    internal static async Task OpenUri(Visual visual, string uri)
    {
        ILauncher? launcher = GetLauncher(visual);
        if (launcher == null) return;

        Uri uriInfo = new(uri);

        try
        {
            await launcher.LaunchUriAsync(uriInfo);
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError(string.Format("Failed to open Uri. '{0}'", uri), ex);
        }
    }
}
