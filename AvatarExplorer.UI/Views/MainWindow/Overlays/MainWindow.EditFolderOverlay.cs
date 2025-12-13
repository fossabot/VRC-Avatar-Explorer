using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private async void EditFoldersOverlay_AddFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await StorageService.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], true);
        if (folders == null || folders.Length == 0) return;

        _addItemWindowValues.Folders.AddRange(folders);
        EditFoldersOverlay_UpdateFolderList();
    }
    private async void EditFoldersOverlay_AddFile_Click(object? sender, RoutedEventArgs e)
    {
        string[]? files = await StorageService.OpenFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], true);
        if (files == null || files.Length == 0) return;

        _addItemWindowValues.Folders.AddRange(files);
        EditFoldersOverlay_UpdateFolderList();
    }

    private void EditFoldersOverlay_RemoveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string folderPath)
        {
            _addItemWindowValues.Folders.RemoveAll(i => i == folderPath);
            EditFoldersOverlay_UpdateFolderList();
        }
    }

    private void EditFoldersOverlay_Border_Click(object? sender, RoutedEventArgs e)
        => EditFoldersOverlay.IsVisible = false;
    private void EditFoldersOverlay_ConfirmButton_Click(object? sender, RoutedEventArgs e)
        => EditFoldersOverlay.IsVisible = false;
    
    #region Methods
    internal void EditFoldersOverlay_UpdateFolderList()
    {
        EditFoldersOverlay_FolderList.Children.Clear();
        EditFoldersOverlay_FolderList.RowDefinitions.Clear();

        for (int i = 0; i < _addItemWindowValues.Folders.Count; i++)
        {
            string folder = _addItemWindowValues.Folders[i];
            EditFoldersOverlay_AddFolderRow(EditFoldersOverlay_FolderList, i, folder, EditFoldersOverlay_RemoveButton_Click);
        }

        if (_addItemWindowValues.Folders.Count > 0)
        {
            AddItemOverlay_FolderNamesTextBlock.Text = string.Format("{0}個: {1}", _addItemWindowValues.Folders.Count, Path.GetFileName(_addItemWindowValues.Folders[0]));
        } else
        {
            AddItemOverlay_FolderNamesTextBlock.Text = "何も選択されていません";
        }
    }
    private void EditFoldersOverlay_AddFolderRow(Grid folderListPanel, int index, string folder, EventHandler<RoutedEventArgs> onRemoveClick)
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
        Grid.SetColumn(folderLabel, 2);
        folderPanel.Children.Add(folderLabel);

        Button folderRemoveButton = new()
        {
            Content = Localizer.Instance[LocalizationKey.UI.Overlay.EditFolder.RemoveFolder],
            FontSize = 14,
            Padding = new Thickness(10, 4),
            Background = new SolidColorBrush(Color.FromRgb(210, 0, 0)),
            Foreground = Brushes.White,
            BorderBrush = Brushes.DarkRed,
            BorderThickness = new Thickness(1),
            Tag = folder
        };
        Grid.SetColumn(folderRemoveButton, 3);
        folderRemoveButton.Click += EditFoldersOverlay_RemoveButton_Click;
        folderPanel.Children.Add(folderRemoveButton);

        Grid.SetRow(rowBorder, folderListPanel.RowDefinitions.Count);
        folderListPanel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        folderListPanel.Children.Add(rowBorder);
    }
    #endregion
}
