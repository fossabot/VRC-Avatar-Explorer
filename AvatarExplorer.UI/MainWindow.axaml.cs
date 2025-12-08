using System;
using System.Collections.Generic;
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
        { ItemTagState.SearchItem, 0 }, // TODO: 検索の時のページが上手く機能してないからここ直す
        { ItemTagState.RootAvatar, 0 },
        { ItemTagState.RootAuthor, 0 },
        { ItemTagState.RootCategory, 0 },
        { ItemTagState.RootSelectedCategory, 0 },
        { ItemTagState.RootSelectedItem, 0 },
        { ItemTagState.ItemFileCategory, 0 }
    };

    private readonly UserUiPreferences _userUiPreferences = new();
    private int ItemsPerPage => _userUiPreferences.ItemsPerPage;

    private bool RemoveBrackets => _avatarExplorer.GetRuntimeSettings().RemoveBrackets;

    private bool CanDisplayPage(ItemTagState itemTagState)
        => _currentPageStates.ContainsKey(itemTagState);

    private int GetCurrentPage(ItemTagState itemTagState)
        => CanDisplayPage(itemTagState) ? _currentPageStates[itemTagState] : -1;

    public MainWindow()
    {
        /* プロジェクトTODO
        TODO: 言語変更を実装する (UIが完成したらやる)
        TODO: 検索中だった場合、検索結果が出てる画面まで戻すのか決める。今は検索状況もリセットされ、その前まで戻される。
        TODO: 右クリックメニューの処理を作る
        TODO: UIのタグを使った翻訳機能を追加する
        TODO: 実装やタグは新しくUIを作って上げることで実装する。右クリックメニューでは扱わない（チェックとかでメモリリークする可能性があるため）
        TODO: 下のボタンの処理を実装する
        TODO: 展開時、カテゴリ名のフォルダ先に展開するかどうかの設定を追加(正直あんまりいらないかも)
        TODO: SCHEMEに対応する
        TODO: アイテムのカテゴリを変更したときにフォルダを移行できるように変更
        TODO: 詳細検索用の画面を追加する
        TODO: アイテム追加時の画面に、現時点での全てのカテゴリをComboBoxに入れておき、その横にボタンでカテゴリを追加できるようにする
        TODO: スクロール位置を保存するようにしたいね
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

        int currentPage = GetCurrentPage(customState); // -1が返された場合は対応していないStateのため、全てのアイテムを表示してあげる

        foreach (ItemCountInfo itemCountInfo in currentPage != -1 ? items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage) : items)
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(itemCountInfo.Item), ItemButton_ContextMenuItem_Click);
            UIUtils.AddItemButton(LeftPanel, new UISelectableItem(itemCountInfo).SetState(customState), RemoveBrackets, itemContextMenu, LeftPanel_ItemButton_Clicked);
        }

        if (currentPage != -1) UIUtils.AddPageButton(LeftPanel, customState, currentPage, ItemsPerPage, items.Count, LeftPanel_ItemButton_Clicked);
    }
    private void LeftPanel_ItemButton_Clicked(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        
        if (button.Tag is ItemTagInfo itemTagInfo)
        {
            _avatarExplorer.SelectClear();
            _avatarExplorer.Select(itemTagInfo.State, itemTagInfo.Value);

            RenderRightPanel();
            LoadCurrentPath();
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

        var items = _avatarExplorer.GetItemsForCurrentState();

        if (items.Count == 0) ShowNoItemsLabel();
        else HideNoItemsLabel();

        ItemTagState itemTagState = ItemTagState.Unknown;
        if (items.Count > 0) itemTagState = new UISelectableItem(items[0]).Tag.State;

        int currentPage = GetCurrentPage(itemTagState); // -1が返された場合は対応していないStateのため、全てのアイテムを表示してあげる

        foreach (ItemCountInfo itemCountInfo in currentPage != -1 ? items.Skip(currentPage * ItemsPerPage).Take(ItemsPerPage) : items)
        {
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(itemCountInfo.Item), ItemButton_ContextMenuItem_Click);
            UIUtils.AddItemButton(RightPanel, new UISelectableItem(itemCountInfo), RemoveBrackets, itemContextMenu, RightPanel_ItemButton_Clicked);
        }

        if (currentPage != -1) UIUtils.AddPageButton(RightPanel, itemTagState, currentPage, ItemsPerPage, items.Count, RightPanel_ItemButton_Clicked);
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
                RenderRightPanel();
            }

            LoadCurrentPath();
        }

        if (button.Tag is PageButtonInfo pageButtonInfo)
        {
            _currentPageStates[pageButtonInfo.ItemTagState] = pageButtonInfo.NextPageValue;
            RenderRightPanel();
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
                // 空白の場合はないということだからスキップする
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
        ExecuteSearchItems();
    }
    private void ExecuteSearchItems()
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
            ContextMenu itemContextMenu = ContextMenuUtils.GetContextMenu(ContextMenuCreator.CreateContextMenu(item), ItemButton_ContextMenuItem_Click);
            UIUtils.AddItemButton(RightPanel, new UISelectableItem(item, 0).SetState(ItemTagState.SearchItem), RemoveBrackets, itemContextMenu, RightPanel_ItemButton_Clicked);
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

        IEnumerable<SelectionNode> currentSelectionNodes = _avatarExplorer.GetCurrentPath();
        if (!currentSelectionNodes.Any())
        {
            PathBox.Text = Localizer.Instance[LocalizationKey.Path.Default];
            return;
        }

        List<SelectionNode> selectionNodes = new();
        foreach (var node in currentSelectionNodes)
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

        if (state == ItemTagState.SearchItem || state == ItemTagState.RootAvatar || state == ItemTagState.RootSelectedItem)
        {
            Item? item = _avatarExplorer.GetAllItems().FirstOrDefault(item => item.ItemPath == value);
            if (item != null) value = item.Title; // アイテムはパスからタイトルに変換する
        }

        if (state == ItemTagState.RootCategory || state == ItemTagState.RootSelectedCategory || state == ItemTagState.ItemFileCategory)
        {
            // カテゴリはValue自体を翻訳する
            // カテゴリ: Search.Category.Textureのような感じで入っているため
            value = Localizer.Instance[value];
        }

            // 翻訳できない(Root以外)はここがnullになるため、valueがパスになる。ある場合はPrefixが翻訳される。
        string? localizationKey = state.GetLocalizationKey();

        return localizationKey == null ? value : Localizer.Instance.GetDisplayName(localizationKey, [value]);
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
        _avatarExplorer.SelectUndo();
        RenderRightPanel();
        LoadCurrentPath();
    }
    private void Main_SortOrder_Changed(object? sender, RoutedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        _avatarExplorer.SetItemsSortOrder((SortOrder)comboBox.SelectedIndex);
        ReloadCurrentWindow();
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
        var item = _avatarExplorer.GetItemByPath(itemPath);
        if (item == null) ShowDialog("エラー", "アイテムが見つかりませんでした");

        return item;
    }
    private async Task ContextMenu_OpenItemFolder(string itemPath)
    {
        var item = ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        await AvaloniaLauncherUtils.OpenFolder(this, ItemUtils.GetItemPath(_avatarExplorer.GetDataRootDirectory(), item.ItemPath));
    }
    private async Task ContextMenu_CopyBoothLink(string itemPath)
    {
        var item = ContextMenu_GetItemByPath(itemPath);
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
    private async Task ContextMenu_OpenBoothLink(string itemPath)
    {
        var item = ContextMenu_GetItemByPath(itemPath);
        if (item == null) return;

        await AvaloniaLauncherUtils.OpenLink(this, item.GetBoothLink());
    }
    private Task ContextMenu_ShowOtherItemsByAuthor(string itemPath)
    {
        var item = ContextMenu_GetItemByPath(itemPath);
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
        var item = ContextMenu_GetItemByPath(itemPath);
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

    //TODO: Add / Edit / SettingsでValueValidate関数を作る

    #region Add / Edit Item Menu
    private Item? _selectedItem = null;
    private readonly AddItemOverlayWindowValues _addItemWindowValues = new();

    private void AddItem_Click(object? sender, RoutedEventArgs e)
    {
        ShowAddItemWindow();
    }

    private void ShowEditItemWindow(Item item)
    {
        _selectedItem = item;
        _addItemWindowValues.FromItem(item);
        SetAddItemWindowValues(_addItemWindowValues);
        AddItemOverlay.IsVisible = true;
    }
    private void ShowAddItemWindow()
    {
        _selectedItem = null;
        _addItemWindowValues.Reset();
        SetAddItemWindowValues(_addItemWindowValues);
        AddItemOverlay.IsVisible = true;
    }
    
    private async void AddItemOverlay_GetBoothItemData_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemWindowValues == null || AddItemOverlay_BoothLinkTextBox == null || AddItemOverlay_BoothLinkTextBox.Text == null) return;
        string boothUrl = AddItemOverlay_BoothLinkTextBox.Text;

        ShowProgress(Localizer.Instance[LocalizationKey.Processing.Booth.Status.Fetching]);
        UpdateProgress(0);
        
        BoothItem? boothItem = await AvatarExplorerApp.GetBoothItem(boothUrl);
        HideProgress();

        if (boothItem == null)
        {
            ShowDialog(Localizer.Instance[LocalizationKey.Error.Default], Localizer.Instance[LocalizationKey.Error.BoothItemNotFound]);
            return;
        }

        _addItemWindowValues.BoothTitle = boothItem.Title;
        _addItemWindowValues.BoothAuthor = boothItem.Shop.Name;
        _addItemWindowValues.BoothAuthorId = boothItem.AuthorId;
        _addItemWindowValues.BoothId = boothItem.BoothId;
        _addItemWindowValues.BoothThumbnailUrl = boothItem.Thumbnails.Count > 0 ? boothItem.Thumbnails[0].Original : string.Empty;

        AddItemOverlay_ResetBoothItemDataButton.IsVisible = true;

        SetAddItemWindowValues(_addItemWindowValues);
    }
    private void AddItemOverlay_ResetBoothItemData_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemWindowValues == null) return;

        _addItemWindowValues.BoothTitle = string.Empty;
        _addItemWindowValues.BoothAuthor = string.Empty;
        _addItemWindowValues.BoothAuthorId = string.Empty;
        _addItemWindowValues.BoothId = -1;
        _addItemWindowValues.BoothThumbnailUrl = string.Empty;

        AddItemOverlay_ResetBoothItemDataButton.IsVisible = false;

        SetAddItemWindowValues(_addItemWindowValues);
    }
    
    private async void AddItemOverlay_AddFolder_Click(object? sender, RoutedEventArgs e)
    {
        var folders = await DialogUtils.OpenFolderDialog(this, "フォルダを選択してください", true);
        _addItemWindowValues.Folders.Clear();
        _addItemWindowValues.Folders.AddRange(folders.Select(i => i.TryGetLocalPath() ?? ""));
        // TODO: フォルダ追加時に右がとこかに、現在選択されているフォルダを [ファイル | フォルダ名] [削除]みたいにリストで表示して編集しやすくしたいよね
    }

    private async void AddItemOverlay_ConfirmButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_addItemWindowValues == null) return;

        var itemCreationContext = new ItemCreationContext();
        itemCreationContext.Folders.AddRange(_addItemWindowValues.Folders);
        itemCreationContext.MaterialFolder = _addItemWindowValues.MaterialFolder;
        itemCreationContext.Title = _addItemWindowValues.BoothTitle;
        itemCreationContext.Author = _addItemWindowValues.BoothAuthor;
        itemCreationContext.AuthorId = _addItemWindowValues.BoothAuthorId;
        itemCreationContext.ThumbnailUrl = _addItemWindowValues.BoothThumbnailUrl;
        itemCreationContext.BoothId = _addItemWindowValues.BoothId;
        itemCreationContext.ItemType = _addItemWindowValues.ItemType;
        itemCreationContext.SupportedAvatars.AddRange(_addItemWindowValues.SupportedAvatars);
        itemCreationContext.LocalizedCategoryName = Localizer.Instance[itemCreationContext.ItemType.GetLocalizationKey() ?? ""];

        if (_selectedItem == null)
        {
            await _avatarExplorer.AddItem(itemCreationContext);
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
        AddItemOverlay_BoothItemTitleTextBox.Text = addItemWindowValues.BoothTitle;
        AddItemOverlay_BoothItemAuthorTextBox.Text = addItemWindowValues.BoothAuthor;
        AddItemOverlay_ItemTypeComboBox.Items.Clear();

        foreach (var category in _avatarExplorer.GetCategories())
        {
            AddItemOverlay_ItemTypeComboBox.Items.Add(Localizer.Instance[((Category)category.Item).ToString()]); // TODO: CategoryをどうItemTypeに変換するかを考える。Tagを使っても良いかも
        }

        AddItemOverlay_ItemTypeComboBox.SelectedIndex = 0;
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
        var folder = await DialogUtils.OpenFolderDialog(this, "フォルダを選択してください", false);
        SettingsOverlay_ItemsFolderPathTextBox.Text = folder.Count > 0 ? (folder[0]?.TryGetLocalPath() ?? "") : "";
    }
    private async void SettingsOverlay_CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        SettingsOverlay.IsVisible = false;
    }

    private async void SettingsOverlay_ApplyButton_Click(object? sender, RoutedEventArgs e)
    {
        ApplySettingsValues();
    }
    private void SetUiValueFromCurrentSettings()
    {
        var runtimeSettings = _avatarExplorer.GetRuntimeSettings();
        var userUiPreferences = _userUiPreferences;

        SettingsOverlay_ItemsFolderPathTextBox.Text = runtimeSettings.DataRootDirectory;
        SettingsOverlay_RemoveBracketsCheckBox.IsChecked = runtimeSettings.RemoveBrackets;
        SettingsOverlay_RemoveOriginalCheckBox.IsChecked = runtimeSettings.RemoveOriginal;
        SettingsOverlay_ItemsPerPageTextBox.Text = userUiPreferences.ItemsPerPage.ToString();
        SettingsOverlay_ThemeComboBox.SelectedIndex = (int)userUiPreferences.Theme;
        SettingsOverlay_DefaultLanguageComboBox.SelectedIndex = userUiPreferences.DefaultLanguage;
        SettingsOverlay_DefaultSortOrderComboBox.SelectedIndex = (int)runtimeSettings.ItemSortOrder;
    }

    private void ApplySettingsValues()
    {
        _avatarExplorer.SetDataRootDirectory(SettingsOverlay_ItemsFolderPathTextBox.Text ?? "");
        _avatarExplorer.SetRemoveBrackets(SettingsOverlay_RemoveBracketsCheckBox.IsChecked ?? false);
        _avatarExplorer.SetRemoveBrackets(SettingsOverlay_RemoveBracketsCheckBox.IsChecked ?? false);
        _userUiPreferences.ItemsPerPage = int.TryParse(SettingsOverlay_ItemsPerPageTextBox.Text, out var result) ? result : 30;
        _userUiPreferences.Theme = (Theme)SettingsOverlay_ThemeComboBox.SelectedIndex;
        _userUiPreferences.DefaultLanguage = SettingsOverlay_DefaultLanguageComboBox.SelectedIndex;
        _avatarExplorer.SetItemsSortOrder((SortOrder)SettingsOverlay_DefaultSortOrderComboBox.SelectedIndex);

        SetUiValueFromCurrentSettings();
        ApplyPreferenceSettingsToUi();

        ReloadCurrentWindow();

        _avatarExplorer.SaveRuntimeSettings();
        _userUiPreferences.Save();
    }

    private void ApplyPreferenceSettingsToUi()
    {
        var currentApplication = Application.Current;
        if (currentApplication != null)
        {
            if (_userUiPreferences.Theme == Models.Theme.Auto) currentApplication.RequestedThemeVariant = ThemeVariant.Default;
            else if (_userUiPreferences.Theme == Models.Theme.Dark) currentApplication.RequestedThemeVariant = ThemeVariant.Dark;
            else if (_userUiPreferences.Theme == Models.Theme.Light) currentApplication.RequestedThemeVariant = ThemeVariant.Light;
        }
    }
    #endregion

    private void ReloadCurrentWindow()
    {
        RenderLeftPanel();
        RenderRightPanel(); // TODO: 検索中だと検索画面が無視されてしまうから、検索中フラグなどでここはチェックするべき
    }
}
