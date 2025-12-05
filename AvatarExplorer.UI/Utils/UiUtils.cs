using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI.Utils;

public static class UIUtils
{
    public static void AddItemButton(StackPanel parent, UISelectableItem item, EventHandler<RoutedEventArgs>? onClick = null)
    {
        var button = new Button
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(15, 0, 25, 10),
            Tag = item.Tag
        };

        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var image = new Image
        {
            Source = IconUtils.GetIcon(item.ImageFileName, item.IconType),
            Width = 70,
            Height = 70
        };

        if (!IconUtils.IsSystemFileIcons(item.ImageFileName))
        {
            image.PointerEntered += (s, e) =>
            {
                image.Width = 200;
                image.Height = 200;
            };

            image.PointerExited += (s, e) =>
            {
                image.Width = 70;
                image.Height = 70;
            };
        }

        var textPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        var titleText = new TextBlock
        {
            Text = Localizer.Instance.GetDisplayName(item.Title),
            FontSize = 16,
            FontWeight = FontWeight.Bold
        };

        var (internalId, args) = item.Description;
        var descText = new TextBlock
        {
            Text = Localizer.Instance.GetDisplayName(internalId, args),
            FontSize = 13
        };

        textPanel.Children.Add(titleText);
        textPanel.Children.Add(descText);

        contentPanel.Children.Add(image);
        contentPanel.Children.Add(textPanel);

        button.Content = contentPanel;
        if (onClick != null) button.Click += onClick;
        
        parent.Children.Add(button);
    }
}
