using AvatarExplorer.Core.Data.Paths;
using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Models.Booth;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

public partial class AvatarExplorerApp
{
    public static readonly string CurrentVersion = "v2.0.0";
    
    private readonly List<Item> _items = new();
    private readonly List<CommonAvatar> _commonAvatars = new();

    private readonly SelectionState _selectionState = new();
    private readonly Dictionary<ItemTagState, Func<SelectionNode, IReadOnlyList<ItemCountInfo>>> _stateHandlers;
    private readonly Dictionary<ActionKey, Func<string, Task>> _contextMenuHandlers;
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

        _contextMenuHandlers = new()
        {
            { ActionKey.FetchThumbnail, ItemButton_ContextMenu_FetchThumbnail }
        };
    }

    #region Database
    public void LoadItemDatabase(string? path = null)
    {
        string loadPath = path ?? SystemPath.ItemDatabasePath;
        List<Item> database = ItemDatabaseService.Load(loadPath);

        _items.Clear();
        _items.AddRange(database);

        UpdateSearchIndex();
    }

    public void LoadCommonAvatarDatabase(string? path = null)
    {
        string loadPath = path ?? SystemPath.CommonAvatarDatabasePath;
        List<CommonAvatar> database = CommonAvatarDatabaseService.Load(loadPath);

        _commonAvatars.Clear();
        _commonAvatars.AddRange(database);
    }

    public void SaveItemDatabase() => ItemDatabaseService.Save(_items);
    public void SaveCommonAvatarDatabase() => CommonAvatarDatabaseService.Save(_commonAvatars);
    #endregion

    #region Runtime Settings
    public void LoadRuntimeSettings()
    {
        RuntimeSettings runtimeSettings = RuntimeSettingsService.Load(SystemPath.RuntimeSettingsFilePath);
        SetRuntimeSettingsInternal(runtimeSettings);
    }
    public void LoadRuntimeSettings(string path)
    {
        RuntimeSettings runtimeSettings = RuntimeSettingsService.Load(path);
        SetRuntimeSettingsInternal(runtimeSettings);
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
    public void UpdateSearchIndex(Item item)
    {
        var avatarNameMaps = ItemUtils.GetAvatarNameMaps(_items);
        item.BuildSearchIndex(avatarNameMaps);
    }
    #endregion

    #region Select API
    public void Select(ItemTagState state, string key) => _selectionState.Push(state, key);
    public void SelectUndo() => _selectionState.Pop();
    public void SelectClear() => _selectionState.Clear();
    #endregion

    #region Get API
    public IReadOnlyList<ItemCountInfo> GetAvatars(bool includeCommonAvatar = false) => ItemAvatarAggregator.Aggregate(_items, _commonAvatars, _runtimeSettings, includeCommonAvatar);
    public IReadOnlyList<ItemCountInfo> GetAuthors() => ItemAuthorAggregator.Aggregate(_items);
    public IReadOnlyList<ItemCountInfo> GetCategories() => ItemCategoryAggregator.Aggregate(_items);

    public IReadOnlyList<Item> GetAllItems() => _items;
    public Item? GetItemByPath(string itemPath) => _items.FirstOrDefault(i => i.ItemPath == itemPath);
    public IReadOnlyList<ItemCountInfo> GetItemsForCurrentState()
    {
        SelectionNode? current = _selectionState.Current;

        if (current == null)
            return new List<ItemCountInfo>();

        if (_stateHandlers.TryGetValue(current.State, out var handler))
            return handler(current);

        return new List<ItemCountInfo>();
    }

    public IReadOnlyList<CommonAvatar> GetCommonAvatars() => _commonAvatars;
    
    #region Current State Internal Handler
    private IReadOnlyList<ItemCountInfo> HandleRootAvatar(SelectionNode selectionNode) => ItemCategoryAggregator.Aggregate(_items.Where(i => AvatarStatusResolver.Resolve(selectionNode.Key, i, _commonAvatars).IsSupportedOrCommon));
    private IReadOnlyList<ItemCountInfo> HandleRootAuthor(SelectionNode selectionNode) => ItemCategoryAggregator.Aggregate(_items.Where(i => i.Author == selectionNode.Key));
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

    private static IReadOnlyList<ItemCountInfo> GetCategoryItemsFromPathInternal(string itemPath)
    {
        List<ItemCountInfo> categoryItems = new();

        FileCategory[] extensionFilters = Enum.GetValues<FileCategory>();
        foreach (FileCategory filter in extensionFilters)
        {
            string[]? filters = filter.GetExtensionFilters();
            if (filters == null) continue;

            string[]? fileNameFilters = filter.GetFileNameFilters();

            FileCategoryItem categoryItem = new(filter);

            foreach (string file in FileSystemService.EnumerateFiles(itemPath))
            {
                string fileExtension = Path.GetExtension(file);
                if (!filters.Contains(fileExtension)) continue;
                
                if (fileNameFilters != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (!fileNameFilters.Any(f => fileName.Contains(f, StringComparison.CurrentCultureIgnoreCase))) continue;
                }

                categoryItem.FilePaths.Add(file);
            }

            if (categoryItem.FilePaths.Count > 0)
            {
                categoryItems.Add(new ItemCountInfo(categoryItem, categoryItem.FilePaths.Count));
            }
        }

        return categoryItems;
    }
    private static IReadOnlyList<ItemCountInfo> GetFilesFromPathInternal(string itemPath, string category)
    {
        List<ItemCountInfo> categoryItems = new();

        FileCategory fileCategory = Enum.GetValues<FileCategory>().FirstOrDefault(i => i.GetLocalizationKey() == category);
        if (fileCategory == default) return categoryItems;

        string[]? filters = fileCategory.GetExtensionFilters();
        if (filters == null) return categoryItems;

        string[]? fileNameFilters = fileCategory.GetFileNameFilters();

        foreach (string file in FileSystemService.EnumerateFiles(itemPath))
        {
            string fileExtension = Path.GetExtension(file);
            if (!filters.Contains(fileExtension)) continue;

            if (fileNameFilters != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                if (!fileNameFilters.Any(f => fileName.Contains(f, StringComparison.CurrentCultureIgnoreCase))) continue;
            }

            categoryItems.Add(new ItemCountInfo(new ItemFile(Path.GetFullPath(file)), 0));
        }

        return categoryItems;
    }

    public RuntimeSettings GetRuntimeSettings() => _runtimeSettings;
    #endregion

    #region Set API
    public bool SetDataRootDirectory(string path) => RuntimeSettingsService.TrySetDataRootDirectory(_runtimeSettings, path);
    public void SetItemsSortOrder(SortOrder sortOrder) => _runtimeSettings.SetSortOrder(sortOrder);
    public void SetRemoveOriginal(bool value) => _runtimeSettings.SetRemoveOriginal(value);
    public void SetRemoveBrackets(bool value) => _runtimeSettings.SetRemoveBrackets(value);
    #endregion

    #region Add API
    public CommonAvatar? AddCommonAvatar(string groupName, IEnumerable<string> avatars)
    {
        if (_commonAvatars.Any(i => i.GroupName == groupName)) return null;

        CommonAvatar commonAvatar = new()
        {
            GroupName = groupName
        };
        commonAvatar.UpdateAvatars(avatars);

        _commonAvatars.Add(commonAvatar);

        return commonAvatar;
    }
    public async Task<(Item? newItem, List<string> processingFailedPaths)> AddItem(ItemCreationContext itemCreationContext)
    {
        var addItemResult = await ItemCreator.FromItemCreationContext(itemCreationContext, _runtimeSettings);
        if (addItemResult.newItem == null) return addItemResult;

        string currentUnixTime = DatetimeUtils.GetCurrentUnixTime();
        addItemResult.newItem.CreatedDate = currentUnixTime;
        addItemResult.newItem.UpdatedDate = currentUnixTime;
        
        _items.Add(addItemResult.newItem);

        SaveItemDatabase();
        UpdateSearchIndex(addItemResult.newItem);

        return addItemResult;
    }

    public async Task<Item> EditItem(Item item, ItemCreationContext itemCreationContext)
    {
        item.SetValuesFromCreationContext(itemCreationContext);
        if (itemCreationContext.Folders.Count > 1) await AddFolders(item, itemCreationContext.Folders.Skip(1).ToArray()); // １個より多い場合は、追加のアイテムとしてインポートしてあげる

        item.UpdatedDate = DatetimeUtils.GetCurrentUnixTime();

        SaveItemDatabase();
        UpdateSearchIndex(item);

        return item;
    }
    #endregion

    #region Rename CommonAvatar Group Name
    public void RenameCommonAvatarGroupName(string previousInternalGroupPath, string newInternalGroupPath)
    {
        foreach (Item item in _items.Where(i => i.SupportedAvatarsView.Contains(previousInternalGroupPath)))
        {
            IEnumerable<string> newSupportedAvatars = item.SupportedAvatarsView
                .Select(i => i == previousInternalGroupPath ? newInternalGroupPath : i);
            item.UpdateSupportedAvatars(newSupportedAvatars);
        }
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
    public async Task<IReadOnlyList<string>> AddFolders(Item item, string[] folders)
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
        return await BoothService.GetItem(boothId);
    }
    #endregion

    #region File API
    public static async Task<string> ModifyUnityPackageFilePath(string filePath, string itemCategoryName = "", IProgress<(string, int)>? progress = null)　=> await FileSystemService.ModifyUnityPackageFilePathsAsync([filePath], [itemCategoryName], progress);
    public static async Task<string> ModifyUnityPackageFilePaths(string[] filePaths, string[] itemCategoryNames, IProgress<(string, int)>? progress = null) => await FileSystemService.ModifyUnityPackageFilePathsAsync(filePaths, itemCategoryNames, progress);
    #endregion

    #region Remove API
    public bool RemoveItem(string itemPath, bool removeFromSupportedAndImplemented = false)
    {
        int removed = _items.RemoveAll(i => i.ItemPath == itemPath);
        if (removeFromSupportedAndImplemented)
        {
            _items.ForEach(i =>
            {
                i.UpdateSupportedAvatars(i.SupportedAvatarsView.Where(a => a != itemPath));
                i.UpdateImplementedAvatars(i.ImplementedAvatarsView.Where(a => a != itemPath));
            });
        }

        return removed > 0;
    }

    public bool RemoveCommonAvatar(string commonAvatarName) => _commonAvatars.RemoveAll(i => i.GroupName == commonAvatarName) > 0;
    #endregion

    #region Search API
    public IReadOnlyList<Item> SearchItems(SearchFilter searchFilter) => SearchService.ExecuteSearch(_items, _commonAvatars, _runtimeSettings, searchFilter);
    #endregion

    #region Save API
    public void SaveRuntimeSettings() => RuntimeSettingsService.Save(_runtimeSettings);
    #endregion

    #region Data Importer API
    public async Task ImportFromV1(string dataFolderPath, Dictionary<ItemType, string> localizedItemTypesMapping, IProgress<(string, int)>? progress = null)
    {
        (List<Item>, List<CommonAvatar>) result = await DataImporter.FromV1(dataFolderPath, _runtimeSettings, localizedItemTypesMapping, progress);
        
        _items.AddRange(result.Item1);
        _commonAvatars.AddRange(result.Item2);

        SaveItemDatabase();
        SaveCommonAvatarDatabase();
    }
    public async Task ImportFromKonoAsset(string dataFolderPath, Dictionary<ItemType, string> localizedItemTypesMapping, IProgress<(string, int)>? progress = null)
    {
        List<Item> items = await DataImporter.FromKonoAsset(dataFolderPath, _runtimeSettings, localizedItemTypesMapping, progress);
        _items.AddRange(items);

        SaveItemDatabase();
        SaveCommonAvatarDatabase();
    }
    #endregion

    #region Data Exporter API
    public async Task ExportToCsv(string filePath, Dictionary<ItemType, string> localizedItemTypesMapping, bool includeImplementedToSupported) => await DataExporter.ToCsv(_items, _commonAvatars, localizedItemTypesMapping, filePath, includeImplementedToSupported);
    #endregion
    
    #region Clear API
    public static void ClearTemp() => FileSystemService.DeleteDirectory(SystemPath.TempFolderPath);
    #endregion
    
    #region Ececute Context Menu Command
    public async Task ExecuteContextMenuItemCommand(ContextMenuAction contextMenuAction)
    {
        if (_contextMenuHandlers.TryGetValue(contextMenuAction.ActionKey, out var handler))
            await handler(contextMenuAction.Tag);
    }
    private async Task ItemButton_ContextMenu_FetchThumbnail(string itemPath)
    {
        Item? item = GetItemByPath(itemPath);
        if (item == null || item.BoothId == -1) return;

        BoothItem? boothItem = await BoothService.GetItem(item.BoothId.ToString());
        if (boothItem == null) return;

        string itemThumbnailFileName = item.BoothId + ".png";
        await ImageDownloader.Download(boothItem.Thumbnails.Count > 0 ? boothItem.Thumbnails[0].Original : string.Empty, Path.Combine(SystemPath.ItemThumbnailsPath, itemThumbnailFileName), true);
        item.ThumbnmailFileName = itemThumbnailFileName;
    }
    #endregion
}
