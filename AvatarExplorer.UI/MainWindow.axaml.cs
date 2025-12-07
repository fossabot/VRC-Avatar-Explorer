using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Utils;

namespace AvatarExplorer.UI;

public partial class MainWindow : Window
{
    private readonly AvatarExplorerApp _avatarExplorer = new();

    private Dictionary<ActionKey, Func<string, Task>>? _contextMenuHandlers;

    public MainWindow()
    {
        /* プロジェクトTODO
        TODO: 言語変更、並び替えを実装する
        TODO: 戻ったときに、どこを表示するのかはっきりする
        TODO: 右クリックメニューの処理を作る
        TODO: UIのタグを使った翻訳機能を追加する
        TODO: 実装やタグは新しくUIを作って上げることで実装する。右クリックメニューでは扱わない（チェックとかでメモリリークする可能性があるため）
        TODO: ページ機能を追加する（Dictionaryで現在のパスを元に保存してもいいかも）
        TODO: 下のボタンの処理を実装する
        */

        InitializeComponent();
        InitializeAvatarExplorer();
        InitializeContextMenuHandlers();
        InitializeNoItemsLabel();

        RenderLeftPanel();
        RenderRightPanel();
    }

    #region Initializing
    private void InitializeAvatarExplorer()
    {
        try
        {
            _avatarExplorer.LoadItemDatabase(true);
            _avatarExplorer.LoadCommonAvatarDatabase(true);
            Localizer.Instance.LoadFromFile("locales/ja-JP.json");
        }
        catch
        {
            // Ignored
        }
    }
    private void InitializeNoItemsLabel()
    {
        if (RightPanelParent == null) return;

        RightPanelParent.Children.Clear();

        var image = new Image
        {
            Source = IconUtils.GetIcon(SystemIcon.NothingIcon),
            Width = 150,
            Height = 150,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        var text = new TextBlock
        {
            Text = Localizer.Instance.GetDisplayName(LocalizationKey.Error.Nothing),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 25
        };

        RightPanelParent.Children.Add(image);
        RightPanelParent.Children.Add(text);
    }
    private void InitializeContextMenuHandlers()
    {
        _contextMenuHandlers = new()
        {
            { ActionKey.OpenItemFolder, OpenItemFolder },
            { ActionKey.CopyBoothLink, CopyBoothLink },
            { ActionKey.OpenBoothLink, OpenBoothLink },
            { ActionKey.ShowOtherItemsByAuthor, ShowOtherItemsByAuthor },
            { ActionKey.ChangeThumbnail, ChangeThumbnail },
            { ActionKey.EditItem, EditItem },
            { ActionKey.AddItemMemo, AddMemo},
            { ActionKey.AddItemFolder, AddItemFolder },
            { ActionKey.EditImplementedAvatar, EditImplementedAvatar },
            { ActionKey.EditItemTag, EditItemTag }
        };
    }
    #endregion

    #region Left Panel
    private void RenderLeftPanel()
    {
        if (LeftPanel == null) return;

        LeftPanel.Children.Clear();

        List<ItemCountInfo> items = new();

        ItemTagState customType = ItemTagState.Unknown;
        switch (LeftFilter.SelectedIndex)
        {
            case 0:
                {
                    items.AddRange(_avatarExplorer.GetAvatars());
                    customType = ItemTagState.RootAvatar;
                    break;
                }
            case 1:
                {
                    items.AddRange(_avatarExplorer.GetAuthors());
                    customType = ItemTagState.RootAuthor;
                    break;
                }
            case 2:
                {
                    items.AddRange(_avatarExplorer.GetCategories());
                    customType = ItemTagState.RootCategory;
                    break;
                }
        }

        foreach (ItemCountInfo itemCountInfo in items.Take(30))
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(itemCountInfo.Item), ContextMenuItem_Click);
            UIUtils.AddItemButton(LeftPanel, new UISelectableItem(itemCountInfo).SetState(customType), itemContextMenu, LeftPanelButton_Clicked);
        }
    }

    private void LeftPanelButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ItemTagInfo itemTagInfo)
        {
            _avatarExplorer.SelectClear();
            _avatarExplorer.Select(itemTagInfo.State, itemTagInfo.Value);

            RenderRightPanel();
            LoadCurrentPath();
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

        var items = _avatarExplorer.GetItemsForCurrentState();

        if (items.Count == 0) ShowNoItemsLabel();
        else HideNoItemsLabel();

        foreach (ItemCountInfo itemCountInfo in items.Take(30))
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(itemCountInfo.Item), ContextMenuItem_Click);
            UIUtils.AddItemButton(RightPanel, new UISelectableItem(itemCountInfo), itemContextMenu, RightPanelButton_Clicked);
        }
    }
    private async void RightPanelButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ItemTagInfo itemTagInfo)
        {
            if (itemTagInfo.State == ItemTagState.ItemFileCategoryOpen)
            {
                string itemPath = itemTagInfo.Value;

                bool isUnitypackage = itemPath.ToLower().EndsWith(".unitypackage");
                if (isUnitypackage)
                {
                    await OpenUnitypackageInternalAsync(itemPath);
                }
                else
                {
                    await AvaloniaLauncherUtils.OpenFile(this, itemPath);
                }
            }
            else
            {
                _avatarExplorer.Select(itemTagInfo.State, itemTagInfo.Value);
                RenderRightPanel();
            }

            LoadCurrentPath();
        }
    }

    private async Task OpenUnitypackageInternalAsync(string itemPath)
    {
        var selectedItem = _avatarExplorer.GetSelectedItem();
        if (selectedItem == null)
        {
            await AvaloniaLauncherUtils.OpenFile(this, itemPath);
            return;
        }

        var progress = new Progress<(string, int, string)>(tuple =>
        {
            if (tuple.Item2 == 100)
            {
                HideProgress();

                // Unitypackage展開後は自動で引数3にUnitypackageのパスが来る
                // 空白の場合はないということ
                if (!string.IsNullOrEmpty(tuple.Item3))
                {
                    _ = AvaloniaLauncherUtils.OpenFile(this, tuple.Item3);
                }
            }
            else
            {
                ShowProgress(Localizer.Instance.GetDisplayName(tuple.Item1, [tuple.Item2.ToString()]));
                UpdateProgress(tuple.Item2);
            }
        });

        AvatarExplorerApp.ModifyUnityPackageFilePath(itemPath, Localizer.Instance.GetDisplayName(selectedItem.Type.GetLocalizationKey() ?? ""), progress: progress);
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
        if (string.IsNullOrEmpty(SearchTextBox.Text))
        {
            RenderRightPanel();
            return;
        }

        RightPanel.Children.Clear();
        SearchFilter searchFilter = SearchUtils.BuildFilter(SearchTextBox.Text);
        IReadOnlyList<Item> items = _avatarExplorer.SearchItems(searchFilter);

        PathBox.Text = searchFilter.ToPathString();

        if (items.Count == 0) ShowNoItemsLabel();
        else HideNoItemsLabel();

        foreach (Item item in items.Take(30))
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(item), ContextMenuItem_Click);
            UIUtils.AddItemButton(RightPanel, new UISelectableItem(item, 0).SetState(ItemTagState.SearchItem), itemContextMenu, RightPanelButton_Clicked);
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

    private void LoadCurrentPath()
    {
        if (PathBox == null) return;

        IEnumerable<SelectionNode> currentSelectionNodes = _avatarExplorer.GetCurrentPath();
        if (!currentSelectionNodes.Any())
        {
            PathBox.Text = Localizer.Instance.GetDisplayName(LocalizationKey.Path.Default);
            return;
        }

        List<SelectionNode> selectionNodes = new();
        foreach (var node in currentSelectionNodes)
        {
            if (node.State == ItemTagState.SearchItem) selectionNodes.Clear();
            selectionNodes.Add(node);
        }

        PathBox.Text = string.Join(
            " > ",
            selectionNodes
                .Select(i =>
                {
                    ItemTagState key = i.State;
                    string value = i.Key;

                    if (i.State == ItemTagState.SearchItem || i.State == ItemTagState.RootAvatar || i.State == ItemTagState.RootSelectedItem)
                    {
                        Item? item = _avatarExplorer.GetAllItems().FirstOrDefault(item => item.ItemPath == i.Key);
                        if (item != null) value = item.Title; // アイテムはパスからタイトルに変換する
                    }

                    if (i.State == ItemTagState.RootCategory || i.State == ItemTagState.RootSelectedCategory || i.State == ItemTagState.ItemFileCategory)
                    {
                        // カテゴリはValue自体を翻訳する
                        // カテゴリ: Search.Category.Textureのような感じで入っているため
                        value = Localizer.Instance.GetDisplayName(value);
                    }

                     // 翻訳できない(Root以外)はここがnullになるため、valueがパスになる。ある場合はPrefixが翻訳される。
                    string? localizationKey = key.GetLocalizationKey();

                    return localizationKey == null ? value : Localizer.Instance.GetDisplayName(localizationKey, [value]);
                })
        );
    }

    private void ShowNoItemsLabel()
    {
        if (RightPanelParent == null) return;

        RightPanelParent.IsVisible = true;
    }

    private void HideNoItemsLabel()
    {
        if (RightPanelParent == null) return;

        RightPanelParent.IsVisible = false;
    }

    #region UI Event Handler
    private void Undo_Click(object? sender, RoutedEventArgs e)
    {
        _avatarExplorer.SelectUndo();
        RenderRightPanel();
        LoadCurrentPath();
    }

    private async void ContextMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is ContextMenuAction contextMenuAction)
        {
            await ExecuteContextMenuItemCommand(contextMenuAction);
        }
    }
    #endregion

    #region ContextMenu
    private async Task ExecuteContextMenuItemCommand(ContextMenuAction contextMenuAction)
    {
        if (contextMenuAction.ActionLayer == ActionLayer.UI)
        {
            if (_contextMenuHandlers != null && _contextMenuHandlers.TryGetValue(contextMenuAction.ActionKey, out var handler))
                await handler(contextMenuAction.Tag);
        }
        else if (contextMenuAction.ActionLayer == ActionLayer.Core)
        {
            await _avatarExplorer.ExecuteContextMenuItemCommand(contextMenuAction);
        }
    }
    private Item? GetItemByPath(string itemPath)
    {
        var item = _avatarExplorer.GetItemByPath(itemPath);
        if (item == null) ShowDialog("エラー", "アイテムが見つかりませんでした");

        return item;
    }
    private async Task OpenItemFolder(string itemPath)
    {
        var item = GetItemByPath(itemPath);
        if (item == null) return;

        await AvaloniaLauncherUtils.OpenFolder(this, ItemUtils.GetItemPath(item.ItemPath));
    }
    private async Task CopyBoothLink(string itemPath)
    {
        var item = GetItemByPath(itemPath);
        if (item == null) return;

        var boothLink = item.GetBoothLink();

        try
        {
            await ClipboardUtils.SetTextToClipboard(boothLink);
            ShowDialog("成功", "クリップボードにリンクをコピーしました。");
        }
        catch
        {
            ShowDialog("エラー", "クリップボードにリンクをコピー出来ませんでした。");
        }
    }
    private async Task OpenBoothLink(string itemPath)
    {
        var item = GetItemByPath(itemPath);
        if (item == null) return;

        await AvaloniaLauncherUtils.OpenLink(this, item.GetBoothLink());
    }
    private Task ShowOtherItemsByAuthor(string itemPath)
    {
        var item = GetItemByPath(itemPath);
        if (item == null) return Task.CompletedTask;

        if (SearchTextBox != null) SearchTextBox.Text = string.Format("Author=\"{0}\"", item.Author);
        UpdateRightPanel();

        return Task.CompletedTask;
    }
    private Task ChangeThumbnail(string itemPath)
    {
        ShowDialog(Localizer.Instance.GetDisplayName(LocalizationKey.Error.Default), Localizer.Instance.GetDisplayName(LocalizationKey.Error.NotImplemented));
        return Task.CompletedTask;
    }
    private Task EditItem(string itemPath)
    {
        ShowDialog(Localizer.Instance.GetDisplayName(LocalizationKey.Error.Default), Localizer.Instance.GetDisplayName(LocalizationKey.Error.NotImplemented));
        return Task.CompletedTask;
    }
    private Task AddMemo(string itemPath)
    {
        ShowDialog(Localizer.Instance.GetDisplayName(LocalizationKey.Error.Default), Localizer.Instance.GetDisplayName(LocalizationKey.Error.NotImplemented));
        return Task.CompletedTask;
    }
    private Task AddItemFolder(string itemPath)
    {
        ShowDialog(Localizer.Instance.GetDisplayName(LocalizationKey.Error.Default), Localizer.Instance.GetDisplayName(LocalizationKey.Error.NotImplemented));
        return Task.CompletedTask;
    }
    private Task EditImplementedAvatar(string itemPath)
    {
        ShowDialog(Localizer.Instance.GetDisplayName(LocalizationKey.Error.Default), Localizer.Instance.GetDisplayName(LocalizationKey.Error.NotImplemented));
        return Task.CompletedTask;
    }
    private Task EditItemTag(string itemPath)
    {
        ShowDialog(Localizer.Instance.GetDisplayName(LocalizationKey.Error.Default), Localizer.Instance.GetDisplayName(LocalizationKey.Error.NotImplemented));
        return Task.CompletedTask;
    }
    #endregion
}
