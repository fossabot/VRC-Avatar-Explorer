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
    internal Item? _selectedItem = null;
    internal readonly AddItemOverlayWindowValues _addItemWindowValues = new();

    private void AddItemOverlay_ShowEditItemWindow(Item item)
    {
        AddItemOverlay_InitializeAddItemWindowCategories();

        _selectedItem = item;
        _addItemWindowValues.FromItem(item);
        AddItemOverlay_SetValuesToUi(_addItemWindowValues);
        AddItemOverlay_BoothLinkTextBox.Text = item.GetBoothLink();
        AddItemOverlay.IsVisible = true;
    }
    private void AddItemOverlay_ShowAddItemWindow(IEnumerable<string>? filePaths = null)
    {
        // もし表示されてる状態でD&Dされたら、フォルダ追加だけしてあげる
        if (AddItemOverlay.IsVisible && filePaths != null)
        {
            _addItemWindowValues.Folders.AddRange(filePaths);
            EditFoldersOverlay_UpdateFolderList();
            return;
        }

        AddItemOverlay_InitializeAddItemWindowCategories();

        _selectedItem = null;
        _addItemWindowValues.Reset();
        AddItemOverlay_SetValuesToUi(_addItemWindowValues);
        AddItemOverlay_BoothLinkTextBox.Text = string.Empty;
        AddItemOverlay.IsVisible = true;

        if (filePaths != null) _addItemWindowValues.Folders.AddRange(filePaths);
        EditFoldersOverlay_UpdateFolderList();
    }
    
    private void AddItemOverlay_InitializeAddItemWindowCategories()
    {
        AddItemOverlay_ItemTypeComboBox.Items.Clear();

        foreach (ItemCountInfo itemCountInfo in _avatarExplorer.GetCategories())
        {
            AddItemOverlay_ItemTypeComboBox.Items.Add(Localizer.Instance[((Category)itemCountInfo.Item).ToString()]);
        }

        if (AddItemOverlay_ItemTypeComboBox.Items.Count > 0) AddItemOverlay_ItemTypeComboBox.SelectedIndex = 0;
    }

    private async void AddItemOverlay_GetBoothItemData_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemWindowValues == null) return;
        string boothUrl = AddItemOverlay_BoothLinkTextBox.Text ?? "";

        if (_avatarExplorer.IsApiCooldownNow)
        {
            ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.BoothApiCooldown]);
            return;
        }
        
        Main_ShowProgress(Localizer.Instance[LocalizationKey.Processing.Booth.Status.Fetching]);
        Main_UpdateProgress(0);
        
        BoothItem? boothItem = await _avatarExplorer.GetBoothItem(boothUrl);
        Main_HideProgress();

        if (boothItem == null)
        {
            ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.BoothItemNotFound]);
            return;
        }

        _addItemWindowValues.Title = boothItem.Title;
        _addItemWindowValues.Author = boothItem.Shop.Name;
        _addItemWindowValues.BoothAuthorId = boothItem.AuthorId;
        _addItemWindowValues.BoothId = boothItem.BoothId;
        _addItemWindowValues.BoothThumbnailUrl = boothItem.Thumbnails.Count > 0 ? boothItem.Thumbnails[0].Original : string.Empty;
        _addItemWindowValues.BoothAuthorThumbnailUrl = boothItem.Shop.ThumbnailUrl;
        _addItemWindowValues.ItemType = (boothItem.EstimatedCategory != ItemType.None && boothItem.EstimatedCategory != ItemType.Unknown) ? boothItem.EstimatedCategory : ItemType.Avatar;

        AddItemOverlay_ResetBoothItemDataButton.IsVisible = true;

        AddItemOverlay_SetValuesToUi(_addItemWindowValues);
    }
    private void AddItemOverlay_ResetBoothItemData_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemWindowValues == null) return;

        _addItemWindowValues.Title = string.Empty;
        _addItemWindowValues.Author = string.Empty;
        _addItemWindowValues.BoothAuthorId = string.Empty;
        _addItemWindowValues.BoothId = -1;
        _addItemWindowValues.BoothThumbnailUrl = string.Empty;
        _addItemWindowValues.BoothAuthorThumbnailUrl = string.Empty;

        AddItemOverlay_ResetBoothItemDataButton.IsVisible = false;

        AddItemOverlay_SetValuesToUi(_addItemWindowValues);
    }

    private async void AddItemOverlay_EditFolder_Click(object? sender, RoutedEventArgs e)
    {
        EditFoldersOverlay_UpdateFolderList();
        EditFoldersOverlay.IsVisible = true;
    }
    private void AddItemOverlay_AddCustomCategory_Click(object? sender, RoutedEventArgs e)
    {
        AddCustomCategory_CustomCategoryTextBox.Text = string.Empty;
        AddCustomCategoryOverlay.IsVisible = true;
    }

    private async void AddItemOverlay_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemWindowValues == null) return;

        AddItemOverlay_SetValuesFromUi(_addItemWindowValues);

        if (!AddItemOverlay_ValidateAddItemWindowValues()) return;

        ItemCreationContext itemCreationContext = new();
        itemCreationContext.Folders.AddRange(_addItemWindowValues.Folders);
        itemCreationContext.MaterialFolder = _addItemWindowValues.MaterialFolder;
        itemCreationContext.Title = _addItemWindowValues.Title;
        itemCreationContext.Author = _addItemWindowValues.Author;
        itemCreationContext.AuthorId = _addItemWindowValues.BoothAuthorId;
        itemCreationContext.ThumbnailUrl = _addItemWindowValues.BoothThumbnailUrl;
        itemCreationContext.AuthorThumbnailUrl = _addItemWindowValues.BoothAuthorThumbnailUrl;
        itemCreationContext.BoothId = _addItemWindowValues.BoothId;

        var categoryInfo = AddItemOverlay_GetCategoryFromItemWindow();
        itemCreationContext.ItemType = categoryInfo.Item1;
        if (categoryInfo.Item1 == ItemType.Custom) itemCreationContext.CustomCategory = categoryInfo.Item2;

        itemCreationContext.SupportedAvatars.AddRange(_addItemWindowValues.SupportedAvatars);
        itemCreationContext.LocalizedItemTypeName = categoryInfo.Item1 == ItemType.Custom ? categoryInfo.Item2 : Localizer.Instance[categoryInfo.Item1.GetLocalizationKey() ?? ""];

        if (_selectedItem == null)
        {
            Main_ShowProgress(Localizer.Instance[LocalizationKey.Processing.ItemAdd.Copying]);
            Main_UpdateProgress(0);
            var (newItem, processingFailedPaths) = await _avatarExplorer.AddItem(itemCreationContext);
            Main_HideProgress();

            if (processingFailedPaths.Count > 0) // フォルダ展開に失敗した時に発生する
            {
                ShowDialog(
                    Localizer.Instance[LocalizationKey.Error.Default],
                    Localizer.Instance.GetDisplayName(LocalizationKey.Error.ItemFolderProcessingFailedPaths, "\n" + string.Join('\n', processingFailedPaths.Select(i => $"- {i}")))
                );
            }

            if (newItem != null) ShowDialog(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.ItemAdd]);
            else ShowDialog(Localizer.Instance[LocalizationKey.UI.Dialog.Failed.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Failed.ItemAdd]);
        }
        else
        {
            _avatarExplorer.EditItem(_selectedItem, itemCreationContext);
            ShowDialog(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.ItemEdit]);
        }

        _selectedItem = null;
        _addItemWindowValues.Reset();
        AddItemOverlay.IsVisible = false;
    }
    private void AddItemOverlay_Close_Click(object? sender, RoutedEventArgs e)
        => AddItemOverlay_CloseInternal();

    private void AddItemOverlay_Border_Click(object? sender, RoutedEventArgs e)
        => AddItemOverlay_CloseInternal();

    private void AddItemOverlay_CloseInternal()
    {
        _selectedItem = null;
        _addItemWindowValues.Reset();
        AddItemOverlay.IsVisible = false;
    }

    
    #region Methods
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
        var validationResult = _addItemWindowValues.Validate();
        if (!validationResult.Item1) ShowDialog(LocalizationKey.Error.Default, Localizer.Instance[validationResult.Item2]);

        return validationResult.Item1;
    }
    #endregion
}
