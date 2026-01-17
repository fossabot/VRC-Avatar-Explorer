using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Services;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    #region Left Filter
    private void Main_LeftFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Main_RenderLeftPanel();
    #endregion

    #region Main Top Buttons
    private void Main_UndoButton_Click(object? sender, RoutedEventArgs e)
    {
        // 選択されていたアイテムが検索結果時のものだったら、キャッシュを元にもう一度検索してあげる
        bool isCurrentSearchNode = _avatarExplorerApp.GetCurrentPathState()?.State == ItemTagState.SearchItem;
        
        Main_CheckPageStates(); // SelectUndoより前にやってあげないと、戻った先の画面のページ情報がリセットされる
        if (!_main_isLastWindowSearch) _avatarExplorerApp.SelectUndo(); // 最後の画面が検索画面だったら、検索だけやめて戻るようにする

        if (isCurrentSearchNode) Main_ExecuteSearchItems();
        else Main_RenderRightPanel();
    }
    private void Main_SettingsButton_Click(object? sender, RoutedEventArgs e) => SettingsOverlay_Show();

    private void Main_AdvancedSearchButton_Click(object? sender, RoutedEventArgs e)
    {
        AdvancedSearchPanel.IsVisible = !AdvancedSearchPanel.IsVisible;
        Main_SearchValue_Changed(sender, e);
    }
    #endregion

    #region Main Bottom Buttons
    // TODO: あとはバックアップだけかも
    private void Main_SortOrderComboBox_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        _avatarExplorerApp.SetItemsSortOrder((SortOrder)comboBox.SelectedIndex);
        Main_ReloadCurrentWindow();
    }
    private void Main_AddItem_Click(object? sender, RoutedEventArgs e) => AddItemOverlay_ShowAdd();
    private void Main_ImportData_Click(object? sender, RoutedEventArgs e) => SelectImportTypeOverlay_Show();

    private async void Main_ExportDataToCsv_Click(object? sender, RoutedEventArgs e)
    {
        YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Overlay.ExportToCsv.IncludeImplementedToSupported]);
        if (result == YesNoResult.Yes) await Main_ExportDataToCsvInternal(true);
        else await Main_ExportDataToCsvInternal(false);
    }
    private async Task Main_ExportDataToCsvInternal(bool includeImplementedToSupported)
    {
        string? filePath = await StorageService.SaveFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectSaveFilePath], "csv");
        if (filePath == null) return;

        var localizedItemTypesMapping = Enum.GetValues<ItemType>().ToDictionary(i => i, i => Localizer.Instance[i.GetLocalizationKey() ?? i.ToString()]);
        await _avatarExplorerApp.ExportToCsv(filePath, localizedItemTypesMapping, includeImplementedToSupported);

        Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.Export]);
    }
    private async void Main_EditCommonAvatars_Click(object? sender, RoutedEventArgs e)
        => EditCommonAvatarsOverlay_Show();
    #endregion

    #region Drag and Drop
    private void Main_DragDrop_Enter(object? sender, DragEventArgs e) => e.DragEffects = DragDropEffects.Copy;
    private void Main_DragDrop_Drop(object? sender, DragEventArgs e)
    {
        IEnumerable<IStorageItem?> storageItems = e.DataTransfer.GetItems(DataFormat.File).Select(i => i.TryGetFile());
        if (storageItems == null) return;

        string[] storageItemPaths = storageItems
            .Select(i => i?.TryGetLocalPath())
            .Where(i => !string.IsNullOrEmpty(i) && (Directory.Exists(i) || File.Exists(i)))
            .ToArray()!;

        AddItemOverlay_ShowAdd(storageItemPaths);
    }
    #endregion

    #region Window Closing
    private void Main_Closing(object? sender, WindowClosingEventArgs e) => AvatarExplorerApp.ClearTemp();
    #endregion
}
