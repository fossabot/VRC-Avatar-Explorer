using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;

namespace AvatarExplorer.UI.Factories;

internal static class ContextMenuFactory
{
    internal static ContextMenu GetContextMenu(ContextMenuAction[] contextMenuActions, EventHandler<RoutedEventArgs>? onClick = null)
    {
        ContextMenu contextMenu = new();

        foreach (ContextMenuAction contextMenuAction in contextMenuActions)
        {
            MenuItem menuItem = new()
            {
                Header = Localizer.Instance[contextMenuAction.DisplayName],
                Tag = contextMenuAction
            };

            if (onClick != null) menuItem.Click += onClick;

            contextMenu.Items.Add(menuItem);
        }

        return contextMenu;
    }
}
