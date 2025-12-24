using System;
using Avalonia.Interactivity;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    internal event EventHandler<RoutedEventArgs>? YesNoDialog_onYesClick = null;
    internal event EventHandler<RoutedEventArgs>? YesNoDialog_onNoClick = null;

    private void YesNoDialog_Show(string title, string content)
    {
        YesNoDialogTitle.Text = title;
        YesNoDialogContent.Text = content;

        YesNoDialogOverlay.IsVisible = true;
    }
    private void YesNoDialog_Hide()
        => YesNoDialogOverlay.IsVisible = false;

    private void YesNoDialog_Yes_Click(object? sender, RoutedEventArgs e)
    {
        YesNoDialog_Hide();
        YesNoDialog_onYesClick?.Invoke(sender, e);
    }
    private void YesNoDialog_No_Click(object? sender, RoutedEventArgs e)
    {
        YesNoDialog_Hide();
        YesNoDialog_onNoClick?.Invoke(sender, e);
    }
}
