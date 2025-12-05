using Avalonia.Controls;
using AvatarExplorer.UI.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.Core.Models;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Interactivity;
using System;
using Avalonia.Threading;
using AvatarExplorer.UI.Models;
using AvatarExplorer.Core.Extensions;

namespace AvatarExplorer.UI;

public partial class MainWindow : Window
{
    private readonly Core.Services.AvatarExplorer _avatarExplorer = new();

    public MainWindow()
    {
        InitializeComponent();
        InitializeAvatarExplorer();

        RenderLeftPanel();
        RenderRightPanel();
    }

    private void InitializeAvatarExplorer()
    {
        try
        {
            _avatarExplorer.LoadItemDatabase(true);
            Localizer.Instance.LoadFromFile("locales/ja-JP.json");
        }
        catch
        {
            // Ignored
        }
    }

    #region Left Panel
    private void RenderLeftPanel()
    {
        if (LeftPanel == null) return;

        LeftPanel.Children.Clear();

        List<ItemCountInfo> items = new();
        
        string customType = string.Empty;
        switch (LeftFilter.SelectedIndex)
        {
            case 0:
            {
                items.AddRange(_avatarExplorer.GetAvatars()); customType = ItemTagState.RootAvatar; break;
            }
            case 1:
            {
                items.AddRange(_avatarExplorer.GetAuthors()); customType = ItemTagState.RootAuthor; break;
            }
            case 2:
            {
                items.AddRange(_avatarExplorer.GetCategories()); customType = ItemTagState.RootCategory; break;
            }
        }

        foreach (ItemCountInfo itemCountInfo in items.Take(30))
        {
            UIUtils.AddItemButton(LeftPanel, new UISelectableItem(itemCountInfo).SetType(customType), LeftPanelButton_Clicked);
        }
    }

    private void LeftPanelButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ItemTagInfo itemTagInfo)
        {
            _avatarExplorer.SelectClear();
            _avatarExplorer.Select(itemTagInfo.Type, itemTagInfo.Value);

            RenderRightPanel();
        }
    }
    
    private void LeftFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RenderLeftPanel();
    }
    #endregion

    #region Right Panel
    private void RenderRightPanel()
    {
        if (RightPanel == null) return;
        RightPanel.Children.Clear();

        foreach (ItemCountInfo itemCountInfo in _avatarExplorer.GetItemsForCurrentState().Take(30))
        {
            UIUtils.AddItemButton(RightPanel, new UISelectableItem(itemCountInfo), RightPanelButton_Clicked);
        }
    }
    private void RightPanelButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ItemTagInfo itemTagInfo)
        {
            if (itemTagInfo.Type == ItemTagState.ItemFileCategoryOpen)
            {
                var selectedItem = _avatarExplorer.GetSelectedItem();
                if (selectedItem == null)
                {
                    _avatarExplorer.OpenFile(itemTagInfo.Value, normalOpen: true);
                    return;
                }

                var progress = new Progress<(string, int)>(tuple =>
                {
                    if (tuple.Item2 == 100)
                    {
                        HideProgress();
                        return;
                    }

                    ShowProgress(Localizer.Instance.GetDisplayName(tuple.Item1));
                    UpdateProgress(tuple.Item2);
                });

                _avatarExplorer.OpenFile(itemTagInfo.Value, Localizer.Instance.GetDisplayName(selectedItem.Type.GetInternalId() ?? ""), progress: progress);
            }
            else
            {
                _avatarExplorer.Select(itemTagInfo.Type, itemTagInfo.Value);
                RenderRightPanel();
            }
        }
    }
    #endregion

    #region Search Box
    private readonly DispatcherTimer _searchTimer = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private void SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchTimer.Stop();
        _searchTimer.Tick -= OnSearchTimerTick;
        _searchTimer.Tick += OnSearchTimerTick;
        _searchTimer.Start();
    }

    private void OnSearchTimerTick(object? sender, EventArgs e)
    {
        _searchTimer.Stop();
        UpdateRightPanel();
    }

    private void UpdateRightPanel()
    {
        if (SearchTextBox == null) return;

        RightPanel.Children.Clear();
        var items = _avatarExplorer.SearchItems(SearchUtils.BuildFilter(SearchTextBox.Text!));

        foreach (Item item in items.Take(30))
        {
            UIUtils.AddItemButton(RightPanel, new UISelectableItem(item, 0));
        }
    }
    #endregion

    #region Dialog
    private void ShowDialog(string title, string content)
    {
        if (DialogTitle == null || DialogContent == null) return;

        DialogTitle.Text = title;
        DialogContent.Text = content;

        DialogOverlay.IsVisible = true;
    }
    private void DialogOK_Click(object? sender, RoutedEventArgs e)
    {
        if (DialogOverlay == null) return;

        DialogOverlay.IsVisible = false;
    }
    #endregion

    #region Progress Dialog
    private void ShowProgress(string title)
    {
        if (ProgressBarTitle == null || ProgressOverlay == null) return;
        ProgressBarTitle.Text = title;
        ProgressOverlay.IsVisible = true;
    }

    private void HideProgress()
    {
        if (ProgressOverlay == null) return;
        ProgressOverlay.IsVisible = false;
    }

    private void UpdateProgress(int value)
    {
        if (ProgressOverlay == null) return;
        ProgressBar.Value = Math.Clamp(value, 0, 100);
        ProgressBar.IsIndeterminate = value == 0;
    }
    #endregion

    #region UI Event Handler
    private void Undo_Click(object? sender, RoutedEventArgs e)
    {
        _avatarExplorer.SelectUndo();
        RenderRightPanel();
    }
    #endregion
}
