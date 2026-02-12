using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvatarExplorer.Core.Services.System;
using ErrorOr;

namespace AvatarExplorer.UI.Services.Utilities;

internal static class LauncherService
{
    private static ILauncher? GetLauncher(Visual visual) => TopLevel.GetTopLevel(visual)?.Launcher;

    internal static async Task<ErrorOr<Success>> OpenFile(Visual visual, string filePath)
    {
        try
        {
            ILauncher? launcher = GetLauncher(visual);
            if (launcher == null) return Error.Failure(description: "Failed to get launcher.");

            FileInfo fileInfo = new(filePath);
            await launcher.LaunchFileInfoAsync(fileInfo);

            return Result.Success;
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostInternalError(string.Format("Failed to open file: '{0}'.", filePath), ex);
            return Error.Failure(description: "Failed to open file.");
        }
    }

    internal static async Task<ErrorOr<Success>> OpenFolder(Visual visual, string folderPath)
    {
        try
        {
            ILauncher? launcher = GetLauncher(visual);
            if (launcher == null) return Error.Failure(description: "Failed to get launcher.");

            DirectoryInfo folderInfo = new(folderPath);
            await launcher.LaunchDirectoryInfoAsync(folderInfo);
            
            return Result.Success;
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError(string.Format("Failed to open directory: '{0}'.", folderPath), ex);
            return Error.Failure(description: "Failed to open directory.");
        }
    }

    internal static async Task<ErrorOr<Success>> OpenUri(Visual visual, string uri)
    {
        try
        {
            ILauncher? launcher = GetLauncher(visual);
            if (launcher == null) return Error.Failure(description: "Failed to get launcher.");

            Uri uriInfo = new(uri);
            await launcher.LaunchUriAsync(uriInfo);

            return Result.Success;
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError(string.Format("Failed to open Uri: '{0}'.", uri), ex);
            return Error.Failure(description: "Failed to open uri.");
        }
    }
}
