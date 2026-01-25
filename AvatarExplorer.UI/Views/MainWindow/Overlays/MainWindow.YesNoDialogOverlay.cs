using System;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using AvatarExplorer.UI.Models;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private TaskCompletionSource<YesNoResult>? _yesNoTcs;

    private Task<YesNoResult> Main_ShowYesNoDialogAsync(string title, string content)
    {
        if (_yesNoTcs != null) throw new InvalidOperationException("YesNoDialog is already shown.");

        _yesNoTcs = new TaskCompletionSource<YesNoResult>();

        YesNoDialogTitle.Text = title;
        YesNoDialogContent.Text = content;
        YesNoDialogOverlay.IsVisible = true;

        return _yesNoTcs.Task;
    }

    private void CloseDialog(YesNoResult result)
    {
        YesNoDialogOverlay.IsVisible = false;

        TaskCompletionSource<YesNoResult>? tcs = _yesNoTcs;
        _yesNoTcs = null;

        tcs?.TrySetResult(result);
    }

    private void YesNoDialog_Yes_Click(object? sender, RoutedEventArgs e) => CloseDialog(YesNoResult.Yes);

    private void YesNoDialog_No_Click(object? sender, RoutedEventArgs e) => CloseDialog(YesNoResult.No);
}
