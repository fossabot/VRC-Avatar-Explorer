using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
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
    private string _lastSearchTextCache = string.Empty; // 最後に実行された検索のキャッシュ
    private string _searchTextCache = string.Empty;
    private bool _isLastWindowSearch = false;
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
        TODO: 実装やタグは新しくUIを作って上げることで実装する。右クリックメニューでは扱わない（チェックとかでメモリリークする可能性があるため）
        TODO: 下のボタンの処理を実装する
        TODO: SCHEMEに対応する
        TODO: アイテムのカテゴリを変更したときにフォルダを移行できるように変更
        TODO: 詳細検索用の画面を追加する（右のアイテム画面の右側に縦長に別ウィンドウみたいな感じで表示するのはありかも？）
        TODO: スクロール位置を保存するようにしたいね
        TODO: ソフト終了時にTempを削除したい
        */

        InitializeComponent();
        InitializeAvatarExplorer();
        InitializeContextMenuHandlers();
        InitializeNoItemsLabel();
        InitializeUserUiPreferences();

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
        if (RightPanelParent == null) return;

        RightPanelParent.Children.Clear();

        RightPanelParent.Children.Add(new Image
        {
            Source = IconUtils.GetIcon(SystemIcon.NothingIcon),
            Width = 150,
            Height = 150,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });

        RightPanelParent.Children.Add(new TextBlock
        {
            Text = Localizer.Instance[LocalizationKey.Error.Nothing],
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            FontSize = 25
        });
    }
    private void InitializeContextMenuHandlers()
    {
        _contextMenuHandlers = new()
        {
            { ActionKey.OpenItemFolder, ContextMenu_OpenItemFolder },
            { ActionKey.CopyBoothLink, ContextMenu_CopyBoothLink },
            { ActionKey.OpenBoothLink, ContextMenu_OpenBoothLink },
            { ActionKey.ShowOtherItemsByAuthor, ContextMenu_ShowOtherItemsByAuthor },
            { ActionKey.ChangeThumbnail, ContextMenu_ChangeThumbnail },
            { ActionKey.EditItem, ContextMenu_EditItem },
            { ActionKey.AddItemMemo, ContextMenu_AddMemo},
            { ActionKey.AddItemFolder, ContextMenu_AddItemFolder },
            { ActionKey.EditImplementedAvatar, ContextMenu_EditImplementedAvatar },
            { ActionKey.EditItemTag, ContextMenu_EditItemTag }
        };
    }
    #endregion

    #region Left Panel
    private void LeftFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        RenderLeftPanel();
    }

    private void RenderLeftPanel()
    {
        if (LeftPanel == null) return;
        LeftPanel.Children.Clear();

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

        int currentPage = GetPage(customState); // -1が返された場合は対応していないStateのため、全てのアイテムを表示してあげる

        foreach (ItemCountInfo itemCountInfo in currentPage != -1 ? items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage) : items)
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(itemCountInfo.Item), ItemButton_ContextMenuItem_Click);
            UIUtils.AddItemButton(LeftPanel, new UISelectableItem(itemCountInfo).SetState(customState), RuntimeSettings.RemoveBrackets, itemContextMenu, LeftPanel_ItemButton_Clicked);
        }

        if (currentPage != -1 && items.Count != 0) UIUtils.AddPageButton(LeftPanel, customState, currentPage, ItemsPerPage, items.Count, LeftPanel_ItemButton_Clicked);
    }
    private void LeftPanel_ItemButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        
        if (button.Tag is ItemTagInfo itemTagInfo)
        {
            _avatarExplorer.SelectClear();
            _avatarExplorer.Select(itemTagInfo.State, itemTagInfo.Value);
            CheckPageStates();

            RenderRightPanel();
        }

        if (button.Tag is PageButtonInfo pageButtonInfo)
        {
            _currentPageStates[pageButtonInfo.ItemTagState] = pageButtonInfo.NextPageValue;
            RenderLeftPanel();
        }
    }
    #endregion

    #region Right Panel
    private void RenderRightPanel()
    {
        if (RightPanel == null) return;
        RightPanel.Children.Clear();

        IReadOnlyList<ItemCountInfo> items = _avatarExplorer.GetItemsForCurrentState();

        if (items.Count == 0) ShowNoItemsLabel();
        else HideNoItemsLabel();

        ItemTagState itemTagState = ItemTagState.Unknown;
        if (items.Count > 0) itemTagState = new UISelectableItem(items[0]).Tag.State;

        int currentPage = GetPage(itemTagState); // -1が返された場合は対応していないStateのため、全てのアイテムを表示してあげる

        foreach (ItemCountInfo itemCountInfo in currentPage != -1 ? items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage) : items)
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(itemCountInfo.Item), ItemButton_ContextMenuItem_Click);
            UIUtils.AddItemButton(RightPanel, new UISelectableItem(itemCountInfo), RuntimeSettings.RemoveBrackets, itemContextMenu, RightPanel_ItemButton_Clicked);
        }

        if (currentPage != -1 && items.Count != 0) UIUtils.AddPageButton(RightPanel, itemTagState, currentPage, ItemsPerPage, items.Count, RightPanel_ItemButton_Clicked);
        _isLastWindowSearch = false;
        LoadCurrentPath();
    }
    private async void RightPanel_ItemButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;

        if (button.Tag is ItemTagInfo itemTagInfo)
        {
            if (itemTagInfo.State == ItemTagState.ItemFileCategoryOpen) // ファイルを押されると、アイテムを開く処理に移行する
            {
                string itemPath = itemTagInfo.Value; // ItemFileCategoryOpenのValueはファイルのパスになっている
                await OpenFileInternalAsync(itemPath);
            }
            else
            {
                _avatarExplorer.Select(itemTagInfo.State, itemTagInfo.Value);
                CheckPageStates();

                RenderRightPanel();
            }
        }

        if (button.Tag is PageButtonInfo pageButtonInfo)
        {
            _currentPageStates[pageButtonInfo.ItemTagState] = pageButtonInfo.NextPageValue;
            if (pageButtonInfo.ItemTagState == ItemTagState.SearchItem) ExecuteSearchItems();
            else RenderRightPanel();
        }
    }
    private async Task OpenFileInternalAsync(string filePath)
    {
        bool isUnitypackage = filePath.ToLower().EndsWith(".unitypackage");
        
        if (isUnitypackage) await OpenUnitypackageInternalAsync(filePath); // Unitypackageだと自動展開処理に移る
        else await AvaloniaLauncherUtils.OpenFile(this, filePath);
    }
    private async Task OpenUnitypackageInternalAsync(string itemPath)
    {
        Item? selectedItem = _avatarExplorer.GetSelectedItem();
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
                // 空白の場合はないということだからスキップする
                if (!string.IsNullOrEmpty(tuple.Item3))
                {
                    _ = AvaloniaLauncherUtils.OpenFile(this, tuple.Item3);
                }
            }
            else
            {
                ShowProgress(Localizer.Instance.GetDisplayName(tuple.Item1, tuple.Item2.ToString()));
                UpdateProgress(tuple.Item2);
            }
        });

        AvatarExplorerApp.ModifyUnityPackageFilePath(itemPath, Localizer.Instance[selectedItem.Type.GetLocalizationKey() ?? ""], progress: progress);
    }
    #endregion

    #region Search Box
    private readonly DispatcherTimer _searchTimer = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private void Main_SearchTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchTimer.Stop();
        _searchTimer.Tick -= OnSearchTimerTick;
        _searchTimer.Tick += OnSearchTimerTick;
        _searchTimer.Start();
    }
    private void OnSearchTimerTick(object? sender, EventArgs e)
    {
        _searchTimer.Stop();
        _searchTextCache = SearchTextBox.Text ?? "";
        ExecuteSearchItems();
    }
    private void ExecuteSearchItems()
    {
        if (string.IsNullOrEmpty(_searchTextCache))
        {
            RenderRightPanel();
            return;
        }

        RightPanel.Children.Clear();

        SearchFilter searchFilter = SearchUtils.BuildFilter(_searchTextCache);
        IReadOnlyList<Item> items = _avatarExplorer.SearchItems(searchFilter);
        
        // 検索文字列が前回と違う場合はページをリセットする
        if (_searchTextCache != _lastSearchTextCache) _currentPageStates[ItemTagState.SearchItem] = 0;
        _lastSearchTextCache = _searchTextCache;

        if (items.Count == 0) ShowNoItemsLabel();
        else HideNoItemsLabel();

        int currentPage = GetPage(ItemTagState.SearchItem); // SearchItemは必ずページが存在しているため

        foreach (Item item in items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage))
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(item), ItemButton_ContextMenuItem_Click);
            UIUtils.AddItemButton(RightPanel, new UISelectableItem(item, 0).SetState(ItemTagState.SearchItem), RuntimeSettings.RemoveBrackets, itemContextMenu, RightPanel_ItemButton_Clicked);
        }

        if (items.Count != 0) UIUtils.AddPageButton(RightPanel, ItemTagState.SearchItem, currentPage, ItemsPerPage, items.Count, RightPanel_ItemButton_Clicked);
        _isLastWindowSearch = true;
        
        PathBox.Text = searchFilter.ToPathString();
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

    #region Path
    private void LoadCurrentPath()
    {
        if (PathBox == null) return;

        IEnumerable<SelectionNode> currentSelectionNodes = _avatarExplorer.GetCurrentPaths();
        if (!currentSelectionNodes.Any())
        {
            PathBox.Text = Localizer.Instance[LocalizationKey.Path.Default];
            return;
        }

        List<SelectionNode> selectionNodes = new();
        foreach (SelectionNode node in currentSelectionNodes)
        {
            if (node.State == ItemTagState.SearchItem) selectionNodes.Clear();
            selectionNodes.Add(node);
        }

        PathBox.Text = string.Join(" > ", selectionNodes.Select(BuildPathTextInternal));
    }

    private string BuildPathTextInternal(SelectionNode selectionNode)
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

    #region No Items Label
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
    #endregion

    #region Main UI Event Handler
    private void Main_UndoButton_Click(object? sender, RoutedEventArgs e)
    {
        // 選択されていたアイテムが検索結果時のものだったら、キャッシュを元にもう一度検索してあげる
        bool isCurrentSearchNode = _avatarExplorer.GetCurrentPathState()?.State == ItemTagState.SearchItem;
        
        CheckPageStates(); // SelectUndoより前にやってあげないと、戻った先の画面のページ情報がリセットされる
        _avatarExplorer.SelectUndo();

        if (isCurrentSearchNode) ExecuteSearchItems();
        else RenderRightPanel();
    }
    private void Main_SortOrderComboBox_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        _avatarExplorer.SetItemsSortOrder((SortOrder)comboBox.SelectedIndex);
        ReloadCurrentWindow();
    }
    private void Main_ImportData_Click(object? sender, RoutedEventArgs e)
    {
        SelectImportTypeOverlay.IsVisible = true;
    }

    private async void Main_ExportDataToCsv_Click(object? sender, RoutedEventArgs e)
    {
        //チェックボックスでやる
        string? filePath = await DialogUtils.SaveFileDialog(this, "保存先を選択してください", ".csv");
        if (filePath == null) return;

        var localizedItemTypesMapping = Enum.GetValues<ItemType>().ToDictionary(i => i, i => Localizer.Instance[i.GetLocalizationKey() ?? i.ToString()]);
        await _avatarExplorer.ExportToCsv(filePath, localizedItemTypesMapping, true);

        ShowDialog("成功", "データの出力が完了しました。");
    }
    private async void ItemButton_ContextMenuItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.Tag is ContextMenuAction contextMenuAction)
            await ExecuteContextMenuItemCommand(contextMenuAction);
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
    private Item? ContextMenu_GetItemByPath(string itemPath)
    {
        Item? item = _avatarExplorer.GetItemByPath(itemPath);
        if (item == null) ShowDialog("エラー", "アイテムが見つかりませんでした"); //TODO: Localizeする

        return item;
    }
    private async Task ContextMenu_OpenItemFolder(string itemPath)
    {
        Item? item = ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        await AvaloniaLauncherUtils.OpenFolder(this, ItemUtils.GetItemPath(RuntimeSettings.DataRootDirectory, item.ItemPath));
    }
    private async Task ContextMenu_CopyBoothLink(string itemPath)
    {
        Item? item = ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        string boothLink = item.GetBoothLink();

        try
        {
            await ClipboardUtils.SetTextToClipboard(boothLink);
            ShowDialog("成功", "クリップボードにリンクをコピーしました。"); //TODO: Localizeする
        }
        catch
        {
            ShowDialog("エラー", "クリップボードにリンクをコピー出来ませんでした。"); //TODO: Localizeする
        }
    }
    private async Task ContextMenu_OpenBoothLink(string itemPath)
    {
        Item? item = ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        await AvaloniaLauncherUtils.OpenLink(this, item.GetBoothLink());
    }
    private Task ContextMenu_ShowOtherItemsByAuthor(string itemPath)
    {
        Item? item = ContextMenu_GetItemByPath(itemPath);
        if (item == null) return Task.CompletedTask;

        if (SearchTextBox != null) SearchTextBox.Text = string.Format("Author=\"{0}\"", item.Author);
        ExecuteSearchItems();

        return Task.CompletedTask;
    }
    private Task ContextMenu_ChangeThumbnail(string itemPath)
    {
        ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.NotImplemented]);
        return Task.CompletedTask;
    }
    private Task ContextMenu_EditItem(string itemPath)
    {
        Item? item = ContextMenu_GetItemByPath(itemPath);
        if (item == null) return Task.CompletedTask;

        ShowEditItemWindow(item);
        return Task.CompletedTask;
    }
    private Task ContextMenu_AddMemo(string itemPath)
    {
        ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.NotImplemented]);
        return Task.CompletedTask;
    }
    private Task ContextMenu_AddItemFolder(string itemPath)
    {
        ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.NotImplemented]);
        return Task.CompletedTask;
    }
    private Task ContextMenu_EditImplementedAvatar(string itemPath)
    {
        ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.NotImplemented]);
        return Task.CompletedTask;
    }
    private Task ContextMenu_EditItemTag(string itemPath)
    {
        ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.NotImplemented]);
        return Task.CompletedTask;
    }
    #endregion

    #region Add / Edit Item Menu
    private Item? _selectedItem = null;
    private readonly AddItemOverlayWindowValues _addItemWindowValues = new();

    private void AddItem_Click(object? sender, RoutedEventArgs e)
    {
        ShowAddItemWindow();
    }

    private void ShowEditItemWindow(Item item)
    {
        InitializeAddItemWindowCategories();

        _selectedItem = item;
        _addItemWindowValues.FromItem(item);
        SetAddItemWindowValues(_addItemWindowValues);
        AddItemOverlay_BoothLinkTextBox.Text = item.GetBoothLink();
        AddItemOverlay.IsVisible = true;
    }
    private void ShowAddItemWindow()
    {
        InitializeAddItemWindowCategories();

        _selectedItem = null;
        _addItemWindowValues.Reset();
        SetAddItemWindowValues(_addItemWindowValues);
        AddItemOverlay_BoothLinkTextBox.Text = string.Empty;
        AddItemOverlay.IsVisible = true;
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
        
        ShowProgress(Localizer.Instance[LocalizationKey.Processing.Booth.Status.Fetching]);
        UpdateProgress(0);
        
        BoothItem? boothItem = await _avatarExplorer.GetBoothItem(boothUrl);
        HideProgress();

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

        SetAddItemWindowValues(_addItemWindowValues);
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

        SetAddItemWindowValues(_addItemWindowValues);
    }
    
    private async void AddItemOverlay_AddFolder_Click(object? sender, RoutedEventArgs e)
    {
        string[]? folders = await DialogUtils.OpenFolderDialog(this, "フォルダを選択してください", true);
        if (folders == null || folders.Length == 0) return;

        _addItemWindowValues.Folders.Clear();
        _addItemWindowValues.Folders.AddRange(folders); // TODO: 存在してるフォルダのみ追加する
        // TODO: フォルダ追加時に右がとこかに、現在選択されているフォルダを [ファイル | フォルダ名] [削除]みたいにリストで表示して編集しやすくしたいよね
    }
    
    private void AddItemOverlay_AddCustomCategory_Click(object? sender, RoutedEventArgs e)
    {
        AddCustomCategory_CustomCategoryTextBox.Text = string.Empty;
        AddCustomCategoryOverlay.IsVisible = true;
    }

    private async void AddItemOverlay_ConfirmButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemWindowValues == null) return;
        if (!ValidateAddItemWindowValues()) return;

        ItemCreationContext itemCreationContext = new();
        itemCreationContext.Folders.AddRange(_addItemWindowValues.Folders);
        itemCreationContext.MaterialFolder = _addItemWindowValues.MaterialFolder;
        itemCreationContext.Title = _addItemWindowValues.Title;
        itemCreationContext.Author = _addItemWindowValues.Author;
        itemCreationContext.AuthorId = _addItemWindowValues.BoothAuthorId;
        itemCreationContext.ThumbnailUrl = _addItemWindowValues.BoothThumbnailUrl;
        itemCreationContext.BoothId = _addItemWindowValues.BoothId;

        var categoryInfo = GetCategoryFromItemWindow();
        itemCreationContext.ItemType = categoryInfo.Item1;
        if (categoryInfo.Item1 == ItemType.Custom) itemCreationContext.CustomCategory = categoryInfo.Item2;

        itemCreationContext.SupportedAvatars.AddRange(_addItemWindowValues.SupportedAvatars);
        itemCreationContext.LocalizedItemTypeName = categoryInfo.Item1 == ItemType.Custom ? categoryInfo.Item2 : Localizer.Instance[categoryInfo.Item1.GetLocalizationKey() ?? ""];

        if (_selectedItem == null)
        {
            var (_, processingFailedPaths) = await _avatarExplorer.AddItem(itemCreationContext);
            if (processingFailedPaths.Count > 0) // フォルダ展開に失敗した時に発生する
            {
                ShowDialog(
                    Localizer.Instance[LocalizationKey.Error.Default],
                    Localizer.Instance.GetDisplayName(LocalizationKey.Error.ItemFolderProcessingFailedPaths, "\n" + string.Join('\n', processingFailedPaths.Select(i => $"- {i}")))
                );
            }
        }
        else
        {
            _avatarExplorer.EditItem(_selectedItem, itemCreationContext);
        }
    }
    private void AddItemOverlay_CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        _selectedItem = null;
        _addItemWindowValues.Reset();
        AddItemOverlay.IsVisible = false;
    }
    private void SetAddItemWindowValues(AddItemOverlayWindowValues addItemWindowValues)
    {
        AddItemOverlay_BoothItemTitleTextBox.Text = addItemWindowValues.Title;
        AddItemOverlay_BoothItemAuthorTextBox.Text = addItemWindowValues.Author;
        AddItemOverlay_ItemTypeComboBox.SelectedIndex = (int)addItemWindowValues.ItemType;
    }
    private void InitializeAddItemWindowCategories()
    {
        AddItemOverlay_ItemTypeComboBox.Items.Clear();

        foreach (ItemCountInfo itemCountInfo in _avatarExplorer.GetCategories())
        {
            AddItemOverlay_ItemTypeComboBox.Items.Add(Localizer.Instance[((Category)itemCountInfo.Item).ToString()]);
        }
    }
    private (ItemType, string) GetCategoryFromItemWindow()
    {
        int selectedIndex = AddItemOverlay_ItemTypeComboBox.SelectedIndex;

        // カスタムカテゴリかどうかのチェック(式: ItemTypeの数 - 無効なItemType数 - カスタムカテゴリ)
        if (selectedIndex >= (Enum.GetValues(typeof(ItemType)).Length - CategoryUtils.InvalidItemTypes.Length - 1)) // ここの1はカスタムカテゴリ分
        {
            return (ItemType.Custom, AddItemOverlay_ItemTypeComboBox.SelectedItem?.ToString() ?? "");
        }

        return ((ItemType)selectedIndex, string.Empty);
    }
    private bool ValidateAddItemWindowValues()
    {
        var validationResult = _addItemWindowValues.Validate();
        if (!validationResult.Item1) ShowDialog(LocalizationKey.Error.Default, Localizer.Instance[validationResult.Item2]);

        return validationResult.Item1;
    }
    #endregion

    #region Settings Menu
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
        ReloadCurrentWindow();
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
        string[]? folders = await DialogUtils.OpenFolderDialog(this, "フォルダを選択してください", false); // TODO: Localizeする
        if (folders == null || folders.Length == 0) return;

        string selectedFolder = folders[0];
        
        SelectImportTypeOverlay.IsVisible = false;

        var localizedItemTypesMapping = Enum.GetValues<ItemType>().ToDictionary(i => i, i => Localizer.Instance[i.GetLocalizationKey() ?? i.ToString()]);

        var progress = new Progress<(string, int, string)>(tuple =>
        {
            if (tuple.Item2 == 100)
            {
                HideProgress();
            }
            else
            {
                ShowProgress(Localizer.Instance.GetDisplayName(tuple.Item1, tuple.Item2.ToString()));
                UpdateProgress(tuple.Item2);
            }
        });

        if (dataImportType == DataImportType.V1) await _avatarExplorer.ImportFromV1(selectedFolder, localizedItemTypesMapping, progress);
        else if (dataImportType == DataImportType.KonoAsset)  await _avatarExplorer.ImportFromKonoAsset(selectedFolder, localizedItemTypesMapping, progress);

        ReloadCurrentWindow();
    }
    #endregion
    
    #region Main UI Methods
    private void ReloadCurrentWindow()
    {
        RenderLeftPanel();

        // 最後に表示されていた画面が検索画面だったら、キャッシュを元にもう一度検索してあげる
        if (_isLastWindowSearch) ExecuteSearchItems();
        else RenderRightPanel();
    }

    private void CheckPageStates()
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
    #endregion
}
