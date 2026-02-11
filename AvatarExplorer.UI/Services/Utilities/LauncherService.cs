using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Services.System;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Services.Utilities;

internal static class LauncherService
{
    private static ILauncher? GetLauncher(Visual visual) => TopLevel.GetTopLevel(visual)?.Launcher;

    internal static async Task OpenFile(Visual visual, string filePath)
    {
        ILauncher? launcher = GetLauncher(visual);
        if (launcher == null) return;

        try
        {
            FileInfo fileInfo = new(filePath);
            await launcher.LaunchFileInfoAsync(fileInfo);
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError(string.Format("Failed to open file: '{0}'.", filePath), ex, Localizer.Instance[LocalizationKey.Error.OpenFileFailed]);
        }
    }

    internal static async Task OpenFolder(Visual visual, string folderPath)
    {
        ILauncher? launcher = GetLauncher(visual);
        if (launcher == null) return;

        try
        {
            DirectoryInfo folderInfo = new(folderPath);
            await launcher.LaunchDirectoryInfoAsync(folderInfo);
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError(string.Format("Failed to open directory: '{0}'.", folderPath), ex, Localizer.Instance[LocalizationKey.Error.OpenFolderFailed]);
        }
    }

    internal static async Task OpenUri(Visual visual, string uri)
    {
        ILauncher? launcher = GetLauncher(visual);
        if (launcher == null) return;

        try
        {
            Uri uriInfo = new(uri);
            await launcher.LaunchUriAsync(uriInfo);
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError(string.Format("Failed to open Uri: '{0}'.", uri), ex, Localizer.Instance[LocalizationKey.Error.OpenUriFailed]);
        }
    }
}
