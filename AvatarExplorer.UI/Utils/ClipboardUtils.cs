// This code is borrowed from Avalonia
// Github Code URL: https://github.com/AvaloniaUI/AvaloniaUI.QuickGuides/blob/main/ClipboardOps/ViewModels/MainWindowViewModel.cs

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace AvatarExplorer.UI.Utils;

internal static class ClipboardUtils
{
    internal static async Task SetTextToClipboard(string text)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow?.Clipboard is not { } provider) return;
        await provider.SetTextAsync(text);
    }
}
