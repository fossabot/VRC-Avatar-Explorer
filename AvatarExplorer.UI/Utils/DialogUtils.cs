using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Utils;

internal static class DialogUtils
{
    internal async static Task<IReadOnlyList<IStorageFile>> OpenFileDialog(Visual visual, string titleKey, bool allowMultiple = false)
    {
        var storageProvider = GetStorageProvider(visual);
        if (storageProvider == null) return [];

        var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Localizer.Instance[titleKey],
            AllowMultiple = allowMultiple
        });

        return files;
    }

    internal async static Task<IReadOnlyList<IStorageFolder>> OpenFolderDialog(Visual visual, string titleKey, bool allowMultiple = false)
    {
        var storageProvider = GetStorageProvider(visual);
        if (storageProvider == null) return [];

        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Localizer.Instance[titleKey],
            AllowMultiple = allowMultiple
        });

        return folders;
    }

    private static IStorageProvider? GetStorageProvider(Visual visual)
        => TopLevel.GetTopLevel(visual)?.StorageProvider;
}
