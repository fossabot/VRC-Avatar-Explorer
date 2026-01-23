using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    internal Dictionary<ActionKey, Func<string, Task>>? _contextMenuHandlers;

    private async void ItemButton_ContextMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is ContextMenuAction contextMenuAction)
            await ItemButton_ExecuteContextMenuItemCommand(contextMenuAction);
    }
    private async Task ItemButton_ExecuteContextMenuItemCommand(ContextMenuAction contextMenuAction)
    {
        if (contextMenuAction.ActionLayer == ActionLayer.UI)
        {
            if (_contextMenuHandlers!.TryGetValue(contextMenuAction.ActionKey, out var handler))
                await handler(contextMenuAction.Tag);
        }
        else if (contextMenuAction.ActionLayer == ActionLayer.Core)
        {
            await _avatarExplorerApp.ExecuteContextMenuItemCommand(contextMenuAction);
            if (contextMenuAction.ReloadRequired) Main_ReloadCurrentWindow();
        }
    }
    
    #region Context Menu Commands
    private Item? ItemButton_ContextMenu_GetItemByPath(string itemPath)
    {
        Item? item = _avatarExplorerApp.GetItemByPath(itemPath);
        if (item == null) Dialog_Show(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.ItemNotFound]);

        return item;
    }
    private async Task ItemButton_ContextMenu_OpenItemFolder(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        await LauncherService.OpenFolder(this, ItemUtils.GetItemPath(RuntimeSettings.DataRootDirectory, item.ItemPath));
    }
    private async Task ItemButton_ContextMenu_CopyBoothLink(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        string boothLink = item.GetBoothLink();

        try
        {
            await ClipboardService.Set(boothLink);
        }
        catch
        {
            // Ignored
        }
    }
    private async Task ItemButton_ContextMenu_OpenBoothLink(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        await LauncherService.OpenLink(this, item.GetBoothLink());
    }
    private Task ItemButton_ContextMenu_ShowOtherItemsByAuthor(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return Task.CompletedTask;

        if (Main_SearchTextBox != null) Main_SearchTextBox.Text = string.Format("Author=\"{0}\"", item.Author);

        return Task.CompletedTask;
    }
    private async Task ItemButton_ContextMenu_ChangeThumbnail(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        string[]? files = await StorageService.OpenFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFilePath], false);
        if (files == null || files.Length == 0) return;

        string selectedFile = files[0];
        await _avatarExplorerApp.UpdateItemThumbnail(item, selectedFile);
        Main_ReloadCurrentWindow();
    }
    private Task ItemButton_ContextMenu_EditItem(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return Task.CompletedTask;

        AddItemOverlay_ShowEdit(item);
        return Task.CompletedTask;
    }
    private async Task ItemButton_ContextMenu_EditItemTitle(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        string? newTitle = await Main_ShowTextDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Title.NewItemTitle], item.Title);
        if (string.IsNullOrEmpty(newTitle)) return;

        item.Title = newTitle;
        _avatarExplorerApp.SaveItemDatabase();

        _avatarExplorerApp.UpdateSearchIndex();

        Main_ReloadCurrentWindow();
    }
    private Task ItemButton_ContextMenu_AddMemo(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return Task.CompletedTask;

        _contextMenu_selectedItem = item;

        AddMemoOverlay_Show(item.ItemMemo);

        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_AddToBulkImportList(string itemPath)
    {
        BulkImportItem_Add(itemPath);
        return Task.CompletedTask;
    }
    private async Task ItemButton_ContextMenu_AddItemFolder(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        string[]? folders = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], true);
        if (folders == null || folders.Length == 0) return;

        ProgressOverlay_Show(Localizer.Instance[LocalizationKey.Processing.ItemAdd.Copying]);
        ProgressOverlay_Update(0);
        IReadOnlyList<string> processingFailedPaths = await _avatarExplorerApp.AddFolders(item, folders);
        ProgressOverlay_Hide();

        if (processingFailedPaths.Count > 0) // フォルダ展開に失敗した時に発生する
        {
            Dialog_Show(
                Localizer.Instance[LocalizationKey.Error.Default],
                Localizer.Instance.GetDisplayName(LocalizationKey.Error.ItemFolderProcessingFailedPaths, "\n" + string.Join('\n', processingFailedPaths.Select(i => $"- {i}")))
            );
        }
    }

    internal Item? _contextMenu_selectedItem = null;
    private Task ItemButton_ContextMenu_EditImplementedAvatar(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return Task.CompletedTask;

        _contextMenu_selectedItem = item;

        EditImplementedAvatarsOverlay_Show(item.ImplementedAvatarsView); //TODO: ここらへんも全部Taskで管理するべき

        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_EditItemTag(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return Task.CompletedTask;

        _contextMenu_selectedItem = item;

        EditTagsOverlay_Show(item.TagsView); //TODO: ここらへんも全部Taskで管理するべき

        return Task.CompletedTask;
    }
    private async Task ItemButton_ContextMenu_RemoveItem(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Remove]);
        if (result != YesNoResult.Yes) return;

        YesNoResult result2 = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.RemoveAvatarFromSupportedAndImplemented]);
        if (result2 == YesNoResult.Yes) _avatarExplorerApp.RemoveItem(item.ItemPath, true);
        else _avatarExplorerApp.RemoveItem(item.ItemPath, false);

        Main_ReloadCurrentWindow();
        Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.Remove]);
    }

    private async Task ItemButton_ContextMenu_OpenFile(string filePath)
    {
        await LauncherService.OpenFile(this, filePath);
    }
    private async Task ItemButton_ContextMenu_OpenFileInExplorer(string filePath)
    {
        if (!ProcessUtils.IsWindows()) return;
        Process.Start("explorer.exe", "/select," + filePath);
    }
    #endregion
}
