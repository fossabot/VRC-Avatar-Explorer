using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private void SelectImportTypeOverlay_Show() => SelectImportTypeOverlay.IsVisible = true;
    private void SelectImportTypeOverlay_Hide() => SelectImportTypeOverlay.IsVisible = false;

    private async Task SelectImportTypeOverlay_DataImportInternal(DataImportType dataImportType)
    {
        string[]? folders = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], false);
        if (folders == null || folders.Length == 0) return;

        string selectedFolder = folders[0];
        
        SelectImportTypeOverlay.IsVisible = false;

        // Item1: LocalizationKey, Item2: ProgressValue
        async Task progressAction((string, int) tuple)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProgressOverlay_Show(Localizer.Instance.GetDisplayName(tuple.Item1, tuple.Item2.ToString()));
                ProgressOverlay_Update(tuple.Item2);
            });
        }

        if (dataImportType == DataImportType.V1) await _avatarExplorerApp.ImportFromV1(selectedFolder, progressAction);
        else if (dataImportType == DataImportType.KonoAsset) await _avatarExplorerApp.ImportFromKonoAsset(selectedFolder, progressAction);

        ProgressOverlay_Hide();

        Main_ReloadCurrentWindow();
    }

    #region Event Handler
    private void SelectImportTypeOverlay_Cancel_Click(object? sender, RoutedEventArgs e) => SelectImportTypeOverlay_Hide();
    private async void SelectImportTypeOverlay_FromV1_Click(object? sender, RoutedEventArgs e) => await SelectImportTypeOverlay_DataImportInternal(DataImportType.V1);
    private async void SelectImportTypeOverlay_FromKonoAsset_Click(object? sender, RoutedEventArgs e) => await SelectImportTypeOverlay_DataImportInternal(DataImportType.KonoAsset);
    #endregion
}
