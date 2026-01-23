using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AvatarExplorer.Core.Services;

namespace AvatarExplorer.UI.Services;

internal static class UnitypackageService
{
    internal static async Task Import(string filePath, string localizedCategoryName, Func<string, int, Task>? onProgress = null, Func<string, Task>? onCompleted = null)
    {
        // Item1: LocalizationKey, Item2: ProgressValue
        var progress = new Progress<(string, int)>(async tuple =>
        {
            if (onProgress != null) await onProgress(tuple.Item1, tuple.Item2);
        });

        string unityPackagePath = await AvatarExplorerApp.ModifyUnityPackageFilePath(filePath, localizedCategoryName, progress);
        if (onCompleted != null) await onCompleted(unityPackagePath);
    }

    internal static async Task BulkImport(string[] filePaths, string[] localizedCategoryNames, Func<string, int, Task>? onProgress = null, Func<string, Task>? onCompleted = null)
    {
        if (filePaths.Length != localizedCategoryNames.Length) return;

        // Item1: LocalizationKey, Item2: ProgressValue
        var progress = new Progress<(string, int)>(async tuple =>
        {
            if (onProgress != null) await onProgress(tuple.Item1, tuple.Item2);
        });

        string unityPackagePath = await AvatarExplorerApp.ModifyUnityPackageFilePaths(filePaths, localizedCategoryNames, progress);
        if (onCompleted != null) await onCompleted(unityPackagePath);
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
