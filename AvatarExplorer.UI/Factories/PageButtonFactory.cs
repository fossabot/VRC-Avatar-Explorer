using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI.Factories;

internal static class PageButtonFactory
{
    private const string ButtonClass = "button";
    private const string PageButtonClass = "pagebutton";

    // TODO: 後でメソッド分ける
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
