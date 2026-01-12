using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Services;

internal static class UnitypackageService
{
    internal static async Task Open(Window window, string itemPath, Item? selectedItem, Func<string, int, Task>? onProgress = null, Func<string, Task>? onCompleted = null)
    {
        if (selectedItem == null)
        {
            await LauncherService.OpenFile(window, itemPath);
            return;
        }

        var progress = new Progress<(string, int, string)>(async tuple =>
        {
            if (tuple.Item2 == 100)
            {
                if (onCompleted != null)
                    await onCompleted(tuple.Item3);
            }
            else
            {
                if (onProgress != null)
                    await onProgress(tuple.Item1, tuple.Item2);
            }
        });

        await AvatarExplorerApp.ModifyUnityPackageFilePath(itemPath, Localizer.Instance[selectedItem.Type.GetLocalizationKey() ?? ""], progress);
    }
}
