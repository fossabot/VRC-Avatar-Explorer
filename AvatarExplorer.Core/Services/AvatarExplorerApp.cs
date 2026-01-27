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
    private readonly Dictionary<string, string> _itemSearchIndexDictionary = new();

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

        ErrorManager.Instance.OnErrorOccured += ErrorLogWriter.Instance.Write;
        ErrorManager.Instance.OnInternalErrorOccured += ErrorLogWriter.Instance.InternalWrite;
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

    public void ResetItemDatabase()
    {
        _items.Clear();
        SaveItemDatabase();
    }
    public void ResetCommonAvatarDatabase()
    {
        _commonAvatars.Clear();
        SaveCommonAvatarDatabase();
    }
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
        _runtimeSettings.SetDataRootDirectory(runtimeSettings.DataRootDirectory);
        _runtimeSettings.SetAutoBackupRootDirectory(runtimeSettings.AutoBackupRootDirectory);
        _runtimeSettings.SetSortOrder(runtimeSettings.ItemSortOrder);
        _runtimeSettings.SetRemoveOriginal(runtimeSettings.RemoveOriginal);
        _runtimeSettings.SetRemoveBrackets(runtimeSettings.RemoveBrackets);
        _runtimeSettings.SetAutoBackupInterval(runtimeSettings.AutoBackupInterval);
    }
    #endregion

    #region Update API
    public void UpdateSearchIndex()
    {
        Dictionary<string, string> avatarTitleMaps = ItemUtils.GetItemTitleMaps(_items.Where(i => i.Type == ItemType.Avatar));
        _items.ForEach(item => _itemSearchIndexDictionary[item.Id] = SearchService.BuildItemSearchIndex(item, avatarTitleMaps, _commonAvatars));
    }
    public void UpdateSearchIndex(string itemId)
    {
        Item? item = GetItemById(itemId);
        if (item == null) return;

        Dictionary<string, string> avatarNameMaps = ItemUtils.GetItemTitleMaps(_items.Where(i => i.Type == ItemType.Avatar));
        _itemSearchIndexDictionary[item.Id] = SearchService.BuildItemSearchIndex(item, avatarNameMaps, _commonAvatars);
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
    public IReadOnlyList<ItemCountInfo> GetCategories(bool includeEmptyCategory = false) => ItemCategoryAggregator.Aggregate(_items, includeEmptyCategory);

    public IReadOnlyList<Item> GetAllItems() => _items;
    public Item? GetItemById(string? itemId)
    {
        if (itemId == null) return null;

        Item? item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item == null) ErrorManager.Instance.PostInternalError(string.Format("The item with the specified ID '{0}' was not found.", itemId));

        return item;
    }
    public IReadOnlyList<ItemCountInfo> GetItemsForCurrentState()
    {
        SelectionNode? current = _selectionState.Current;
        if (current == null) return Array.Empty<ItemCountInfo>();

        if (_stateHandlers.TryGetValue(current.State, out var handler))
            return handler(current);

        return Array.Empty<ItemCountInfo>();
    }

    public IReadOnlyList<CommonAvatar> GetCommonAvatars() => _commonAvatars;
    public CommonAvatar? GetCommonAvatarById(string? groupId)
    {
        if (groupId == null) return null;

        CommonAvatar? commonAvatar = _commonAvatars.FirstOrDefault(i => i.Id == groupId);
        if (commonAvatar == null) ErrorManager.Instance.PostInternalError(string.Format("The common avatar group with the specified ID '{0}' was not found.", groupId));
        
        return commonAvatar;
    }
    
    #region Current State Internal Handler
    private IReadOnlyList<ItemCountInfo> HandleRootAvatar(SelectionNode selectionNode) => ItemCategoryAggregator.Aggregate(_items.Where(i => AvatarStatusResolver.Resolve(i, selectionNode.Key, _commonAvatars).IsSupportedOrCommon));
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

                AvatarStatus avatarStatus = AvatarStatusResolver.Resolve(item, rootSelectionNode.Key, _commonAvatars);
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
        Item? item = GetItemById(selectionNode.Key);
        if (item == null) return new List<ItemCountInfo>();

        return GetCategoryItemsFromPathInternal(ItemUtils.GetItemPath(_runtimeSettings.DataRootDirectory, item.ItemPath));
    }
    private IReadOnlyList<ItemCountInfo> HandleItemFileCategory(SelectionNode selectionNode)
    {
        SelectionNode? fileSelectionNode = _selectionState.Search(ItemTagState.RootSelectedItem | ItemTagState.SearchItem);
        if (fileSelectionNode == null) return new List<ItemCountInfo>();

        Item? item = GetItemById(fileSelectionNode.Key);
        if (item == null) return new List<ItemCountInfo>();

        return GetFilesFromPathInternal(ItemUtils.GetItemPath(_runtimeSettings.DataRootDirectory, item.ItemPath), selectionNode.Key);
    }
    #endregion

    public IEnumerable<SelectionNode> GetCurrentPaths() => _selectionState.GetCurrentPath();
    public SelectionNode? GetCurrentPathState() => _selectionState.Current;

    public Item? GetSelectedItem()
    {
        SelectionNode? itemSelectionNode = _selectionState.Search(ItemTagState.RootSelectedItem | ItemTagState.SearchItem);
        if (itemSelectionNode == null) return null;

        return _items.FirstOrDefault(i => i.Id == itemSelectionNode.Key);
    }

    private static IReadOnlyList<ItemCountInfo> GetCategoryItemsFromPathInternal(string itemPath)
    {
        List<ItemFileCategoryDefinition> categoryDefinitions = Enum.GetValues<FileCategory>()
            .Select(c => new ItemFileCategoryDefinition()
            {
                FileCategory = c,
                ExtensionFilters = c.GetExtensionFilters(),
                FilenameFilters = c.GetFileNameFilters(),
                Item = new FileCategoryItem(c)
            })
            .Where(x => x.ExtensionFilters != null)
            .ToList();

        List<string> unknownFiles = new();

        foreach (string file in FileSystemService.EnumerateFiles(itemPath))
        {
            string extension = Path.GetExtension(file);
            string fileName = Path.GetFileNameWithoutExtension(file);
            ItemFileCategoryDefinition? matched = categoryDefinitions.FirstOrDefault(def => def.ExtensionFilters!.Contains(extension) && (def.FilenameFilters == null || def.FilenameFilters.Any(f => fileName.Contains(f, StringComparison.CurrentCultureIgnoreCase))));

            if (matched != null) matched.Item.FilePaths.Add(file);
            else unknownFiles.Add(file);
        }

        List<ItemCountInfo> result = categoryDefinitions
            .Where(x => x.Item.FilePaths.Count > 0)
            .Select(x => new ItemCountInfo(x.Item, x.Item.FilePaths.Count))
            .ToList();

        if (unknownFiles.Count > 0)
        {
            FileCategoryItem unknownItem = new(FileCategory.Unknown);
            unknownItem.FilePaths.AddRange(unknownFiles);
            result.Add(new ItemCountInfo(unknownItem, unknownFiles.Count));
        }

        return result;
    }
    private static IReadOnlyList<ItemCountInfo> GetFilesFromPathInternal(string itemPath, string category)
    {
        FileCategory targetCategory = Enum.GetValues<FileCategory>()
            .FirstOrDefault(i => i.GetLocalizationKey() == category);

        if (targetCategory == default) return Array.Empty<ItemCountInfo>();

        string[]? extensionFilters = targetCategory.GetExtensionFilters();
        string[]? fileNameFilters = targetCategory.GetFileNameFilters();

        List<ItemCountInfo> result = new();

        foreach (string file in FileSystemService.EnumerateFiles(itemPath))
        {
            bool isMatch;
            
            if (targetCategory == FileCategory.Unknown)
            {
                isMatch = !Enum.GetValues<FileCategory>()
                    .Where(c => c != FileCategory.Unknown && c.GetExtensionFilters() != null)
                    .Any(c =>
                    {
                        var exts = c.GetExtensionFilters();
                        var names = c.GetFileNameFilters();
                        return exts!.Contains(Path.GetExtension(file)) && (names == null || names.Any(f => Path.GetFileNameWithoutExtension(file).Contains(f, StringComparison.CurrentCultureIgnoreCase)));
                    });
            }
            else
            {
                string extension = Path.GetExtension(file);
                string fileName = Path.GetFileNameWithoutExtension(file);
                isMatch = extensionFilters != null && extensionFilters.Contains(extension) && (fileNameFilters == null || fileNameFilters.Any(f => fileName.Contains(f, StringComparison.CurrentCultureIgnoreCase)));
            }

            if (isMatch) result.Add(new ItemCountInfo(new ItemFile(Path.GetFullPath(file)), 0));
        }

        return result;
    }

    public RuntimeSettings GetRuntimeSettings() => _runtimeSettings;
    #endregion

    #region Set API
    public void SetDataRootDirectory(string path) => _runtimeSettings.SetDataRootDirectory(path);
    public void SetAutoBackupRootDirectory(string path)
    {
        _runtimeSettings.SetAutoBackupRootDirectory(path);
        _backupManager.SetAutoBackupPath(path);
    }
    public void SetItemsSortOrder(SortOrder sortOrder) => _runtimeSettings.SetSortOrder(sortOrder);
    public void SetRemoveOriginal(bool value) => _runtimeSettings.SetRemoveOriginal(value);
    public void SetRemoveBrackets(bool value) => _runtimeSettings.SetRemoveBrackets(value);
    public void SetAutoBackupInterval(int value)
    {
        _runtimeSettings.SetAutoBackupInterval(value);
        _backupManager.SetAutoBackupInterval(value);
    }
    #endregion

    #region Add API
    public void AddCommonAvatar(string groupName, IEnumerable<string>? avatars = null)
    {
        CommonAvatar commonAvatar = new()
        {
            GroupName = groupName
        };

        if (avatars != null)
        {
            commonAvatar.UpdateAvatars(avatars);
            UpdateSearchIndex();
        }

        _commonAvatars.Add(commonAvatar);

        SaveCommonAvatarDatabase();
    }
    public async Task<(Item? newItem, List<string> processingFailedPaths)> AddItem(ItemCreationContext itemCreationContext)
    {
        (Item? newItem, List<string> processingFailedPaths) addItemResult = await ItemCreator.FromItemCreationContext(itemCreationContext, _runtimeSettings);
        if (addItemResult.newItem == null) return addItemResult;

        string currentUnixTime = DatetimeUtils.GetCurrentUnixTime();
        addItemResult.newItem.CreatedDate = currentUnixTime;
        addItemResult.newItem.UpdatedDate = currentUnixTime;
        
        _items.Add(addItemResult.newItem);
        UpdateSearchIndex(addItemResult.newItem.Id);

        SaveItemDatabase();

        return addItemResult;
    }
    public async Task<IReadOnlyList<string>> AddItemPaths(string itemId, string[] paths)
    {
        Item? item = GetItemById(itemId);
        if (item == null) return paths.ToList();

        List<string> processingFailedPaths = await FileSystemService.ExtractItemPaths(ItemUtils.GetItemPath(_runtimeSettings.DataRootDirectory, item.ItemPath), paths);
        return processingFailedPaths;
    }
    #endregion

    #region Edit API
    public async Task<Item?> EditItem(string itemId, ItemCreationContext itemCreationContext)
    {
        Item? item = GetItemById(itemId);
        if (item == null) return null;

        item.SetValuesFromCreationContext(itemCreationContext);

        // １個より多い場合は追加のアイテムとしてインポートしてあげる(0がRootフォルダー想定)
        if (itemCreationContext.ItemPaths.Count > 1) await AddItemPaths(item.Id, itemCreationContext.ItemPaths.Skip(1).ToArray());

        item.UpdatedDate = DatetimeUtils.GetCurrentUnixTime();
        UpdateSearchIndex();

        SaveItemDatabase();

        return item;
    }
    #endregion

    #region Update Thumbnail API
    public async Task<Item?> UpdateItemThumbnail(string itemId, string imageFilePath)
    {
        Item? item = GetItemById(itemId);
        if (item == null) return null;

        string? newFileFullPath = await FileSystemService.CopyFile(imageFilePath, Path.Combine(SystemPath.ItemThumbnailsPath, Path.GetFileName(imageFilePath)), true);
        if (newFileFullPath == null) return null;

        item.ThumbnmailFileName = Path.GetFileName(newFileFullPath);
        SaveItemDatabase();

        return item;
    }
    public async Task<Item?> UpdateAuthorThumbnail(string itemId, string imageFilePath)
    {
        Item? item = GetItemById(itemId);
        if (item == null) return null;

        string? newFileFullPath = await FileSystemService.CopyFile(imageFilePath, Path.Combine(SystemPath.AuthorThumbnailsPath, Path.GetFileName(imageFilePath)), true);
        if (newFileFullPath == null) return null;

        item.AuthorThumbnmailFileName = Path.GetFileName(newFileFullPath);
        SaveItemDatabase();

        return item;
    }
    #endregion

    #region Replace API
    public void ReplaceCommonAvatarGroupToSupportedAvatars(string groupId)
    {
        CommonAvatar? commonAvatar = GetCommonAvatarById(groupId);
        if (commonAvatar == null) return;

        string internalId = commonAvatar.GetInternalId();

        _items.ForEach(i => i.UpdateSupportedAvatars(i.SupportedAvatarsView.SelectMany(i => i == internalId ? commonAvatar.AvatarsView : [i]).Distinct()));
    }
    public void ReplaceSupportedAvatarsToCommonAvatarGroup(string groupId)
    {
        CommonAvatar? commonAvatar = GetCommonAvatarById(groupId);
        if (commonAvatar == null) return;

        string internalId = commonAvatar.GetInternalId();

        _items.ForEach(i => i.UpdateSupportedAvatars(i.SupportedAvatarsView.Select(i => commonAvatar.AvatarsView.Contains(i) ? internalId : i).Distinct()));
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
    public static async Task<string?> ModifyUnityPackageFilePath(string filePath, string itemCategoryName = "", Func<(string, int), Task>? reportProgress = null) => await FileSystemService.ModifyUnityPackageFilePathsAsync([filePath], [itemCategoryName], reportProgress);
    public static async Task<string?> ModifyUnityPackageFilePaths(string[] filePaths, string[] itemCategoryNames, Func<(string, int), Task>? reportProgress = null) => await FileSystemService.ModifyUnityPackageFilePathsAsync(filePaths, itemCategoryNames, reportProgress);
    #endregion

    #region Remove API
    public bool RemoveItem(string itemId, bool removeItemFromSupportedAndImplemented = false)
    {
        int removed = _items.RemoveAll(i => i.Id == itemId);
        if (removeItemFromSupportedAndImplemented)
        {
            _items.ForEach(i =>
            {
                i.UpdateSupportedAvatars(i.SupportedAvatarsView.Where(a => a != itemId));
                i.UpdateImplementedAvatars(i.ImplementedAvatarsView.Where(a => a != itemId));
            });
        }

        return removed > 0;
    }

    public bool RemoveCommonAvatar(string commonAvatarId) => _commonAvatars.RemoveAll(i => i.Id == commonAvatarId) > 0;
    #endregion

    #region Search API
    public IReadOnlyList<Item> SearchItems(SearchFilter searchFilter) => SearchService.ExecuteSearch(_items, _commonAvatars, _itemSearchIndexDictionary, _runtimeSettings, searchFilter);
    #endregion

    #region Save API
    public void SaveRuntimeSettings() => RuntimeSettingsService.Save(_runtimeSettings);
    #endregion

    #region Data Importer API
    public async Task ImportFromV1(string dataFolderPath, Func<(string, int), Task>? reportProgress = null)
    {
        (List<Item>, List<CommonAvatar>) result = await DataImporter.FromV1(dataFolderPath, _runtimeSettings, reportProgress);
        
        _items.AddRange(result.Item1);
        _commonAvatars.AddRange(result.Item2);

        SaveItemDatabase();
        SaveCommonAvatarDatabase();
    }
    public async Task ImportFromKonoAsset(string dataFolderPath, Func<(string, int), Task>? reportProgress = null)
    {
        List<Item> items = await DataImporter.FromKonoAsset(dataFolderPath, _runtimeSettings, reportProgress);
        _items.AddRange(items);

        SaveItemDatabase();
        SaveCommonAvatarDatabase();
    }
    #endregion

    #region Data Exporter API
    public async Task ExportToCsv(string filePath, Dictionary<ItemType, string> localizedItemTypesMapping, bool includeCommonToSupported) => await DataExporter.ToCsv(_items, _commonAvatars, localizedItemTypesMapping, _runtimeSettings, filePath, includeCommonToSupported);
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
    private async Task ItemButton_ContextMenu_FetchThumbnail(string itemId)
    {
        Item? item = GetItemById(itemId);
        if (item == null || item.BoothId == -1) return;

        if (IsApiCooldownNow) return;
        
        _lastBoothApiGetTime = DateTime.Now; // 時間を更新する
        BoothItem? boothItem = await BoothService.GetItem(item.BoothId.ToString());
        if (boothItem == null) return;

        string itemThumbnailFileName = item.BoothId + ".png";
        bool result = await ImageDownloader.Fetch(boothItem.Thumbnails.Count > 0 ? boothItem.Thumbnails[0].Original : string.Empty, Path.Combine(SystemPath.ItemThumbnailsPath, itemThumbnailFileName), true);
        if (result) item.ThumbnmailFileName = itemThumbnailFileName;
    }
    #endregion

    #region Backup API
    private readonly BackupManager _backupManager = new();
    public void StartAutoBackup() => _backupManager.StartAutoBackup(_runtimeSettings.AutoBackupInterval, _runtimeSettings.AutoBackupRootDirectory); // minutes
    public async Task StopAutoBackup() => await _backupManager.StopAutoBackup();
    public async Task ExecuteBackup(string path) => await _backupManager.ExecuteBackup(path);
    #endregion
}
