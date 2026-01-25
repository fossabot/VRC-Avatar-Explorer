using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using AvatarExplorer.Core.Localization;
using AvatarExplorer.Core.Models;
using AvatarExplorer.UI.Localization;
using AvatarExplorer.UI.Models;
using Material.Icons;
using Material.Icons.Avalonia;

namespace AvatarExplorer.UI.Factories;

internal static class PageButtonFactory
{
    private const string ButtonClass = "button";
    private const string PageButtonClass = "pagebutton";

    internal static void AddPageButton(StackPanel parent, ItemTagState itemTagState, int currentPageValue, int itemsPerPage, int totalItemCount, EventHandler<RoutedEventArgs>? onClick = null)
    {
        int totalPages = (int)Math.Ceiling((double)totalItemCount / itemsPerPage);
        if (totalPages <= 0) return;

        Panel pageInfoPanel = new();

        StackPanel pageInfo = CreatePageInfoPanel(currentPageValue, totalPages, itemsPerPage, totalItemCount);
        
        Grid pageButtonGrid = new() { ColumnDefinitions = new("*,*,*,*"), ColumnSpacing = 10, Margin = new(50, 0, 50, 0) };
        AddNavigationButtons(pageButtonGrid, itemTagState, currentPageValue, totalPages, onClick);
        
        pageInfoPanel.Children.Add(pageButtonGrid);
        pageInfoPanel.Children.Add(pageInfo);

        parent.Children.Add(pageInfoPanel);
    }

    private static StackPanel CreatePageInfoPanel(int currentPage, int totalPages, int itemsPerPage, int totalCount)
    {
        StackPanel panel = new() { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

        string pageText = Localizer.Instance.GetDisplayName(LocalizationKey.UI.ItemWindow.Page, [(currentPage + 1).ToString(), totalPages.ToString()]);
        panel.Children.Add(new TextBlock { Text = pageText, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center });

        int start = (currentPage * itemsPerPage) + 1;
        int end = Math.Min(start + itemsPerPage - 1, totalCount);
        string rangeText = Localizer.Instance.GetDisplayName(LocalizationKey.UI.ItemWindow.PageItemCount, [start.ToString(), end.ToString(), totalCount.ToString()]);
        panel.Children.Add(new TextBlock { Text = rangeText, FontSize = 15, HorizontalAlignment = HorizontalAlignment.Center });

        return panel;
    }

    private static void AddNavigationButtons(Grid grid, ItemTagState state, int current, int total, EventHandler<RoutedEventArgs>? onClick)
    {
        if (current > 0) grid.Children.Add(CreateButton(GetMaterialIcon(MaterialIconKind.FirstPage), HorizontalAlignment.Right, 0, new(state, PageButtonState.First, 0), onClick));
        if (current > 0) grid.Children.Add(CreateButton(GetMaterialIcon(MaterialIconKind.ChevronLeft), HorizontalAlignment.Left, 1, new(state, PageButtonState.Back, current - 1), onClick));
        if (current < total - 1) grid.Children.Add(CreateButton(GetMaterialIcon(MaterialIconKind.ChevronRight), HorizontalAlignment.Right, 2, new(state, PageButtonState.Next, current + 1), onClick));
        if (current < total - 1) grid.Children.Add(CreateButton(GetMaterialIcon(MaterialIconKind.LastPage), HorizontalAlignment.Left, 3, new(state, PageButtonState.Last, total - 1), onClick));
    }

    private static MaterialIcon GetMaterialIcon(MaterialIconKind materialIconKind, double size = 25)
    {
        return new() { Kind = materialIconKind, Width = size, Height = size };
    }

    private static Button CreateButton(MaterialIcon content, HorizontalAlignment align, int column, PageButtonInfo info, EventHandler<RoutedEventArgs>? onClick)
    {
        Button button = new() { Content = content, HorizontalAlignment = align, Tag = info };
        button.Classes.AddRange([ButtonClass, PageButtonClass]);
        if (onClick != null) button.Click += onClick;

        Grid.SetColumn(button, column);

        return button;
    }
}
