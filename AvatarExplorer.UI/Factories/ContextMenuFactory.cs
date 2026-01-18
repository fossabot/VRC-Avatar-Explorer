using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;
using Material.Icons;
using Material.Icons.Avalonia;

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

            MaterialIcon? materialIcon = GetMaterialIcon(contextMenuAction.ContextMenuIconType);
            if (materialIcon != null) menuItem.Icon = materialIcon;

            if (onClick != null) menuItem.Click += onClick;

            contextMenu.Items.Add(menuItem);

            if (contextMenuAction.AddSeparator) contextMenu.Items.Add(new Separator());
        }

        return contextMenu;
    }

    private static MaterialIcon? GetMaterialIcon(ContextMenuIconType contextMenuIconType)
    {
        MaterialIconKind? materialIconKind = GetMaterialIconKind(contextMenuIconType);
        if (materialIconKind == null) return null;

        return new MaterialIcon()
        {
            Kind = (MaterialIconKind)materialIconKind,
            Width = 16,
            Height = 16
        };
    }

    private static MaterialIconKind? GetMaterialIconKind(ContextMenuIconType contextMenuIconType)
    {
        return contextMenuIconType switch
        {
            ContextMenuIconType.Open => MaterialIconKind.OpenInNew,
            ContextMenuIconType.Copy => MaterialIconKind.ContentCopy,
            ContextMenuIconType.Add => MaterialIconKind.Add,
            ContextMenuIconType.Edit => MaterialIconKind.Edit,
            ContextMenuIconType.Fetch => MaterialIconKind.Download,
            ContextMenuIconType.Delete => MaterialIconKind.Delete,
            _ => null
        };
    }
}
