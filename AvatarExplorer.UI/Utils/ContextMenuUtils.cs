using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Utils;

internal static class ContextMenuUtils
{
    internal static ContextMenu GetContextMenu(List<ContextMenuAction> contextMenuActions, EventHandler<RoutedEventArgs>? onClick = null)
    {
        ContextMenu contextMenu = new();

        foreach (ContextMenuAction contextMenuAction in contextMenuActions)
        {
            MenuItem menuItem = new()
            {
                Header = Localizer.Instance.GetDisplayName(contextMenuAction.DisplayName),
                Tag = contextMenuAction
            };

            if (onClick != null) menuItem.Click += onClick;

            contextMenu.Items.Add(menuItem);
        }

        return contextMenu;
    }
}
