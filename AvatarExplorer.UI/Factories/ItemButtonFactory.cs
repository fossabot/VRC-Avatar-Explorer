using System;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;
using AvatarExplorer.UI.Utils;

namespace AvatarExplorer.UI.Factories;

internal static class ItemButtonFactory
{
    internal static Button AddItemButton(StackPanel parent, UISelectableItem item, bool removeBrackets, ContextMenu? contextMenu = null, EventHandler<RoutedEventArgs>? onClick = null, EventHandler<RoutedEventArgs>? onTagClick = null)
    {
        Button itemButton = new()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(15, 0, 20, 0),
            Tag = item.Tag
        };

        StackPanel contentPanel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10
        };

        Image itemIcon = new()
        {
            Source = ImageService.Get(item.ImageFileName, item.IconType),
            Width = 70,
            Height = 70,
            Stretch = Stretch.Uniform
        };
        RenderOptions.SetBitmapInterpolationMode(itemIcon, BitmapInterpolationMode.HighQuality);

        if (!IconUtils.IsSystemIcon(item.ImageFileName))
        {
            itemIcon.PointerEntered += (s, e) =>
            {
                itemIcon.Width = 200;
                itemIcon.Height = double.NaN;
            };

            itemIcon.PointerExited += (s, e) =>
            {
                itemIcon.Width = 70;
                itemIcon.Height = 70;
            };
        }
        contentPanel.Children.Add(itemIcon);

        StackPanel textPanel = new()
        {
            Orientation = Orientation.Vertical
        };

        string itemTitle = StateFlagUtils.IsCategoryState(item.Tag.State) ? Localizer.Instance[item.Title] : item.Title;
        if (removeBrackets && StateFlagUtils.IsItemState(item.Tag.State)) itemTitle = ItemUtils.RemoveBrackets(itemTitle); // アイテムの場合は括弧を削除してあげる

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

        // タグパネルが横に無限に伸びてしまっているのを修正したいが、StackPanelを使っていると難しいため、いつかはGridに移行したい
        WrapPanel tagPanel = new()
        {
            Orientation = Orientation.Horizontal,
            ItemSpacing = 10,
            LineSpacing = 3,
            Margin = new Thickness(0, 5, 0, 0)
        };

        if (!string.IsNullOrEmpty(item.CommonAvatarName))
        {
            Button commonAvatarButton = GetTagButton(Localizer.Instance.GetDisplayName(LocalizationKey.UI.Button.Tag.CommonAvatar, item.CommonAvatarName));
            commonAvatarButton.FontWeight = FontWeight.Bold;
            commonAvatarButton.Background = new SolidColorBrush(Colors.Green);
            tagPanel.Children.Add(commonAvatarButton);
        }

        foreach (string itemTag in item.ItemTags)
        {
            tagPanel.Children.Add(GetTagButton(itemTag, onTagClick));
        }

        textPanel.Children.Add(tagPanel);

        contentPanel.Children.Add(textPanel);

        itemButton.Content = contentPanel;
        if (StateFlagUtils.IsItemState(item.Tag.State))
        {
            ToolTip.SetTip(itemButton, GetTooltipTextFromItem(item));
            ToolTip.SetBetweenShowDelay(itemButton, -1);
        }

        if (contextMenu != null && contextMenu.ItemCount > 0) itemButton.ContextMenu = contextMenu;
        if (onClick != null) itemButton.Click += onClick;

        parent.Children.Add(itemButton);

        return itemButton;
    }

    internal static Button GetTagButton(string text, EventHandler<RoutedEventArgs>? onClick = null)
    {
        Button tagButton = new Button()
        {
            Content = text,
            CornerRadius = new CornerRadius(15),
            Height = 28,
            FontSize = 13,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        if (onClick != null) tagButton.Click += onClick;
        return tagButton;
    }

    private static string? GetTooltipTextFromItem(UISelectableItem item)
    {
        StringBuilder toolTipTextBuilder = new();

        // AppendLine()1つだとシンプルな改行になるので、1行空けたければ2ついる
        toolTipTextBuilder.Append(item.Title);

        toolTipTextBuilder.AppendLine();
        toolTipTextBuilder.AppendLine();

        toolTipTextBuilder.Append(Localizer.Instance.GetDisplayName(LocalizationKey.UI.Button.ToolTip.CreatedDate, item.CreatedDate));
        toolTipTextBuilder.AppendLine();
        toolTipTextBuilder.Append(Localizer.Instance.GetDisplayName(LocalizationKey.UI.Button.ToolTip.UpdatedDate, item.UpdatedDate));
        
        if (!string.IsNullOrEmpty(item.ItemMemo))
        {
            toolTipTextBuilder.AppendLine();
            toolTipTextBuilder.AppendLine();
            
            toolTipTextBuilder.Append(item.ItemMemo);
        }

        return toolTipTextBuilder.ToString();
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

        Grid pageGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            ColumnSpacing = 10,
            Margin = new Thickness(30, 0, 30, 0)
        };

        StackPanel pageInfoStackPanel = new()
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Grid.SetColumn(pageInfoStackPanel, 0);
        Grid.SetColumnSpan(pageInfoStackPanel, 4);

        TextBlock pageTextBlock = new()
        {
            Text = Localizer.Instance.GetDisplayName(LocalizationKey.UI.ItemWindow.Page, [(currentPageValue + 1).ToString(), totalPages.ToString()]),
            FontSize = 15,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        pageInfoStackPanel.Children.Add(pageTextBlock);

        TextBlock itemsCountTextBlock = new()
        {
            Text = Localizer.Instance.GetDisplayName(LocalizationKey.UI.ItemWindow.PageItemCount, [start.ToString(), end.ToString(), totalItemCount.ToString()]),
            FontSize = 15,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        pageInfoStackPanel.Children.Add(itemsCountTextBlock);

        pageGrid.Children.Add(pageInfoStackPanel);

        if (renderFirstButton)
        {
            Button firstButton = new()
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
            Button backButton = new()
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
            Button nextButton = new()
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
            Button lastButton = new()
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
