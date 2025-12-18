using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvatarExplorer.UI.Factories;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    internal readonly List<string> _editTagsOverlay_selectedTags = new();

    internal void EditTagsOverlay_Show(List<string>? tags = null)
    {
        EditTagsOverlay.IsVisible = true;
        EditTagsOverlay_InitializeList(tags);
    }
    internal void EditTagsOverlay_Hide()
        => EditTagsOverlay.IsVisible = false;

    internal void EditTagsOverlay_InitializeList(List<string>? tags = null)
    {
        _editTagsOverlay_selectedTags.Clear();
        if (tags != null) _editTagsOverlay_selectedTags.AddRange(tags);

        EditTagsOverlay_RefleshList();
        EditTagsOverlay_ReloadTagList();
    }
    internal void EditTagsOverlay_RefleshList()
    {
        EditTagsOverlay_TagComboBox.Items.Clear();
        IEnumerable<string> tags = _avatarExplorerApp.GetAllItems().SelectMany(i => i.Tags).Distinct();

        foreach (string tag in tags)
        {
            EditTagsOverlay_TagComboBox.Items.Add(new ComboBoxItem() { Content = tag });
        }
    }
    internal void EditTagsOverlay_ReloadTagList()
    {
        EditTagsOverlay_TagList.Children.Clear();

        foreach (string tag in _editTagsOverlay_selectedTags)
        {
            EditTagsOverlay_TagList.Children.Add(ItemButtonFactory.GetTagButton(tag, EditTagsOverlay_Tag_Click));
        }
    }

    #region Event Handler
    private void EditTagsOverlay_Tag_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Content is string tag)
        {
            _editTagsOverlay_selectedTags.RemoveAll(i => i == tag);
            EditTagsOverlay_ReloadTagList();
        }
    }
    private void EditTagsOverlay_TagTextBox_KeyDown(object? sender, KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Key == Key.Enter)
        {
            if (string.IsNullOrEmpty(EditTagsOverlay_TagTextBox.Text) || _editTagsOverlay_selectedTags.Contains(EditTagsOverlay_TagTextBox.Text)) return;
            _editTagsOverlay_selectedTags.Add(EditTagsOverlay_TagTextBox.Text);
            EditTagsOverlay_ReloadTagList();

            EditTagsOverlay_TagTextBox.Text = string.Empty;
        }
    }
    private void EditTagsOverlay_TagComboBox_SelectionChanged(object? sender, RoutedEventArgs e)
    {
        if (EditTagsOverlay_TagComboBox.SelectedItem == null || _editTagsOverlay_selectedTags.Contains(((ComboBoxItem)EditTagsOverlay_TagComboBox.SelectedItem).Content))
        {
            EditTagsOverlay_TagComboBox.SelectedIndex = -1;
            return;
        }

        _editTagsOverlay_selectedTags.Add((string)EditTagsOverlay_TagComboBox.SelectedItem);
        EditTagsOverlay_ReloadTagList();

        EditTagsOverlay_TagComboBox.SelectedIndex = -1;
    }
    private void EditTagsOverlay_Cancel_Click(object? sender, RoutedEventArgs e)
        => EditTagsOverlay_Hide();
    private void EditTagsOverlay_Border_Click(object? sender, RoutedEventArgs e)
        => EditTagsOverlay_Hide();
    private void EditTagsOverlay_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        if (_contextMenu_selectedItem != null)
        {
            _contextMenu_selectedItem.Tags.Clear();
            _contextMenu_selectedItem.Tags.AddRange(_editTagsOverlay_selectedTags);
        }

        EditTagsOverlay_Hide();
        Main_ReloadCurrentWindow();
    }
    #endregion
}
