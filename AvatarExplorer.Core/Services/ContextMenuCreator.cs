using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;

namespace AvatarExplorer.Core.Services;

public static class ContextMenuCreator
{
    public static ContextMenuAction[] CreateContextMenu(ISelectableItem selectableItem)
    {
        if (selectableItem is Item item) return CreateContextMenuFromItemInternal(item);
        else return [];
    }

    private static ContextMenuAction[] CreateContextMenuFromItemInternal(Item item)
    {
        ContextMenuAction[] contextMenuActions =
        [
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.OpenFolder, ActionKey.OpenItemFolder, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Booth.Copy, ActionKey.CopyBoothLink, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Booth.Open, ActionKey.OpenBoothLink, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.ShowOtherItemsByAuthor, ActionKey.ShowOtherItemsByAuthor, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Thumbnail.Change, ActionKey.ChangeThumbnail, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Thumbnail.Fetch, ActionKey.FetchThumbnail, ActionLayer.Core, item.ItemPath, true),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Edit.Default, ActionKey.EditItem, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Add.Memo, ActionKey.AddItemMemo, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Add.Folder, ActionKey.AddItemFolder, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Edit.Implemented, ActionKey.EditImplementedAvatar, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Edit.Tag, ActionKey.EditItemTag, ActionLayer.UI, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Remove, ActionKey.RemoveItem, ActionLayer.UI, item.ItemPath),
        ];

        return contextMenuActions;
    }
}
