using AvatarExplorer.Core.Extensions;
using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Services;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core;

public class AvatarExplorer
{
    private readonly List<Item> _items = new();
    private readonly List<CommonAvatar> _commonAvatars = new();

    private readonly SelectionState _selectionState = new();

    #region Database
    public void LoadItemDatabase(string path)
    {
        var database = DatabaseUtils.LoadItemsData(path);

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
    public IReadOnlyList<Author> GetAuthors()
    {
        List<Author> authors = new();

        foreach (Item item in _items)
        {
            if (authors.Any(author => author.Name == item.Author)) continue;
            authors.Add(new Author
            {
                Name = item.Author,
                AuthorId = item.AuthorId,
                AuthorItemCount = _items.Count(i => i.Author == item.Author)
            });
        }

        return authors;
    }

    public IReadOnlyList<Item> GetAvatars()
    {
        return _items.Where(i => i.Type == ItemType.Avatar).ToList();
    }

    public IReadOnlyList<Category> GetCategories()
    {
        return CategoryUtils.GetCategories(_items);
    }

    public IReadOnlyList<Item> GetAllItems()
    {
        return _items;
    }

    public IReadOnlyList<ISelectableItem> GetItemsForCurrentState()
    {
        SelectionNode? currentSelectionNode = _selectionState.Current;
        if (currentSelectionNode == null) return new List<ISelectableItem>();

        switch (currentSelectionNode.Type)
        {
            case "Root.Avatar":
                {
                    return GetCategoriesFromItems(_items.Where(i => i.SupportedAvatars.Count == 0 || i.SupportedAvatars.Contains(currentSelectionNode.Key) || i.ImplementedAvatars.Contains(currentSelectionNode.Key)), true);
                }

            case "Root.Author":
                {
                    return GetCategoriesFromItems(_items.Where(i => i.Author == currentSelectionNode.Key), true);
                }

            case "Root.Category":
                {
                    return _items.Where(i => (i.Type == ItemType.Custom && i.CustomCategory == currentSelectionNode.Key) || (i.Type.GetInternalId() == currentSelectionNode.Key)).ToList();
                }

            case "Item.Category":
                {
                    SelectionNode? rootSelectionNode = _selectionState.Root;
                    if (rootSelectionNode == null) return new List<ISelectableItem>();
                    
                    if (rootSelectionNode.Type == "Root.Avatar")
                    {
                        return _items.Where(i => (i.SupportedAvatars.Count == 0 || i.SupportedAvatars.Contains(rootSelectionNode.Key) || i.ImplementedAvatars.Contains(rootSelectionNode.Key)) &&
                            ((i.Type == ItemType.Custom && i.CustomCategory == currentSelectionNode.Key) || (i.Type.GetInternalId() == currentSelectionNode.Key))
                        ).ToList();
                    }
                    else if (rootSelectionNode.Type == "Root.Author")
                    {
                        return _items.Where(i => i.Author == rootSelectionNode.Key &&
                            ((i.Type == ItemType.Custom && i.CustomCategory == currentSelectionNode.Key) || (i.Type.GetInternalId() == currentSelectionNode.Key))
                        ).ToList();
                    }

                    break;
                }
        }

        return new List<ISelectableItem>();
    }

    private static List<Category> GetCategoriesFromItems(IEnumerable<Item> items, bool isRoot)
    {
        IEnumerable<Category> itemCategories = items
            .Where(i => i.Type != ItemType.Custom)
            .Select(i => i.Type)
            .Distinct()
            .Select(i => new Category(i)
            {
                CategoryItemCount = items.Count(item => item.Type == i)
            });

        IEnumerable<Category> itemCustomCategories = items
            .Where(i => i.Type == ItemType.Custom)
            .Select(i => i.CustomCategory)
            .Distinct()
            .Select(i => new Category(i)
            {
                CategoryItemCount = items.Count(item => item.CustomCategory == i)
            });

        var allCategories = itemCategories.Concat(itemCustomCategories).ToList();
        if (isRoot) allCategories.ForEach(c => c.CustomTagType = "Root.Category");

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
