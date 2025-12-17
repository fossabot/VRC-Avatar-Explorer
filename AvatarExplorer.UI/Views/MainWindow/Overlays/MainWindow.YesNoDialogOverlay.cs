using System;
using Avalonia.Interactivity;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    internal EventHandler<RoutedEventArgs>? _yesNoDialog_onYesClick = null;
    internal EventHandler<RoutedEventArgs>? _yesNoDialog_onNoClick = null;

    private void YesNoDialog_Show(string title, string content)
    {
        YesNoDialogTitle.Text = title;
        YesNoDialogContent.Text = content;

        YesNoDialogOverlay.IsVisible = true;
    }
    private void YesNoDialog_Yes_Click(object? sender, RoutedEventArgs e)
    {
        _yesNoDialog_onYesClick?.Invoke(sender, e);
        YesNoDialog_CloseInternal();
    }
    private void YesNoDialog_No_Click(object? sender, RoutedEventArgs e)
    {
        _yesNoDialog_onNoClick?.Invoke(sender, e);
        YesNoDialog_CloseInternal();
    }

    private void YesNoDialog_CloseInternal()
        => YesNoDialogOverlay.IsVisible = false;
}
