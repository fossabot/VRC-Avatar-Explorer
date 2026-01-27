using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Data.Links;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.Booth;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models.OverlayValues;
using System.IO;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using AvatarExplorer.UI.Services;
using System.Threading.Tasks;
using AvatarExplorer.UI.Extensions;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    internal string? _addItemOverlay_selectedItemId = null;
    internal readonly AddItemOverlayWindowValues _addItemOverlay_addItemWindowValues = new();

    private void AddItemOverlay_ShowEdit(Item item)
    {
        AddItemOverlay_InitializeAddItemWindowCategories();

        _addItemOverlay_selectedItemId = item.Id;
        AddItemOverlay_BoothLinkTextBox.Text = item.BoothId == -1 ? string.Empty : item.GetBoothLink();

        _addItemOverlay_addItemWindowValues.Folders.Clear();
        _addItemOverlay_addItemWindowValues.Folders.Add(ItemUtils.GetItemPath(RuntimeSettings.DataRootDirectory, item.ItemPath));
        _addItemOverlay_addItemWindowValues.FromItem(item);
        AddItemOverlay_UpdateFolderList();
        AddItemOverlay_SetValuesToUi(_addItemOverlay_addItemWindowValues);

        AddItemOverlay.IsVisible = true;

        AddItemOverlay_UpdateSupportedAvatarsLabel();
    }
    private void AddItemOverlay_ShowAdd(IEnumerable<string>? filePaths = null)
    {
        // もし表示されてる状態でD&Dされたら、フォルダ追加だけしてあげる
        if (AddItemOverlay.IsVisible && filePaths != null)
        {
            _addItemOverlay_addItemWindowValues.Folders.AddRange(filePaths);
            AddItemOverlay_UpdateFolderList();
            return;
        }

        AddItemOverlay_InitializeAddItemWindowCategories();
        
        _addItemOverlay_selectedItemId = null;
        AddItemOverlay_BoothLinkTextBox.Text = string.Empty;

        _addItemOverlay_addItemWindowValues.Reset();
        
        if (filePaths != null) _addItemOverlay_addItemWindowValues.Folders.AddRange(filePaths);
        AddItemOverlay_UpdateFolderList();

        AddItemOverlay_SetValuesToUi(_addItemOverlay_addItemWindowValues);

        AddItemOverlay.IsVisible = true;

        AddItemOverlay_UpdateSupportedAvatarsLabel();
    }
    private async Task AddItemOverlay_ShowAdd(LaunchInfo launchInfo)
    {
        AddItemOverlay_InitializeAddItemWindowCategories();

        _addItemOverlay_selectedItemId = null;
        AddItemOverlay_BoothLinkTextBox.Text = string.Format(BoothLink.ItemURLWithoutAuthorFormat, launchInfo.AssetId);

        _addItemOverlay_addItemWindowValues.Reset();
        
        _addItemOverlay_addItemWindowValues.Folders.AddRange(launchInfo.AssetDirs);
        AddItemOverlay_SetValuesToUi(_addItemOverlay_addItemWindowValues);

        AddItemOverlay.IsVisible = true;

        AddItemOverlay_UpdateFolderList();

        AddItemOverlay_UpdateSupportedAvatarsLabel();
        
        await AddItemOverlay_GetBoothItemData();
    }
    private void AddItemOverlay_Hide()
    {
        _addItemOverlay_selectedItemId = null;
        _addItemOverlay_addItemWindowValues.Reset();
        _editSupportedAvatarsOverlay_selectedAvatars.Clear();
        AddItemOverlay.IsVisible = false;
    }

    internal void AddItemOverlay_UpdateFolderList()
    {
        AddItemOverlay_FolderList.Children.Clear();
        AddItemOverlay_FolderList.RowDefinitions.Clear();

        for (int i = 0; i < _addItemOverlay_addItemWindowValues.Folders.Count; i++)
        {
            string folder = _addItemOverlay_addItemWindowValues.Folders[i];
            AddItemOverlay_AddFolderRow(AddItemOverlay_FolderList, i, folder);
        }
    }
    private void AddItemOverlay_AddFolderRow(Grid folderListPanel, int index, string folder)
    {
        Border rowBorder = new()
        {
            BorderBrush = Brushes.Gray,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(8, 6)
        };

        Grid folderPanel = new()
        {
            ColumnDefinitions = new ColumnDefinitions("30,10,*,Auto,5"),
            ColumnSpacing = 6
        };
        rowBorder.Child = folderPanel;

        TextBlock indexLabel = new()
        {
            Text = (index + 1).ToString(),
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeight.Bold
        };
        Grid.SetColumn(indexLabel, 0);
        folderPanel.Children.Add(indexLabel);

        TextBlock folderLabel = new()
        {
            Text = Path.GetFileName(folder),
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeight.Medium,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        ToolTip.SetTip(folderLabel, Path.GetFileName(folder));
        Grid.SetColumn(folderLabel, 2);
        folderPanel.Children.Add(folderLabel);

        Button folderRemoveButton = new()
        {
            Content = Localizer.Instance[LocalizationKey.UI.Overlay.AddItem.RemoveFolder],
            FontSize = 14,
            Padding = new Thickness(10, 4),
            Background = new SolidColorBrush(Color.FromRgb(210, 0, 0)),
            Foreground = Brushes.White,
            BorderBrush = Brushes.DarkRed,
            BorderThickness = new Thickness(1),
            Tag = folder
        };
        Grid.SetColumn(folderRemoveButton, 3);
        folderRemoveButton.Click += AddItemOverlay_RemoveButton_Click;
        folderPanel.Children.Add(folderRemoveButton);

        if (_addItemOverlay_selectedItemId != null && index == 0)
        {
            folderRemoveButton.IsEnabled = false; // 親フォルダは削除できないように
        }

        Grid.SetRow(rowBorder, folderListPanel.RowDefinitions.Count);
        folderListPanel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        folderListPanel.Children.Add(rowBorder);
    }
    private void AddItemOverlay_RemoveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string folderPath)
        {
            _addItemOverlay_addItemWindowValues.Folders.RemoveAll(i => i == folderPath);
            AddItemOverlay_UpdateFolderList();
        }
    }

    private void AddItemOverlay_InitializeAddItemWindowCategories()
    {
        AddItemOverlay_ItemTypeComboBox.Items.Clear();
        AddItemOverlay_ItemTypeComboBox.Items.AddRange(_avatarExplorerApp.GetCategories(includeEmptyCategory: true).Select(i => Localizer.Instance[((Category)i.Item).ToString()]));

        if (AddItemOverlay_ItemTypeComboBox.Items.Count > 0) AddItemOverlay_ItemTypeComboBox.SelectedIndex = 0;
    }

    private void AddItemOverlay_UpdateSupportedAvatarsLabel()
    {
        AddItemOverlay_EditSupportedAvatarsButton.Content = string.Format(Localizer.Instance.GetDisplayName(LocalizationKey.UI.Overlay.AddItem.SelectedAvatarsCount, _addItemOverlay_addItemWindowValues.SupportedAvatarsView.Count.ToString()));
    }

    private void AddItemOverlay_SetValuesToUi(AddItemOverlayWindowValues addItemWindowValues)
    {
        AddItemOverlay_BoothItemTitleTextBox.Text = addItemWindowValues.Title;
        AddItemOverlay_BoothItemAuthorTextBox.Text = addItemWindowValues.Author;
        AddItemOverlay_ItemTypeComboBox.SelectedIndex = (int)addItemWindowValues.ItemType;
        AddItemOverlay_UpdateSupportedAvatarsLabel();
        AddItemOverlay_InternalAuthorIdTextBox.Text = addItemWindowValues.BoothAuthorId;
        AddItemOverlay_InternalBoothIdTextBox.Text = addItemWindowValues.BoothId == -1 ? string.Empty : addItemWindowValues.BoothId.ToString();
        AddItemOverlay_InternalImageURLTextBox.Text = addItemWindowValues.BoothThumbnailUrl;
        AddItemOverlay_InternalAuthorImageURLTextBox.Text = addItemWindowValues.BoothAuthorThumbnailUrl;
    }
    private void AddItemOverlay_SetValuesFromUi(AddItemOverlayWindowValues addItemWindowValues)
    {
        addItemWindowValues.Title = AddItemOverlay_BoothItemTitleTextBox.Text ?? string.Empty;
        addItemWindowValues.Author = AddItemOverlay_BoothItemAuthorTextBox.Text ?? string.Empty;
        addItemWindowValues.BoothAuthorId = AddItemOverlay_InternalAuthorIdTextBox.Text ?? string.Empty;
        addItemWindowValues.BoothId = int.TryParse(AddItemOverlay_InternalBoothIdTextBox.Text ?? string.Empty, out int id) ? id : -1;
        addItemWindowValues.BoothThumbnailUrl = AddItemOverlay_InternalImageURLTextBox.Text ?? string.Empty;
        addItemWindowValues.BoothAuthorThumbnailUrl = AddItemOverlay_InternalAuthorImageURLTextBox.Text ??string.Empty;
    }
    
    private (ItemType, string) AddItemOverlay_GetCategoryFromItemWindow()
    {
        int selectedIndex = AddItemOverlay_ItemTypeComboBox.SelectedIndex;

        // カスタムカテゴリかどうかのチェック(式: ItemTypeの数 - 無効なItemType数 - カスタムカテゴリ)
        if (selectedIndex >= (Enum.GetValues<ItemType>().Length - CategoryUtils.InvalidItemTypes.Length - 1))
        {
            return (ItemType.Custom, AddItemOverlay_ItemTypeComboBox.SelectedItem?.ToString() ?? string.Empty);
        }

        return ((ItemType)selectedIndex, string.Empty);
    }
    private bool AddItemOverlay_ValidateAddItemWindowValues()
    {
        string errorMessage = _addItemOverlay_addItemWindowValues.Validate();
        bool result = string.IsNullOrEmpty(errorMessage);
        if (!result) Dialog_Show(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[errorMessage]);

        return result;
    }

    #region Event Handler
    private async void AddItemOverlay_GetBoothItemData_Click(object? sender, RoutedEventArgs e) => await AddItemOverlay_GetBoothItemData();
    private async Task AddItemOverlay_GetBoothItemData()
    {
        string boothUrl = AddItemOverlay_BoothLinkTextBox.Text ?? string.Empty;

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
        _addItemOverlay_addItemWindowValues.ItemType = CategoryUtils.InvalidItemTypes.Contains(boothItem.EstimatedCategory) ? ItemType.Avatar : boothItem.EstimatedCategory;
        
        AddItemOverlay_SetValuesToUi(_addItemOverlay_addItemWindowValues);
    }
    private async void AddItemOverlay_AddCustomCategory_Click(object? sender, RoutedEventArgs e)
    {
        string? customCategory = await Main_ShowTextDialogAsync(Localizer.Instance[LocalizationKey.UI.Dialog.Title.AddCustomCategory]);
        if (string.IsNullOrEmpty(customCategory)) return;

        int index = AddItemOverlay_ItemTypeComboBox.Items.Add(customCategory);
        AddItemOverlay_ItemTypeComboBox.SelectedIndex = index;
    }
    private void AddItemOverlay_EditSupportedAvatars_Click(object? sender, RoutedEventArgs e)
    {
        EditSupportedAvatarsOverlay_Show(_addItemOverlay_addItemWindowValues.SupportedAvatarsView);
    }
    private async void AddItemOverlay_AddFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], true);
        if (folders == null || folders.Length == 0) return;

        _addItemOverlay_addItemWindowValues.Folders.AddRange(folders);
        AddItemOverlay_UpdateFolderList();
    }
    private async void AddItemOverlay_AddFile_Click(object? sender, RoutedEventArgs e)
    {
        string[]? files = await StorageService.OpenFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], true);
        if (files == null || files.Length == 0) return;

        _addItemOverlay_addItemWindowValues.Folders.AddRange(files);
        AddItemOverlay_UpdateFolderList();
    }

    private async void AddItemOverlay_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemOverlay_addItemWindowValues == null) return;

        AddItemOverlay_SetValuesFromUi(_addItemOverlay_addItemWindowValues);

        if (!AddItemOverlay_ValidateAddItemWindowValues()) return;

        ItemCreationContext itemCreationContext = new();
        itemCreationContext.Folders.AddRange(_addItemOverlay_addItemWindowValues.Folders);
        itemCreationContext.Title = _addItemOverlay_addItemWindowValues.Title;
        itemCreationContext.Author = _addItemOverlay_addItemWindowValues.Author;
        itemCreationContext.AuthorId = _addItemOverlay_addItemWindowValues.BoothAuthorId;
        itemCreationContext.ThumbnailUrl = _addItemOverlay_addItemWindowValues.BoothThumbnailUrl;
        itemCreationContext.AuthorThumbnailUrl = _addItemOverlay_addItemWindowValues.BoothAuthorThumbnailUrl;
        itemCreationContext.BoothId = _addItemOverlay_addItemWindowValues.BoothId;

        (ItemType itemType, string customCategory) = AddItemOverlay_GetCategoryFromItemWindow();
        itemCreationContext.ItemType = itemType;
        if (itemType == ItemType.Custom) itemCreationContext.CustomCategory = customCategory;

        itemCreationContext.SupportedAvatars.AddRange(_addItemOverlay_addItemWindowValues.SupportedAvatarsView);

        if (_addItemOverlay_selectedItemId == null)
        {
            ProgressOverlay_Show(Localizer.Instance[LocalizationKey.Processing.ItemAdd.Copying]);
            ProgressOverlay_Update(0);
            (Item? newItem, List<string> processingFailedPaths) = await _avatarExplorerApp.AddItem(itemCreationContext);
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
            ProgressOverlay_Show(Localizer.Instance[LocalizationKey.Processing.ItemAdd.Copying]);
            ProgressOverlay_Update(0);
            Item? edittedItem = await _avatarExplorerApp.EditItem(_addItemOverlay_selectedItemId, itemCreationContext);
            ProgressOverlay_Hide();

            if (edittedItem != null) Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.ItemEdit]);
            else Dialog_Show(Localizer.Instance[LocalizationKey.UI.Dialog.Failed.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Failed.ItemEdit]);
        }

        AddItemOverlay_Hide();
    }
    
    private void AddItemOverlay_Close_Click(object? sender, RoutedEventArgs e) => AddItemOverlay_Hide();
    #endregion
}
