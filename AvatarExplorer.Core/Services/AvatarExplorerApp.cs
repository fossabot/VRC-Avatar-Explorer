using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.Booth;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

public partial class AvatarExplorerApp
{
    private readonly List<Item> _items = new();
    private readonly List<CommonAvatar> _commonAvatars = new();

    private readonly SelectionState _selectionState = new();
    private readonly Dictionary<ItemTagState, Func<SelectionNode, IReadOnlyList<ItemCountInfo>>> _stateHandlers;
    private readonly RuntimeSettings _runtimeSettings = new();

    public AvatarExplorerApp()
    {
        _stateHandlers = new()
        {
            { ItemTagState.SearchItem, HandleRootSelectedItem },
            { ItemTagState.RootAvatar, HandleRootAvatar },
            { ItemTagState.RootAuthor, HandleRootAuthor },
            { ItemTagState.RootCategory, HandleRootCategory },
            { ItemTagState.RootSelectedCategory, HandleRootSelectedCategory },
            { ItemTagState.RootSelectedItem, HandleRootSelectedItem },
            { ItemTagState.ItemFileCategory, HandleItemFileCategory }
        };
    }

    #region Database
    public void LoadItemDatabase(bool fromV1 = false)
    {
        List<Item> database = fromV1 ? ItemDatabaseService.LoadItemsDataFromV1(SystemPath.ItemDatabasePath) : ItemDatabaseService.LoadItemsData(SystemPath.ItemDatabasePath);

        _items.Clear();
        _items.AddRange(database);
    }

    public void LoadCommonAvatarDatabase(bool fromV1 = false)
    {
        List<CommonAvatar> database = fromV1 ? CommonAvatarDatabaseService.LoadCommonAvatarsDataFromV1(SystemPath.CommonAvatarDatabasePath) :  CommonAvatarDatabaseService.LoadCommonAvatarsData(SystemPath.CommonAvatarDatabasePath);

        _commonAvatars.Clear();
        _commonAvatars.AddRange(database);
    }

    public void LoadItemDatabase(string path, bool fromV1 = false)
    {
        List<Item> database = fromV1 ? ItemDatabaseService.LoadItemsDataFromV1(path) : ItemDatabaseService.LoadItemsData(path);

        _items.Clear();
        _items.AddRange(database);
    }

    public void LoadCommonAvatarDatabase(string path, bool fromV1 = false)
    {
        List<CommonAvatar> database = fromV1 ? CommonAvatarDatabaseService.LoadCommonAvatarsDataFromV1(path) :  CommonAvatarDatabaseService.LoadCommonAvatarsData(path);

        _commonAvatars.Clear();
        _commonAvatars.AddRange(database);
    }
    #endregion

    #region Runtime Settings
    public void LoadRuntimeSettings()
    {
        RuntimeSettings runtimeSettings = RuntimeSettingsService.LoadRuntimeSettings(SystemPath.RuntimeSettingsFilePath);
        SetRuntimeSettingsInternal(runtimeSettings);
        RuntimeSettingsService.Save(_runtimeSettings);
    }
    public void LoadRuntimeSettings(string path)
    {
        RuntimeSettings runtimeSettings = RuntimeSettingsService.LoadRuntimeSettings(path);
        SetRuntimeSettingsInternal(runtimeSettings);
        RuntimeSettingsService.Save(_runtimeSettings);
    }
    private void SetRuntimeSettingsInternal(RuntimeSettings runtimeSettings)
    {
        RuntimeSettingsService.TrySetDataRootDirectory(_runtimeSettings, runtimeSettings.DataRootDirectory);
        _runtimeSettings.SetSortOrder(runtimeSettings.ItemSortOrder);
        _runtimeSettings.SetRemoveOriginal(runtimeSettings.RemoveOriginal);
        _runtimeSettings.SetRemoveBrackets(runtimeSettings.RemoveBrackets);
    }
    #endregion

    #region Update API
    public void UpdateSearchIndex()
    {
        var avatarNameMaps = ItemUtils.GetAvatarNameMaps(_items);
        _items.ForEach(i => i.BuildSearchIndex(avatarNameMaps));
    }
    #endregion

    #region Select API
    public void Select(ItemTagState type, string key)
    {
        _selectionState.Push(type, key);
    }

    public void SelectUndo()
    {
        _selectionState.Pop();
    }

    public void SelectClear()
    {
        _selectionState.Clear();
    }
    #endregion

    #region Get API
    public IReadOnlyList<ItemCountInfo> GetAuthors()
    {
        return _items
            .GroupBy(item => new { item.Author, item.AuthorThumbnmailFileName })
            .Select(g => new ItemCountInfo(
                new Author
                {
                    Name = g.Key.Author,
                    AuthorThumbnailFileName = g.Key.AuthorThumbnmailFileName
                },
                g.Count()
            ))
            .ToList();
    }
    public IReadOnlyList<ItemCountInfo> GetAvatars()
    {
        return _items
            .Where(i => i.Type == ItemType.Avatar)
            .GetSortedItems(_runtimeSettings)
            .Select(i => new ItemCountInfo(i, 0))
            .ToList();
    }
    public IReadOnlyList<ItemCountInfo> GetCategories()
    {
        return ItemCategoryAggregator.Aggregate(_items).ToList();
    }
    public IReadOnlyList<Item> GetAllItems()
    {
        return _items;
    }
    public IReadOnlyList<CommonAvatar> GetCommonAvatars()
    {
        return _commonAvatars;
    }
    public Item? GetItemByPath(string itemPath)
    {
        return _items.FirstOrDefault(i => i.ItemPath == itemPath);
    }

    public IReadOnlyList<ItemCountInfo> GetItemsForCurrentState()
    {
        SelectionNode? current = _selectionState.Current;

        if (current == null)
            return new List<ItemCountInfo>();

        if (_stateHandlers.TryGetValue(current.State, out var handler))
            return handler(current);

        return new List<ItemCountInfo>();
    }
    #region Current State Internal Handler
    private IReadOnlyList<ItemCountInfo> HandleRootAvatar(SelectionNode selectionNode)
    {
        return GetCategoriesFromItemsInternal(_items.Where(i => AvatarStatusResolver.Resolve(selectionNode.Key, i, _commonAvatars).IsSupportedOrCommon));
    }
    private IReadOnlyList<ItemCountInfo> HandleRootAuthor(SelectionNode selectionNode)
    {
        return GetCategoriesFromItemsInternal(_items.Where(i => i.Author == selectionNode.Key));
    }
    private IReadOnlyList<ItemCountInfo> HandleRootCategory(SelectionNode selectionNode)
    {
        return _items
            .Where(i => CategoryUtils.IsCategoryMatch(i, selectionNode.Key))
            .GetSortedItems(_runtimeSettings)
            .Select(i => new ItemCountInfo(i, 0))
            .ToList();
    }
    private IReadOnlyList<ItemCountInfo> HandleRootSelectedCategory(SelectionNode selectionNode)
    {
        SelectionNode? rootSelectionNode = _selectionState.Root;
        if (rootSelectionNode == null) return new List<ItemCountInfo>();

        if (rootSelectionNode.State == ItemTagState.RootAvatar)
        {
            List<ItemCountInfo> filteredResult = new();

            foreach (Item item in _items)
            {
                if (!CategoryUtils.IsCategoryMatch(item, selectionNode.Key)) continue;

                AvatarStatus avatarStatus = AvatarStatusResolver.Resolve(rootSelectionNode.Key, item, _commonAvatars);
                if (!avatarStatus.IsSupportedOrCommon) continue;
                
                filteredResult.Add(new ItemCountInfo(item, 0, avatarStatus.IsOnlyCommon ? avatarStatus.CommonAvatarName : string.Empty));
            }

            return filteredResult
                .GetSortedItemsFromCountInfo(_runtimeSettings)
                .ToList();
        }
        else if (rootSelectionNode.State == ItemTagState.RootAuthor)
        {
            return _items
                .Where(i => CategoryUtils.IsCategoryMatch(i, selectionNode.Key) && i.Author == rootSelectionNode.Key)
                .GetSortedItems(_runtimeSettings)
                .Select(i => new ItemCountInfo(i, 0))
                .ToList();
        }

        return new List<ItemCountInfo>();
    }
    private IReadOnlyList<ItemCountInfo> HandleRootSelectedItem(SelectionNode selectionNode)
    {
        return GetCategoryItemsFromPathInternal(ItemUtils.GetItemPath(_runtimeSettings.DataRootDirectory, selectionNode.Key));
    }
    private IReadOnlyList<ItemCountInfo> HandleItemFileCategory(SelectionNode selectionNode)
    {
        SelectionNode? fileSelectionNode = _selectionState.Search(ItemTagState.RootSelectedItem | ItemTagState.SearchItem);
        if (fileSelectionNode == null) return new List<ItemCountInfo>();

        return GetFilesFromPathInternal(ItemUtils.GetItemPath(_runtimeSettings.DataRootDirectory, fileSelectionNode.Key), selectionNode.Key);
    }
    #endregion

    public IEnumerable<SelectionNode> GetCurrentPaths() => _selectionState.GetCurrentPath();
    public SelectionNode? GetCurrentPathState() => _selectionState.Current;

    public Item? GetSelectedItem()
    {
        SelectionNode? itemSelectionNode = _selectionState.Search(ItemTagState.RootSelectedItem | ItemTagState.SearchItem);
        if (itemSelectionNode == null) return null;

        return _items.FirstOrDefault(i => i.ItemPath == itemSelectionNode.Key);
    }

    private static List<ItemCountInfo> GetCategoryItemsFromPathInternal(string itemPath)
    {
        List<ItemCountInfo> categoryItems = new();

        FileCategory[] extensionFilters = Enum.GetValues<FileCategory>();
        foreach (FileCategory filter in extensionFilters)
        {
            string[]? filters = filter.GetExtensionFilters();
            if (filters == null) continue;

            FileCategoryItem categoryItem = new()
            {
                FileCategory = filter
            };

            foreach (string file in FileSystemService.EnumerateFiles(itemPath))
            {
                string fileExtension = Path.GetExtension(file);
                if (filters.Contains(fileExtension))
                {
                    categoryItem.FilePaths.Add(file);
                }
            }

            if (categoryItem.FilePaths.Count > 0)
            {
                categoryItems.Add(new ItemCountInfo(categoryItem, categoryItem.FilePaths.Count));
            }
        }

        return categoryItems;
    }
    private static List<ItemCountInfo> GetFilesFromPathInternal(string itemPath, string category)
    {
        List<ItemCountInfo> categoryItems = new();

        FileCategory fileCategory = Enum.GetValues<FileCategory>().FirstOrDefault(i => i.GetLocalizationKey() == category);
        if (fileCategory == default) return new();

        string[]? filters = fileCategory.GetExtensionFilters();
        if (filters == null) return new();

        foreach (string file in FileSystemService.EnumerateFiles(itemPath))
        {
            string fileExtension = Path.GetExtension(file);
            if (filters.Contains(fileExtension))
            {
                categoryItems.Add(new ItemCountInfo(new ItemFile(Path.GetFullPath(file)), 0));
            }
        }

        return categoryItems;
    }
    private static List<ItemCountInfo> GetCategoriesFromItemsInternal(IEnumerable<Item> items)
    {
        IEnumerable<ItemCountInfo> itemCategories = items
            .Where(i => i.Type != ItemType.Custom)
            .Select(i => i.Type)
            .Distinct()
            .Select(i => new ItemCountInfo(new Category(i), items.Count(item => item.Type == i)));

        IEnumerable<ItemCountInfo> itemCustomCategories = items
            .Where(i => i.Type == ItemType.Custom)
            .Select(i => i.CustomCategory)
            .Distinct()
            .Select(i => new ItemCountInfo(new Category(i), items.Count(item => item.CustomCategory == i)));

        return itemCategories.Concat(itemCustomCategories).ToList();
    }

    public RuntimeSettings GetRuntimeSettings()
    {
        return _runtimeSettings;
    }
    #endregion

    #region Set API
    public bool SetDataRootDirectory(string path)
    {
        // このパスをアイテムフォルダの親フォルダとして見るようになる（アイテムの相対パスの親がこのフォルダであると設定する）
        return RuntimeSettingsService.TrySetDataRootDirectory(_runtimeSettings, path);
    }
    public void SetItemsSortOrder(SortOrder sortOrder)
    {
        _runtimeSettings.SetSortOrder(sortOrder);
    }
    public void SetRemoveOriginal(bool value)
    {
        _runtimeSettings.SetRemoveOriginal(value);
    }
    public void SetRemoveBrackets(bool value)
    {
        _runtimeSettings.SetRemoveBrackets(value);
    }
    #endregion

    #region Add API
    public async Task<(Item? newItem, List<string> processingFailedPaths)> AddItem(ItemCreationContext itemCreationContext)
    {
        var addItemResult = await ItemCreator.FromItemCreationContext(itemCreationContext, _runtimeSettings);
        if (addItemResult.newItem != null) _items.Add(addItemResult.newItem); // アイテムの追加に失敗していなければここで追加してあげる

        return addItemResult;
    }

    public Item EditItem(Item item, ItemCreationContext itemCreationContext)
    {
        return item.SetValuesFromCreationContext(itemCreationContext);
    }
    #endregion

    #region Update API
    public async Task<Item?> UpdateItemThumbnail(Item item, string imageFilePath)
    {
        string? newFileFullPath = await FileSystemService.CopyFile(imageFilePath, Path.Combine(SystemPath.ItemThumbnailsPath, Path.GetFileName(imageFilePath)), true);
        if (newFileFullPath == null) return null;

        item.ThumbnmailFileName = Path.GetFileName(newFileFullPath);

        return item;
    }
    public async Task<Item?> UpdateAuthorThumbnail(Item item, string imageFilePath)
    {
        string? newFileFullPath = await FileSystemService.CopyFile(imageFilePath, Path.Combine(SystemPath.AuthorThumbnailsPath, Path.GetFileName(imageFilePath)), true);
        if (newFileFullPath == null) return null;

        item.ThumbnmailFileName = Path.GetFileName(newFileFullPath);

        return item;
    }
    #endregion

    #region Add API
    public async Task<List<string>> AddFolders(Item item, string[] folders)
    {
        List<string> processingFailedPaths = await FileSystemService.ExtractItemFolders(ItemUtils.GetItemPath(_runtimeSettings.DataRootDirectory, item.ItemPath), folders);
        return processingFailedPaths;
    }
    #endregion

    #region Booth API
    private DateTime _lastBoothApiGetTime;
    public bool IsApiCooldownNow => _lastBoothApiGetTime.AddSeconds(5) > DateTime.Now;
    public async Task<BoothItem?> GetBoothItem(string boothUrl)
    {
        if (string.IsNullOrEmpty(boothUrl)) return null;
        if (IsApiCooldownNow) return null;

        string boothId = boothUrl.Split('/')[^1];

        _lastBoothApiGetTime = DateTime.Now; // 時間を更新する
        return await BoothService.GetBoothItemAsync(boothId);
    }
    #endregion

    #region File API
    public static async Task ModifyUnityPackageFilePath(string itemPath, string itemCategoryName = "", IProgress<(string, int, string)>? progress = null)
    {
        await FileSystemService.ModifyUnityPackageFilePathAsync(itemPath, itemCategoryName, progress);
    }
    #endregion

    #region Remove API
    public bool RemoveItem(string itemPath, bool removeFromSupportedAndImplemented = false)
    {
        int removed = _items.RemoveAll(i => i.ItemPath == itemPath);
        if (removeFromSupportedAndImplemented)
        {
            _items.ForEach(i =>
            {
                i.SupportedAvatars.RemoveAll(a => a == itemPath);
                i.ImplementedAvatars.RemoveAll(a => a == itemPath);
            });
        }

        return removed > 0;
    }

    public bool RemoveCommonAvatar(string commonAvatarName)
    {
        int removed = _commonAvatars.RemoveAll(i => i.GroupName == commonAvatarName);
        return removed > 0;
    }
    #endregion

    #region Search API
    public IReadOnlyList<Item> SearchItems(SearchFilter filter)
    {
        var avatarNameMaps = ItemUtils.GetAvatarNameMaps(_items);

        return _items
            .Where(i => SearchService.Matches(filter, avatarNameMaps, _commonAvatars, i, _runtimeSettings.DataRootDirectory))
            .OrderByDescending(i => SearchUtils.GetScore(i, filter.SearchWords))
            .ToList();
    }
    #endregion

    #region Save API
    public void SaveRuntimeSettings()
    {
        RuntimeSettingsService.Save(_runtimeSettings);
    }
    #endregion

    #region Data Importer API
    public async Task ImportFromV1(string dataFolderPath, Dictionary<ItemType, string> localizedItemTypesMapping, IProgress<(string, int, string)>? progress = null)
    {
        (List<Item>, List<CommonAvatar>) result = await DataImporter.FromV1(dataFolderPath, _runtimeSettings, localizedItemTypesMapping, progress);
        
        _items.Clear();
        _items.AddRange(result.Item1);

        _commonAvatars.Clear();
        _commonAvatars.AddRange(result.Item2);
    }

    public Task ImportFromKonoAsset(string dataFolderPath, Dictionary<ItemType, string> localizedItemTypesMapping, IProgress<(string, int, string)>? progress = null)
    {
        // TODO: KonoAsset Importerを作る。IKonoAssetItemみたいなInterfaceで全部読み込んでしまうのが良さそ
        throw new NotImplementedException();
    }

    #endregion

    #region Data Exporter API
    public async Task ExportToCsv(string filePath, Dictionary<ItemType, string> localizedItemTypesMapping, bool includeImplementedToSupported)
    {
        await DataExporter.ExportToCsv(_items, _commonAvatars, localizedItemTypesMapping, filePath, includeImplementedToSupported);
    }
    #endregion
    
    #region Clear API
    public static void ClearTemp()
    {
        try
        {
            if (!Directory.Exists(SystemPath.TempFolderPath)) return;
            Directory.Delete(SystemPath.TempFolderPath, true);
        }
        catch
        {
            // Ignored
        }
    }
    #endregion
    
    #region Ececute Context Menu Command
    public async Task ExecuteContextMenuItemCommand(ContextMenuAction contextMenuAction)
    {
        ActionKey actionKey = contextMenuAction.ActionKey;

        switch (actionKey)
        {
            case ActionKey.FetchThumbnail: throw new NotImplementedException();
        }
    }
    #endregion
}
