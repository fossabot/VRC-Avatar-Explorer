using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

public class AvatarExplorer
{
    private readonly List<Item> _items = new();
    private readonly List<CommonAvatar> _commonAvatars = new();

    private readonly SelectionState _selectionState = new();

    #region Database
    public void LoadItemDatabase(bool fromV1 = false)
    {
        var database = fromV1 ? DatabaseUtils.LoadItemsDataFromV1(SystemPath.ItemDatabasePath) : DatabaseUtils.LoadItemsData(SystemPath.ItemDatabasePath);

        _items.Clear();
        _items.AddRange(database);
    }

    public void LoadItemDatabase(string path, bool fromV1 = false)
    {
        var database = fromV1 ? DatabaseUtils.LoadItemsDataFromV1(path) : DatabaseUtils.LoadItemsData(path);

        _items.Clear();
        _items.AddRange(database);
    }

    public void LoadCommonAvatarDatabase(string path)
    {
        throw new NotImplementedException();
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
    public void Select(string type, string key)
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
                new Author { Name = g.Key.Author, AuthorThumbnailFileName = g.Key.AuthorThumbnmailFileName },
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

    public IReadOnlyList<ItemCountInfo> GetItemsForCurrentState()
    {
        SelectionNode? currentSelectionNode = _selectionState.Current;
        if (currentSelectionNode == null) return new List<ItemCountInfo>();

        switch (currentSelectionNode.Type)
        {
            case "Root.Avatar":
                {
                    return GetCategoriesFromItems(_items.Where(i => i.SupportedAvatars.Count == 0 || i.SupportedAvatars.Contains(currentSelectionNode.Key) || i.ImplementedAvatars.Contains(currentSelectionNode.Key)));
                }

            case "Root.Author":
                {
                    return GetCategoriesFromItems(_items.Where(i => i.Author == currentSelectionNode.Key));
                }

            case "Root.Category":
                {
                    return _items
                        .Where(i => (i.Type == ItemType.Custom && i.CustomCategory == currentSelectionNode.Key) || (i.Type.GetInternalId() == currentSelectionNode.Key))
                        .Select(i => new ItemCountInfo(i, 0))
                        .ToList();
                }

            case "Item.Category":
                {
                    SelectionNode? rootSelectionNode = _selectionState.Root;
                    if (rootSelectionNode == null) return new List<ItemCountInfo>();
                    
                    if (rootSelectionNode.Type == "Root.Avatar")
                    {
                        return _items.Where(i => (i.SupportedAvatars.Count == 0 || i.SupportedAvatars.Contains(rootSelectionNode.Key) || i.ImplementedAvatars.Contains(rootSelectionNode.Key)) &&
                            ((i.Type == ItemType.Custom && i.CustomCategory == currentSelectionNode.Key) || (i.Type.GetInternalId() == currentSelectionNode.Key))
                        ).Select(i => new ItemCountInfo(i, 0)).ToList();
                    }
                    else if (rootSelectionNode.Type == "Root.Author")
                    {
                        return _items.Where(i => i.Author == rootSelectionNode.Key &&
                            ((i.Type == ItemType.Custom && i.CustomCategory == currentSelectionNode.Key) || (i.Type.GetInternalId() == currentSelectionNode.Key))
                        ).Select(i => new ItemCountInfo(i, 0)).ToList();
                    }

                    break;
                }
            
            case "Item":
                {
                    var fileItems = new List<ItemCountInfo>();

                    string itemPath = ItemUtils.GetItemPath(currentSelectionNode.Key);

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
                            fileItems.Add(new ItemCountInfo(categoryItem, categoryItem.FilePaths.Count));
                        }
                    }
                    
                    return fileItems;
                }
        }

        return new List<ItemCountInfo>();
    }

    private static List<ItemCountInfo> GetCategoriesFromItems(IEnumerable<Item> items)
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
        int removed = _commonAvatars.RemoveAll(i => i.Name == commonAvatarName);
        return removed > 0;
    }
    #endregion

    #region Search API
    public IReadOnlyList<Item> SearchItems(SearchFilter filter)
    {
        var avatarNameMaps = DatabaseUtils.GetAvatarNameMaps(_items);
        
        return _items
            .Where(i => filter.Matches(avatarNameMaps, _commonAvatars, i))
            .OrderByDescending(i => SearchUtils.GetScore(i, filter.SearchWords))
            .ToList();
    }
    #endregion
}
