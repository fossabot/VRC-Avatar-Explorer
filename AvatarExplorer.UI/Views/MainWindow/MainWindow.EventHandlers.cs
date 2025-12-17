using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    #region Left Filter
    private void Main_LeftFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Main_RenderLeftPanel();
    }
    #endregion

    #region Main Top Buttons
    private void Main_UndoButton_Click(object? sender, RoutedEventArgs e)
    {
        // 選択されていたアイテムが検索結果時のものだったら、キャッシュを元にもう一度検索してあげる
        bool isCurrentSearchNode = _avatarExplorer.GetCurrentPathState()?.State == ItemTagState.SearchItem;
        
        Main_CheckPageStates(); // SelectUndoより前にやってあげないと、戻った先の画面のページ情報がリセットされる
        if (!_main_isLastWindowSearch) _avatarExplorer.SelectUndo(); // 最後の画面が検索画面だったら、検索だけやめて戻るようにする

        if (isCurrentSearchNode) Main_ExecuteSearchItems();
        else Main_RenderRightPanel();
    }
    private void Main_SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        SetUiValueFromCurrentSettings();
        SettingsOverlay.IsVisible = true;
    }
    #endregion

    #region Main Bottom Buttons
    // TODO: あとはバックアップと共通素体管理画面だけかも
    private void Main_SortOrderComboBox_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        _avatarExplorer.SetItemsSortOrder((SortOrder)comboBox.SelectedIndex);
        Main_ReloadCurrentWindow();
    }
    private void Main_AddItem_Click(object? sender, RoutedEventArgs e)
    {
        AddItemOverlay_ShowAddItemWindow();
    }
    private void Main_ImportData_Click(object? sender, RoutedEventArgs e)
    {
        SelectImportTypeOverlay.IsVisible = true;
    }
    private async void Main_ExportDataToCsv_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: チェックボックスで共通素体を含めるかどうかのチェックをする
        string? filePath = await StorageService.SaveFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectSaveFilePath], ".csv");
        if (filePath == null) return;

        var localizedItemTypesMapping = Enum.GetValues<ItemType>().ToDictionary(i => i, i => Localizer.Instance[i.GetLocalizationKey() ?? i.ToString()]);
        await _avatarExplorer.ExportToCsv(filePath, localizedItemTypesMapping, true);

        Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.Export]);
    }
    #endregion

    #region Drag and Drop
    private void Main_DragDrop_Enter(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }
    private void Main_DragDrop_Drop(object? sender, DragEventArgs e)
    {
        IEnumerable<IStorageItem>? storageItems = e.Data.GetFiles();
        if (storageItems == null) return;

        string[] storageItemPaths = storageItems
            .Select(i => i.TryGetLocalPath())
            .Where(i => !string.IsNullOrEmpty(i) && (Directory.Exists(i) || File.Exists(i)))
            .ToArray()!;

        AddItemOverlay_ShowAddItemWindow(storageItemPaths);
    }
    #endregion

    #region Window Closing
    private void Main_Closing(object? sender, WindowClosingEventArgs e)
    {
        AvatarExplorerApp.ClearTemp();
    }
    #endregion
}
