using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI.Utils;

internal static class UIUtils
{
    internal static void AddItemButton(StackPanel parent, UISelectableItem item, bool removeBrackets, ContextMenu? contextMenu = null, EventHandler<RoutedEventArgs>? onClick = null)
    {
        var itemButton = new Button()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(15, 0, 20, 0),
            Tag = item.Tag
        };

        var contentPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        var itemIcon = new Image()
        {
            Source = IconUtils.GetIcon(item.ImageFileName, item.IconType),
            Width = 70,
            Height = 70
        };
        if (!IconUtils.IsSystemFileIcons(item.ImageFileName))
        {
            itemIcon.PointerEntered += (s, e) =>
            {
                itemIcon.Width = 200;
                itemIcon.Height = 200;
            };

            itemIcon.PointerExited += (s, e) =>
            {
                itemIcon.Width = 70;
                itemIcon.Height = 70;
            };
        }
        contentPanel.Children.Add(itemIcon);

        var textPanel = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        string itemTitle = (item.Tag.State == ItemTagState.RootCategory || item.Tag.State == ItemTagState.RootSelectedCategory || item.Tag.State == ItemTagState.ItemFileCategory) ? Localizer.Instance[item.Title] : item.Title;
        if (removeBrackets && (item.Tag.State == ItemTagState.RootAvatar || item.Tag.State == ItemTagState.SearchItem || item.Tag.State == ItemTagState.RootSelectedItem)) itemTitle = ItemUtils.RemoveBrackets(itemTitle); // アイテムの場合は括弧を削除してあげる

        textPanel.Children.Add(new TextBlock()
        {
            Text = itemTitle,
            FontSize = 16,
            FontWeight = FontWeight.Bold
        });
        textPanel.Children.Add(new TextBlock()
        {
            Text = Localizer.Instance.GetDisplayName(item.Description.LocalizationKey, item.Description.Args),
            FontSize = 13
        });

        // TODO: タグパネルが横に無限に伸びてしまっているのを修正する
        var tagPanel = new WrapPanel()
        {
            Orientation = Orientation.Horizontal,
            ItemSpacing = 10,
            LineSpacing = 3,
            Margin = new Thickness(0, 5, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        foreach (string itemTag in item.ItemTags)
        {
            tagPanel.Children.Add(new Button()
            {
                Content = itemTag,
                CornerRadius = new CornerRadius(15),
                Height = 28,
                FontSize = 13,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
            });
        }
        
        textPanel.Children.Add(tagPanel);

        contentPanel.Children.Add(textPanel);

        itemButton.Content = contentPanel;

        if (contextMenu != null && contextMenu.ItemCount > 0) itemButton.ContextMenu = contextMenu;
        if (onClick != null) itemButton.Click += onClick;

        parent.Children.Add(itemButton);
    }

    internal static void AddPageButton(StackPanel parent, ItemTagState itemTagState, int currentPageValue, int itemsPerPage, int totalItemCount, EventHandler<RoutedEventArgs>? onClick = null)
    {
        int totalPages = (int)Math.Ceiling((double)totalItemCount / itemsPerPage);

        int start = 0;
        int end = 0;

        bool isValidPage = currentPageValue >= 0 && currentPageValue < totalPages;

        if (isValidPage)
        {
            start = (currentPageValue * itemsPerPage) + 1;
            end = Math.Min(start + itemsPerPage - 1, totalItemCount);
        }

        bool renderFirstButton = currentPageValue > 0;
        bool renderBackButton = currentPageValue > 0;
        bool renderNextButton = currentPageValue < totalPages - 1;
        bool renderLastButton = currentPageValue < totalPages - 1;

        var pageGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            ColumnSpacing = 10,
            Margin = new Thickness(30, 0, 30, 0)
        };

        var pageInfoStackPanel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Grid.SetColumn(pageInfoStackPanel, 0);
        Grid.SetColumnSpan(pageInfoStackPanel, 4);

        // TODO: Localizeする
        var pageTextBlock = new TextBlock()
        {
            Text = $"{currentPageValue + 1}/{totalPages}ページ",
            FontSize = 15,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        pageInfoStackPanel.Children.Add(pageTextBlock);

        var itemsCountTextBlock = new TextBlock()
        {
            Text = $"{start} - {end} / {totalItemCount}個の項目",
            FontSize = 15,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        pageInfoStackPanel.Children.Add(itemsCountTextBlock);

        pageGrid.Children.Add(pageInfoStackPanel);

        if (renderFirstButton)
        {
            var firstButton = new Button()
            {
                Content = "<<",
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Right,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = new PageButtonInfo(itemTagState, PageButtonState.First, 0),
                Width = 50
            };
            
            Grid.SetColumn(firstButton, 0);
            if (onClick != null) firstButton.Click += onClick;
            pageGrid.Children.Add(firstButton);
        }

        if (renderBackButton)
        {
            var backButton = new Button()
            {
                Content = "<",
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Left,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = new PageButtonInfo(itemTagState, PageButtonState.Back, currentPageValue - 1),
                Width = 50
            };
            Grid.SetColumn(backButton, 1);
            if (onClick != null) backButton.Click += onClick;
            pageGrid.Children.Add(backButton);
        }

        if (renderNextButton)
        {
            var nextButton = new Button()
            {
                Content = ">",
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Right,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = new PageButtonInfo(itemTagState, PageButtonState.Next, currentPageValue + 1),
                Width = 50
            };
            Grid.SetColumn(nextButton, 2);
            if (onClick != null) nextButton.Click += onClick;
            pageGrid.Children.Add(nextButton);
        }

        if (renderLastButton)
        {
            var lastButton = new Button()
            {
                Content = ">>",
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Left,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Tag = new PageButtonInfo(itemTagState, PageButtonState.Last, totalPages - 1),
                Width = 50
            };
            Grid.SetColumn(lastButton, 3);
            if (onClick != null) lastButton.Click += onClick;
            pageGrid.Children.Add(lastButton);
        }

        parent.Children.Add(pageGrid);
    }
}
