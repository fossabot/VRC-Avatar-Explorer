using Avalonia.Controls;
using AvatarExplorer.UI.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.Core.Models;
using System.Linq;
using AvatarExplorer.Core.Interfaces;
using System.Collections.Generic;
using Avalonia.Interactivity;
using System;
using Avalonia.Threading;

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

        List<ISelectableItem> items = new();
        
        string customTagType = string.Empty;
        switch (LeftFilter.SelectedIndex)
        {
            case 0:
            {
                items.AddRange(_avatarExplorer.GetAvatars());
                customTagType = "Root.Avatar";
                break;
            }
            case 1:
            {
                items.AddRange(_avatarExplorer.GetAuthors());
                customTagType = "Root.Author";
                break;
            }
            case 2:{
                items.AddRange(_avatarExplorer.GetCategories());
                customTagType = "Root.Category";
                break;
            }
        }

        foreach (ISelectableItem item in items.Take(30))
        {
            item.CustomTagType = customTagType;
            UIUtils.AddItemButton(LeftPanel, item, LeftPanelButton_Clicked);
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

        foreach (ISelectableItem item in _avatarExplorer.GetItemsForCurrentState().Take(30))
        {
            UIUtils.AddItemButton(RightPanel, item, RightPanelButton_Clicked);
        }
    }
    private void RightPanelButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ItemTagInfo itemTagInfo)
        {
            _avatarExplorer.Select(itemTagInfo.Type, itemTagInfo.Value);
            RenderRightPanel();
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
            UIUtils.AddItemButton(RightPanel, item);
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

    private void UpdateProgress(int value, bool isIndeterminate)
    {
        if (ProgressOverlay == null) return;

        ProgressBar.Value = Math.Clamp(value, 0, 100);
        ProgressBar.IsIndeterminate = isIndeterminate;
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
