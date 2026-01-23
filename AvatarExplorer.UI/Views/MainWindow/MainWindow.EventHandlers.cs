using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;

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
    #endregion

    #region Main Bottom Buttons
    private void Main_ShowSidePanel_Click(object? sender, PointerPressedEventArgs e)
    {
        if (!Main_SidePanelBorder.IsVisible) SidePanel_Show();
        else SidePanel_Hide();
    }
    private void Main_AddItem_Click(object? sender, RoutedEventArgs e) => AddItemOverlay_ShowAdd();
    #endregion

    #region Drag and Drop
    private async void ItemButton_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Button button) return;

        if (button.Tag is ItemTagInfo itemTagInfo)
        {
            DataTransferItem item = new();
            item.Set(DataFormat.Text, () => itemTagInfo.Value);

            DataTransfer dragData = new();
            dragData.Add(item);

            await Task.Delay(300);

            // 300ms後もそのボタンがクリックされていたら、長押しとみなしてD&D処理を開始する
            if (!button.IsPressed) return;

            await DragDrop.DoDragDropAsync(e, dragData, DragDropEffects.Copy);
        }
    }
    private void Main_DragDrop_Over(object? sender, DragEventArgs e)
    {
        // ファイルのD&D: File | アイテムボタンのD&D: Text
        if (e.DataTransfer.Contains(DataFormat.File) || e.DataTransfer.Contains(DataFormat.Text)) e.DragEffects = DragDropEffects.Copy;
    }
    private void Main_DragDrop_Drop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File)) return;
        
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
