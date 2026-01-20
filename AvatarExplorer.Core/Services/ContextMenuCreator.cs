using AvatarExplorer.Core.Interfaces;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;

namespace AvatarExplorer.Core.Services;

public static class ContextMenuCreator
{
    public static ContextMenuAction[] Create(ISelectableItem selectableItem)
    {
        if (selectableItem is Item item) return CreateFromItem(item);
        if (selectableItem is ItemFile itemFile) return CreateFromItemFile(itemFile);
        else return [];
    }

    private static ContextMenuAction[] CreateFromItem(Item item)
    {
        ContextMenuAction[] contextMenuActions =
        [
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.OpenFolder, ActionKey.OpenItemFolder, ActionLayer.UI, ContextMenuIconType.Open, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.ShowOtherItemsByAuthor, ActionKey.ShowOtherItemsByAuthor, ActionLayer.UI, ContextMenuIconType.Open, item.ItemPath, addSeparator: true),
            
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Add.Folder, ActionKey.AddItemFolder, ActionLayer.UI, ContextMenuIconType.Add, item.ItemPath, addSeparator: true),

            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Booth.Open, ActionKey.OpenBoothLink, ActionLayer.UI, ContextMenuIconType.Open, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Booth.Copy, ActionKey.CopyBoothLink, ActionLayer.UI, ContextMenuIconType.Copy, item.ItemPath, addSeparator: true),

            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Edit.Default, ActionKey.EditItem, ActionLayer.UI, ContextMenuIconType.Edit, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Edit.Title, ActionKey.EditItemTitle, ActionLayer.UI, ContextMenuIconType.Edit, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Edit.Tag, ActionKey.EditItemTag, ActionLayer.UI, ContextMenuIconType.Edit, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Edit.Memo, ActionKey.AddItemMemo, ActionLayer.UI, ContextMenuIconType.Edit, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Edit.Implemented, ActionKey.EditImplementedAvatar, ActionLayer.UI, ContextMenuIconType.Edit, item.ItemPath, addSeparator: true),

            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Thumbnail.Change, ActionKey.ChangeThumbnail, ActionLayer.UI, ContextMenuIconType.Edit, item.ItemPath),
            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Thumbnail.Fetch, ActionKey.FetchThumbnail, ActionLayer.Core, ContextMenuIconType.Fetch, item.ItemPath, addSeparator: true),

            new ContextMenuAction(LocalizationKey.ContextMenu.Item.Remove, ActionKey.RemoveItem, ActionLayer.UI, ContextMenuIconType.Delete, item.ItemPath)
        ];

        return contextMenuActions;
    }

    private static ContextMenuAction[] CreateFromItemFile(ItemFile itemFile)
    {
        List<ContextMenuAction> contextMenuActions =
        [
            new ContextMenuAction(LocalizationKey.ContextMenu.ItemFile.OpenFile, ActionKey.OpenFile, ActionLayer.UI, ContextMenuIconType.Open, itemFile.FullPath)
        ];

        if (ProcessUtils.IsWindows())
        {
            contextMenuActions.Add(new ContextMenuAction(LocalizationKey.ContextMenu.ItemFile.OpenFileInExplorer, ActionKey.OpenFileInExplorer, ActionLayer.UI, ContextMenuIconType.Open, itemFile.FullPath));
        }

        return contextMenuActions.ToArray();
    }
}
