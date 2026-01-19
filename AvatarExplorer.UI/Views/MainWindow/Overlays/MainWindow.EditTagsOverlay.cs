using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvatarExplorer.UI.Extensions;
using AvatarExplorer.UI.Factories;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private readonly List<string> _editTagsOverlay_selectedTags = new();
    private static readonly Vector VectorMaxValue = new(double.MaxValue, double.MaxValue);

    private void EditTagsOverlay_Show(IReadOnlyList<string>? tags = null)
    {
        EditTagsOverlay.IsVisible = true;
        EditTagsOverlay_TagTextBox.Text = string.Empty;
        EditTagsOverlay_InitializeList(tags);
    }
    private void EditTagsOverlay_Hide() => EditTagsOverlay.IsVisible = false;

    private void EditTagsOverlay_InitializeList(IReadOnlyList<string>? tags = null)
    {
        _editTagsOverlay_selectedTags.Clear();
        if (tags != null) _editTagsOverlay_selectedTags.AddRange(tags);

        EditTagsOverlay_RefleshList();
        EditTagsOverlay_ReloadTagList();
    }
    private void EditTagsOverlay_RefleshList()
    {
        EditTagsOverlay_TagComboBox.Items.Clear();
        EditTagsOverlay_TagComboBox.Items.AddRange(
            _avatarExplorerApp.GetAllItems()
                .SelectMany(i => i.TagsView)
                .Distinct()
                .Select(i => new ComboBoxItem() { Content = i })
        );
    }
    private void EditTagsOverlay_ReloadTagList()
    {
        EditTagsOverlay_TagList.Children.Clear();

        foreach (string tag in _editTagsOverlay_selectedTags)
        {
            Button tagButton = ItemButtonFactory.GetTagButton(tag, EditTagsOverlay_Tag_Click);
            tagButton.Classes.Add("accent");
            EditTagsOverlay_TagList.Children.Add(tagButton);
        }

        EditTagsOverlay_TagListScrollViewer.Offset = VectorMaxValue;
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
    private void EditTagsOverlay_TagTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) EditTagsOverlay_AddTagByText();
        else if (e.Key == Key.Escape) EditTagsOverlay_TagTextBox.Text = string.Empty;
    }
    private void EditTagsOverlay_AddTagButton_Click(object? sender, RoutedEventArgs e) => EditTagsOverlay_AddTagByText();
    private void EditTagsOverlay_AddTagByText()
    {
        if (!string.IsNullOrEmpty(EditTagsOverlay_TagTextBox.Text) && !_editTagsOverlay_selectedTags.Contains(EditTagsOverlay_TagTextBox.Text))
        {
            _editTagsOverlay_selectedTags.Add(EditTagsOverlay_TagTextBox.Text);
        }

        EditTagsOverlay_ReloadTagList();
        EditTagsOverlay_TagTextBox.Text = string.Empty;
    }
    private void EditTagsOverlay_TagComboBox_SelectionChanged(object? sender, RoutedEventArgs e)
    {
        string? selectedTag = (EditTagsOverlay_TagComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (string.IsNullOrEmpty(selectedTag) || _editTagsOverlay_selectedTags.Contains(selectedTag))
        {
            EditTagsOverlay_TagComboBox.SelectedIndex = -1;
            return;
        }

        _editTagsOverlay_selectedTags.Add(selectedTag);
        EditTagsOverlay_ReloadTagList();

        EditTagsOverlay_TagComboBox.SelectedIndex = -1;
    }
    private void EditTagsOverlay_Cancel_Click(object? sender, RoutedEventArgs e) => EditTagsOverlay_Hide();
    private void EditTagsOverlay_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        if (_contextMenu_selectedItem != null)
        {
            _contextMenu_selectedItem.UpdateTags(_editTagsOverlay_selectedTags);
            _avatarExplorerApp.SaveItemDatabase();
            _avatarExplorerApp.UpdateSearchIndex(_contextMenu_selectedItem);
        }

        EditTagsOverlay_Hide();
        Main_ReloadCurrentWindow();
    }
    #endregion
}
