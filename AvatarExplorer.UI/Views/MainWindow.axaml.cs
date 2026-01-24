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
using AvatarExplorer.UI.Extensions;
using AvatarExplorer.UI.Factories;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;

namespace AvatarExplorer.UI;

public partial class MainWindow : Window
{
    private readonly AvatarExplorerApp _avatarExplorerApp = new();
    private readonly PageManager _main_pageManager = new();
    private readonly ScrollManager _main_scrollManager = new();

    private string _main_lastSearchTextCache = string.Empty; // 最後に実行された検索のキャッシュ
    private string _main_searchTextCache = string.Empty;
    private bool _main_isLastWindowSearch = false;

    private ItemTagState _main_lastRightPanelItemTagState = ItemTagState.None;

    private readonly UserPreferences _userPreferences = new();
    private int ItemsPerPage => _userPreferences.ItemsPerPage;

    private RuntimeSettings RuntimeSettings => _avatarExplorerApp.GetRuntimeSettings();

    public MainWindow()
    {
        /* プロジェクトTODO
        TODO: UIのタグを使った翻訳機能を追加する
        TODO: 共通素体グループ作成時に、元のアバターを置き換えるかどうかをダイアログで決める
        TODO: 共通素体グループ削除時に、共通素体の中のアバターに置き換えるかどうかをダイアログで決める
        TODO: インポート時の重複ファイルの処理方法を変更できるようにする
        TODO: 設定画面の処理をリファクタリングする
        TODO: Reload関連の関数などを、イベント経由で行うようにしたい。OnDatabaseUpdateみたいな
        */

        InitializeComponent();
        InitializeContextMenuHandlers();
        
        InitializeTitle();
        InitializeLanguageBox();
        InitializeAvatarExplorer();
        InitializeNoItemsLabel();
        InitializeUserPreferences();

        InitializePipeServer();

        // 設定画面の設定
        SettingsOverlay_SetUiValueFromCurrentSettings();
        SettingsOverlay_ApplySettingsValues();
    }

    private async void Main_Loaded(object? sender, RoutedEventArgs e)
    {
        Main_RenderLeftPanel();
        Main_RenderRightPanel();

        // Scheme Check (Only Windows)
        if (ProcessUtils.IsWindows()) await CheckScheme();
        
        await UpdateDialogOverlay_Check();

        CheckFirstLaunching();
    }

    public async Task SetApplicationArgs(string[]? args)
    {
        if (args == null || args.Length == 0 || string.IsNullOrEmpty(args[0])) return;

        LaunchInfo launchInfo = LaunchInfoService.GetLaunchInfo(args[0]);
        if (launchInfo.AssetDirs.Length != 0 && !string.IsNullOrEmpty(launchInfo.AssetId)) await AddItemOverlay_ShowAdd(launchInfo);
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
        Main_LeftPanelScrollViewer.Offset = _main_scrollManager.GetScrollValue(customState);

        int currentPage = _main_pageManager.GetPage(customState); // -1が返された場合は対応していないStateのため、全てのアイテムを表示してあげる

        foreach (ItemCountInfo itemCountInfo in currentPage != -1 ? items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage) : items)
        {
            ContextMenu itemContextMenu = ContextMenuFactory.GetContextMenu(ContextMenuCreator.Create(itemCountInfo.Item), ItemButton_ContextMenuItem_Click);
            Button itemButton = ItemButtonFactory.AddItemButton(Main_LeftPanel, new UISelectableItem(itemCountInfo).SetState(customState), RuntimeSettings, _userPreferences, itemContextMenu, LeftPanel_ItemButton_Click);
        
            // アイテム(アバター)の場合はD&Dイベントを登録してあげる
            if (StateFlagUtils.IsDraggableState(customState)) itemButton.AddHandler(PointerPressedEvent, ItemButton_PointerPressed, RoutingStrategies.Tunnel);
        }

        if (currentPage != -1 && items.Count != 0) PageButtonFactory.AddPageButton(Main_LeftPanel, customState, currentPage, ItemsPerPage, items.Count, LeftPanel_ItemButton_Click);
    }
    private void LeftPanel_ItemButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        
        if (button.Tag is ItemTagInfo itemTagInfo)
        {
            _avatarExplorerApp.SelectClear();
            _avatarExplorerApp.Select(itemTagInfo.State, itemTagInfo.Value);
            Main_CheckPageStates();
            _main_scrollManager.ResetAllScrollValues(); // 左のパネルのボタンは全てRootのため、スクロール状況を全てリセットしてしまう

            Main_RenderRightPanel();
        }

        if (button.Tag is PageButtonInfo pageButtonInfo)
        {
            _main_pageManager.SetPage(pageButtonInfo.ItemTagState, pageButtonInfo.NextPageValue);
            _main_scrollManager.SetScroll(pageButtonInfo.ItemTagState, new()); // 今のStateのページをリセットしてあげる
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
        Main_RightPanelScrollViewer.Offset = _main_scrollManager.GetScrollValue(itemTagState);

        int currentPage = _main_pageManager.GetPage(itemTagState); // -1が返された場合は対応していないStateのため、全てのアイテムを表示してあげる

        foreach (ItemCountInfo itemCountInfo in currentPage != -1 ? items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage) : items)
        {
            ContextMenu itemContextMenu = ContextMenuFactory.GetContextMenu(ContextMenuCreator.Create(itemCountInfo.Item), ItemButton_ContextMenuItem_Click);
            Button itemButton = ItemButtonFactory.AddItemButton(Main_RightPanel, new UISelectableItem(itemCountInfo), RuntimeSettings, _userPreferences, itemContextMenu, RightPanel_ItemButton_Click);
            
            // アイテムの場合はD&Dイベントを登録してあげる
            if (StateFlagUtils.IsDraggableState(itemTagState)) itemButton.AddHandler(PointerPressedEvent, ItemButton_PointerPressed, RoutingStrategies.Tunnel);
        }

        if (currentPage != -1 && items.Count != 0) PageButtonFactory.AddPageButton(Main_RightPanel, itemTagState, currentPage, ItemsPerPage, items.Count, RightPanel_ItemButton_Click);
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
                _main_scrollManager.SetScroll(itemTagInfo.State, Main_RightPanelScrollViewer.Offset); // 次の画面に行くため、今のStateのスクロール位置を保存する

                Main_RenderRightPanel();
            }
        }

        if (button.Tag is PageButtonInfo pageButtonInfo)
        {
            _main_pageManager.SetPage(pageButtonInfo.ItemTagState, pageButtonInfo.NextPageValue);
            _main_scrollManager.SetScroll(pageButtonInfo.ItemTagState, new()); // ページは今のStateをリセットしてあげる

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
        _main_searchTextCache = Main_SearchTextBox.Text ?? string.Empty;
        Main_ExecuteSearchItems();
    }
    private void Main_ExecuteSearchItems(string searchText = "")
    {
        if (!string.IsNullOrEmpty(searchText)) _main_searchTextCache = searchText;

        SearchFilter searchFilter = SearchFilterBuilder.Build(_main_searchTextCache);
        if (AdvancedSearchPanel_Enable.IsChecked ?? false) AdvancedSearchPanel_ApplyValues(searchFilter);

        if (string.IsNullOrEmpty(_main_searchTextCache) && searchFilter.IsEmpty)
        {
            Main_RenderRightPanel();
            return;
        }

        // 検索画面に切り替わる時に、前の画面のスクロール位置を保存してあげる
        if (!_main_isLastWindowSearch) _main_scrollManager.SetScroll( _main_lastRightPanelItemTagState, Main_RightPanelScrollViewer.Offset);
        
        // 検索文字列が前回と違う場合はページ、スクロール位置をリセットする
        if (_main_searchTextCache != _main_lastSearchTextCache)
        {
            _main_pageManager.SetPage(ItemTagState.SearchItem, 0);
            _main_scrollManager.SetScroll(ItemTagState.SearchItem, new());
        }
        _main_lastSearchTextCache = _main_searchTextCache;

        
        Main_RightPanel.Children.Clear();

        IReadOnlyList<Item> items = _avatarExplorerApp.SearchItems(searchFilter);

        if (items.Count == 0) Main_ShowNoItemsLabel();
        else Main_HideNoItemsLabel();

        // スクロール位置をDictionaryから復元してあげる
        Main_RightPanelScrollViewer.Offset = _main_scrollManager.GetScrollValue(ItemTagState.SearchItem);

        int currentPage = _main_pageManager.GetPage(ItemTagState.SearchItem); // SearchItemは必ずページが存在しているため

        foreach (Item item in items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage))
        {
            ContextMenu itemContextMenu = ContextMenuFactory.GetContextMenu(ContextMenuCreator.Create(item), ItemButton_ContextMenuItem_Click);
            Button itemButton = ItemButtonFactory.AddItemButton(Main_RightPanel, new UISelectableItem(item, 0).SetState(ItemTagState.SearchItem), RuntimeSettings, _userPreferences, itemContextMenu, RightPanel_ItemButton_Click);

            // D&Dイベントを登録してあげる
            itemButton.AddHandler(PointerPressedEvent, ItemButton_PointerPressed, RoutingStrategies.Tunnel);
        }

        if (items.Count != 0) PageButtonFactory.AddPageButton(Main_RightPanel, ItemTagState.SearchItem, currentPage, ItemsPerPage, items.Count, RightPanel_ItemButton_Click);
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
        Main_PathTextBox.Text = string.Join(" > ", selectionNodes.Select(i => PathService.BuildPath(items, i, RuntimeSettings.RemoveBrackets)));
    }
    #endregion
    
    #region Main Methods
    private void Main_ExecuteUndo()
    {
        // 選択されていたアイテムが検索結果時のものだったら、キャッシュを元にもう一度検索してあげる
        bool isCurrentSearchNode = _avatarExplorerApp.GetCurrentPathState()?.State == ItemTagState.SearchItem;
        
        Main_CheckPageStates(); // SelectUndoより前にやってあげないと、戻った先の画面のページ情報がリセットされる
        if (!_main_isLastWindowSearch) _avatarExplorerApp.SelectUndo(); // 最後の画面が検索画面だったら、検索だけやめて戻るようにする

        if (isCurrentSearchNode) Main_ExecuteSearchItems();
        else Main_RenderRightPanel();
    }
    private void Main_ReloadCurrentWindow()
    {
        Main_RenderLeftPanel();

        // 最後に表示されていた画面が検索画面だったら、キャッシュを元にもう一度検索してあげる
        if (_main_isLastWindowSearch) Main_ExecuteSearchItems();
        else
        {
            // 再読込する前に、前の画面のスクロール位置を保存してあげる
            _main_scrollManager.SetScroll(_main_lastRightPanelItemTagState, Main_RightPanelScrollViewer.Offset);
            Main_RenderRightPanel();
        }

        ReloadBulkImportItemButtons();
    }

    private void Main_CheckPageStates()
    {
        List<ItemTagState> selectedItemTagStates = new();

        foreach (SelectionNode selectionNode in _avatarExplorerApp.GetCurrentPaths().Where(i => !selectedItemTagStates.Contains(i.State)))
        {
            selectedItemTagStates.Add(selectionNode.State);
        }

        foreach (var pageInfo in _main_pageManager.GetKeys().Where(i => !selectedItemTagStates.Contains(i)))
        {
            _main_pageManager.ResetPageValue(pageInfo);
        }
    }

    private void Main_ShowNoItemsLabel()
    {
        Main_RightPanelParent.IsVisible = true;
    }
    private void Main_HideNoItemsLabel()
    {
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
        if (selectedItem == null) return;

        await UnitypackageService.Import(itemPath, selectedItem.Type == ItemType.Custom ? selectedItem.CustomCategory : Localizer.Instance[selectedItem.Type.GetLocalizationKey() ?? selectedItem.Type.ToString()],
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
