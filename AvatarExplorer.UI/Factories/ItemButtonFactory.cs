using System;
using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.Core.Utils;
using AvatarExplorer.UI.Extensions;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using AvatarExplorer.UI.Services;
using AvatarExplorer.UI.Utils;

namespace AvatarExplorer.UI.Factories;

internal static class ItemButtonFactory
{
    private const string ButtonClass = "button";
    private const string PageButtonClass = "pagebutton";

    internal static Button AddItemButton(StackPanel parent, UISelectableItem item, RuntimeSettings runtimeSettings, UserPreferences userPreferences, ContextMenu? contextMenu = null, EventHandler<RoutedEventArgs>? onClick = null)
    {
        Button itemButton = CreateBaseButton(item);
        
        Grid contentGrid = new() { ColumnSpacing = 10, ColumnDefinitions = new("Auto,*") };

        // アイコン
        Image itemIcon = CreateItemIcon(item, userPreferences);
        contentGrid.Children.Add(itemIcon);
        Grid.SetColumn(itemIcon, 0);

        // テキスト + タグ部分
        Grid textGrid = CreateTextAndTagGrid(item, runtimeSettings);
        contentGrid.Children.Add(textGrid);
        Grid.SetColumn(textGrid, 1);

        itemButton.Content = contentGrid;
        SetupButtonInteractions(itemButton, item, runtimeSettings, contextMenu, onClick);

        parent.Children.Add(itemButton);
        return itemButton;
    }

    private static Button CreateBaseButton(UISelectableItem item)
    {
        Button button = new() { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top, Margin = new(15, 0, 20, 0), Tag = item.Tag };
        button.Classes.Add(ButtonClass);
        return button;
    }

    private static Image CreateItemIcon(UISelectableItem item, UserPreferences userPreferences)
    {
        Image itemIcon = new()
        {
            Source = ImageService.Get(item.ImageFileName, item.IconType),
            Width = userPreferences.NormalIconSize,
            Height = userPreferences.NormalIconSize,
            Stretch = Stretch.Fill,
            VerticalAlignment = VerticalAlignment.Top
        };
        BitmapInterpolationMode bitmapInterpolationMode = userPreferences.AntiAliasingMode.GetInterpolationMode();
        if (bitmapInterpolationMode != BitmapInterpolationMode.None && bitmapInterpolationMode != BitmapInterpolationMode.Unspecified) RenderOptions.SetBitmapInterpolationMode(itemIcon, bitmapInterpolationMode);

        if (!IconUtils.IsSystemIcon(item.ImageFileName) && userPreferences.EnableHoverIconSize)
        {
            itemIcon.PointerEntered += (s, e) =>
            {
                itemIcon.Width = userPreferences.HoverIconSize;
                itemIcon.Height = double.NaN;
            };

            itemIcon.PointerExited += (s, e) =>
            {
                itemIcon.Width = userPreferences.NormalIconSize;
                itemIcon.Height = userPreferences.NormalIconSize;
            };
        }

        return itemIcon;
    }

    private static Grid CreateTextAndTagGrid(UISelectableItem item, RuntimeSettings runtimeSettings)
    {
        Grid textGrid = new() { RowDefinitions = new("Auto,Auto,5,*") };

        string itemTitle = GetFormattedTitle(item, runtimeSettings);
        
        TextBlock titleTextBlock = new() { Text = itemTitle, FontSize = 16, FontWeight = FontWeight.Bold };
        Grid.SetRow(titleTextBlock, 0);
        textGrid.Children.Add(titleTextBlock);

        TextBlock descriptionTextBlock = new() { Text = Localizer.Instance.GetDisplayName(item.Description.LocalizationKey, item.Description.Args), FontSize = 13 };
        Grid.SetRow(descriptionTextBlock, 1);
        textGrid.Children.Add(descriptionTextBlock);

        WrapPanel tagPanel = CreateTagPanel(item);
        Grid.SetRow(tagPanel, 3);
        textGrid.Children.Add(tagPanel);

        return textGrid;
    }

    private static string GetFormattedTitle(UISelectableItem item, RuntimeSettings runtimeSettings)
    {
        string title = StateFlagUtils.IsCategoryState(item.Tag.State) ? Localizer.Instance[item.Title] : item.Title;

        // アイテムの場合は設定をチェックして括弧を削除してあげる
        if (runtimeSettings.RemoveBrackets && StateFlagUtils.IsItemState(item.Tag.State))
        {
            title = ItemUtils.RemoveBrackets(title);
        }

        return title;
    }

    private static WrapPanel CreateTagPanel(UISelectableItem item)
    {
        WrapPanel tagPanel = new() { Orientation = Orientation.Horizontal, ItemSpacing = 5, LineSpacing = 5 };

        if (!string.IsNullOrEmpty(item.CommonAvatarName))
        {
            Button commonAvatarButton = GetTagButton(Localizer.Instance.GetDisplayName(LocalizationKey.UI.Button.Tag.CommonAvatar, item.CommonAvatarName));
            commonAvatarButton.FontWeight = FontWeight.Bold;
            commonAvatarButton.Foreground = new SolidColorBrush(Colors.White);
            commonAvatarButton.Background = new SolidColorBrush(Colors.Green);
            tagPanel.Children.Add(commonAvatarButton);
        }

        foreach (string itemTag in item.ItemTagsView)
        {
            Button tagButton = GetTagButton(itemTag, onClick: null);
            tagButton.Classes.Add("accent");
            tagPanel.Children.Add(tagButton);
        }
        return tagPanel;
    }

    private static void SetupButtonInteractions(Button button, UISelectableItem item, RuntimeSettings runtimeSettings, ContextMenu? contextMenu, EventHandler<RoutedEventArgs>? onClick)
    {
        if (StateFlagUtils.IsItemState(item.Tag.State))
        {
            ToolTip.SetTip(button, GetTooltipTextFromItem(item));
            ToolTip.SetBetweenShowDelay(button, -1);
        }
        else if (item.Tag.State == ItemTagState.ItemFileCategoryOpen)
        {
            ToolTip.SetTip(button, Localizer.Instance.GetDisplayName(LocalizationKey.UI.Button.ToolTip.FilePath, Path.GetRelativePath(runtimeSettings.DataRootDirectory, item.Tag.Value)));
            ToolTip.SetBetweenShowDelay(button, -1);
        }

        if (contextMenu != null && contextMenu.ItemCount > 0) button.ContextMenu = contextMenu;
        if (onClick != null) button.Click += onClick;
    }

    internal static Button GetTagButton(string text, EventHandler<RoutedEventArgs>? onClick = null)
    {
        Button tagButton = new() { Content = text, CornerRadius = new(15), Height = 30, FontSize = 13.5, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center };

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

        Grid pageGrid = new() { ColumnDefinitions = new("*,*,*,*"), ColumnSpacing = 10, Margin = new(50, 0, 50, 0) };

        StackPanel pageInfoStackPanel = new() { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

        Grid.SetColumn(pageInfoStackPanel, 0);
        Grid.SetColumnSpan(pageInfoStackPanel, 4);

        string pageTextString = Localizer.Instance.GetDisplayName(LocalizationKey.UI.ItemWindow.Page, [(currentPageValue + 1).ToString(), totalPages.ToString()]);
        TextBlock pageTextBlock = new() { Text = pageTextString, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        pageInfoStackPanel.Children.Add(pageTextBlock);

        string itemsCountTextString = Localizer.Instance.GetDisplayName(LocalizationKey.UI.ItemWindow.PageItemCount, [start.ToString(), end.ToString(), totalItemCount.ToString()]);
        TextBlock itemsCountTextBlock = new() { Text = itemsCountTextString, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        pageInfoStackPanel.Children.Add(itemsCountTextBlock);

        pageGrid.Children.Add(pageInfoStackPanel);

        if (renderFirstButton)
        {
            Button firstButton = new() { Content = "<<", HorizontalAlignment = HorizontalAlignment.Right, Tag = new PageButtonInfo(itemTagState, PageButtonState.First, 0) };
            firstButton.Classes.AddRange([ButtonClass, PageButtonClass]);
            
            Grid.SetColumn(firstButton, 0);
            if (onClick != null) firstButton.Click += onClick;
            pageGrid.Children.Add(firstButton);
        }

        if (renderBackButton)
        {
            Button backButton = new() { Content = "<", HorizontalAlignment = HorizontalAlignment.Left, Tag = new PageButtonInfo(itemTagState, PageButtonState.Back, currentPageValue - 1) };
            backButton.Classes.AddRange([ButtonClass, PageButtonClass]);

            Grid.SetColumn(backButton, 1);
            if (onClick != null) backButton.Click += onClick;
            pageGrid.Children.Add(backButton);
        }

        if (renderNextButton)
        {
            Button nextButton = new() { Content = ">", HorizontalAlignment = HorizontalAlignment.Right, Tag = new PageButtonInfo(itemTagState, PageButtonState.Next, currentPageValue + 1) };
            nextButton.Classes.AddRange([ButtonClass, PageButtonClass]);

            Grid.SetColumn(nextButton, 2);
            if (onClick != null) nextButton.Click += onClick;
            pageGrid.Children.Add(nextButton);
        }

        if (renderLastButton)
        {
            Button lastButton = new() { Content = ">>", HorizontalAlignment = HorizontalAlignment.Left, Tag = new PageButtonInfo(itemTagState, PageButtonState.Last, totalPages - 1) };
            lastButton.Classes.AddRange([ButtonClass, PageButtonClass]);

            Grid.SetColumn(lastButton, 3);
            if (onClick != null) lastButton.Click += onClick;
            pageGrid.Children.Add(lastButton);
        }

        parent.Children.Add(pageGrid);
    }
}
