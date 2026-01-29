using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models.Items;
using AvatarExplorer.Core.Services.System;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models.Common;
using AvatarExplorer.UI.Models.ContextMenu;
using AvatarExplorer.UI.Services.Utilities;

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
        if (_contextMenuHandlers!.TryGetValue(contextMenuAction.ActionKey, out var handler))
            await handler(contextMenuAction.Tag);
    }

    #region Context Menu Commands
    private Item? ItemButton_ContextMenu_GetItemById(string itemId)
    {
        Item? item = _avatarExplorerApp.GetItemById(itemId);
        if (item == null) Dialog_Show(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.ItemNotFound]);

        return item;
    }
    private async Task ItemButton_ContextMenu_OpenItemFolder(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return;

        await LauncherService.OpenFolder(this, ItemUtils.GetItemPath(RuntimeSettings.DataRootDirectory, item.ItemPath));
    }
    private async Task ItemButton_ContextMenu_CopyBoothLink(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return;

        string boothLink = item.GetBoothLink();

        try
        {
            await ClipboardService.Set(boothLink);
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError(string.Format("Failed to set text to clipboard '{0}'.", boothLink), ex);
        }
    }
    private async Task ItemButton_ContextMenu_OpenBoothLink(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return;

        await LauncherService.OpenUri(this, item.GetBoothLink());
    }
    private Task ItemButton_ContextMenu_ShowOtherItemsByAuthor(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return Task.CompletedTask;

        if (Main_SearchTextBox != null) Main_SearchTextBox.Text = string.Format("Author=\"{0}\"", item.Author);

        return Task.CompletedTask;
    }
    private async Task ItemButton_ContextMenu_ChangeThumbnail(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return;

        string[]? files = await StorageService.OpenFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFilePath], false);
        if (files == null || files.Length == 0) return;

        string selectedFile = files[0];
        await _avatarExplorerApp.UpdateItemThumbnail(item.Id, selectedFile);
        Main_ReloadCurrentWindow();
    }
    private async Task ItemButton_ContextMenu_FetchThumbnail(string itemId)
    {
        await _avatarExplorerApp.FetchAndUpdateThumbnailImage(itemId);
        Main_ReloadCurrentWindow();
    }
    private Task ItemButton_ContextMenu_EditItem(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return Task.CompletedTask;

        AddItemOverlay_ShowEdit(item);
        return Task.CompletedTask;
    }
    private async Task ItemButton_ContextMenu_EditItemTitle(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return;

        string? newTitle = await Main_ShowTextDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Title.NewItemTitle], item.Title);
        if (string.IsNullOrEmpty(newTitle)) return;

        item.Title = newTitle;
        _avatarExplorerApp.SaveItemDatabase();

        _avatarExplorerApp.UpdateSearchIndex();

        Main_ReloadCurrentWindow();
    }
    private Task ItemButton_ContextMenu_AddMemo(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return Task.CompletedTask;

        _contextMenu_selectedItemId = item.Id;

        AddMemoOverlay_Show(item.ItemMemo);

        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_AddToBulkImportList(string itemId)
    {
        BulkImportItem_Add(itemId);
        return Task.CompletedTask;
    }
    private async Task ItemButton_ContextMenu_AddItemFile(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return;

        string[]? files = await StorageService.OpenFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFilePath], true);
        if (files == null || files.Length == 0) return;

        await ItemButton_ContextMenu_AddItemPathsInternal(item, files);
    }
    private async Task ItemButton_ContextMenu_AddItemFolder(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return;

        string[]? folders = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], true);
        if (folders == null || folders.Length == 0) return;

        await ItemButton_ContextMenu_AddItemPathsInternal(item, folders);
    }
    private async Task ItemButton_ContextMenu_AddItemPathsInternal(Item item, string[] itemPaths)
    {
        ProgressOverlay_Show(Localizer.Instance[LocalizationKey.Processing.ItemAdd.Copying]);
        ProgressOverlay_Update(0);
        IReadOnlyList<string> processingFailedPaths = await _avatarExplorerApp.AddItemPaths(item.Id, itemPaths);
        ProgressOverlay_Hide();

        if (processingFailedPaths.Count > 0) // 処理に失敗したファイル、もしくはフォルダがあった時
        {
            Dialog_Show(
                Localizer.Instance[LocalizationKey.Error.Default],
                Localizer.Instance.GetDisplayName(LocalizationKey.Error.ItemFolderProcessingFailedPaths, "\n" + string.Join('\n', processingFailedPaths.Select(i => $"- {Path.GetFileName(i)}")))
            );
        }
    }

    internal string? _contextMenu_selectedItemId = null;
    private Task ItemButton_ContextMenu_EditImplementedAvatar(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return Task.CompletedTask;

        _contextMenu_selectedItemId = item.Id;

        EditImplementedAvatarsOverlay_Show(item.ImplementedAvatarsView);

        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_EditItemTag(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return Task.CompletedTask;

        _contextMenu_selectedItemId = item.Id;

        EditTagsOverlay_Show(item.TagsView);

        return Task.CompletedTask;
    }
    private async Task ItemButton_ContextMenu_RemoveItem(string itemId)
    {
        Item? item = ItemButton_ContextMenu_GetItemById(itemId);
        if (item == null) return;

        YesNoResult result = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance.GetDisplayName(LocalizationKey.UI.Dialog.Confirmation.RemoveItem, item.Title));
        if (result != YesNoResult.Yes) return;

        YesNoResult result2 = await Main_ShowYesNoDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Confirmation.RemoveAvatarFromSupportedAndImplemented]);
        if (result2 == YesNoResult.Yes) _avatarExplorerApp.RemoveItem(item.Id, true);
        else _avatarExplorerApp.RemoveItem(item.Id, false);

        Main_ReloadCurrentWindow();
        Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.Remove]);
    }

    private async Task ItemButton_ContextMenu_OpenFile(string filePath)
    {
        await LauncherService.OpenFile(this, filePath);
    }
    private Task ItemButton_ContextMenu_OpenFileInExplorer(string filePath)
    {
        if (!ProcessUtils.IsWindows()) return Task.CompletedTask;

        try
        {
            Process.Start("explorer.exe", "/select," + filePath);
        }
        catch (Exception ex)
        {
            ErrorManager.Instance.PostError(string.Format("Failed to open file. '{0}'", filePath), ex);
        }

        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_AddFileToBulkImportList(string filePath)
    {
        string? itemId = _avatarExplorerApp.GetSelectedItem()?.Id;
        if (itemId == null) return Task.CompletedTask;

        BulkImportItem_Add(itemId, filePath);
        return Task.CompletedTask;
    }
    #endregion
}
