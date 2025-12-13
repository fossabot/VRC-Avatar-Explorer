using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Services;

internal static class StorageService
{
    internal static async Task<string[]?> OpenFileDialog(Visual visual, string titleKey, bool allowMultiple = false)
    {
        IStorageProvider? storageProvider = GetStorageProvider(visual);
        if (storageProvider == null) return [];

        IReadOnlyList<IStorageFile> files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Localizer.Instance[titleKey],
            AllowMultiple = allowMultiple
        });

        string[] filePaths = files
            .Select(i => i.TryGetLocalPath())
            .Where(i => !string.IsNullOrEmpty(i) && File.Exists(i))
            .ToArray()!;

        return filePaths.Length == 0 ? null : filePaths;
    }

    internal static async Task<string[]?> OpenFolderDialog(Visual visual, string titleKey, bool allowMultiple = false)
    {
        IStorageProvider? storageProvider = GetStorageProvider(visual);
        if (storageProvider == null) return [];

        IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Localizer.Instance[titleKey],
            AllowMultiple = allowMultiple
        });

        string[] FolderPaths = folders
            .Select(i => i.TryGetLocalPath())
            .Where(i => !string.IsNullOrEmpty(i) && Directory.Exists(i))
            .ToArray()!;

        return FolderPaths.Length == 0 ? null : FolderPaths;
    }

    internal static async Task<string?> SaveFileDialog(Visual visual, string titleKey, string defaultExtension)
    {
        IStorageProvider? storageProvider = GetStorageProvider(visual);
        if (storageProvider == null) return null;

        IStorageFile? file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = Localizer.Instance[titleKey],
            DefaultExtension = defaultExtension
        });

        return file?.TryGetLocalPath();
    }
    
    private static IStorageProvider? GetStorageProvider(Visual visual)
        => TopLevel.GetTopLevel(visual)?.StorageProvider;
}
