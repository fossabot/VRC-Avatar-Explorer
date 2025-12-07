using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

public class AvatarExplorerApp
{
    private readonly List<Item> _items = new();
    private readonly List<CommonAvatar> _commonAvatars = new();

    private readonly SelectionState _selectionState = new();
    private readonly Dictionary<ItemTagState, Func<SelectionNode, IReadOnlyList<ItemCountInfo>>> _stateHandlers;

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
        var database = fromV1 ? DatabaseUtils.LoadItemsDataFromV1(SystemPath.ItemDatabasePath) : DatabaseUtils.LoadItemsData(SystemPath.ItemDatabasePath);

        _items.Clear();
        _items.AddRange(database);
    }

    public void LoadCommonAvatarDatabase(bool fromV1 = false)
    {
        var database = fromV1 ? DatabaseUtils.LoadCommonAvatarsDataFromV1(SystemPath.CommonAvatarDatabasePath) :  DatabaseUtils.LoadCommonAvatarsData(SystemPath.CommonAvatarDatabasePath);

        _commonAvatars.Clear();
        _commonAvatars.AddRange(database);
    }

    public void LoadItemDatabase(string path, bool fromV1 = false)
    {
        var database = fromV1 ? DatabaseUtils.LoadItemsDataFromV1(path) : DatabaseUtils.LoadItemsData(path);

        _items.Clear();
        _items.AddRange(database);
    }

    public void LoadCommonAvatarDatabase(string path, bool fromV1 = false)
    {
        var database = fromV1 ? DatabaseUtils.LoadCommonAvatarsDataFromV1(path) :  DatabaseUtils.LoadCommonAvatarsData(path);

        _commonAvatars.Clear();
        _commonAvatars.AddRange(database);
    }
    #endregion

    #region Update API
    public void UpdateSearchIndex()
    {
        var avatarNameMaps = DatabaseUtils.GetAvatarNameMaps(_items);
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
        return _items.Where(i => i.Type == ItemType.Avatar).Select(i => new ItemCountInfo(i, 0)).ToList();
    }

    public IReadOnlyList<ItemCountInfo> GetCategories()
    {
        return CategoryUtils.GetCategories(_items).ToList();
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
    private IReadOnlyList<ItemCountInfo> HandleRootAvatar(SelectionNode selectionNode)
    {
        return GetCategoriesFromItemsInternal(_items.Where(i => ItemUtils.GetAvatarStatus(i, _commonAvatars, selectionNode.Key).IsSupportedOrCommon));
    }
    private IReadOnlyList<ItemCountInfo> HandleRootAuthor(SelectionNode selectionNode)
    {
        return GetCategoriesFromItemsInternal(_items.Where(i => i.Author == selectionNode.Key));
    }
    private IReadOnlyList<ItemCountInfo> HandleRootCategory(SelectionNode selectionNode)
    {
        return _items
            .Where(i => CategoryUtils.IsCategoryMatch(i, selectionNode.Key))
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

                AvatarStatus avatarStatus = ItemUtils.GetAvatarStatus(item, _commonAvatars, rootSelectionNode.Key);
                if (!avatarStatus.IsSupportedOrCommon) continue;
                
                filteredResult.Add(new ItemCountInfo(item, 0, avatarStatus.OnlyCommon ? avatarStatus.CommonAvatarName : string.Empty));
            }

            return filteredResult;
        }
        else if (rootSelectionNode.State == ItemTagState.RootAuthor)
        {
            return _items
                .Where(i => CategoryUtils.IsCategoryMatch(i, selectionNode.Key) && i.Author == rootSelectionNode.Key)
                .Select(i => new ItemCountInfo(i, 0))
                .ToList();
        }

        return new List<ItemCountInfo>();
    }
    private IReadOnlyList<ItemCountInfo> HandleRootSelectedItem(SelectionNode selectionNode)
    {
        return GetCategoryItemsFromPathInternal(ItemUtils.GetItemPath(selectionNode.Key));
    }
    private IReadOnlyList<ItemCountInfo> HandleItemFileCategory(SelectionNode selectionNode)
    {
        SelectionNode? fileSelectionNode = _selectionState.Search(ItemTagState.RootSelectedItem);
        if (fileSelectionNode == null) return new List<ItemCountInfo>();

        return GetFilesFromPathInternal(ItemUtils.GetItemPath(fileSelectionNode.Key), selectionNode.Key);
    }

    public IEnumerable<SelectionNode> GetCurrentPath() => _selectionState.GetCurrentPath();

    public Item? GetSelectedItem()
    {
        SelectionNode? itemSelectionNode = _selectionState.Search(ItemTagState.RootSelectedItem);
        if (itemSelectionNode == null) return null;

        return _items.FirstOrDefault(i => i.ItemPath == itemSelectionNode.Key);
    }

    private static List<ItemCountInfo> GetCategoryItemsFromPathInternal(string itemPath)
    {
        List<ItemCountInfo> categoryItems = new();

        FileCategory[] extensionFilters = Enum.GetValues<FileCategory>();
        foreach (var filter in extensionFilters)
        {
            var filters = filter.GetExtensionFilters();
            if (filters == null) continue;

            var categoryItem = new FileCategoryItem
            {
                FileCategory = filter
            };

            foreach (var file in FileSystemUtils.EnumerateFiles(itemPath))
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

        foreach (var file in FileSystemUtils.EnumerateFiles(itemPath))
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
    #endregion

    #region File API
    public static void ModifyUnityPackageFilePath(string itemPath, string itemCategoryName = "", IProgress<(string, int, string)>? progress = null)
    {
        FileSystemUtils.ModifyUnityPackageFilePathAsync(itemPath, itemCategoryName, progress);
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
        var avatarNameMaps = DatabaseUtils.GetAvatarNameMaps(_items);

        return _items
            .Where(i => SearchUtils.Matches(filter, avatarNameMaps, _commonAvatars, i))
            .OrderByDescending(i => SearchUtils.GetScore(i, filter.SearchWords))
            .ToList();
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
