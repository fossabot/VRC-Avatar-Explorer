using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Extensions;
using AvatarExplorer.UI.Factories;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow : Window
{
    internal readonly AvatarExplorerApp _avatarExplorerApp = new();

    internal readonly Dictionary<ItemTagState, int> _main_currentPageStates = new()
    {
        { ItemTagState.SearchItem, 0 },
        { ItemTagState.RootAvatar, 0 },
        { ItemTagState.RootAuthor, 0 },
        { ItemTagState.RootCategory, 0 },
        { ItemTagState.RootSelectedCategory, 0 },
        { ItemTagState.RootSelectedItem, 0 },
        { ItemTagState.ItemFileCategoryOpen, 0 }
    };
    internal readonly Dictionary<ItemTagState, Vector> _main_currentScrollValues = new()
    {
        { ItemTagState.SearchItem, new() },
        { ItemTagState.RootAvatar, new() },
        { ItemTagState.RootAuthor, new() },
        { ItemTagState.RootCategory, new() },
        { ItemTagState.RootSelectedCategory, new() },
        { ItemTagState.RootSelectedItem, new() },
        { ItemTagState.ItemFileCategory, new() },
        { ItemTagState.ItemFileCategoryOpen, new() }
    };

    private string _main_lastSearchTextCache = string.Empty; // 最後に実行された検索のキャッシュ
    private string _main_searchTextCache = string.Empty;
    internal bool _main_isLastWindowSearch = false;

    private ItemTagState _main_lastRightPanelItemTagState = ItemTagState.None;

    internal readonly UserPreferences _userPreferences = new();
    internal int ItemsPerPage => _userPreferences.ItemsPerPage;

    private bool IsPageSupported(ItemTagState itemTagState)
        => _main_currentPageStates.ContainsKey(itemTagState);
    private int GetPage(ItemTagState itemTagState)
        => IsPageSupported(itemTagState) ? _main_currentPageStates[itemTagState] : -1;

    internal RuntimeSettings RuntimeSettings => _avatarExplorerApp.GetRuntimeSettings();

    public MainWindow()
    {
        /* プロジェクトTODO
        TODO: 言語変更を実装する (UIが完成したらやる)
        TODO: UIのタグを使った翻訳機能を追加する
        TODO: アイテムのカテゴリを変更したときにフォルダを移行できるようにしたい
        TODO: アップデータを作る
        TODO: オーバーレイの背景の色をBindingかなにかで指定してあげる。ライトモードでも使えるように。
        TODO: デフォルトの保存先を空にし、起動時にフォルダがなければ選択してあげる。
        */

        InitializeCurrentPath();
        InitializeComponent();
        InitializeLanguageBox();
        InitializeAvatarExplorer();
        InitializeContextMenuHandlers();
        InitializeNoItemsLabel();
        InitializeUserPreferences();
        InitializePipeServer();

        // Scheme Check (Only Windows)
        if (ProcessUtils.IsWindows()) _ = CheckScheme();

        Main_RenderLeftPanel();
        Main_RenderRightPanel();
    }

    public void SetArgs(string[]? args)
    {
        if (args?.Length > 0)
        {
            if (string.IsNullOrEmpty(args[0])) return;

            LaunchInfo launchInfo = LaunchInfoService.GetLaunchInfo(args[0]);
            if (launchInfo.AssetDirs.Length != 0 && !string.IsNullOrEmpty(launchInfo.AssetId)) AddItemOverlay_ShowAdd(launchInfo);
        }
    }

    #region Left Panel
    private void Main_RenderLeftPanel()
    {
        if (Main_LeftPanel == null) return;
        Main_LeftPanel.Children.Clear();

        List<ItemCountInfo> items = new();

        ItemTagState customState = ItemTagState.None;
        switch (Main_LeftFilter.SelectedIndex)
        {
            case 0:
                {
                    items.AddRange(_avatarExplorerApp.GetAvatars());
                    customState = ItemTagState.RootAvatar;
                    break;
                }
            case 1:
                {
                    items.AddRange(_avatarExplorerApp.GetAuthors());
                    customState = ItemTagState.RootAuthor;
                    break;
                }
            case 2:
                {
                    items.AddRange(_avatarExplorerApp.GetCategories());
                    customState = ItemTagState.RootCategory;
                    break;
                }
        }

        // スクロール位置をDictionaryから復元してあげる
        Main_RestoreScrollViewerOffset(Main_LeftPanelScrollViewer, customState);

        int currentPage = GetPage(customState); // -1が返された場合は対応していないStateのため、全てのアイテムを表示してあげる

        foreach (ItemCountInfo itemCountInfo in currentPage != -1 ? items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage) : items)
        {
            ContextMenu itemContextMenu = ContextMenuFactory.GetContextMenu(ContextMenuCreator.Create(itemCountInfo.Item), ItemButton_ContextMenuItem_Click);
            ItemButtonFactory.AddItemButton(Main_LeftPanel, new UISelectableItem(itemCountInfo).SetState(customState), RuntimeSettings.RemoveBrackets, itemContextMenu, LeftPanel_ItemButton_Click);
        }

        if (currentPage != -1 && items.Count != 0) ItemButtonFactory.AddPageButton(Main_LeftPanel, customState, currentPage, ItemsPerPage, items.Count, LeftPanel_ItemButton_Click);
    }
    private void LeftPanel_ItemButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        
        if (button.Tag is ItemTagInfo itemTagInfo)
        {
            _avatarExplorerApp.SelectClear();
            _avatarExplorerApp.Select(itemTagInfo.State, itemTagInfo.Value);
            Main_CheckPageStates();
            Main_ResetAllScrollViewerOffset(); // 左のパネルのボタンは全てRootのため、スクロール状況を全てリセットしてしまう

            Main_RenderRightPanel();
        }

        if (button.Tag is PageButtonInfo pageButtonInfo)
        {
            _main_currentPageStates[pageButtonInfo.ItemTagState] = pageButtonInfo.NextPageValue;
            Main_ResetScrollViewerOffset(pageButtonInfo.ItemTagState); // 今のStateのページをリセットしてあげる
            Main_RenderLeftPanel();
        }
    }
    #endregion

    #region Right Panel
    private void Main_RenderRightPanel()
    {
        if (Main_RightPanel == null) return;
        Main_RightPanel.Children.Clear();

        IReadOnlyList<ItemCountInfo> items = _avatarExplorerApp.GetItemsForCurrentState();

        if (items.Count == 0) Main_ShowNoItemsLabel();
        else Main_HideNoItemsLabel();

        ItemTagState itemTagState = ItemTagState.None;
        if (items.Count > 0) itemTagState = new UISelectableItem(items[0]).Tag.State;
        _main_lastRightPanelItemTagState = itemTagState;
        
        // スクロール位置をDictionaryから復元してあげる
        Main_RestoreScrollViewerOffset(Main_RightPanelScrollViewer, itemTagState);

        int currentPage = GetPage(itemTagState); // -1が返された場合は対応していないStateのため、全てのアイテムを表示してあげる

        foreach (ItemCountInfo itemCountInfo in currentPage != -1 ? items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage) : items)
        {
            ContextMenu itemContextMenu = ContextMenuFactory.GetContextMenu(ContextMenuCreator.Create(itemCountInfo.Item), ItemButton_ContextMenuItem_Click);
            ItemButtonFactory.AddItemButton(Main_RightPanel, new UISelectableItem(itemCountInfo), RuntimeSettings.RemoveBrackets, itemContextMenu, RightPanel_ItemButton_Click);
        }

        if (currentPage != -1 && items.Count != 0) ItemButtonFactory.AddPageButton(Main_RightPanel, itemTagState, currentPage, ItemsPerPage, items.Count, RightPanel_ItemButton_Click);
        _main_isLastWindowSearch = false;
        Main_LoadCurrentPath();
    }
    private async void RightPanel_ItemButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;

        if (button.Tag is ItemTagInfo itemTagInfo)
        {
            if (itemTagInfo.State == ItemTagState.ItemFileCategoryOpen) // ファイルを押されると、アイテムを開く処理に移行する
            {
                string itemPath = itemTagInfo.Value; // ItemFileCategoryOpenのValueはファイルのパスになっている
                await Main_OpenFileInternalAsync(itemPath);
            }
            else
            {
                _avatarExplorerApp.Select(itemTagInfo.State, itemTagInfo.Value);
                Main_CheckPageStates();
                Main_SaveScrollViewerOffset(Main_RightPanelScrollViewer, itemTagInfo.State); // 次の画面に行くため、今のStateのスクロール位置を保存する

                Main_RenderRightPanel();
            }
        }

        if (button.Tag is PageButtonInfo pageButtonInfo)
        {
            _main_currentPageStates[pageButtonInfo.ItemTagState] = pageButtonInfo.NextPageValue;
            Main_ResetScrollViewerOffset(pageButtonInfo.ItemTagState); // ページは今のStateをリセットしてあげる

            if (pageButtonInfo.ItemTagState == ItemTagState.SearchItem) Main_ExecuteSearchItems();
            else Main_RenderRightPanel();
        }
    }
    #endregion

    #region Search Box
    private readonly DispatcherTimer _searchTimer = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private void Main_SearchValue_Changed(object? sender, RoutedEventArgs e)
    {
        _searchTimer.Stop();
        _searchTimer.Tick -= Main_OnSearchTimerTick;
        _searchTimer.Tick += Main_OnSearchTimerTick;
        _searchTimer.Start();
    }
    private void Main_OnSearchTimerTick(object? sender, EventArgs e)
    {
        _searchTimer.Stop();
        _main_searchTextCache = Main_SearchTextBox.Text ?? "";
        Main_ExecuteSearchItems();
    }
    private void Main_ExecuteSearchItems(string searchText = "")
    {
        if (!string.IsNullOrEmpty(searchText)) _main_searchTextCache = searchText;

        SearchFilter searchFilter = SearchFilterBuilder.Build(_main_searchTextCache);
        if (AdvancedSearchPanel.IsVisible) AdvancedSearchPanel_ApplyValues(searchFilter);

        if (string.IsNullOrEmpty(_main_searchTextCache) && searchFilter.IsEmpty)
        {
            Main_RenderRightPanel();
            return;
        }

        // 検索画面に切り替わる時に、前の画面のスクロール位置を保存してあげる
        if (!_main_isLastWindowSearch) Main_SaveScrollViewerOffset(Main_RightPanelScrollViewer, _main_lastRightPanelItemTagState);

        Main_RightPanel.Children.Clear();

        IReadOnlyList<Item> items = _avatarExplorerApp.SearchItems(searchFilter);
        
        // 検索文字列が前回と違う場合はページ、スクロール位置をリセットする
        if (_main_searchTextCache != _main_lastSearchTextCache)
        {
            _main_currentPageStates[ItemTagState.SearchItem] = 0;
            _main_currentScrollValues[ItemTagState.SearchItem] = new();
        }
        _main_lastSearchTextCache = _main_searchTextCache;

        if (items.Count == 0) Main_ShowNoItemsLabel();
        else Main_HideNoItemsLabel();

        // スクロール位置をDictionaryから復元してあげる
        Main_RestoreScrollViewerOffset(Main_RightPanelScrollViewer, ItemTagState.SearchItem);

        int currentPage = GetPage(ItemTagState.SearchItem); // SearchItemは必ずページが存在しているため

        foreach (Item item in items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage))
        {
            ContextMenu itemContextMenu = ContextMenuFactory.GetContextMenu(ContextMenuCreator.Create(item), ItemButton_ContextMenuItem_Click);
            ItemButtonFactory.AddItemButton(Main_RightPanel, new UISelectableItem(item, 0).SetState(ItemTagState.SearchItem), RuntimeSettings.RemoveBrackets, itemContextMenu, RightPanel_ItemButton_Click);
        }

        if (items.Count != 0) ItemButtonFactory.AddPageButton(Main_RightPanel, ItemTagState.SearchItem, currentPage, ItemsPerPage, items.Count, RightPanel_ItemButton_Click);
        _main_isLastWindowSearch = true;
        
        Main_PathTextBox.Text = searchFilter.ToPathString();
    }
    #endregion

    #region Path Processing
    private void Main_LoadCurrentPath()
    {
        if (Main_PathTextBox == null) return;

        IEnumerable<SelectionNode> currentSelectionNodes = _avatarExplorerApp.GetCurrentPaths();
        if (!currentSelectionNodes.Any())
        {
            Main_PathTextBox.Text = Localizer.Instance[LocalizationKey.Path.Default];
            return;
        }

        List<SelectionNode> selectionNodes = new();
        foreach (SelectionNode node in currentSelectionNodes)
        {
            if (node.State == ItemTagState.SearchItem) selectionNodes.Clear();
            selectionNodes.Add(node);
        }

        IReadOnlyList<Item> items = _avatarExplorerApp.GetAllItems();
        Main_PathTextBox.Text = string.Join(" > ", selectionNodes.Select(i => PathService.BuildPath(items, i)));
    }
    #endregion
    
    #region Main Methods
    private void Main_ReloadCurrentWindow()
    {
        Main_RenderLeftPanel();

        // 最後に表示されていた画面が検索画面だったら、キャッシュを元にもう一度検索してあげる
        if (_main_isLastWindowSearch) Main_ExecuteSearchItems();
        else Main_RenderRightPanel();
    }

    private void Main_CheckPageStates()
    {
        List<ItemTagState> selectedItemTagStates = new();

        foreach (SelectionNode selectionNode in _avatarExplorerApp.GetCurrentPaths().Where(i => !selectedItemTagStates.Contains(i.State)))
        {
            selectedItemTagStates.Add(selectionNode.State);
        }

        foreach (var pageInfo in _main_currentPageStates.Where(i => !selectedItemTagStates.Contains(i.Key)))
        {
            _main_currentPageStates[pageInfo.Key] = 0;
        }
    }
    
    private void Main_RestoreScrollViewerOffset(ScrollViewer scrollViewer, ItemTagState itemTagState)
    {
        if (_main_currentScrollValues.TryGetValue(itemTagState, out Vector scrollValue))
            scrollViewer.Offset = scrollValue;
    }
    private void Main_SaveScrollViewerOffset(ScrollViewer scrollViewer, ItemTagState itemTagState)
    {
        if (!_main_currentScrollValues.ContainsKey(itemTagState)) return;
        _main_currentScrollValues[itemTagState] = scrollViewer.Offset;
    }
    private void Main_ResetScrollViewerOffset(ItemTagState itemTagState)
    {
        if (!_main_currentScrollValues.ContainsKey(itemTagState)) return;
        _main_currentScrollValues[itemTagState] = new();
    }
    private void Main_ResetAllScrollViewerOffset()
    {
        foreach (ItemTagState key in _main_currentScrollValues.Keys)
            _main_currentScrollValues[key] = new();
    }

    private void Main_ShowNoItemsLabel()
    {
        if (Main_RightPanelParent == null) return;
        Main_RightPanelParent.IsVisible = true;
    }
    private void Main_HideNoItemsLabel()
    {
        if (Main_RightPanelParent == null) return;
        Main_RightPanelParent.IsVisible = false;
    }
    
    private async Task Main_OpenFileInternalAsync(string filePath)
    {
        bool isUnitypackage = filePath.ToLower().EndsWith(".unitypackage");
        
        if (isUnitypackage) await Main_OpenUnitypackageInternalAsync(filePath); // Unitypackageだと自動展開処理に移る
        else await LauncherService.OpenFile(this, filePath);
    }
    private async Task Main_OpenUnitypackageInternalAsync(string itemPath)
    {
        Item? selectedItem = _avatarExplorerApp.GetSelectedItem();

        await UnitypackageService.Open(this, itemPath, selectedItem,
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
    #endregion
}
