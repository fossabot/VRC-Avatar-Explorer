using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Factories;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private readonly List<BulkImportItem> _bulkImportPanel_bulkImportItems = new();

    private void BulkImportItem_Add(string itemId)
    {
        SidePanel_Show();
        
        int bulkImportPanelTabIndex = SidePanel_TabControl.Items.IndexOf(SidePanel_BulkImportPanelTab);
        if (bulkImportPanelTabIndex != -1 && SidePanel_TabControl.SelectedIndex != bulkImportPanelTabIndex) SidePanel_TabControl.SelectedIndex = bulkImportPanelTabIndex;

        _bulkImportPanel_bulkImportItems.Add(new BulkImportItem(itemId));
        ReloadBulkImportItemButtons();
        
        SidePanel_BulkImportPanelScrollViewer.Offset = AvaloniaVectorUtils.MaxValue;
    }

    private void BulkImportItemButton_Copy_Click(int itemIndex)
    {
        BulkImportItem item = _bulkImportPanel_bulkImportItems[itemIndex];
        BulkImportItem_Add(item.ItemId);
    }

    private void BulkImportItemButton_Remove_Click(int itemIndex)
    {
        _bulkImportPanel_bulkImportItems.RemoveAt(itemIndex);
        ReloadBulkImportItemButtons();
    }

    private void BulkImportItemButton_SelectionChanged(int itemIndex, int selectedIndex)
    {
        _bulkImportPanel_bulkImportItems[itemIndex].SelectedIndex = selectedIndex;
    }

    private async void BulkImportPanel_Import_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            List<string> filePaths = new();
            List<string> categories = new();

            foreach (BulkImportItem bulkImportItem in _bulkImportPanel_bulkImportItems)
            {
                Item? item = _avatarExplorerApp.GetItemById(bulkImportItem.ItemId);
                if (item == null) continue;

                string filePath = UnitypackageService.GetUnitypackagePaths(ItemUtils.GetItemPath(RuntimeSettings.DataRootDirectory, item.ItemPath))[bulkImportItem.SelectedIndex];
                if (filePaths.Contains(filePath)) continue;

                filePaths.Add(filePath);

                if (item.Type == ItemType.Custom) categories.Add(item.CustomCategory);
                else categories.Add(Localizer.Instance[item.Type.GetLocalizationKey() ?? item.Type.ToString()]);
            }

            string[] itemFilePaths = filePaths.ToArray();
            string[] localizedCategoryNames = categories.ToArray();

            if (itemFilePaths.Length != localizedCategoryNames.Length) throw new InvalidOperationException("Length not matched");

            await UnitypackageService.BulkImport(
                itemFilePaths,
                localizedCategoryNames,
                onProgress: async (name, percent) =>
                {
                    ProgressOverlay_Show(Localizer.Instance.GetDisplayName(name, percent.ToString()));
                    ProgressOverlay_Update(percent);
                },
                onCompleted: async (resultPath) =>
                {
                    ProgressOverlay_Hide();

                    if (!string.IsNullOrEmpty(resultPath))
                        await LauncherService.OpenFile(this, resultPath);
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Dialog_Show(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.CategoryLengthNotMatched]);
            return;
        }
    }

    private void BulkImportPanel_Reset_Click(object? sender, RoutedEventArgs e)
    {
        _bulkImportPanel_bulkImportItems.Clear();
        ReloadBulkImportItemButtons();
    }

    private void ReloadBulkImportItemButtons()
    {
        SidePanel_BulkImportPanel.Children.Clear();

        for (int i = 0; i < _bulkImportPanel_bulkImportItems.Count; i++)
        {
            BulkImportItem bulkImportItem = _bulkImportPanel_bulkImportItems[i];
            Item? item = _avatarExplorerApp.GetItemById(bulkImportItem.ItemId);
            if (item == null) continue;

            UnitypackageSelectorButtonFactory.AddItemButton(SidePanel_BulkImportPanel, new UISelectableItem(new ItemCountInfo(item, 0)), RuntimeSettings, _userPreferences, i, bulkImportItem.SelectedIndex, BulkImportItemButton_Copy_Click, BulkImportItemButton_Remove_Click, BulkImportItemButton_SelectionChanged);
        }
    }

    private void BulkImportPanel_DragDrop_Drop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.Text)) return;

        string? itemId = e.DataTransfer.TryGetText();
        if (string.IsNullOrEmpty(itemId)) return;

        if (_avatarExplorerApp.GetItemById(itemId) != null) BulkImportItem_Add(itemId);
    }
}
