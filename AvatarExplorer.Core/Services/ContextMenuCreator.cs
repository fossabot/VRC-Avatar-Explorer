using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

public static class ContextMenuCreator
{
    public static List<ContextMenuAction> CreateContextMenu(ISelectableItem selectableItem)
    {
        // TODO: 他のISelectableItemの条件も作る
        if (selectableItem is Item item) return CreateContextMenuFromItemInternal(item);
        return new List<ContextMenuAction>();
    }

    private static List<ContextMenuAction> CreateContextMenuFromItemInternal(Item item)
    {
        List<ContextMenuAction> contextMenuActions =
        [
            new ContextMenuAction("ContextMenu.Item.OpenFolder", ActionKey.OpenItemFolder, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction("ContextMenu.Item.Booth.Copy", ActionKey.CopyBoothLink, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction("ContextMenu.Item.Booth.Open", ActionKey.OpenBoothLink, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction("ContextMenu.Item.ShowOtherItemsByAuthor", ActionKey.ShowOtherItemsByAuthor, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction("ContextMenu.Item.Thumbnail.Change", ActionKey.ChangeThumbnail, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction("ContextMenu.Item.Thumbnail.Fetch", ActionKey.FetchThumbnail, ActionLayer.Core, item.ItemPath, true),
            new ContextMenuAction("ContextMenu.Item.Edit", ActionKey.EditItem, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction("ContextMenu.Item.Add.Memo", ActionKey.AddItemMemo, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction("ContextMenu.Item.Add.Folder", ActionKey.AddItemFolder, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction("ContextMenu.Item.Edit.Implemented", ActionKey.EditImplementedAvatar, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction("ContextMenu.Item.Edit.Tag", ActionKey.EditItemTag, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction("ContextMenu.Item.Remove", ActionKey.RemoveItem, ActionLayer.Core, item.ItemPath, true),
        ];

        return contextMenuActions;
    }
}
