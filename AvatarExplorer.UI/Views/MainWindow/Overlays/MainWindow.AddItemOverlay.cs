using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.Booth;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models.OverlayValues;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    internal Item? _addItemOverlay_selectedItem = null;
    internal readonly AddItemOverlayWindowValues _addItemOverlay_addItemWindowValues = new();

    private void AddItemOverlay_ShowEdit(Item item)
    {
        AddItemOverlay_InitializeAddItemWindowCategories();

        _addItemOverlay_selectedItem = item;
        _addItemOverlay_addItemWindowValues.FromItem(item);
        AddItemOverlay_SetValuesToUi(_addItemOverlay_addItemWindowValues);
        AddItemOverlay_BoothLinkTextBox.Text = item.GetBoothLink();
        AddItemOverlay.IsVisible = true;
    }
    private void AddItemOverlay_ShowAdd(IEnumerable<string>? filePaths = null)
    {
        // もし表示されてる状態でD&Dされたら、フォルダ追加だけしてあげる
        if (AddItemOverlay.IsVisible && filePaths != null)
        {
            _addItemOverlay_addItemWindowValues.Folders.AddRange(filePaths);
            EditFoldersOverlay_UpdateFolderList();
            return;
        }

        AddItemOverlay_InitializeAddItemWindowCategories();

        _addItemOverlay_selectedItem = null;
        _addItemOverlay_addItemWindowValues.Reset();
        AddItemOverlay_SetValuesToUi(_addItemOverlay_addItemWindowValues);
        AddItemOverlay_BoothLinkTextBox.Text = string.Empty;
        AddItemOverlay.IsVisible = true;

        if (filePaths != null) _addItemOverlay_addItemWindowValues.Folders.AddRange(filePaths);
        EditFoldersOverlay_UpdateFolderList();
    }
    private void AddItemOverlay_Hide()
    {
        _addItemOverlay_selectedItem = null;
        _addItemOverlay_addItemWindowValues.Reset();
        AddItemOverlay.IsVisible = false;
    }

    private void AddItemOverlay_InitializeAddItemWindowCategories()
    {
        AddItemOverlay_ItemTypeComboBox.Items.Clear();

        foreach (ItemCountInfo itemCountInfo in _avatarExplorerApp.GetCategories())
        {
            AddItemOverlay_ItemTypeComboBox.Items.Add(Localizer.Instance[((Category)itemCountInfo.Item).ToString()]);
        }

        if (AddItemOverlay_ItemTypeComboBox.Items.Count > 0) AddItemOverlay_ItemTypeComboBox.SelectedIndex = 0;
    }

    private void AddItemOverlay_SetValuesToUi(AddItemOverlayWindowValues addItemWindowValues)
    {
        AddItemOverlay_BoothItemTitleTextBox.Text = addItemWindowValues.Title;
        AddItemOverlay_BoothItemAuthorTextBox.Text = addItemWindowValues.Author;
    }
    private void AddItemOverlay_SetValuesFromUi(AddItemOverlayWindowValues addItemWindowValues)
    {
        addItemWindowValues.Title = AddItemOverlay_BoothItemTitleTextBox.Text ?? "";
        addItemWindowValues.Author = AddItemOverlay_BoothItemAuthorTextBox.Text ?? "";
    }
    private (ItemType, string) AddItemOverlay_GetCategoryFromItemWindow()
    {
        int selectedIndex = AddItemOverlay_ItemTypeComboBox.SelectedIndex;

        // カスタムカテゴリかどうかのチェック(式: ItemTypeの数 - 無効なItemType数 - カスタムカテゴリ)
        if (selectedIndex >= (Enum.GetValues<ItemType>().Length - CategoryUtils.InvalidItemTypes.Length - 1)) // ここの1はカスタムカテゴリ分
        {
            return (ItemType.Custom, AddItemOverlay_ItemTypeComboBox.SelectedItem?.ToString() ?? "");
        }

        return ((ItemType)selectedIndex, string.Empty);
    }
    private bool AddItemOverlay_ValidateAddItemWindowValues()
    {
        var validationResult = _addItemOverlay_addItemWindowValues.Validate();
        if (!validationResult.Item1) Dialog_Show(LocalizationKey.Error.Default, Localizer.Instance[validationResult.Item2]);

        return validationResult.Item1;
    }

    #region Event Handler
    private async void AddItemOverlay_GetBoothItemData_Click(object? sender, RoutedEventArgs e)
    {
        string boothUrl = AddItemOverlay_BoothLinkTextBox.Text ?? "";

        if (_avatarExplorerApp.IsApiCooldownNow)
        {
            Dialog_Show(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.BoothApiCooldown]);
            return;
        }
        
        ProgressOverlay_Show(Localizer.Instance[LocalizationKey.Processing.Booth.Status.Fetching]);
        ProgressOverlay_Update(0);
        
        BoothItem? boothItem = await _avatarExplorerApp.GetBoothItem(boothUrl);
        ProgressOverlay_Hide();

        if (boothItem == null)
        {
            Dialog_Show(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.BoothItemNotFound]);
            return;
        }

        _addItemOverlay_addItemWindowValues.Title = boothItem.Title;
        _addItemOverlay_addItemWindowValues.Author = boothItem.Shop.Name;
        _addItemOverlay_addItemWindowValues.BoothAuthorId = boothItem.AuthorId;
        _addItemOverlay_addItemWindowValues.BoothId = boothItem.BoothId;
        _addItemOverlay_addItemWindowValues.BoothThumbnailUrl = boothItem.Thumbnails.Count > 0 ? boothItem.Thumbnails[0].Original : string.Empty;
        _addItemOverlay_addItemWindowValues.BoothAuthorThumbnailUrl = boothItem.Shop.ThumbnailUrl;
        _addItemOverlay_addItemWindowValues.ItemType = (boothItem.EstimatedCategory != ItemType.None && boothItem.EstimatedCategory != ItemType.Unknown) ? boothItem.EstimatedCategory : ItemType.Avatar;

        AddItemOverlay_ResetBoothItemDataButton.IsVisible = true;

        AddItemOverlay_SetValuesToUi(_addItemOverlay_addItemWindowValues);
    }
    private void AddItemOverlay_ResetBoothItemData_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemOverlay_addItemWindowValues == null) return;

        _addItemOverlay_addItemWindowValues.Title = string.Empty;
        _addItemOverlay_addItemWindowValues.Author = string.Empty;
        _addItemOverlay_addItemWindowValues.BoothAuthorId = string.Empty;
        _addItemOverlay_addItemWindowValues.BoothId = -1;
        _addItemOverlay_addItemWindowValues.BoothThumbnailUrl = string.Empty;
        _addItemOverlay_addItemWindowValues.BoothAuthorThumbnailUrl = string.Empty;

        AddItemOverlay_ResetBoothItemDataButton.IsVisible = false;

        AddItemOverlay_SetValuesToUi(_addItemOverlay_addItemWindowValues);
    }

    private async void AddItemOverlay_EditFolder_Click(object? sender, RoutedEventArgs e)
    {
        EditFoldersOverlay_Show();
    }
    private void AddItemOverlay_AddCustomCategory_Click(object? sender, RoutedEventArgs e)
    {
        AddCustomCategory_Show();
    }
    private void AddItemOverlay_EditSupportedAvatars_Click(object? sender, RoutedEventArgs e)
    {
        EditSupportedAvatarsOverlay_Show(_addItemOverlay_addItemWindowValues.SupportedAvatars);
    }

    private async void AddItemOverlay_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemOverlay_addItemWindowValues == null) return;

        AddItemOverlay_SetValuesFromUi(_addItemOverlay_addItemWindowValues);

        if (!AddItemOverlay_ValidateAddItemWindowValues()) return;

        ItemCreationContext itemCreationContext = new();
        itemCreationContext.Folders.AddRange(_addItemOverlay_addItemWindowValues.Folders);
        itemCreationContext.MaterialFolder = _addItemOverlay_addItemWindowValues.MaterialFolder;
        itemCreationContext.Title = _addItemOverlay_addItemWindowValues.Title;
        itemCreationContext.Author = _addItemOverlay_addItemWindowValues.Author;
        itemCreationContext.AuthorId = _addItemOverlay_addItemWindowValues.BoothAuthorId;
        itemCreationContext.ThumbnailUrl = _addItemOverlay_addItemWindowValues.BoothThumbnailUrl;
        itemCreationContext.AuthorThumbnailUrl = _addItemOverlay_addItemWindowValues.BoothAuthorThumbnailUrl;
        itemCreationContext.BoothId = _addItemOverlay_addItemWindowValues.BoothId;

        var categoryInfo = AddItemOverlay_GetCategoryFromItemWindow();
        itemCreationContext.ItemType = categoryInfo.Item1;
        if (categoryInfo.Item1 == ItemType.Custom) itemCreationContext.CustomCategory = categoryInfo.Item2;

        itemCreationContext.SupportedAvatars.AddRange(_addItemOverlay_addItemWindowValues.SupportedAvatars);
        itemCreationContext.LocalizedItemTypeName = categoryInfo.Item1 == ItemType.Custom ? categoryInfo.Item2 : Localizer.Instance[categoryInfo.Item1.GetLocalizationKey() ?? ""];

        if (_addItemOverlay_selectedItem == null)
        {
            ProgressOverlay_Show(Localizer.Instance[LocalizationKey.Processing.ItemAdd.Copying]);
            ProgressOverlay_Update(0);
            var (newItem, processingFailedPaths) = await _avatarExplorerApp.AddItem(itemCreationContext);
            ProgressOverlay_Hide();

            if (processingFailedPaths.Count > 0) // フォルダ展開に失敗した時に発生する
            {
                Dialog_Show(
                    Localizer.Instance[LocalizationKey.Error.Default],
                    Localizer.Instance.GetDisplayName(LocalizationKey.Error.ItemFolderProcessingFailedPaths, "\n" + string.Join('\n', processingFailedPaths.Select(i => $"- {i}")))
                );
            }

            if (newItem != null) Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.ItemAdd]);
            else Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Failed.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Failed.ItemAdd]);
        }
        else
        {
            _avatarExplorerApp.EditItem(_addItemOverlay_selectedItem, itemCreationContext);
            Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.ItemEdit]);
        }

        AddItemOverlay_Hide();
    }
    
    private void AddItemOverlay_Close_Click(object? sender, RoutedEventArgs e)
        => AddItemOverlay_Hide();
    private void AddItemOverlay_Border_Click(object? sender, RoutedEventArgs e)
        => AddItemOverlay_Hide();
    #endregion
}
