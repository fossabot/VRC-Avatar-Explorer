using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.Booth;
using AvatarExplorer.Core.Services;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Models.OverlayValues;
using AvatarExplorer.UI.Utils;

namespace AvatarExplorer.UI;

public partial class MainWindow : Window
{
    private readonly AvatarExplorerApp _avatarExplorer = new();

    private Dictionary<ActionKey, Func<string, Task>>? _contextMenuHandlers;
    private readonly Dictionary<ItemTagState, int> _currentPageStates = new()
    {
        { ItemTagState.SearchItem, 0 },
        { ItemTagState.RootAvatar, 0 },
        { ItemTagState.RootAuthor, 0 },
        { ItemTagState.RootCategory, 0 },
        { ItemTagState.RootSelectedCategory, 0 },
        { ItemTagState.RootSelectedItem, 0 },
        { ItemTagState.ItemFileCategoryOpen, 0 }
    };
    private readonly Dictionary<ItemTagState, Vector> _currentScrollValues = new()
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
    private string _lastSearchTextCache = string.Empty; // 最後に実行された検索のキャッシュ
    private string _searchTextCache = string.Empty;
    private bool _isLastWindowSearch = false;
    private ItemTagState _lastRightPanelItemTagState = ItemTagState.Unknown;
    private readonly UserUiPreferences _userUiPreferences = new();
    
    private int ItemsPerPage => _userUiPreferences.ItemsPerPage;
    private RuntimeSettings RuntimeSettings => _avatarExplorer.GetRuntimeSettings();

    private bool IsPageSupported(ItemTagState itemTagState)
        => _currentPageStates.ContainsKey(itemTagState);
    private int GetPage(ItemTagState itemTagState)
        => IsPageSupported(itemTagState) ? _currentPageStates[itemTagState] : -1;

    public MainWindow()
    {
        /* プロジェクトTODO
        TODO: 言語変更を実装する (UIが完成したらやる)
        TODO: 右クリックメニューの処理を作る
        TODO: UIのタグを使った翻訳機能を追加する
        TODO: 対応アバター、実装やタグは新しくUIを作って上げることで実装する。右クリックメニューでは扱わない（チェックとかでメモリリークする可能性があるため）
        TODO: 下のボタンの処理を実装する
        TODO: SCHEMEに対応する
        TODO: アイテムのカテゴリを変更したときにフォルダを移行できるように変更
        TODO: 詳細検索用の画面を追加する（右のアイテム画面の右側に縦長に別ウィンドウみたいな感じで表示するのはありかも？）
        */

        InitializeComponent();
        InitializeAvatarExplorer();
        InitializeContextMenuHandlers();
        InitializeNoItemsLabel();
        InitializeUserUiPreferences();

        Main_RenderLeftPanel();
        Main_RenderRightPanel();
    }

    #region Initializing
    private void InitializeAvatarExplorer()
    {
        try
        {
            _avatarExplorer.LoadItemDatabase(true);
            _avatarExplorer.LoadCommonAvatarDatabase(true);
            _avatarExplorer.LoadRuntimeSettings();
            ApplyRuntimeSettingsToUi(); // 並び替え順をセットするため
            Localizer.Instance.LoadFromFile("locales/ja-JP.json");
        }
        catch
        {
            // Ignored
        }
    }

    private void InitializeUserUiPreferences()
    {
        var userUiPreferences = SettingsUtils.LoadUserPreferences(SystemPath.UserPreferencesFilePath);
        _userUiPreferences.FromOther(userUiPreferences);

        ApplyPreferenceSettingsToUi();

        _userUiPreferences.Save();
    }
    private void InitializeNoItemsLabel()
    {
        if (Main_RightPanelParent == null) return;

        Main_RightPanelParent.Children.Clear();

        Main_RightPanelParent.Children.Add(new Image
        {
            Source = IconUtils.GetIcon(SystemIcon.NothingIcon),
            Width = 150,
            Height = 150,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        });

        Main_RightPanelParent.Children.Add(new TextBlock
        {
            Text = Localizer.Instance[LocalizationKey.Error.Nothing],
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 25
        });
    }
    private void InitializeContextMenuHandlers()
    {
        _contextMenuHandlers = new()
        {
            { ActionKey.OpenItemFolder, ItemButton_ContextMenu_OpenItemFolder },
            { ActionKey.CopyBoothLink, ItemButton_ContextMenu_CopyBoothLink },
            { ActionKey.OpenBoothLink, ItemButton_ContextMenu_OpenBoothLink },
            { ActionKey.ShowOtherItemsByAuthor, ItemButton_ContextMenu_ShowOtherItemsByAuthor },
            { ActionKey.ChangeThumbnail, ItemButton_ContextMenu_ChangeThumbnail },
            { ActionKey.EditItem, ItemButton_ContextMenu_EditItem },
            { ActionKey.AddItemMemo, ItemButton_ContextMenu_AddMemo},
            { ActionKey.AddItemFolder, ItemButton_ContextMenu_AddItemFolder },
            { ActionKey.EditImplementedAvatar, ItemButton_ContextMenu_EditImplementedAvatar },
            { ActionKey.EditItemTag, ItemButton_ContextMenu_EditItemTag }
        };
    }
    #endregion

    #region Left Panel
    private void LeftFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Main_RenderLeftPanel();
    }

    private void Main_RenderLeftPanel()
    {
        if (Main_LeftPanel == null) return;
        Main_LeftPanel.Children.Clear();

        List<ItemCountInfo> items = new();

        ItemTagState customState = ItemTagState.Unknown;
        switch (LeftFilter.SelectedIndex)
        {
            case 0:
                {
                    items.AddRange(_avatarExplorer.GetAvatars());
                    customState = ItemTagState.RootAvatar;
                    break;
                }
            case 1:
                {
                    items.AddRange(_avatarExplorer.GetAuthors());
                    customState = ItemTagState.RootAuthor;
                    break;
                }
            case 2:
                {
                    items.AddRange(_avatarExplorer.GetCategories());
                    customState = ItemTagState.RootCategory;
                    break;
                }
        }

        // スクロール位置をDictionaryから復元してあげる
        Main_RestoreScrollViewerOffset(Main_LeftPanelScrollViewer, customState);

        int currentPage = GetPage(customState); // -1が返された場合は対応していないStateのため、全てのアイテムを表示してあげる

        foreach (ItemCountInfo itemCountInfo in currentPage != -1 ? items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage) : items)
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(itemCountInfo.Item), ItemButton_ContextMenuItem_Click);
            UIUtils.AddItemButton(Main_LeftPanel, new UISelectableItem(itemCountInfo).SetState(customState), RuntimeSettings.RemoveBrackets, itemContextMenu, LeftPanel_ItemButton_Clicked);
        }

        if (currentPage != -1 && items.Count != 0) UIUtils.AddPageButton(Main_LeftPanel, customState, currentPage, ItemsPerPage, items.Count, LeftPanel_ItemButton_Clicked);
    }
    private void LeftPanel_ItemButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        
        if (button.Tag is ItemTagInfo itemTagInfo)
        {
            _avatarExplorer.SelectClear();
            _avatarExplorer.Select(itemTagInfo.State, itemTagInfo.Value);
            Main_CheckPageStates();
            Main_ResetAllScrollViewerOffset(); // 左のパネルのボタンは全てRootのため、スクロール状況を全てリセットしてしまう

            Main_RenderRightPanel();
        }

        if (button.Tag is PageButtonInfo pageButtonInfo)
        {
            _currentPageStates[pageButtonInfo.ItemTagState] = pageButtonInfo.NextPageValue;
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

        IReadOnlyList<ItemCountInfo> items = _avatarExplorer.GetItemsForCurrentState();

        if (items.Count == 0) Main_ShowNoItemsLabel();
        else Main_HideNoItemsLabel();

        ItemTagState itemTagState = ItemTagState.Unknown;
        if (items.Count > 0) itemTagState = new UISelectableItem(items[0]).Tag.State;
        _lastRightPanelItemTagState = itemTagState;
        
        // スクロール位置をDictionaryから復元してあげる
        Main_RestoreScrollViewerOffset(Main_RightPanelScrollViewer, itemTagState);

        int currentPage = GetPage(itemTagState); // -1が返された場合は対応していないStateのため、全てのアイテムを表示してあげる

        foreach (ItemCountInfo itemCountInfo in currentPage != -1 ? items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage) : items)
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(itemCountInfo.Item), ItemButton_ContextMenuItem_Click);
            UIUtils.AddItemButton(Main_RightPanel, new UISelectableItem(itemCountInfo), RuntimeSettings.RemoveBrackets, itemContextMenu, RightPanel_ItemButton_Clicked);
        }

        if (currentPage != -1 && items.Count != 0) UIUtils.AddPageButton(Main_RightPanel, itemTagState, currentPage, ItemsPerPage, items.Count, RightPanel_ItemButton_Clicked);
        _isLastWindowSearch = false;
        Main_LoadCurrentPath();
    }
    private async void RightPanel_ItemButton_Clicked(object? sender, RoutedEventArgs e)
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
                _avatarExplorer.Select(itemTagInfo.State, itemTagInfo.Value);
                Main_CheckPageStates();
                Main_SaveScrollViewerOffset(Main_RightPanelScrollViewer, itemTagInfo.State); // 次の画面に行くため、今のStateのスクロール位置を保存する

                Main_RenderRightPanel();
            }
        }

        if (button.Tag is PageButtonInfo pageButtonInfo)
        {
            _currentPageStates[pageButtonInfo.ItemTagState] = pageButtonInfo.NextPageValue;
            Main_ResetScrollViewerOffset(pageButtonInfo.ItemTagState); // ページは今のStateをリセットしてあげる

            if (pageButtonInfo.ItemTagState == ItemTagState.SearchItem) Main_ExecuteSearchItems();
            else Main_RenderRightPanel();
        }
    }
    private async Task Main_OpenFileInternalAsync(string filePath)
    {
        bool isUnitypackage = filePath.ToLower().EndsWith(".unitypackage");
        
        if (isUnitypackage) await Main_OpenUnitypackageInternalAsync(filePath); // Unitypackageだと自動展開処理に移る
        else await AvaloniaLauncherUtils.OpenFile(this, filePath);
    }
    private async Task Main_OpenUnitypackageInternalAsync(string itemPath)
    {
        Item? selectedItem = _avatarExplorer.GetSelectedItem();
        if (selectedItem == null)
        {
            await AvaloniaLauncherUtils.OpenFile(this, itemPath);
            return;
        }

        var progress = new Progress<(string, int, string)>(async tuple =>
        {
            if (tuple.Item2 == 100)
            {
                Main_HideProgress();

                // Unitypackage展開後は自動で引数3にUnitypackageのパスが来る
                // 空白の場合はないということだからスキップする
                if (!string.IsNullOrEmpty(tuple.Item3))
                {
                    await AvaloniaLauncherUtils.OpenFile(this, tuple.Item3);
                }
            }
            else
            {
                Main_ShowProgress(Localizer.Instance.GetDisplayName(tuple.Item1, tuple.Item2.ToString()));
                Main_UpdateProgress(tuple.Item2);
            }
        });

        await AvatarExplorerApp.ModifyUnityPackageFilePath(itemPath, Localizer.Instance[selectedItem.Type.GetLocalizationKey() ?? ""], progress: progress);
    }
    #endregion

    #region Search Box
    private readonly DispatcherTimer _searchTimer = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private void Main_SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchTimer.Stop();
        _searchTimer.Tick -= Main_OnSearchTimerTick;
        _searchTimer.Tick += Main_OnSearchTimerTick;
        _searchTimer.Start();
    }
    private void Main_OnSearchTimerTick(object? sender, EventArgs e)
    {
        _searchTimer.Stop();
        _searchTextCache = Main_SearchTextBox.Text ?? "";
        Main_ExecuteSearchItems();
    }
    private void Main_ExecuteSearchItems(string searchText = "")
    {
        if (!string.IsNullOrEmpty(searchText)) _searchTextCache = searchText;

        if (string.IsNullOrEmpty(_searchTextCache))
        {
            Main_RenderRightPanel();
            return;
        }

        // 検索画面に切り替わる時に、前の画面のスクロール位置を保存してあげる
        if (!_isLastWindowSearch) Main_SaveScrollViewerOffset(Main_RightPanelScrollViewer, _lastRightPanelItemTagState);

        Main_RightPanel.Children.Clear();

        SearchFilter searchFilter = SearchUtils.BuildFilter(_searchTextCache);
        IReadOnlyList<Item> items = _avatarExplorer.SearchItems(searchFilter);
        
        // 検索文字列が前回と違う場合はページ、スクロール位置をリセットする
        if (_searchTextCache != _lastSearchTextCache)
        {
            _currentPageStates[ItemTagState.SearchItem] = 0;
            _currentScrollValues[ItemTagState.SearchItem] = new();
        }
        _lastSearchTextCache = _searchTextCache;

        if (items.Count == 0) Main_ShowNoItemsLabel();
        else Main_HideNoItemsLabel();

        // スクロール位置をDictionaryから復元してあげる
        Main_RestoreScrollViewerOffset(Main_RightPanelScrollViewer, ItemTagState.SearchItem);

        int currentPage = GetPage(ItemTagState.SearchItem); // SearchItemは必ずページが存在しているため

        foreach (Item item in items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage))
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(item), ItemButton_ContextMenuItem_Click);
            UIUtils.AddItemButton(Main_RightPanel, new UISelectableItem(item, 0).SetState(ItemTagState.SearchItem), RuntimeSettings.RemoveBrackets, itemContextMenu, RightPanel_ItemButton_Clicked);
        }

        if (items.Count != 0) UIUtils.AddPageButton(Main_RightPanel, ItemTagState.SearchItem, currentPage, ItemsPerPage, items.Count, RightPanel_ItemButton_Clicked);
        _isLastWindowSearch = true;
        
        Main_PathTextBox.Text = searchFilter.ToPathString();
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
    private void Dialog_OKButton_Click(object? sender, RoutedEventArgs e)
    {
        if (DialogOverlay == null) return;

        DialogOverlay.IsVisible = false;
    }
    #endregion

    #region Progress Dialog
    private void Main_ShowProgress(string title)
    {
        if (ProgressBarTitle == null || ProgressOverlay == null) return;
        ProgressBarTitle.Text = title;
        ProgressOverlay.IsVisible = true;
    }
    private void Main_HideProgress()
    {
        if (ProgressOverlay == null) return;
        ProgressOverlay.IsVisible = false;
    }
    private void Main_UpdateProgress(int value)
    {
        if (ProgressOverlay == null) return;
        ProgressBar.Value = Math.Clamp(value, 0, 100);
        ProgressBar.IsIndeterminate = value == 0;
    }
    #endregion

    #region Path
    private void Main_LoadCurrentPath()
    {
        if (Main_PathTextBox == null) return;

        IEnumerable<SelectionNode> currentSelectionNodes = _avatarExplorer.GetCurrentPaths();
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

        Main_PathTextBox.Text = string.Join(" > ", selectionNodes.Select(Main_BuildPathTextInternal));
    }
    private string Main_BuildPathTextInternal(SelectionNode selectionNode)
    {
        ItemTagState state = selectionNode.State;
        string value = selectionNode.Key;

        if (StateFlagUtils.ItemsFlag.HasFlag(state))
        {
            Item? item = _avatarExplorer.GetAllItems().FirstOrDefault(item => item.ItemPath == value);
            if (item != null) value = item.Title; // アイテムはパスからタイトルに変換する
        }

        if (StateFlagUtils.CategoriesFlag.HasFlag(state))
        {
            // カテゴリはValue自体を翻訳する
            // カテゴリ: Search.Category.Textureのような感じで入っているため
            value = Localizer.Instance[value];
        }

        // 翻訳できないタグ(Root以外)はここがnullになるため、valueがパスになる。ある場合はPrefixが翻訳される。
        string? localizationKey = state.GetLocalizationKey();

        return localizationKey == null ? value : Localizer.Instance.GetDisplayName(localizationKey, value);
    }
    #endregion

    #region Main UI Event Handler
    private void Main_UndoButton_Click(object? sender, RoutedEventArgs e)
    {
        // 選択されていたアイテムが検索結果時のものだったら、キャッシュを元にもう一度検索してあげる
        bool isCurrentSearchNode = _avatarExplorer.GetCurrentPathState()?.State == ItemTagState.SearchItem;
        
        Main_CheckPageStates(); // SelectUndoより前にやってあげないと、戻った先の画面のページ情報がリセットされる
        if (!_isLastWindowSearch) _avatarExplorer.SelectUndo(); // 最後の画面が検索画面だったら、検索だけやめて戻るようにする

        if (isCurrentSearchNode) Main_ExecuteSearchItems();
        else Main_RenderRightPanel();
    }
    private void Main_SortOrderComboBox_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        _avatarExplorer.SetItemsSortOrder((SortOrder)comboBox.SelectedIndex);
        Main_ReloadCurrentWindow();
    }
    private void Main_AddItem_Click(object? sender, RoutedEventArgs e)
    {
        AddItemOverlay_ShowAddItemWindow();
    }
    private void Main_ImportData_Click(object? sender, RoutedEventArgs e)
    {
        SelectImportTypeOverlay.IsVisible = true;
    }
    private async void Main_ExportDataToCsv_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: チェックボックスで共通素体を含めるかどうかのチェックをする
        string? filePath = await DialogUtils.SaveFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectSaveFilePath], ".csv");
        if (filePath == null) return;

        var localizedItemTypesMapping = Enum.GetValues<ItemType>().ToDictionary(i => i, i => Localizer.Instance[i.GetLocalizationKey() ?? i.ToString()]);
        await _avatarExplorer.ExportToCsv(filePath, localizedItemTypesMapping, true);

        ShowDialog(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.Export]);
    }
    private void Main_DragDrop_Enter(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }
    private void Main_DragDrop_Drop(object? sender, DragEventArgs e)
    {
        IEnumerable<IStorageItem>? storageItems = e.Data.GetFiles();
        if (storageItems == null) return;

        string[] storageItemPaths = storageItems.Select(i => i.TryGetLocalPath()).Where(i => !string.IsNullOrEmpty(i) && (Directory.Exists(i) || File.Exists(i))).ToArray()!;
        AddItemOverlay_ShowAddItemWindow(storageItemPaths);
    }
    
    private void Main_Closing(object? sender, WindowClosingEventArgs e)
    {
        AvatarExplorerApp.ClearTemp();
    }
    
    private async void ItemButton_ContextMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is ContextMenuAction contextMenuAction)
            await ItemButton_ExecuteContextMenuItemCommand(contextMenuAction);
    }
    #endregion

    #region Item Button Event Handler
    private async Task ItemButton_ExecuteContextMenuItemCommand(ContextMenuAction contextMenuAction)
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
    private Item? ItemButton_ContextMenu_GetItemByPath(string itemPath)
    {
        Item? item = _avatarExplorer.GetItemByPath(itemPath);
        if (item == null) ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.ItemNotFound]);

        return item;
    }
    private async Task ItemButton_ContextMenu_OpenItemFolder(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        await AvaloniaLauncherUtils.OpenFolder(this, ItemUtils.GetItemPath(RuntimeSettings.DataRootDirectory, item.ItemPath));
    }
    private async Task ItemButton_ContextMenu_CopyBoothLink(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        string boothLink = item.GetBoothLink();

        try
        {
            await ClipboardUtils.SetTextToClipboard(boothLink);
        }
        catch
        {
            // Ignored
        }
    }
    private async Task ItemButton_ContextMenu_OpenBoothLink(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        await AvaloniaLauncherUtils.OpenLink(this, item.GetBoothLink());
    }
    private Task ItemButton_ContextMenu_ShowOtherItemsByAuthor(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return Task.CompletedTask;

        if (Main_SearchTextBox != null) Main_SearchTextBox.Text = string.Format("Author=\"{0}\"", item.Author);

        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_ChangeThumbnail(string itemPath)
    {
        ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.NotImplemented]);
        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_EditItem(string itemPath)
    {
        Item? item = ItemButton_ContextMenu_GetItemByPath(itemPath);
        if (item == null) return Task.CompletedTask;

        AddItemOverlay_ShowEditItemWindow(item);
        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_AddMemo(string itemPath)
    {
        ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.NotImplemented]);
        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_AddItemFolder(string itemPath)
    {
        ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.NotImplemented]);
        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_EditImplementedAvatar(string itemPath)
    {
        ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.NotImplemented]);
        return Task.CompletedTask;
    }
    private Task ItemButton_ContextMenu_EditItemTag(string itemPath)
    {
        ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.NotImplemented]);
        return Task.CompletedTask;
    }
    #endregion

    #region Add / Edit Item Overlay
    private Item? _selectedItem = null;
    private readonly AddItemOverlayWindowValues _addItemWindowValues = new();

    private void AddItemOverlay_ShowEditItemWindow(Item item)
    {
        AddItemOverlay_InitializeAddItemWindowCategories();

        _selectedItem = item;
        _addItemWindowValues.FromItem(item);
        AddItemOverlay_SetValuesToUi(_addItemWindowValues);
        AddItemOverlay_BoothLinkTextBox.Text = item.GetBoothLink();
        AddItemOverlay.IsVisible = true;
    }
    private void AddItemOverlay_ShowAddItemWindow(IEnumerable<string>? filePaths = null)
    {
        // もし表示されてる状態でD&Dされたら、フォルダ追加だけしてあげる
        if (AddItemOverlay.IsVisible && filePaths != null)
        {
            _addItemWindowValues.Folders.AddRange(filePaths);
            EditFoldersOverlay_UpdateFolderList();
            return;
        }

        AddItemOverlay_InitializeAddItemWindowCategories();

        _selectedItem = null;
        _addItemWindowValues.Reset();
        AddItemOverlay_SetValuesToUi(_addItemWindowValues);
        AddItemOverlay_BoothLinkTextBox.Text = string.Empty;
        AddItemOverlay.IsVisible = true;

        if (filePaths != null) _addItemWindowValues.Folders.AddRange(filePaths);
        EditFoldersOverlay_UpdateFolderList();
    }
    
    private void AddItemOverlay_InitializeAddItemWindowCategories()
    {
        AddItemOverlay_ItemTypeComboBox.Items.Clear();

        foreach (ItemCountInfo itemCountInfo in _avatarExplorer.GetCategories())
        {
            AddItemOverlay_ItemTypeComboBox.Items.Add(Localizer.Instance[((Category)itemCountInfo.Item).ToString()]);
        }

        if (AddItemOverlay_ItemTypeComboBox.Items.Count > 0) AddItemOverlay_ItemTypeComboBox.SelectedIndex = 0;
    }

    private async void AddItemOverlay_GetBoothItemData_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemWindowValues == null) return;
        string boothUrl = AddItemOverlay_BoothLinkTextBox.Text ?? "";

        if (_avatarExplorer.IsApiCooldownNow)
        {
            ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.BoothApiCooldown]);
            return;
        }
        
        Main_ShowProgress(Localizer.Instance[LocalizationKey.Processing.Booth.Status.Fetching]);
        Main_UpdateProgress(0);
        
        BoothItem? boothItem = await _avatarExplorer.GetBoothItem(boothUrl);
        Main_HideProgress();

        if (boothItem == null)
        {
            ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.BoothItemNotFound]);
            return;
        }

        _addItemWindowValues.Title = boothItem.Title;
        _addItemWindowValues.Author = boothItem.Shop.Name;
        _addItemWindowValues.BoothAuthorId = boothItem.AuthorId;
        _addItemWindowValues.BoothId = boothItem.BoothId;
        _addItemWindowValues.BoothThumbnailUrl = boothItem.Thumbnails.Count > 0 ? boothItem.Thumbnails[0].Original : string.Empty;
        _addItemWindowValues.ItemType = (boothItem.EstimatedCategory != ItemType.None && boothItem.EstimatedCategory != ItemType.Unknown) ? boothItem.EstimatedCategory : ItemType.Avatar;

        AddItemOverlay_ResetBoothItemDataButton.IsVisible = true;

        AddItemOverlay_SetValuesToUi(_addItemWindowValues);
    }
    private void AddItemOverlay_ResetBoothItemData_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemWindowValues == null) return;

        _addItemWindowValues.Title = string.Empty;
        _addItemWindowValues.Author = string.Empty;
        _addItemWindowValues.BoothAuthorId = string.Empty;
        _addItemWindowValues.BoothId = -1;
        _addItemWindowValues.BoothThumbnailUrl = string.Empty;

        AddItemOverlay_ResetBoothItemDataButton.IsVisible = false;

        AddItemOverlay_SetValuesToUi(_addItemWindowValues);
    }

    private async void AddItemOverlay_EditFolder_Click(object? sender, RoutedEventArgs e)
    {
        EditFoldersOverlay_UpdateFolderList();
        EditFoldersOverlay.IsVisible = true;
    }
    private void AddItemOverlay_AddCustomCategory_Click(object? sender, RoutedEventArgs e)
    {
        AddCustomCategory_CustomCategoryTextBox.Text = string.Empty;
        AddCustomCategoryOverlay.IsVisible = true;
    }

    private async void AddItemOverlay_ConfirmButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemWindowValues == null) return;

        AddItemOverlay_SetValuesFromUi(_addItemWindowValues);

        if (!AddItemOverlay_ValidateAddItemWindowValues()) return;

        ItemCreationContext itemCreationContext = new();
        itemCreationContext.Folders.AddRange(_addItemWindowValues.Folders);
        itemCreationContext.MaterialFolder = _addItemWindowValues.MaterialFolder;
        itemCreationContext.Title = _addItemWindowValues.Title;
        itemCreationContext.Author = _addItemWindowValues.Author;
        itemCreationContext.AuthorId = _addItemWindowValues.BoothAuthorId;
        itemCreationContext.ThumbnailUrl = _addItemWindowValues.BoothThumbnailUrl;
        itemCreationContext.BoothId = _addItemWindowValues.BoothId;

        var categoryInfo = AddItemOverlay_GetCategoryFromItemWindow();
        itemCreationContext.ItemType = categoryInfo.Item1;
        if (categoryInfo.Item1 == ItemType.Custom) itemCreationContext.CustomCategory = categoryInfo.Item2;

        itemCreationContext.SupportedAvatars.AddRange(_addItemWindowValues.SupportedAvatars);
        itemCreationContext.LocalizedItemTypeName = categoryInfo.Item1 == ItemType.Custom ? categoryInfo.Item2 : Localizer.Instance[categoryInfo.Item1.GetLocalizationKey() ?? ""];

        if (_selectedItem == null)
        {
            Main_ShowProgress(Localizer.Instance[LocalizationKey.Processing.ItemAdd.Copying]);
            Main_UpdateProgress(0);
            var (newItem, processingFailedPaths) = await _avatarExplorer.AddItem(itemCreationContext);
            Main_HideProgress();

            if (processingFailedPaths.Count > 0) // フォルダ展開に失敗した時に発生する
            {
                ShowDialog(
                    Localizer.Instance[LocalizationKey.Error.Default],
                    Localizer.Instance.GetDisplayName(LocalizationKey.Error.ItemFolderProcessingFailedPaths, "\n" + string.Join('\n', processingFailedPaths.Select(i => $"- {i}")))
                );
            }

            if (newItem != null) ShowDialog(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.ItemAdd]);
            else ShowDialog(Localizer.Instance[LocalizationKey.UI.Dialog.Failed.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Failed.ItemAdd]);
        }
        else
        {
            _avatarExplorer.EditItem(_selectedItem, itemCreationContext);
            ShowDialog(Localizer.Instance[LocalizationKey.UI.Dialog.Success.Default], Localizer.Instance[LocalizationKey.UI.Dialog.Success.ItemEdit]);
        }

        _selectedItem = null;
        _addItemWindowValues.Reset();
        AddItemOverlay.IsVisible = false;
    }
    private void AddItemOverlay_CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        _selectedItem = null;
        _addItemWindowValues.Reset();
        AddItemOverlay.IsVisible = false;
    }
    
    private void AddItemOverlay_SetValuesToUi(AddItemOverlayWindowValues addItemWindowValues)
    {
        AddItemOverlay_BoothItemTitleTextBox.Text = addItemWindowValues.Title;
        AddItemOverlay_BoothItemAuthorTextBox.Text = addItemWindowValues.Author;
    }
    private void AddItemOverlay_SetValuesFromUi(AddItemOverlayWindowValues addItemWindowValues)
    {
        addItemWindowValues.Title = AddItemOverlay_BoothItemTitleTextBox.Text ?? "";
        addItemWindowValues.Author = AddItemOverlay_BoothItemAuthorTextBox.Text ?? "";
    }
    private (ItemType, string) AddItemOverlay_GetCategoryFromItemWindow()
    {
        int selectedIndex = AddItemOverlay_ItemTypeComboBox.SelectedIndex;

        // カスタムカテゴリかどうかのチェック(式: ItemTypeの数 - 無効なItemType数 - カスタムカテゴリ)
        if (selectedIndex >= (Enum.GetValues<ItemType>().Length - CategoryUtils.InvalidItemTypes.Length - 1)) // ここの1はカスタムカテゴリ分
        {
            return (ItemType.Custom, AddItemOverlay_ItemTypeComboBox.SelectedItem?.ToString() ?? "");
        }

        return ((ItemType)selectedIndex, string.Empty);
    }
    private bool AddItemOverlay_ValidateAddItemWindowValues()
    {
        var validationResult = _addItemWindowValues.Validate();
        if (!validationResult.Item1) ShowDialog(LocalizationKey.Error.Default, Localizer.Instance[validationResult.Item2]);

        return validationResult.Item1;
    }
    #endregion

    #region Edit Folder Overlay
    private async void EditFoldersOverlay_AddFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await DialogUtils.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], true);
        if (folders == null || folders.Length == 0) return;

        _addItemWindowValues.Folders.AddRange(folders);
        EditFoldersOverlay_UpdateFolderList();
    }
    private async void EditFoldersOverlay_AddFile_Click(object? sender, RoutedEventArgs e)
    {
        string[]? files = await DialogUtils.OpenFileDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], true);
        if (files == null || files.Length == 0) return;

        _addItemWindowValues.Folders.AddRange(files);
        EditFoldersOverlay_UpdateFolderList();
    }
    private void EditFoldersOverlay_ConfirmButton_Click(object? sender, RoutedEventArgs e)
    {
        EditFoldersOverlay.IsVisible = false;
    }
    
    private void EditFoldersOverlay_UpdateFolderList()
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
    private void EditFoldersOverlay_RemoveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string folderPath)
        {
            _addItemWindowValues.Folders.RemoveAll(i => i == folderPath);
            EditFoldersOverlay_UpdateFolderList();
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
    
    #region Add CustomCategory Overlay
    private void AddCustomCategory_AddButton_Click(object? sender, RoutedEventArgs e)
    {
        AddCustomCategoryOverlay.IsVisible = false;

        if (string.IsNullOrEmpty(AddCustomCategory_CustomCategoryTextBox.Text)) return;
        int index = AddItemOverlay_ItemTypeComboBox.Items.Add(AddCustomCategory_CustomCategoryTextBox.Text);
        AddItemOverlay_ItemTypeComboBox.SelectedIndex = index;
    }
    private void AddCustomCategory_CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        AddCustomCategoryOverlay.IsVisible = false;
    }
    #endregion

    #region Settings Overlay
    private void Main_SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        SetUiValueFromCurrentSettings();
        SettingsOverlay.IsVisible = true;
    }

    private async void SettingsOverlay_OpenFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await DialogUtils.OpenFolderDialog(this, "フォルダを選択してください", false);
        if (folders == null || folders.Length == 0) return;

        SettingsOverlay_ItemsFolderPathTextBox.Text = folders[0];
    }
    private void SettingsOverlay_CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        SettingsOverlay.IsVisible = false;
    }
    private void SettingsOverlay_ApplyButton_Click(object? sender, RoutedEventArgs e)
    {
        ApplySettingsValues();
        Main_ReloadCurrentWindow();
    }
    
    private void SetUiValueFromCurrentSettings() // 設定画面を読み込んだ時に値をセットするための関数
    {
        RuntimeSettings runtimeSettings = _avatarExplorer.GetRuntimeSettings();
        UserUiPreferences userUiPreferences = _userUiPreferences;

        SettingsOverlay_ItemsFolderPathTextBox.Text = runtimeSettings.DataRootDirectory;
        SettingsOverlay_RemoveBracketsCheckBox.IsChecked = runtimeSettings.RemoveBrackets;
        SettingsOverlay_RemoveOriginalCheckBox.IsChecked = runtimeSettings.RemoveOriginal;
        SettingsOverlay_ItemsPerPageTextBox.Text = userUiPreferences.ItemsPerPage.ToString();
        SettingsOverlay_ThemeComboBox.SelectedIndex = (int)userUiPreferences.Theme;
        SettingsOverlay_DefaultLanguageComboBox.SelectedIndex = userUiPreferences.DefaultLanguage;
        SettingsOverlay_DefaultSortOrderComboBox.SelectedIndex = (int)runtimeSettings.ItemSortOrder;
    }
    private void ApplySettingsValues() // 設定の適用ボタンが押されたときのみ
    {
        _avatarExplorer.SetDataRootDirectory(SettingsOverlay_ItemsFolderPathTextBox.Text ?? "");
        _avatarExplorer.SetRemoveBrackets(SettingsOverlay_RemoveBracketsCheckBox.IsChecked ?? false);
        _avatarExplorer.SetRemoveOriginal(SettingsOverlay_RemoveOriginalCheckBox.IsChecked ?? false);
        _userUiPreferences.SetItemsPerPage(int.TryParse(SettingsOverlay_ItemsPerPageTextBox.Text, out var result) ? result : 30);
        _userUiPreferences.SetTheme((Theme)SettingsOverlay_ThemeComboBox.SelectedIndex);
        _userUiPreferences.SetLanguage(SettingsOverlay_DefaultLanguageComboBox.SelectedIndex);
        _avatarExplorer.SetItemsSortOrder((SortOrder)SettingsOverlay_DefaultSortOrderComboBox.SelectedIndex);

        ApplyPreferenceSettingsToUi();
        ApplyRuntimeSettingsToUi();

        // 適用時は自動で保存する
        _avatarExplorer.SaveRuntimeSettings();
        _userUiPreferences.Save();
    }
    
    private void ApplyPreferenceSettingsToUi()
    {
        Application? currentApplication = Application.Current;
        if (currentApplication != null)
        {
            /*
                これも設定する
                TransparencyLevelHint="AcrylicBlur"
                Background="Transparent"
            */

            if (_userUiPreferences.Theme == Models.Theme.Auto) currentApplication.RequestedThemeVariant = ThemeVariant.Default;
            else if (_userUiPreferences.Theme == Models.Theme.Dark) currentApplication.RequestedThemeVariant = ThemeVariant.Dark;
            else if (_userUiPreferences.Theme == Models.Theme.Light) currentApplication.RequestedThemeVariant = ThemeVariant.Light;
        }
        
        Main_LanguageComboBox.SelectedIndex = _userUiPreferences.DefaultLanguage;
    }
    private void ApplyRuntimeSettingsToUi()
    {
        Main_SortOrderComboBox.SelectedIndex = (int)_avatarExplorer.GetRuntimeSettings().ItemSortOrder;
    }
    #endregion

    #region Data Import Overlay
    private void SelectImportTypeOverlay_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        SelectImportTypeOverlay.IsVisible = false;
    }
    private void SelectImportTypeOverlay_FromV1_Click(object? sender, RoutedEventArgs e)
        => DataImportInternal(DataImportType.V1);
    private void SelectImportTypeOverlay_FromKonoAsset_Click(object? sender, RoutedEventArgs e)
        => DataImportInternal(DataImportType.KonoAsset);
    private async void DataImportInternal(DataImportType dataImportType)
    {
        string[]? folders = await DialogUtils.OpenFolderDialog(this, Localizer.Instance[LocalizationKey.UI.Dialog.SelectFolderPath], false);
        if (folders == null || folders.Length == 0) return;

        string selectedFolder = folders[0];
        
        SelectImportTypeOverlay.IsVisible = false;

        var localizedItemTypesMapping = Enum.GetValues<ItemType>().ToDictionary(i => i, i => Localizer.Instance[i.GetLocalizationKey() ?? i.ToString()]);

        var progress = new Progress<(string, int, string)>(tuple =>
        {
            if (tuple.Item2 == 100)
            {
                Main_HideProgress();
            }
            else
            {
                Main_ShowProgress(Localizer.Instance.GetDisplayName(tuple.Item1, tuple.Item2.ToString()));
                Main_UpdateProgress(tuple.Item2);
            }
        });

        if (dataImportType == DataImportType.V1) await _avatarExplorer.ImportFromV1(selectedFolder, localizedItemTypesMapping, progress);
        else if (dataImportType == DataImportType.KonoAsset)  await _avatarExplorer.ImportFromKonoAsset(selectedFolder, localizedItemTypesMapping, progress);

        Main_ReloadCurrentWindow();
    }
    #endregion
    
    #region Main UI Methods
    private void Main_ReloadCurrentWindow()
    {
        Main_RenderLeftPanel();

        // 最後に表示されていた画面が検索画面だったら、キャッシュを元にもう一度検索してあげる
        if (_isLastWindowSearch) Main_ExecuteSearchItems();
        else Main_RenderRightPanel();
    }

    private void Main_CheckPageStates()
    {
        List<ItemTagState> selectedItemTagStates = new();

        foreach (SelectionNode selectionNode in _avatarExplorer.GetCurrentPaths())
        {
            if (!selectedItemTagStates.Contains(selectionNode.State))
                selectedItemTagStates.Add(selectionNode.State);
        }

        foreach (var pageInfo in _currentPageStates)
        {
            if (!selectedItemTagStates.Contains(pageInfo.Key))
                _currentPageStates[pageInfo.Key] = 0;
        }
    }
    
    private void Main_RestoreScrollViewerOffset(ScrollViewer scrollViewer, ItemTagState itemTagState)
    {
        if (_currentScrollValues.TryGetValue(itemTagState, out Vector scrollValue))
            scrollViewer.Offset = scrollValue;
    }
    private void Main_SaveScrollViewerOffset(ScrollViewer scrollViewer, ItemTagState itemTagState)
    {
        if (!_currentScrollValues.ContainsKey(itemTagState)) return;
        _currentScrollValues[itemTagState] = scrollViewer.Offset;
    }
    private void Main_ResetScrollViewerOffset(ItemTagState itemTagState)
    {
        if (!_currentScrollValues.ContainsKey(itemTagState)) return;
        _currentScrollValues[itemTagState] = new();
    }
    private void Main_ResetAllScrollViewerOffset()
    {
        foreach (ItemTagState key in _currentScrollValues.Keys)
            _currentScrollValues[key] = new();
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
    #endregion
}
