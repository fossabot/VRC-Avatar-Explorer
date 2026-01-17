using System;
using System.Threading.Tasks;
using Avalonia.Interactivity;

namespace AvatarExplorer.UI;

public partial class MainWindow
{
    private TaskCompletionSource<string?>? _textTcs;

    private Task<string?> Main_ShowTextDialogAsync(string title, string initialText = "")
    {
        if (_textTcs != null)
            throw new InvalidOperationException("TextDialog is already shown.");

        _textTcs = new TaskCompletionSource<string?>();

        TextDialogOverlay_Title.Text = title;
        if (!string.IsNullOrEmpty(initialText)) TextDialogOverlay_Content.Text = initialText;
        TextDialogOverlay.IsVisible = true;

        return _textTcs.Task;
    }

    private void TextDialogOverlay_Close(string? result)
    {
        TextDialogOverlay.IsVisible = false;

        var tcs = _textTcs;
        _textTcs = null;

        tcs?.TrySetResult(result);
    }

    private void TextDialogOverlay_Add_Click(object? sender, RoutedEventArgs e) => TextDialogOverlay_Close(TextDialogOverlay_Content.Text);

    private void TextDialogOverlay_Cancel_Click(object? sender, RoutedEventArgs e) => TextDialogOverlay_Close(null);
}
