using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace AvatarExplorer.UI.Services;

internal static class StorageService
{
    private static IStorageProvider? GetStorageProvider(Visual visual) => TopLevel.GetTopLevel(visual)?.StorageProvider;
    
    internal static async Task<string[]?> OpenFileDialog(Visual visual, string title, bool allowMultiple = false)
    {
        IStorageProvider? storageProvider = GetStorageProvider(visual);
        if (storageProvider == null) return [];

        IReadOnlyList<IStorageFile> files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = allowMultiple
        });

        string[] filePaths = files
            .Select(i => i.TryGetLocalPath())
            .Where(i => !string.IsNullOrEmpty(i) && File.Exists(i))
            .ToArray()!;

        return filePaths.Length == 0 ? null : filePaths;
    }

    internal static async Task<string[]?> OpenFolderDialog(Visual visual, string title, bool allowMultiple = false, string? initialPath = null)
    {
        IStorageProvider? storageProvider = GetStorageProvider(visual);
        if (storageProvider == null) return [];

        FolderPickerOpenOptions folderPickerOpenOptions = new()
        {
            Title = title,
            AllowMultiple = allowMultiple
        };

        if (!string.IsNullOrEmpty(initialPath)) folderPickerOpenOptions.SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(initialPath);

        IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(folderPickerOpenOptions);

        string[] FolderPaths = folders
            .Select(i => i.TryGetLocalPath())
            .Where(i => !string.IsNullOrEmpty(i) && Directory.Exists(i))
            .ToArray()!;

        return FolderPaths.Length == 0 ? null : FolderPaths;
    }

    internal static async Task<string?> SaveFileDialog(Visual visual, string title, string defaultExtension)
    {
        IStorageProvider? storageProvider = GetStorageProvider(visual);
        if (storageProvider == null) return null;

        IStorageFile? file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            DefaultExtension = defaultExtension
        });

        return file?.TryGetLocalPath();
    }

    internal static async Task<IStorageFile?> GetStorageFileFromPath(Visual visual, string filePath)
    {
        IStorageProvider? storageProvider = GetStorageProvider(visual);
        if (storageProvider == null) return null;

        return await storageProvider.TryGetFileFromPathAsync(filePath);
    }
}
