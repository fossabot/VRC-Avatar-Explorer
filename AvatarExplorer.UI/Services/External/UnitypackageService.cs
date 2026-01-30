using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvatarExplorer.Core.Services.IO;
using AvatarExplorer.Core.Services.System;

namespace AvatarExplorer.UI.Services.External;

internal static class UnitypackageService
{
    internal static async Task Import(string filePath, string localizedCategoryName, Func<string, int, Task>? onProgress = null, Func<string?, Task>? onCompleted = null)
    {
        // Item1: LocalizationKey, Item2: ProgressValue
        async Task progressAction((string, int) tuple)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (onProgress != null) await onProgress(tuple.Item1, tuple.Item2);
            });
        }

        string? unitypackagePath = await AvatarExplorerApp.ModifyUnitypackageFilePath(filePath, localizedCategoryName, progressAction);
        if (onCompleted != null) await onCompleted(unitypackagePath);
    }

    internal static async Task BulkImport(string[] filePaths, string[] localizedCategoryNames, Func<string, int, Task>? onProgress = null, Func<string?, Task>? onCompleted = null)
    {
        if (filePaths.Length != localizedCategoryNames.Length) return;

        // Item1: LocalizationKey, Item2: ProgressValue
        async Task progressAction((string, int) tuple)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (onProgress != null) await onProgress(tuple.Item1, tuple.Item2);
            });
        }

        string? unitypackagePath = await AvatarExplorerApp.ModifyUnitypackageFilePaths(filePaths, localizedCategoryNames, progressAction);
        if (onCompleted != null) await onCompleted(unitypackagePath);
    }

    internal static IReadOnlyList<string> GetUnitypackagePaths(string itemPath)
    {
        List<string> unitypackageFilePaths = new();

        foreach (string filePath in FileSystemService.EnumerateFiles(itemPath))
        {
            bool isUnitypackage = filePath.ToLower().EndsWith(".unitypackage");
            if (!isUnitypackage) continue;

            unitypackageFilePaths.Add(filePath);
        }

        return unitypackageFilePaths;
    }
}
