using System;
using System.Linq;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void SelectImportTypeOverlay_Border_Click(object? sender, RoutedEventArgs e)
        => SelectImportTypeOverlay_CloseInternal();
    private void SelectImportTypeOverlay_Cancel_Click(object? sender, RoutedEventArgs e)
        => SelectImportTypeOverlay_CloseInternal();
        
    private void SelectImportTypeOverlay_CloseInternal()
        => SelectImportTypeOverlay.IsVisible = false;
        
    private void SelectImportTypeOverlay_FromV1_Click(object? sender, RoutedEventArgs e)
        => DataImportInternal(DataImportType.V1);
    private void SelectImportTypeOverlay_FromKonoAsset_Click(object? sender, RoutedEventArgs e)
        => DataImportInternal(DataImportType.KonoAsset);
    private async void DataImportInternal(DataImportType dataImportType)
    {
        string[]? folders = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], false);
        if (folders == null || folders.Length == 0) return;

        string selectedFolder = folders[0];
        
        SelectImportTypeOverlay.IsVisible = false;

        var localizedItemTypesMapping = Enum.GetValues<ItemType>().ToDictionary(i => i, i => Localizer.Instance[i.GetLocalizationKey() ?? i.ToString()]);

        var progress = new Progress<(string, int, string)>(tuple =>
        {
            if (tuple.Item2 == 100)
            {
                Main_HideProgress();
            }
            else
            {
                Main_ShowProgress(Localizer.Instance.GetDisplayName(tuple.Item1, tuple.Item2.ToString()));
                Main_UpdateProgress(tuple.Item2);
            }
        });

        if (dataImportType == DataImportType.V1) await _avatarExplorer.ImportFromV1(selectedFolder, localizedItemTypesMapping, progress);
        else if (dataImportType == DataImportType.KonoAsset)  await _avatarExplorer.ImportFromKonoAsset(selectedFolder, localizedItemTypesMapping, progress);

        Main_ReloadCurrentWindow();
    }
}
